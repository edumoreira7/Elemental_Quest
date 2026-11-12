using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// In-memory campaign contract. Local fragments never call this reward API.
public static class QuestCampaign
{
    static readonly HashSet<string> events = new HashSet<string>();
    static readonly HashSet<int> crystals = new HashSet<int>();
    public static readonly string[] Routes = { "FasePrincipal", "CumeDosVentos", "VulcaoCinzento", "CasteloDoEclipse" };
    public static readonly string[] Arenas = { "ArenaFloresta", "ArenaCume", "ArenaVulcao", "ArenaEclipse" };
    public static int CrystalCount => crystals.Count;
    public static bool HasCrystal(int region) => crystals.Contains(region);
    public static bool HasEvent(string id) => events.Contains(id);
    public static bool MarkEvent(string id) => events.Add(id);
    public static bool Complete => HasEvent("C06") && CrystalCount == 4;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset() { events.Clear(); crystals.Clear(); }
    // Called by the single C02-C05 completion callback (normal or skipped).
    // Prerequisites prevent direct scene entry from manufacturing campaign progress.
    public static bool CompleteGuardian(int region)
    {
        if(region < 0 || region > 3) return false;
        for(int i=0;i<region;i++) if(!HasCrystal(i)) return false;
        crystals.Add(region);
        events.Add("C0" + (region+2));
        return true;
    }
    public static bool CompleteEpilogue()
    {
        if(CrystalCount != 4 || !HasEvent("C05")) return false;
        events.Add("C06"); QuestSession.Venceu=true; return true;
    }
    public static void Travel(string scene)
    {
        Time.timeScale=1;
        QuestForestStage.ResetarProgresso();
        SceneManager.LoadScene(scene);
    }
    public static void LeaveArena(int region)
    {
        if(!HasCrystal(region)) return;
        Travel(region==3 ? "Epilogo" : Routes[region+1]);
    }
    public static void Retry(int region)
    {
        if(region==0) QuestForestStage.VoltarAPreparacao();
        else if(region==1) QuestCumeStage.ReturnToPreparation();
        else if(region>1 && region<4) Travel(Routes[region]);
    }
}
