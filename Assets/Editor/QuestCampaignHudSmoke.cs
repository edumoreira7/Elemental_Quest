using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class QuestCampaignHudSmoke
{
    static double entered;
    [InitializeOnLoadMethod]
    static void Resume()
    {
        if(!Application.isBatchMode||Array.IndexOf(Environment.GetCommandLineArgs(),"QuestCampaignHudSmoke.Run")<0)return;
        EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;
        if(EditorApplication.isPlaying){entered=EditorApplication.timeSinceStartup;EditorApplication.update-=Check;EditorApplication.update+=Check;}
    }
    public static void Run(){SessionState.SetInt("EQ_HUD_STEP",0);EditorSceneManager.OpenScene("Assets/Scenes/Inicio.unity");EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;EditorApplication.isPlaying=true;}
    static void Changed(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode)return;entered=EditorApplication.timeSinceStartup;EditorApplication.update-=Check;EditorApplication.update+=Check;}
    static void Check()
    {
        if(EditorApplication.timeSinceStartup-entered<2)return;
        try {
            int step=SessionState.GetInt("EQ_HUD_STEP",0);
            if(step==0){var menu=UnityEngine.Object.FindAnyObjectByType<QuestMainMenu>();if(menu==null)throw new Exception("Menu missing");menu.Jogar();if(QuestCampaign.CrystalCount!=0)throw new Exception("New campaign dirty");menu.ContinuarAbertura();}
            else if(step==1){if(SceneManager.GetActiveScene().name!="FasePrincipal"||QuestSession.Atual==null||UnityEngine.Object.FindAnyObjectByType<QuestElementPowers>()==null)throw new Exception("Forest/HUD/powers missing");for(int i=0;i<4;i++)if(!QuestCampaign.CompleteGuardian(i))throw new Exception("Prepared reward failed");SceneManager.LoadScene("Epilogo");}
            else if(step==2){if(!QuestCampaign.Complete||!QuestSession.Venceu||QuestCampaign.HasEvent("C06"))throw new Exception("Ending/cutscene flags mismatch");QuestSession.NovoJogo();}
            else {if(SceneManager.GetActiveScene().name!="FasePrincipal"||QuestCampaign.CrystalCount!=0||QuestSession.Venceu||QuestCampaign.Complete)throw new Exception("New game did not reset");Debug.Log("EQ_CAMPAIGN_HUD_OK: menu opening, forest/powers, prepared four-crystal ending, clean new game; no battles/cutscenes played");EditorApplication.update-=Check;EditorApplication.Exit(0);return;}
            SessionState.SetInt("EQ_HUD_STEP",step+1);entered=EditorApplication.timeSinceStartup;
        }catch(Exception error){Debug.LogException(error);EditorApplication.update-=Check;EditorApplication.Exit(1);}
    }
}
