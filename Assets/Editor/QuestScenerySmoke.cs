using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class QuestScenerySmoke
{
    static double started;
    static readonly string[] Scenes={"CumeDosVentos","ArenaCume","VulcaoCinzento","ArenaVulcao"};
    [InitializeOnLoadMethod]static void Resume(){if(!Application.isBatchMode||Array.IndexOf(Environment.GetCommandLineArgs(),"QuestScenerySmoke.Run")<0)return;EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;if(EditorApplication.isPlaying){started=EditorApplication.timeSinceStartup;EditorApplication.update-=Check;EditorApplication.update+=Check;}}
    public static void Run(){SessionState.SetInt("EQ_ART_STEP",0);EditorSceneManager.OpenScene("Assets/Scenes/"+Scenes[0]+".unity");EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;EditorApplication.isPlaying=true;}
    static void Changed(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode)return;started=EditorApplication.timeSinceStartup;EditorApplication.update-=Check;EditorApplication.update+=Check;}
    static void Check(){if(EditorApplication.timeSinceStartup-started<2)return;try{int step=SessionState.GetInt("EQ_ART_STEP",0);var art=UnityEngine.Object.FindAnyObjectByType<QuestRegionScenery>();if(art==null||art.background==null||art.DecoratedFloors!=(step%2==0?7:1))throw new Exception("Regional scenery/floor mismatch "+Scenes[step]);if(QuestSession.Atual==null||QuestSession.Atual.totalItens!=(step%2==0?20:0))throw new Exception("Collection total changed");if(Camera.main.transform.Find("Background regional composto")==null)throw new Exception("Background not camera-bound");Debug.Log("EQ_SCENERY_SCENE_OK "+Scenes[step]);step++;if(step==Scenes.Length){Debug.Log("EQ_SCENERY_ALL_OK: four scenes, backgrounds, visual floors and fragment totals; no full traversal");EditorApplication.update-=Check;EditorApplication.Exit(0);return;}SessionState.SetInt("EQ_ART_STEP",step);SceneManager.LoadScene(Scenes[step]);started=EditorApplication.timeSinceStartup;}catch(Exception e){Debug.LogException(e);EditorApplication.update-=Check;EditorApplication.Exit(1);}}
}
