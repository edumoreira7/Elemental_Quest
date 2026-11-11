using System;
using UnityEngine;

public class QuestElementTraining : MonoBehaviour
{
    public enum Objetivo { Selecao, Terra, Gelo, ArDuplo, ArPlanar, Fogo, Combinacao }

    [Serializable]
    public class Estacao
    {
        public string nome;
        public Objetivo tipo;
        public float inicio;
        public float fim;
        public string dica;
        public float pontoTerra;
        public BoxCollider2D chegada;
        public SpriteRenderer marco;
        public QuestElementTarget[] alvos = new QuestElementTarget[0];
        [NonSerialized] public bool Concluida;
        [NonSerialized] public bool UsouTerra;
        [NonSerialized] public bool UsouGelo;
        [NonSerialized] public bool UsouAr;
        [NonSerialized] public bool UsouPlanar;
        [NonSerialized] public bool UsouFogo;
    }

    public QuestTutorialArea tutorial;
    public Transform jogador;
    public Estacao[] estacoes;
    public bool Concluiu { get; private set; }
    public int Concluidas { get; private set; }
    private QuestElementPowers poderes;
    private Collider2D colisorJogador;
    private Rigidbody2D corpo;
    private QuestElementPowers.Elemento elementoAnterior;
    private float feedbackAte;
    private string feedback;
    private bool trocouElemento;

    void Start()
    {
        poderes = jogador.GetComponent<QuestElementPowers>();
        colisorJogador = jogador.GetComponent<Collider2D>();
        corpo = jogador.GetComponent<Rigidbody2D>();
        elementoAnterior = poderes.Atual;
    }

    void FixedUpdate()
    {
        if (colisorJogador == null)
            return;
        foreach (Estacao estacao in estacoes)
        {
            foreach (QuestElementTarget alvo in estacao.alvos)
            {
                if (alvo == null || alvo.tipo == QuestElementTarget.Tipo.Raizes)
                    continue;
                Collider2D superficie = alvo.GetComponent<Collider2D>();
                bool ignorar = superficie.isTrigger;
                if (Physics2D.GetIgnoreCollision(superficie, colisorJogador) != ignorar)
                    Physics2D.IgnoreCollision(superficie, colisorJogador, ignorar);
            }
        }
    }

    void LateUpdate()
    {
        if (poderes == null || (QuestSession.Atual != null && QuestSession.Atual.Pausado))
            return;
        float posicaoX = jogador.position.x;
        trocouElemento |= poderes.Atual != elementoAnterior;
        if (posicaoX >= estacoes[0].inicio && trocouElemento && !estacoes[0].Concluida)
            Completar(estacoes[0]);
        Estacao atual = null;
        foreach (Estacao estacao in estacoes)
        {
            if (posicaoX < estacao.inicio || posicaoX >= estacao.fim)
                continue;
            atual = estacao;
            Avaliar(estacao);
            break;
        }
        elementoAnterior = poderes.Atual;
        Concluidas = 0;
        foreach (Estacao estacao in estacoes)
        {
            if (estacao.Concluida)
                Concluidas++;
        }
        Concluiu = estacoes[estacoes.Length - 1].Concluida;
        if (atual != null)
            tutorial.DefinirDica(Time.time < feedbackAte ? feedback : Dica(atual));
    }

