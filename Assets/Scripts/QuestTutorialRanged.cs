using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class QuestTutorialRanged : MonoBehaviour
{
    public QuestTutorialArea tutorial;
    public Transform jogador;
    public QuestEnemy primeiro;
    public QuestEnemy segundo;
    public SpriteRenderer marco;
    public float deslocamentoX;
    public float fimDaArea = 10000;
    public float saidaLocal = 321;
    public bool PrimeiroDerrotado => primeiro == null || primeiro.vidas <= 0;
    public bool SegundoDerrotado => segundo == null || segundo.vidas <= 0;
    public bool Concluiu { get; private set; }
    public float UltimoDisparo { get; private set; } = -10;
    private readonly HashSet<QuestEnemyShot> tirosVistos = new HashSet<QuestEnemyShot>();
    private Camera cameraJogo;
    private Collider2D colisorJogador;
    private int vidasAnteriores;
    private float danoAte;

    void Start()
    {
        cameraJogo = Camera.main;
        colisorJogador = jogador.GetComponent<Collider2D>();
        vidasAnteriores = QuestSession.Vidas;
        foreach(var inimigo in new[]{primeiro,segundo})if(inimigo!=null){inimigo.combatePermitido=false;inimigo.enabled=true;inimigo.gameObject.SetActive(true);}
    }

    public bool Visivel(QuestEnemy inimigo)
    {
        if (inimigo == null || cameraJogo == null) return false;
        Vector3 ponto = cameraJogo.WorldToViewportPoint(inimigo.transform.position + Vector3.up * .6f);
        return ponto.z > 0 && ponto.x >= .08f && ponto.x <= .92f && ponto.y >= .08f && ponto.y <= .92f;
    }

    private void Controlar(QuestEnemy inimigo, bool permitido)
    {
        if (inimigo == null) return;
        if (inimigo.vidas <= 0)
        {
            inimigo.enabled = true;
            return;
        }
        bool visivel = permitido && Visivel(inimigo);
        inimigo.combatePermitido = visivel;
    }

    void Update()
    {
        if (QuestSession.Atual != null && QuestSession.Atual.Pausado) return;
        Controlar(primeiro, jogador.position.x - deslocamentoX >= 272 && jogador.position.x < fimDaArea);
        Controlar(segundo, primeiro == null && jogador.position.x - deslocamentoX >= 294 && jogador.position.x < fimDaArea);
        tirosVistos.RemoveWhere(tiro => tiro == null);
        foreach (QuestEnemyShot tiro in FindObjectsByType<QuestEnemyShot>())
        {
            if (tiro.transform.position.x - deslocamentoX >= 270 && tiro.transform.position.x - deslocamentoX <= 320 && tirosVistos.Add(tiro))
                UltimoDisparo = Time.time;
        }
        if (QuestSession.Vidas < vidasAnteriores) danoAte = Time.time + 1.8f;
        vidasAnteriores = QuestSession.Vidas;
    }

    void LateUpdate()
    {
        float x = jogador.position.x - deslocamentoX;
        if (x < 270 || jogador.position.x >= fimDaArea || (QuestSession.Atual != null && QuestSession.Atual.Pausado)) return;
        if (PrimeiroDerrotado && SegundoDerrotado && x >= saidaLocal && !Concluiu)
        {
            Concluiu = true;
            marco.color = new Color(.35f, 1, .4f);
            QuestMagicArt.Criar("Tutorial/Recursos/03-Coleta", marco.transform.position, Vector2.one);
        }
        string dica;
        if (Concluiu) dica = "Proximo: desafio final";
        else if (Time.time < danoAte) dica = "Recue enquanto pisca";
        else if (!PrimeiroDerrotado)
        {
            if (x < 275) dica = "Observe o atirador";
            else if (x < 279.6f && colisorJogador.bounds.min.y < -2.1f) dica = "Pule para o abrigo";
            else if (x <= 279.6f) dica = Time.time - UltimoDisparo < 1.8f ? "Desca e contra-ataque" : "Espere o disparo";
            else dica = "J: ataque entre tiros";
        }
        else if (x < 296 && segundo != null) dica = "Pulo e abrigo: avance";
        else if (!SegundoDerrotado) dica = Time.time - UltimoDisparo < 1.8f ? "Avance depois do tiro" : "Espaco: desvie do tiro";
        else dica = "Siga para o marco final";
        tutorial.DefinirDica(dica);
    }
}
