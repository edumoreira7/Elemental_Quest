using System;
using System.IO;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuestGameplayPolishReview
{
    const string Output="C:/Users/morei/Documents/Jogos Digitais/GameplayPolish/";
    static int sceneIndex,step;
    static double next;
    static bool waveOnly;
    static readonly string[] Scenes={"CumeDosVentos","VulcaoCinzento","CasteloDoEclipse","ArenaCume","ArenaVulcao","ArenaEclipse","ArenaFloresta"};
    static readonly StringBuilder report=new StringBuilder();
    [MenuItem("Tools/Elemental Quest/Conferir ajustes de gameplay")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)return;
        waveOnly=false;
        sceneIndex=0;step=0;report.Clear();
        File.WriteAllText(Output+"play-review.txt","RUNNING");
        EditorSceneManager.OpenScene("Assets/Scenes/"+Scenes[0]+".unity");
        next=EditorApplication.timeSinceStartup+5;EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    [MenuItem("Tools/Elemental Quest/Conferir onda do Guardiao")]
    public static void RunWave()
    {
        if(EditorApplication.isPlaying)return;
        waveOnly=true;sceneIndex=6;step=0;report.Clear();
        EditorSceneManager.OpenScene("Assets/Scenes/ArenaFloresta.unity");
        next=EditorApplication.timeSinceStartup+5;EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        try {
            next=EditorApplication.timeSinceStartup+.8;
            if(sceneIndex<3)ReviewStage();else ReviewArena();
        }catch(Exception ex){report.AppendLine(ex.ToString());File.WriteAllText(Output+(waveOnly?"boss-wave-review.txt":"play-review.txt"),report.ToString());Debug.LogException(ex);EditorApplication.update-=Tick;EditorApplication.isPlaying=false;}
    }
    static void ReviewStage()
    {
        int region=sceneIndex+2;var camera=Camera.main;
        if(step==0) {
            var details=UnityEngine.Object.FindAnyObjectByType<QuestStageDetails>();
            if(details==null||details.PropsPlaced<6||details.PortalsPlaced!=1)throw new Exception("Props/portal missing "+Scenes[sceneIndex]);
            var player=UnityEngine.Object.FindAnyObjectByType<QuestPlayer>();
            if(Mathf.Abs(player.transform.lossyScale.x-.72f)>.01f)throw new Exception("Player scale mismatch");
            if(QuestSession.Atual.totalItens!=20)throw new Exception("Fragment count changed");
            camera.GetComponent<QuestCamera>().enabled=false;
            CheckTraversal(region,player);
            if(region==2)foreach(var enemy in UnityEngine.Object.FindObjectsByType<QuestEnemy>())if(enemy.tipo==QuestEnemy.Tipo.Atirador&&(!enemy.atiradorFixo||enemy.direcaoFixa!=-1||enemy.limiteEsquerdo!=enemy.limiteDireito))throw new Exception("Cume firing lane not fixed");
            if(region==3)foreach(var target in UnityEngine.Object.FindObjectsByType<QuestElementTarget>())if(target.tipo==QuestElementTarget.Tipo.Lava){target.Aplicar(QuestElementPowers.Elemento.Gelo);if(target.GetComponent<BoxCollider2D>().isTrigger)throw new Exception("Ice bridge missing");}
            camera.transform.position=new Vector3(8,.5f,-10);step++;
        }else if(step==1){Capture("fase-"+region+"-inicio");step++;}
        else if(step==2){camera.transform.position=new Vector3(region==2?32:region==3?66:78,.5f,-10);step++;}
        else if(step==3){Capture("fase-"+region+"-percurso");step++;}
        else if(step==4){camera.transform.position=new Vector3(104,.5f,-10);step++;}
        else if(step==5){Capture("fase-"+region+"-portal");step++;}
        else NextScene();
    }
    static void ReviewArena()
    {
        if(step==0) {
            QuestSession.Vidas=3;
            var protectedPlayer=UnityEngine.Object.FindAnyObjectByType<QuestPlayer>();
            protectedPlayer.enabled=false;
            typeof(QuestPlayer).GetField("invulneravelAte",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(protectedPlayer,Time.time+100);
            if(sceneIndex<6) {
                var details=UnityEngine.Object.FindAnyObjectByType<QuestStageDetails>();
                if(details==null||details.PropsPlaced!=2||details.PortalsPlaced!=1)throw new Exception("Arena details missing");
                var player=UnityEngine.Object.FindAnyObjectByType<QuestPlayer>();if(Mathf.Abs(player.transform.lossyScale.x-.72f)>.01f)throw new Exception("Arena duck scale mismatch");
                foreach(var child in details.GetComponentsInChildren<Transform>(true))if(child.name.StartsWith("Saida para "))child.gameObject.SetActive(true);
                if(sceneIndex==3)SetState<QuestTempestArena>("GustWarning");
                if(sceneIndex==4)SetState<QuestMagmaArena>("SpitWarning");
                if(sceneIndex==5)SetState<QuestEclipseArena>("ShotWarning");
            } else {
                var boss=UnityEngine.Object.FindAnyObjectByType<QuestRuinBoss>();typeof(QuestRuinBoss).GetMethod("Preparar",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(boss,null);
            }
            step++;next=EditorApplication.timeSinceStartup+.2;
        }else if(step==1){Capture(Scenes[sceneIndex]+"-aviso");step++;next=EditorApplication.timeSinceStartup+(sceneIndex==6?.9:1.8);}
        else if(step==2){
            if(waveOnly){
                var wave=UnityEngine.Object.FindAnyObjectByType<QuestBossWave>();
                if(wave==null)throw new Exception("Boss wave missing");
                SpriteRenderer drawing=null;foreach(var renderer in wave.GetComponentsInChildren<SpriteRenderer>())if(renderer.name=="Tamanho visual do disparo")drawing=renderer;
                if(drawing==null||!drawing.enabled||Mathf.Abs(drawing.transform.localScale.x-1.45f)>.01f||drawing.sharedMaterial.mainTexture!=drawing.sprite.texture)throw new Exception("Visible wave resize mismatch");
                report.AppendLine("Live wave drawing verified at 1.45x with current animation texture.");
            }
            Capture(Scenes[sceneIndex]+"-ataque");step++;
        }
        else NextScene();
    }
    static void SetState<T>(string state)where T:MonoBehaviour
    {
        var method=typeof(T).GetMethod("Set",BindingFlags.NonPublic|BindingFlags.Instance);
        method.Invoke(UnityEngine.Object.FindAnyObjectByType<T>(),new object[]{Enum.Parse(method.GetParameters()[0].ParameterType,state),1f});
    }
    static void CheckTraversal(int region,QuestPlayer player)
    {
        var floors=QuestStageLayout.Floors(region);var previous=Physics2D.simulationMode;Physics2D.simulationMode=SimulationMode2D.Script;
        var probe=new GameObject("Probe de travessia",typeof(Rigidbody2D),typeof(BoxCollider2D));
        var body=probe.GetComponent<Rigidbody2D>();body.mass=1;body.gravityScale=3;body.freezeRotation=true;
        var box=probe.GetComponent<BoxCollider2D>();box.size=player.GetComponent<Collider2D>().bounds.size;
        int needsDouble=0,needsGlide=0;
        try {
            for(int i=0;i<5;i++) {
                bool normal=Jump(body,box,floors[i],floors[i+1],false,false);
                bool doubled=Jump(body,box,floors[i],floors[i+1],true,false);
                bool full=normal||doubled||Jump(body,box,floors[i],floors[i+1],true,true);
                if(!full)throw new Exception("Unreachable gap region "+region+" index "+i);
                if(!normal)needsDouble++;if(!normal&&!doubled)needsGlide++;
            }
            if(region==2&&(needsDouble==0||needsGlide==0))throw new Exception("Cume does not require both Air abilities");
            report.AppendLine("Region "+region+": five physical crossings reachable; normal jump fails "+needsDouble+", double without glide fails "+needsGlide+". Props and portals present; 20 fragments; duck scale 0.72.");
        } finally {UnityEngine.Object.DestroyImmediate(probe);Physics2D.simulationMode=previous;}
    }
    static bool Jump(Rigidbody2D body,BoxCollider2D box,Vector3 from,Vector3 to,bool extra,bool glide)
    {
        body.position=new Vector2(from.y-.45f,from.z+box.size.y*.5f+.03f);body.linearVelocity=new Vector2(7,10);body.gravityScale=3;
        Physics2D.SyncTransforms();float glideTime=0;
        for(int i=0;i<180;i++) {
            if(extra&&i==16)body.linearVelocity=new Vector2(7,10);
            bool flying=glide&&body.linearVelocity.y<0&&glideTime<1.2f;
            body.gravityScale=flying?.6f:3;
            if(flying){glideTime+=.02f;body.linearVelocity=new Vector2(7,Mathf.Max(body.linearVelocity.y,-1.5f));}
            else body.linearVelocity=new Vector2(7,body.linearVelocity.y);
            Physics2D.Simulate(.02f);
            if(body.position.x>=to.x-box.size.x*.45f&&body.position.x<=to.y&&Mathf.Abs(box.bounds.min.y-to.z)<.08f&&body.linearVelocity.y<.1f)return true;
            if(body.position.y<to.z-2||body.position.x>to.y+1)return false;
        }
        return false;
    }
    static void Capture(string name){ScreenCapture.CaptureScreenshot(Output+name+".png");}
    static void NextScene()
    {
        report.AppendLine("Visual captures: "+Scenes[sceneIndex]);
        sceneIndex++;step=0;
        if(sceneIndex==Scenes.Length){report.AppendLine(waveOnly?"PASS: visible boss wave scale and texture checked in Play.":"PASS: physics probes and visual captures. Not a complete player-controlled run.");File.WriteAllText(Output+(waveOnly?"boss-wave-review.txt":"play-review.txt"),report.ToString());EditorApplication.update-=Tick;EditorApplication.isPlaying=false;return;}
        SceneManager.LoadScene(Scenes[sceneIndex]);next=EditorApplication.timeSinceStartup+4;
    }
}


