#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public class QuestCombatRevisionTest:MonoBehaviour
{
    int checks;bool failed;
    static BindingFlags fields=BindingFlags.Instance|BindingFlags.NonPublic;
    public static void Begin(){EditorSceneManager.OpenScene("Assets/Scenes/FasePrincipal.unity");EditorApplication.isPlaying=true;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init(){if(!Environment.GetCommandLineArgs().Contains("-questCombatRevisionTest")||FindAnyObjectByType<QuestCombatRevisionTest>()!=null)return;var o=new GameObject("Validacao do ritmo do Guardiao");DontDestroyOnLoad(o);o.AddComponent<QuestCombatRevisionTest>();}
    void Check(bool ok,string message){if(!ok){failed=true;Debug.LogError("EQ_COMBAT_REVISION_FAIL: "+message);EditorApplication.Exit(1);throw new Exception(message);}checks++;Debug.Log("EQ_COMBAT_CHECK "+checks+": "+message);}
    void Update(){if(Time.realtimeSinceStartup>180&&!failed){failed=true;Debug.LogError("EQ_COMBAT_REVISION_TIMEOUT");EditorApplication.Exit(2);}}
    IEnumerator Start()
    {
        bool artOk=true;float reference=0;
        foreach(string folder in Directory.GetDirectories("Assets/ElementalQuest/GuardiaoRuinasVisual/Frames"))
        {
            string path=Directory.GetFiles(folder,"*.png").OrderBy(p=>p).First().Replace('\\','/');
            var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes(path));var pixels=texture.GetPixels32();int bottom=texture.height,top=0;
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>128){bottom=Math.Min(bottom,y);top=Math.Max(top,y);}
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);float height=(top-bottom+1)/sprite.pixelsPerUnit;
            if(reference==0)reference=height;artOk&=Mathf.Abs(height-reference)<.001f&&Mathf.Abs(sprite.pivot.y-bottom)<.01f;Destroy(texture);
        }
        Check(artOk,"Oito animacoes com escala de referencia uniforme e pes alinhados");
        yield return new WaitForSeconds(.5f);
        var enemies=FindObjectsByType<QuestEnemy>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        Check(enemies.Length==5&&enemies.All(e=>e.gameObject.activeInHierarchy&&e.enabled),"Cinco inimigos presentes e ativos desde o inicio");
        float[] xs=enemies.Select(e=>e.transform.position.x).ToArray();yield return new WaitForSeconds(.6f);
        Check(enemies.Select((e,i)=>Mathf.Abs(e.transform.position.x-xs[i])>.05f).Count(v=>v)>=3,"Patrulhas funcionam mesmo distantes do jogador");
        Check(enemies.All(e=>!e.combatePermitido)&&FindObjectsByType<QuestEnemyShot>(FindObjectsSortMode.None).Length==0,"Sem ataques fora da camera");
        SceneManager.LoadScene("Tutorial");yield return new WaitForSeconds(.6f);
        enemies=FindObjectsByType<QuestEnemy>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        Check(enemies.Length>=3&&enemies.All(e=>e.gameObject.activeInHierarchy&&e.enabled),"Inimigos do tutorial tambem presentes desde o inicio");
        SceneManager.LoadScene("ArenaFloresta");yield return new WaitForSeconds(.6f);
        var boss=FindAnyObjectByType<QuestRuinBoss>();var pato=FindAnyObjectByType<PatoController>();pato.enabled=false;
        Capture("boss-escala-idle.png");
        var body=pato.GetComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.linearVelocity=Vector2.zero;
        QuestSession.Vidas=100;Time.timeScale=3;
        var player=pato.GetComponent<QuestPlayer>();var playerCollider=pato.GetComponent<Collider2D>();
        Vector3 ground=new Vector3(-5,-2.35f,0);pato.transform.position=ground;body.position=ground;
        int initial=boss.Vida;
        yield return Shoot(boss,pato);
        Check(boss.Vida==initial,"Nucleo protegido rejeita projetil real");
        bool low=false,high=false;QuestBossWave last=null;float until=Time.time+25;
        while(boss.Atual!=QuestRuinBoss.Estado.Vulneravel&&Time.time<until)
        {
            CheckIf(boss.AtaquesNaSequencia<boss.TamanhoSequencia,boss.Atual!=QuestRuinBoss.Estado.Vulneravel,"Nao abre nucleo no meio da sequencia");
            var wave=boss.OndaAtual;
            if(wave!=null&&wave!=last)
            {
                last=wave;bool upper=boss.AtaqueAlto;
                Capture(upper?"boss-ataque-alto.png":"boss-ataque-baixo.png");
                float direction=boss.Direcao;
                pato.transform.position=new Vector3(wave.transform.position.x+direction*1.4f,ground.y,0);body.position=pato.transform.position;Physics2D.SyncTransforms();
                typeof(QuestPlayer).GetField("invulneravelAte",fields).SetValue(player,0f);int before=QuestSession.Vidas;
                yield return new WaitForSeconds(.35f);
                if(upper){Check(QuestSession.Vidas==before,"Onda alta nao acerta pato no chao");high=true;}
                else {Check(QuestSession.Vidas==before-1,"Onda baixa causa dano real no chao");low=true;}
                pato.transform.position=ground;body.position=ground;
            }
            yield return null;
        }
        Check(low&&high&&boss.Atual==QuestRuinBoss.Estado.Vulneravel&&boss.AtaquesNaSequencia==4,"Primeira abertura apenas depois de quatro ataques");
        Check(boss.OndaAtual==null,"Nenhuma onda persiste durante vulnerabilidade");
        Capture("boss-escala-vulneravel.png");
        yield return Shoot(boss,pato);Check(boss.Vida==initial-1,"Magia existente causa dano na janela de vulnerabilidade");
        yield return new WaitForSeconds(.5f);
        Check(boss.Atual!=QuestRuinBoss.Estado.Idle&&boss.Atual!=QuestRuinBoss.Estado.Vulneravel,"Retoma sequencia sem pausa longa apos receber dano");
        // Isola a geometria das ondas: a mesma colisao deve permitir pular a baixa e ficar sob a alta.
        boss.enabled=false;
        if(boss.OndaAtual!=null)Destroy(boss.OndaAtual.gameObject);
        yield return null;
        foreach(bool upper in new[]{false,true})
        {
            var wave=Instantiate(boss.prefabOnda,new Vector3(-6,upper?-1.05f:-2.62f,0),Quaternion.identity).GetComponent<QuestBossWave>();wave.velocidade=6;wave.alcance=16;wave.Inicializar(boss,1);
            pato.transform.position=ground+Vector3.up*1.6f;body.position=pato.transform.position;Physics2D.SyncTransforms();
            typeof(QuestPlayer).GetField("invulneravelAte",fields).SetValue(player,0f);int before=QuestSession.Vidas;
            yield return new WaitForSeconds(.35f);
            Check(upper?QuestSession.Vidas==before-1:QuestSession.Vidas==before,upper?"Onda alta atinge a faixa de pulo":"Pulo evita onda baixa");Destroy(wave.gameObject);
        }
        pato.transform.position=ground;body.position=ground;
        // Vida preparada para exercer a transicao real de fase pelo receptor de dano existente.
        typeof(QuestRuinBoss).GetField("<Vida>k__BackingField",fields).SetValue(boss,4);
        typeof(QuestRuinBoss).GetMethod("Trocar",fields).Invoke(boss,new object[]{QuestRuinBoss.Estado.Vulneravel,2f});
        yield return Shoot(boss,pato);boss.enabled=true;yield return new WaitForSeconds(.4f);
        Check(boss.SegundaEtapa&&boss.TamanhoSequencia==6,"Segunda metade inicia sequencia de seis ataques");
        until=Time.time+25;int highest=0;
        while(boss.Atual!=QuestRuinBoss.Estado.Vulneravel&&Time.time<until){highest=Math.Max(highest,boss.AtaquesNaSequencia);yield return null;}
        Check(boss.Atual==QuestRuinBoss.Estado.Vulneravel&&boss.AtaquesNaSequencia==6&&highest>=5,"Segunda abertura aguarda seis ataques completos");
        typeof(QuestRuinBoss).GetField("<Vida>k__BackingField",fields).SetValue(boss,1);
        yield return Shoot(boss,pato);Check(boss.Atual==QuestRuinBoss.Estado.Derrotado&&boss.Vida==0&&!boss.GetComponent<Collider2D>().enabled,"Vitoria interrompe dano e derrota o boss");
        yield return new WaitForSeconds(1.3f);Check(boss.arena.saida.activeSelf,"Derrota libera portal de conclusao");
        Time.timeScale=1;Debug.Log("EQ_COMBAT_REVISION_OK "+checks);EditorApplication.Exit(0);
    }
    void CheckIf(bool condition,bool ok,string message){if(condition&&!ok)Check(false,message);}
    void Capture(string path)
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
        var camera=Camera.main;var previous=camera.targetTexture;var active=RenderTexture.active;var rect=camera.rect;
        var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.rect=new Rect(0,0,1,1);camera.Render();RenderTexture.active=rt;
        var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
        camera.targetTexture=previous;camera.rect=rect;RenderTexture.active=active;Destroy(image);Destroy(rt);
    }
    IEnumerator Shoot(QuestRuinBoss boss,PatoController pato)
    {
        var shot=Instantiate(pato.prefabPoder,boss.GetComponent<Collider2D>().bounds.center,Quaternion.identity);
        shot.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Physics2D.SyncTransforms();yield return new WaitForSeconds(.1f);
    }
}
#endif

