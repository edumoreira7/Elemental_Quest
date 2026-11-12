using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class QuestCastleSmoke
{
    static double started;
    [InitializeOnLoadMethod]
    static void Resume()
    {
        if(!Application.isBatchMode||Array.IndexOf(Environment.GetCommandLineArgs(),"QuestCastleSmoke.Run")<0)return;
        EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;
        if(EditorApplication.isPlaying){started=EditorApplication.timeSinceStartup;EditorApplication.update-=Check;EditorApplication.update+=Check;}
    }
    public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/CasteloDoEclipse.unity");EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;EditorApplication.isPlaying=true;}
    static void Changed(PlayModeStateChange change){if(change!=PlayModeStateChange.EnteredPlayMode)return;started=EditorApplication.timeSinceStartup;EditorApplication.update-=Check;EditorApplication.update+=Check;}
    static void Check()
    {
        if(EditorApplication.timeSinceStartup-started<2)return;EditorApplication.update-=Check;
        try {
            var stage=UnityEngine.Object.FindAnyObjectByType<QuestCastleStage>();
            if(stage==null||stage.Player==null||stage.Enemies.Count!=5||stage.FragmentCount!=20||QuestSession.Atual.totalItens!=20)throw new Exception("Stage/player/enemy/fragment activation mismatch");
            int roots=0;
            foreach(var target in UnityEngine.Object.FindObjectsByType<QuestElementTarget>()) {
                if(target.tipo!=QuestElementTarget.Tipo.Raizes)throw new Exception("Unexpected lava/water system");roots++;
            }
            if(roots!=1||stage.PlatformCount!=7||Camera.main==null)throw new Exception("Geometry/reference mismatch");
            if(stage.Enemies[0].tipo!=QuestEnemy.Tipo.Guaxinim||stage.Enemies[1].tipo!=QuestEnemy.Tipo.Atirador||stage.Enemies[2].tipo!=QuestEnemy.Tipo.Guardiao)throw new Exception("Archetypes missing");
            if(!stage.Enemies[0].name.Contains("Gato")||!stage.Enemies[1].name.Contains("Corvo")||stage.Enemies[1].prefabDisparo==null||!stage.Enemies[2].name.Contains("Urso")||stage.Enemies[2].atacar.Length!=5)throw new Exception("Castle skins/projectile missing");
            Debug.Log("EQ_CASTLE_ACTIVATION_OK: player, five enemies, three archetypes, cat, raven and bear skins, 20 fragments, seven platforms, one root, no lava; no full traversal");EditorApplication.Exit(0);
        } catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
}
