using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-80)]
public class QuestForestStage : MonoBehaviour
{
    [Serializable] public class Encontro { public QuestEnemy inimigo; public float inicio, fim; }
    public Transform jogador;
    public Encontro[] encontros;
    public static bool Retomar { get; private set; }
    public static Vector3 Destino { get; private set; }
    public bool Saindo { get; private set; }
    QuestPlayer vida;
    Camera cameraJogo;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Limpar() { Retomar=false; }
    public static void ResetarProgresso(){Retomar=false;Destino=Vector3.zero;}
    void Start()
    {
        vida=jogador.GetComponent<QuestPlayer>(); cameraJogo=Camera.main;
        foreach(var encontro in encontros) if(encontro.inimigo!=null) {
            encontro.inimigo.combatePermitido=false;
            encontro.inimigo.enabled=true;
            encontro.inimigo.gameObject.SetActive(true);
        }
        if(Retomar)
        {
            jogador.position=Destino; jogador.GetComponent<Rigidbody2D>().position=Destino;
            vida.AtivarCheckpoint(Destino);
            cameraJogo.transform.position=new Vector3(Destino.x+2,cameraJogo.transform.position.y,-10);
        }
    }
    void Update()
    {
        if(Saindo || (QuestSession.Atual!=null && QuestSession.Atual.Pausado)) return;
        foreach(var encontro in encontros)
        {
            var inimigo=encontro.inimigo; if(inimigo==null) continue;
            if(inimigo.vidas<=0) {inimigo.enabled=true; continue;}
            Vector3 tela=cameraJogo.WorldToViewportPoint(inimigo.transform.position+Vector3.up*.6f);
            bool atuar=jogador.position.x>=encontro.inicio && jogador.position.x<encontro.fim && tela.z>0 && tela.x>.06f && tela.x<.94f && tela.y>0 && tela.y<1;
            inimigo.combatePermitido=atuar;
        }
    }
    void LateUpdate()
    {
        if(Saindo || vida==null) return;
        if(vida.pontoSeguro.x>1) {Destino=vida.pontoSeguro; Retomar=true;}
    }
    public void EntrarNaArena()
    {
        if(Saindo) return;
        Saindo=true; Retomar=false; Time.timeScale=1;
        SceneManager.LoadScene("ArenaFloresta");
    }
    public static void VoltarAPreparacao()
    {
        Retomar=true; Destino=new Vector3(151,-2.35f,0); Time.timeScale=1;
        SceneManager.LoadScene("FasePrincipal");
    }
}
