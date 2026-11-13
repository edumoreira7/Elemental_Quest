using UnityEngine;
using UnityEngine.SceneManagement;
public class QuestBossArena : MonoBehaviour
{
    public QuestRuinBoss boss;
    public Camera cameraJogo;
    public GameObject saida;
    public bool Saindo {get;private set;}
    public bool Concluiu {get;private set;}
    float liberarEm=-1;
    bool presentationStarted;
    void Start(){saida.SetActive(false);FixarCamera();}
    void LateUpdate()
    {
        FixarCamera();
        if(liberarEm>0 && Time.time>=liberarEm && !presentationStarted)
        {
            presentationStarted=true;
            if(QuestCampaign.HasEvent("C02"))ConcluirApresentacao();
            else QuestCutsceneDirector.Play("C02", ConcluirApresentacao);
        }
    }
    void ConcluirApresentacao(){if(QuestCampaign.CompleteGuardian(0))saida.SetActive(true);}
    void FixarCamera()
    {
        cameraJogo.transform.position=new Vector3(0,.5f,-10);cameraJogo.orthographicSize=5.5f;
        float alvo=16f/9,aspecto=Screen.width/(float)Mathf.Max(1,Screen.height);
        if(aspecto>alvo){float w=alvo/aspecto;cameraJogo.rect=new Rect((1-w)/2,0,w,1);}
        else {float h=aspecto/alvo;cameraJogo.rect=new Rect(0,(1-h)/2,1,h);}
    }
    public void BossDerrotado(){liberarEm=Time.time+1.2f;}
    public void Concluir()
    {
        if(Concluiu || Saindo || boss.Atual!=QuestRuinBoss.Estado.Derrotado || !saida.activeSelf)return;
        if(!QuestCampaign.HasCrystal(0))return;
        Concluiu=true;Saindo=true;QuestCampaign.LeaveArena(0);
    }
    public void RetornarAposDerrota()
    {if(Saindo)return;Saindo=true;boss.Interromper();QuestForestStage.VoltarAPreparacao();}
    void OnGUI()
    {
        var anterior=GUI.matrix;float escala=Mathf.Min(Screen.width/1280f,Screen.height/720f);
        GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*escala)/2,(Screen.height-720*escala)/2),Quaternion.identity,Vector3.one*escala);
        var estilo=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=20};
        GUI.Label(new Rect(340,164,600,28),"GUARDIAO DAS RUINAS",estilo);
        var cor=GUI.color;GUI.color=new Color(.04f,.08f,.06f,.95f);GUI.DrawTexture(new Rect(410,195,460,20),Texture2D.whiteTexture);
        GUI.color=new Color(.25f,.9f,.3f);GUI.DrawTexture(new Rect(413,198,454*boss.Vida/boss.vidaMaxima,14),Texture2D.whiteTexture);GUI.color=cor;
        if(Time.time<boss.CuraAte)
            GUI.Label(new Rect(300,222,680,28),"Terra absorvida: o Guardiao recupera vida!",estilo);
        if(Concluiu){GUI.Box(new Rect(340,280,600,160),"");GUI.Label(new Rect(360,300,560,50),"Fase concluida!",estilo);if(GUI.Button(new Rect(490,372,300,42),"Voltar ao menu")){Saindo=true;Time.timeScale=1;SceneManager.LoadScene("Inicio");}}
        GUI.matrix=anterior;
    }
}
