using UnityEngine;
using UnityEngine.SceneManagement;

// S02 scene shell. Regional traversal, bosses and C00 presentations attach here.
// Preview navigation intentionally grants no crystals or narrative completion.
public class QuestCampaignScene : MonoBehaviour
{
    public int region;
    public bool arena;
    public bool epilogue;
    bool leaving;
    public bool GuardianDefeated { get; private set; }
    public string CompletionEvent => epilogue ? "C06" : "C0"+(region+2);
    public void NotifyGuardianDefeated() { if(arena) GuardianDefeated=true; }
    // C00 invokes this once after the presentation or skip; never on arena entry.
    public void CompletePresentation()
    {
        if(epilogue) QuestCampaign.CompleteEpilogue();
        else if(arena && GuardianDefeated) QuestCampaign.CompleteGuardian(region);
    }
    public void ReturnAfterDefeat() { if(!leaving) {leaving=true;QuestCampaign.Retry(region);} }
    void Go(string scene) { if(leaving)return;leaving=true;QuestCampaign.Travel(scene); }
    void OnGUI()
    {
        float w=Mathf.Min(700,Screen.width-32);
        GUILayout.BeginArea(new Rect((Screen.width-w)/2,30,w,Screen.height-60),GUI.skin.box);
        GUILayout.Label(epilogue ? "Epílogo" : new[]{"Floresta", "Cume dos Ventos Partidos", "Vulcão Cinzento", "Castelo do Eclipse"}[region] + (arena?" — arena":" — percurso"));
        GUILayout.Label("Cristais da campanha: "+QuestCampaign.CrystalCount+" / 4");
        GUILayout.Label("Estrutura S02. Percurso, luta, arte e apresentação desta cena aguardam suas tarefas específicas.");
        if(epilogue) {
            GUILayout.Label(QuestCampaign.Complete?"Campanha concluída":"Restauração e créditos aguardam C06. A prévia não conclui a campanha.");
        } else if(!arena) {
            if(GUILayout.Button("Seguir para a arena (prévia de navegação)"))Go(QuestCampaign.Arenas[region]);
        } else {
            GUILayout.Label("Evento de conclusão: "+CompletionEvent+". Recompensa somente após guardião e apresentação.");
            if(QuestCampaign.HasCrystal(region) && GUILayout.Button("Usar saída")) QuestCampaign.LeaveArena(region);
            if(GUILayout.Button("Voltar à preparação"))ReturnAfterDefeat();
            if(GUILayout.Button("Prévia da próxima cena — sem recompensa"))Go(region==3?"Epilogo":QuestCampaign.Routes[region+1]);
        }
        if(GUILayout.Button("Voltar ao menu"))QuestSession.VoltarAoMenu();
        GUILayout.EndArea();
    }
}
