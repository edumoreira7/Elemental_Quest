using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestSession : MonoBehaviour
{
    public int fase;
    public int totalItens;
    public string nomeFase;
    public bool avisarColetaCompleta = true;
    public QuestTutorialFinal tutorialFinal;
    public QuestBossArena bossArena;
    public static int Vidas = 3;
    public static int Pontos;
    public static bool Venceu;
    public static QuestSession Atual;
    public int Coletados { get; private set; }
    public bool Pausado { get; private set; }
    string aviso;float avisoAte;int pontosInicioFase;bool reiniciando;
    GUIStyle texto,pequeno,titulo,botao;
    QuestElementPowers poderesHud;
    void Awake(){Atual=this;Time.timeScale=1;pontosInicioFase=Pontos;}
    void Update(){if(Input.GetKeyDown(KeyCode.Escape)){Pausado=!Pausado;Time.timeScale=Pausado?0:1;}}
    public static void PrepararNovaPartida(){Vidas=3;Pontos=0;Venceu=false;Time.timeScale=1;QuestForestStage.ResetarProgresso();QuestTutorialFinal.ReiniciarProgresso();}
    public static void NovoJogo(){PrepararNovaPartida();SceneManager.LoadScene("FasePrincipal");}
    public static void IniciarTutorial(){PrepararNovaPartida();SceneManager.LoadScene("Tutorial");}
    public static void VoltarAoMenu(){Time.timeScale=1;SceneManager.LoadScene("Inicio");}
    public void Coletar(){Coletados++;Pontos+=100;if(avisarColetaCompleta&&Coletados==totalItens)Avisar("Cristais coletados!");}
    public void Avisar(string mensagem){aviso=mensagem;avisoAte=Time.unscaledTime+3;}
    public bool ColetarVida(){if(reiniciando||Vidas>=3)return false;Vidas++;Avisar("Vida recuperada!");return true;}
    public void Portal(){if(tutorialFinal!=null)tutorialFinal.TentarConcluir();else Avisar("Siga ate a saida da area.");}
    public void PerderVida(){if(reiniciando)return;Vidas=Mathf.Max(0,Vidas-1);if(Vidas==0){reiniciando=true;Vidas=3;Pontos=pontosInicioFase;Venceu=false;Time.timeScale=1;if(bossArena!=null)bossArena.RetornarAposDerrota();else SceneManager.LoadScene(gameObject.scene.name);}}
    void Painel(Rect r){var cor=GUI.color;GUI.color=new Color(.025f,.07f,.14f,.93f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=cor;}
    void OnGUI()
    {
        if(texto==null){texto=new GUIStyle(GUI.skin.label){fontSize=24,alignment=TextAnchor.MiddleCenter};pequeno=new GUIStyle(texto){fontSize=18};titulo=new GUIStyle(texto){fontSize=42,fontStyle=FontStyle.Bold};botao=new GUIStyle(GUI.skin.button){fontSize=22};}
        var anterior=GUI.matrix;float s=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*s)/2,(Screen.height-720*s)/2),Quaternion.identity,Vector3.one*s);
        Painel(new Rect(20,18,1240,145));GUI.Label(new Rect(35,27,395,50),nomeFase,pequeno);
        if(poderesHud==null)poderesHud=FindAnyObjectByType<QuestElementPowers>();if(poderesHud!=null)QuestHudArt.Mostrar(poderesHud);
        GUI.Label(new Rect(675,27,260,50),bossArena==null?"CRISTAIS  "+Coletados+" / "+totalItens:"GUARDIAO",texto);GUI.Label(new Rect(960,27,275,50),"PONTOS  "+Pontos.ToString("00000"),texto);
        if(Time.unscaledTime<avisoAte){Painel(new Rect(260,175,760,56));GUI.Label(new Rect(270,182,740,40),aviso,pequeno);}
        if(Pausado){Painel(new Rect(400,240,480,300));GUI.Label(new Rect(430,265,420,65),"PAUSA",titulo);if(GUI.Button(new Rect(460,355,360,60),"CONTINUAR",botao)){Pausado=false;Time.timeScale=1;}if(GUI.Button(new Rect(460,433,360,52),"VOLTAR AO MENU",botao))VoltarAoMenu();}
        GUI.matrix=anterior;
    }
}