    private void Avaliar(Estacao estacao)
    {
        if (estacao.Concluida)
            return;
        if (estacao.tipo == Objetivo.Selecao)
        {
            if (poderes.Atual != elementoAnterior)
                Completar(estacao);
            return;
        }
        RaycastHit2D apoio = Physics2D.Raycast(new Vector2(colisorJogador.bounds.center.x, colisorJogador.bounds.min.y + .05f), Vector2.down, .2f, 1 << 3);
        estacao.UsouTerra |= apoio.collider != null && apoio.collider.gameObject.name == "Terra/01-Bloco-Intacto" && Mathf.Abs(corpo.linearVelocity.y) < .2f;
        estacao.UsouAr |= poderes.Atual == QuestElementPowers.Elemento.Ar && poderes.SaltoExtraUsado;
        estacao.UsouPlanar |= poderes.Atual == QuestElementPowers.Elemento.Ar && poderes.TempoPlanado >= .2f;
        foreach (QuestElementTarget alvo in estacao.alvos)
        {
            if (alvo == null)
            {
                estacao.UsouFogo = true;
                continue;
            }
            Collider2D colisao = alvo.GetComponent<Collider2D>();
            if (alvo.tipo == QuestElementTarget.Tipo.Raizes)
                estacao.UsouFogo |= !colisao.enabled;
            else
                estacao.UsouGelo |= !colisao.isTrigger;
        }
        bool usoCorreto = estacao.tipo == Objetivo.Terra ? estacao.UsouTerra
            : estacao.tipo == Objetivo.Gelo ? estacao.UsouGelo
            : estacao.tipo == Objetivo.ArDuplo ? estacao.UsouAr
            : estacao.tipo == Objetivo.ArPlanar ? estacao.UsouPlanar
            : estacao.tipo == Objetivo.Fogo ? estacao.UsouFogo
            : estacao.UsouTerra && estacao.UsouGelo && estacao.UsouAr && estacao.UsouFogo;
        if (usoCorreto && estacao.chegada != null && estacao.chegada.bounds.Intersects(colisorJogador.bounds))
            Completar(estacao);
    }

    private void Completar(Estacao estacao)
    {
        estacao.Concluida = true;
        estacao.marco.color = new Color(.35f, 1f, .4f);
        GameObject texto = new GameObject("Marco concluido");
        texto.transform.SetParent(estacao.marco.transform.parent, true);
        texto.transform.position = estacao.marco.transform.position + Vector3.up * .5f;
        TextMesh legenda = texto.AddComponent<TextMesh>();
        legenda.text = "OK";
        legenda.fontSize = 40;
        legenda.characterSize = .09f;
        legenda.anchor = TextAnchor.MiddleCenter;
        legenda.color = new Color(.35f, 1f, .4f);
        texto.GetComponent<MeshRenderer>().sortingOrder = 25;
        QuestMagicArt.Criar("Tutorial/Recursos/03-Coleta", estacao.marco.transform.position, Vector2.one);
        feedback = estacao.nome + ": concluido";
        feedbackAte = Time.time + 1.5f;
    }

    private string Dica(Estacao estacao)
    {
        if (estacao.Concluida)
            return estacao.tipo == Objetivo.Selecao ? "Mana regenera sozinha" : estacao.tipo == Objetivo.Combinacao ? "Treino concluido" : "Marco verde: siga adiante";
        if (estacao.tipo == Objetivo.Combinacao)
        {
            if (!estacao.UsouTerra) return DicaTerra(estacao, "Terra: suba pelo bloco");
            if (!estacao.UsouGelo) return "Gelo: atravesse a agua";
            if (!estacao.UsouAr) return "Ar: alcance o alto";
            if (!estacao.UsouFogo) return "Fogo: abra a saida";
            return "Alcance o marco final";
        }
        if (estacao.tipo == Objetivo.Terra) return DicaTerra(estacao, estacao.dica);
        if (estacao.tipo == Objetivo.Gelo && estacao.UsouGelo) return "Atravesse pelo gelo";
        if (estacao.tipo == Objetivo.Fogo && estacao.UsouFogo) return "Passe pelas raizes";
        if (poderes.Mana < poderes.CustoAtual) return "Mana baixa: aguarde";
        return estacao.dica;
    }

    private string DicaTerra(Estacao estacao, string inicial)
    {
        GameObject bloco = poderes.BlocoAtual;
        if (bloco != null && bloco.transform.position.x >= estacao.inicio && bloco.transform.position.x < estacao.fim)
        {
            if (estacao.pontoTerra > 0 && Mathf.Abs(bloco.transform.position.x - estacao.pontoTerra - 1.2f) > .8f)
                return "Refaca junto ao simbolo";
            if (!bloco.GetComponent<Collider2D>().enabled) return "Aguarde o bloco surgir";
            return estacao.UsouTerra ? "Salte ate o marco" : "Pule, depois avance";
        }
        if (estacao.pontoTerra > 0 && Mathf.Abs(jogador.position.x - estacao.pontoTerra) > .8f)
            return "Conjure no simbolo Terra";
        return poderes.Mana < poderes.CustoAtual ? "Mana baixa: aguarde" : inicial;
    }
}
