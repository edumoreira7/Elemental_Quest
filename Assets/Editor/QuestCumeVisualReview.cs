using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Explicit editor-only visual review; never runs in a player build.
public static class QuestCumeVisualReview
{
    const string Output = "C:/Users/morei/Documents/Jogos Digitais/CumeVisualFix";
    static readonly float[] Positions = { 5, 28, 60, 104 };
    static int step;
    static double next;
    [MenuItem("Tools/Elemental Quest/Conferir visual da fase 2")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)return;
        EditorSceneManager.OpenScene("Assets/Scenes/CumeDosVentos.unity");
        step=0; next=EditorApplication.timeSinceStartup+5;
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;
        EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(EditorApplication.timeSinceStartup<next||!EditorApplication.isPlaying)return;
        try {
            var art=UnityEngine.Object.FindAnyObjectByType<QuestRegionScenery>();
            if(art==null||!art.cume)throw new Exception("Cume scenery missing");
            if(art.DecoratedFloors!=(step<=8?7:1))throw new Exception("Floor count changed");
            var camera=Camera.main;
            if(step<8) {
                var follow=camera.GetComponent<QuestCamera>();if(follow!=null)follow.enabled=false;
                if(step%2==0) {camera.transform.position=new Vector3(Positions[step/2],.5f,-10);}
                else {Validate(art);ScreenCapture.CaptureScreenshot(Output+"/cume-"+(step/2)+".png");}
                step++;next=EditorApplication.timeSinceStartup+.8;
                
            } else if(step==8) { SceneManager.LoadScene("ArenaCume");step++;next=EditorApplication.timeSinceStartup+3;
            } else if(step==9) {
                Validate(art);ScreenCapture.CaptureScreenshot(Output+"/arena-cume.png");step++;next=EditorApplication.timeSinceStartup+1;
            } else {
                File.WriteAllText(Output+"/play-review.txt","PASS: Cume at camera x=5,28,60,104 and ArenaCume. 7/1 floors; each scenery material matches its sprite texture; background material separated from floor. Screenshots captured in Play. Combat mechanics not modified or fully simulated.");
                EditorApplication.update-=Tick;EditorApplication.isPlaying=false;
            }
        } catch(Exception ex) {
            File.WriteAllText(Output+"/play-review.txt",ex.ToString());Debug.LogException(ex);EditorApplication.update-=Tick;EditorApplication.isPlaying=false;
        }
    }
    static void Validate(QuestRegionScenery art)
    {
        var background=Camera.main.transform.Find("Background regional composto").GetComponent<SpriteRenderer>();
        foreach(var sr in art.GetComponentsInChildren<SpriteRenderer>()) {
            if(sr.name!="Tile regional - visual")continue;
            if(sr.sharedMaterial==background.sharedMaterial)throw new Exception("Backdrop and floor share a material");
            if(sr.sharedMaterial.mainTexture!=sr.sprite.texture)throw new Exception("Floor material texture mismatch");
        }
    }
}

