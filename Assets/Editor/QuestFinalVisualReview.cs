using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuestFinalVisualReview
{
    const string Output="C:/Users/morei/Documents/Jogos Digitais/FinalVisualFix/";
    static int step;
    static double next;
    static readonly float[] Positions={5,28,60,104};
    [MenuItem("Tools/Elemental Quest/Conferir castelo e avisos dos bosses")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)return;
        EditorSceneManager.OpenScene("Assets/Scenes/CasteloDoEclipse.unity");
        step=0;next=EditorApplication.timeSinceStartup+5;
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        try {
            next=EditorApplication.timeSinceStartup+.4;
            if(step<8) {
                var art=UnityEngine.Object.FindAnyObjectByType<QuestCastleScenery>();ValidateCastle(art,7);
                Camera.main.GetComponent<QuestCamera>().enabled=false;
                if(step%2==0)Camera.main.transform.position=new Vector3(Positions[step/2],.5f,-10);
                else Capture("castle-"+(step/2));
                next=EditorApplication.timeSinceStartup+.8;
            } else switch(step) {
                case 8:Load("ArenaEclipse");break;
                case 9:ValidateCastle(UnityEngine.Object.FindAnyObjectByType<QuestCastleScenery>(),1);Warn<QuestEclipseArena>("ShotWarning");break;
                case 10:Capture("eclipse-hint");Delay(1.85);break;
                case 11:CheckShot<QuestEclipseGust>(1.25f);Capture("eclipse-shot");break;
                case 12:Warn<QuestEclipseArena>("WaveWarning");break;
                case 13:Capture("eclipse-wave-hint");Delay(1.85);break;
                case 14:CheckShot<QuestEclipseGust>(1.2f);Capture("eclipse-wave");break;
                case 15:Load("ArenaCume");break;
                case 16:Warn<QuestTempestArena>("GustWarning");break;
                case 17:Capture("cume-hint");Delay(1.85);break;
                case 18:CheckShot<QuestTempestGust>(1.35f);Capture("cume-shot");break;
                case 19:Load("ArenaVulcao");break;
                case 20:Warn<QuestMagmaArena>("SpitWarning");break;
                case 21:Capture("magma-hint");Delay(1.85);break;
                case 22:CheckShot<QuestMagmaGust>(1.15f);Capture("magma-shot");break;
                case 23:
                    File.WriteAllText(Output+"play-review.txt","PASS: castle at x=5,28,60,104 and Eclipse arena have texture-specific materials, floor counts 7/1. Captured compact hints and enlarged active shots for Eclipse, Coruja and Magma; projectile visual scales checked. No completed battles simulated.");
                    EditorApplication.update-=Tick;EditorApplication.isPlaying=false;return;
            }
            step++;
        } catch(Exception ex){File.WriteAllText(Output+"play-review.txt",ex.ToString()+" step="+step);Debug.LogException(ex);EditorApplication.update-=Tick;EditorApplication.isPlaying=false;}
    }
    static void Delay(double seconds){next=EditorApplication.timeSinceStartup+seconds;}
    static void Load(string name){SceneManager.LoadScene(name);Delay(3);}
    static void Capture(string name){ScreenCapture.CaptureScreenshot(Output+name+".png");}
    static void Warn<T>(string state) where T:MonoBehaviour
    {
        foreach(var shot in UnityEngine.Object.FindObjectsByType<QuestEclipseGust>())UnityEngine.Object.DestroyImmediate(shot.gameObject);
        foreach(var shot in UnityEngine.Object.FindObjectsByType<QuestTempestGust>())UnityEngine.Object.DestroyImmediate(shot.gameObject);
        foreach(var shot in UnityEngine.Object.FindObjectsByType<QuestMagmaGust>())UnityEngine.Object.DestroyImmediate(shot.gameObject);
        var arena=UnityEngine.Object.FindAnyObjectByType<T>();
        var method=typeof(T).GetMethod("Set",BindingFlags.NonPublic|BindingFlags.Instance);
        var stateType=method.GetParameters()[0].ParameterType;
        method.Invoke(arena,new object[]{Enum.Parse(stateType,state),1f});Delay(.25);
    }
    static void CheckShot<T>(float scale) where T:MonoBehaviour
    {
        var shot=UnityEngine.Object.FindAnyObjectByType<T>();
        if(shot==null||!shot.GetComponent<SpriteRenderer>().enabled||Mathf.Abs(shot.transform.localScale.x-scale)>.001f)throw new Exception("Projectile visibility/scale mismatch: "+typeof(T).Name);
        var renderer=shot.GetComponent<SpriteRenderer>();if(renderer.sharedMaterial.mainTexture!=renderer.sprite.texture)throw new Exception("Projectile texture mismatch");
    }
    static void ValidateCastle(QuestCastleScenery art,int count)
    {
        if(art==null||art.DecoratedFloors!=count)throw new Exception("Castle floor count mismatch");
        var sky=Camera.main.transform.Find("Eclipse - panorama").GetComponent<SpriteRenderer>();
        foreach(var sr in art.GetComponentsInChildren<SpriteRenderer>()) {
            if(sr.name!="Eclipse - alvenaria"&&sr.name!="Eclipse - plataforma")continue;
            if(sr.sharedMaterial==sky.sharedMaterial||sr.sharedMaterial.mainTexture!=sr.sprite.texture)throw new Exception("Castle material mismatch");
        }
    }
}

