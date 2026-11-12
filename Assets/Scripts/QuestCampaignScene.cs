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
    void Start(){if(epilogue)QuestCampaign.CompleteEpilogue();}
    void DrawEnding()
    {
        var previous=GUI.matrix;float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2),Quaternion.identity,Vector3.one*scale);
        GUI.Box(new Rect(170,130,940,460),"");var text=new GUIStyle(GUI.skin.label){fontSize=26,wordWrap=true,alignment=TextAnchor.MiddleCenter};
        GUI.Label(new Rect(210,165,860,250),QuestCampaign.Complete?"O ECLIPSE PASSOU\n\nOs quatro cristais voltaram a brilhar.\nAurelia esta livre. Quackerius e Umbrius voltam juntos.\n\nFim da jornada":"EPILOGO\n\nRecupere os quatro cristais para concluir a jornada.",text);
        if(GUI.Button(new Rect(450,440,380,50),"VOLTAR AO MENU"))QuestSession.VoltarAoMenu();
        if(GUI.Button(new Rect(450,510,380,50),"NOVO JOGO"))QuestSession.NovoJogo();
        GUI.matrix=previous;
    }
    void OnGUI()
    {
        if(epilogue){DrawEnding();return;}
        float w=Mathf.Min(700,Screen.width-32);
        GUILayout.BeginArea(new Rect((Screen.width-w)/2,30,w,Screen.height-60),GUI.skin.box);
        GUILayout.Label(epilogue ? "EpÃ­logo" : new[]{"Floresta", "Cume dos Ventos Partidos", "VulcÃ£o Cinzento", "Castelo do Eclipse"}[region] + (arena?" â€” arena":" â€” percurso"));
        GUILayout.Label("Cristais da campanha: "+QuestCampaign.CrystalCount+" / 4");
        GUILayout.Label("Estrutura S02. Percurso, luta, arte e apresentaÃ§Ã£o desta cena aguardam suas tarefas especÃ­ficas.");
        if(epilogue) {
            GUILayout.Label(QuestCampaign.Complete?"Campanha concluÃ­da":"RestauraÃ§Ã£o e crÃ©ditos aguardam C06. A prÃ©via nÃ£o conclui a campanha.");
        } else if(!arena) {
            if(GUILayout.Button("Seguir para a arena (prÃ©via de navegaÃ§Ã£o)"))Go(QuestCampaign.Arenas[region]);
        } else {
            GUILayout.Label("Evento de conclusÃ£o: "+CompletionEvent+". Recompensa somente apÃ³s guardiÃ£o e apresentaÃ§Ã£o.");
            if(QuestCampaign.HasCrystal(region) && GUILayout.Button("Usar saÃ­da")) QuestCampaign.LeaveArena(region);
            if(GUILayout.Button("Voltar Ã  preparaÃ§Ã£o"))ReturnAfterDefeat();
            if(GUILayout.Button("PrÃ©via da prÃ³xima cena â€” sem recompensa"))Go(region==3?"Epilogo":QuestCampaign.Routes[region+1]);
        }
        if(GUILayout.Button("Voltar ao menu"))QuestSession.VoltarAoMenu();
        GUILayout.EndArea();
    }
}
