using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class QuestVolcanoSmoke
{
    static double started;
    [InitializeOnLoadMethod]
    static void Resume()
    {
        if(!Application.isBatchMode||Array.IndexOf(Environment.GetCommandLineArgs(),"QuestVolcanoSmoke.Run")<0)return;
        EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;
        if(EditorApplication.isPlaying){started=EditorApplication.timeSinceStartup;EditorApplication.update-=Check;EditorApplication.update+=Check;}
    }
    public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/VulcaoCinzento.unity");EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;EditorApplication.isPlaying=true;}
    static void Changed(PlayModeStateChange change){if(change!=PlayModeStateChange.EnteredPlayMode)return;started=EditorApplication.timeSinceStartup;EditorApplication.update-=Check;EditorApplication.update+=Check;}
    static void Check()
    {
        if(EditorApplication.timeSinceStartup-started<2)return;EditorApplication.update-=Check;
        try {
            var stage=UnityEngine.Object.FindAnyObjectByType<QuestVolcanoStage>();
            if(stage==null||stage.Player==null||stage.Enemies.Count!=5||stage.FragmentCount!=20||QuestSession.Atual.totalItens!=20)throw new Exception("Stage/player/enemy/fragment activation mismatch");
            int lava=0,roots=0;
            foreach(var target in UnityEngine.Object.FindObjectsByType<QuestElementTarget>()) {
                if(target.tipo==QuestElementTarget.Tipo.Raizes){roots++;continue;}
                if(target.tipo!=QuestElementTarget.Tipo.Lava)continue;lava++;
                if(!target.GetComponent<BoxCollider2D>().isTrigger)throw new Exception("Lava initially solid");
                if(!target.Aplicar(QuestElementPowers.Elemento.Gelo)||target.GetComponent<BoxCollider2D>().isTrigger)throw new Exception("Existing freeze contract failed");
            }
            if(lava!=2||roots!=1)throw new Exception("Element targets missing");
            if(!stage.Enemies[0].name.Contains("Raposa")||!stage.Enemies[1].name.Contains("Lagarto")||!stage.Enemies[2].name.Contains("Javali")||stage.Enemies[2].parado.Length!=4||stage.Enemies[2].atacar.Length!=5)throw new Exception("Skin references missing");
            Debug.Log("EQ_VOLCANO_ACTIVATION_OK: player, five enemies, three skins, 20 fragments, two freezeable targets, one root; no full traversal");EditorApplication.Exit(0);
        } catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
}
