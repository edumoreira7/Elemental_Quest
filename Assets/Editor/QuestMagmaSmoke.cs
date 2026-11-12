using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Narrow activation check only; does not simulate a completed battle.
public static class QuestMagmaSmoke
{
    static double started;
    [InitializeOnLoadMethod]
    static void ResumeAfterReload()
    {
        if(!Application.isBatchMode || Array.IndexOf(Environment.GetCommandLineArgs(),"QuestMagmaSmoke.Run")<0)return;
        EditorApplication.playModeStateChanged-=Changed; EditorApplication.playModeStateChanged+=Changed;
        if(EditorApplication.isPlaying) { started=EditorApplication.timeSinceStartup;EditorApplication.update-=Check;EditorApplication.update+=Check; }
    }
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/ArenaVulcao.unity");
        EditorApplication.playModeStateChanged+=Changed;
        EditorApplication.isPlaying=true;
    }
    static void Changed(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode)return;
        started=EditorApplication.timeSinceStartup;EditorApplication.update+=Check;
    }
    static void Check()
    {
        if(EditorApplication.timeSinceStartup-started<2)return;
        EditorApplication.update-=Check;
        try {
            var arena=UnityEngine.Object.FindAnyObjectByType<QuestMagmaArena>();
            if(arena==null||arena.Health!=6||arena.Leaving)throw new Exception("Arena inactive");
            if(UnityEngine.Object.FindAnyObjectByType<QuestPlayer>()==null)throw new Exception("Player missing");
            if(Camera.main==null||!Camera.main.orthographic)throw new Exception("Fixed camera missing");
            if(arena.idle.Length!=6||arena.gustFlight.Length!=4)throw new Exception("Frame references missing");
            if(GameObject.Find("Saida para o Castelo")!=null)throw new Exception("Exit prematurely active");
            Debug.Log("EQ_MAGMA_ACTIVATION_OK: arena, player, camera, frames, locked exit; no battle validation");
            EditorApplication.Exit(0);
        } catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}

