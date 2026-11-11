using UnityEngine;

public class QuestTutorialCombat : MonoBehaviour
{
    public QuestTutorialArea tutorial;
    public Transform jogador;
    public QuestEnemy inimigo;
    public SpriteRenderer marco;
    public float inicio = 223;
    public float fimObservacao = 232;
    public float saida = 252;
    public float fimDaArea = 270;
    public bool Derrotado => inimigo == null || inimigo.vidas <= 0;
    public bool Concluiu { get; private set; }
    private QuestElementPowers poderes;
    private int vidasAnteriores;
    private float avisoDanoAte;

    void Start()
    {
        poderes = jogador.GetComponent<QuestElementPowers>();
        vidasAnteriores = QuestSession.Vidas;
    }

    void LateUpdate()
    {
        if (QuestSession.Atual != null && QuestSession.Atual.Pausado)
            return;
        if (QuestSession.Vidas < vidasAnteriores)
            avisoDanoAte = Time.time + 1.8f;
        vidasAnteriores = QuestSession.Vidas;
        if (jogador.position.x < inicio || jogador.position.x >= fimDaArea)
            return;
        if (!Concluiu && Derrotado && jogador.position.x >= saida)
        {
            Concluiu = true;
            marco.color = new Color(.35f, 1f, .4f);
            QuestMagicArt.Criar("Tutorial/Recursos/03-Coleta", marco.transform.position, Vector2.one);
        }
        string dica = Concluiu ? "Encontro concluido"
            : Derrotado ? "Passagem livre: avance"
            : Time.time < avisoDanoAte ? "Recue enquanto pisca"
            : jogador.position.x < fimObservacao ? "Observe a patrulha"
            : jogador.position.x >= saida ? "Derrote o guaxinim"
            : Vector2.Distance(jogador.position, inimigo.transform.position) < 2.5f ? "Afaste-se antes do golpe"
            : poderes.Atual == QuestElementPowers.Elemento.Terra ? "3 + J: ataque de longe"
            : "J: ataque; A/D: recue";
        tutorial.DefinirDica(dica);
    }
}
