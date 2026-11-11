using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PatoController))]
[DefaultExecutionOrder(-20)]
public class QuestElementPowers : MonoBehaviour
{
    public enum Elemento { Terra, Ar, Fogo, Gelo }
    public Elemento Atual { get; private set; } = Elemento.Fogo;
    public float Mana { get; private set; } = 100f;
    public bool SaltoExtraUsado => saltoExtra;
    public float TempoPlanado => tempoPlanar;
    public float CustoAtual => custos[(int)Atual];
    public GameObject BlocoAtual => bloco;
    public float manaMaxima = 100f;
    public float regeneracao = 12f;
    public float duracaoBloco = 8f;
    public float duracaoPlanar = 1.2f;
    private readonly float[] custos = { 25f, 10f, 15f, 30f };
    private readonly float[] recargas = { 1.5f, .3f, .4f, 2f };
    private readonly float[] proximos = new float[4];
    private Elemento reservado;
    private bool pendente;
    private bool saltoExtra;
    private float tempoPlanar;
    private float gravidadeOriginal;
    private Rigidbody2D corpo;
    private Collider2D colisor;
    private GameObject bloco;
    private GameObject vento;
    private GameObject pose;
    private SpriteRenderer visualJogador;
    private float tamanhoPose;
    private bool posePlanar;

    void Awake()
    {
        corpo = GetComponent<Rigidbody2D>();
        colisor = GetComponent<Collider2D>();
        gravidadeOriginal = corpo.gravityScale;
        visualJogador = GetComponent<SpriteRenderer>();
        tamanhoPose = visualJogador.bounds.size.y;
    }

    void Update()
    {
        if (QuestSession.Atual != null && QuestSession.Atual.Pausado)
            return;
        Mana = Mathf.Min(manaMaxima, Mana + regeneracao * Time.deltaTime);
        if (pendente)
            return;
        for (int indice = 0; indice < 4; indice++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + indice)))
                Selecionar((Elemento)indice);
        }
    }

    public void Selecionar(Elemento elemento)
    {
        if (!pendente)
            Atual = elemento;
    }

    public bool Reservar()
    {
        if (pendente || !Gastar(Atual))
            return false;
        reservado = Atual;
        pendente = true;
        MostrarPose("PatoMago-" + Atual, .6f);
        return true;
    }

    private bool Gastar(Elemento elemento)
    {
        int indice = (int)elemento;
        if (Mana < custos[indice] || Time.time < proximos[indice])
            return false;
        Mana -= custos[indice];
        proximos[indice] = Time.time + recargas[indice];
        return true;
    }

    public bool SaltarExtra(float impulso)
    {
        if (Atual != Elemento.Ar || saltoExtra || !Gastar(Elemento.Ar))
            return false;
        saltoExtra = true;
        MostrarPose("PatoMago-PuloDuploAr", .6f);
        corpo.linearVelocity = new Vector2(corpo.linearVelocity.x, impulso);
        QuestMagicArt.Criar("Ar/03-Impulso-Pulo-Duplo", transform.position, new Vector2(1.5f, 1.5f));
        return true;
    }

    public void AtualizarVoo(bool noChao)
    {
        if (noChao && corpo.linearVelocity.y <= .1f)
        {
            saltoExtra = false;
            tempoPlanar = 0f;
        }
        bool planar = !noChao && Atual == Elemento.Ar && Input.GetKey(KeyCode.J)
            && corpo.linearVelocity.y < 0 && tempoPlanar < duracaoPlanar && Mana > 0;
        corpo.gravityScale = planar ? gravidadeOriginal * .2f : gravidadeOriginal;
        if (planar)
        {
            if (pose == null)
            {
                pose = QuestMagicArt.Criar("Pato/PatoMago-Planar", transform.position, new Vector2(0, tamanhoPose), 6, true);
                posePlanar = pose != null;
            }
            tempoPlanar += Time.deltaTime;
            Mana = Mathf.Max(0, Mana - 20f * Time.deltaTime);
            corpo.linearVelocity = new Vector2(corpo.linearVelocity.x, Mathf.Max(corpo.linearVelocity.y, -1.5f));
            if (vento == null)
                vento = QuestMagicArt.Criar("Ar/04-Correntes-Planar", transform.position, new Vector2(1.5f, 1.5f), 6, true);
            if (vento != null)
                vento.transform.position = transform.position;
        }
        else
        {
            if (vento != null)
                Destroy(vento);
            if (posePlanar)
            {
                Destroy(pose);
                posePlanar = false;
            }
        }
    }

    public void Lancar(Vector3 origem, float direcao, GameObject prefab)
    {
        if (!pendente)
            return;
        pendente = false;
        string pasta = reservado.ToString();
        string emissao = reservado == Elemento.Terra ? "06-Emissao-Cajado" : reservado == Elemento.Ar ? "05-Emissao-Cajado" : reservado == Elemento.Fogo ? "02-Emissao-Cajado" : "03-Emissao-Cajado";
        QuestMagicArt.Criar(pasta + "/" + emissao, origem, Vector2.one);
        if (reservado == Elemento.Terra)
        {
            CriarBloco(direcao);
            return;
        }
        if (reservado == Elemento.Gelo)
        {
            Vector2 centro = new Vector2(colisor.bounds.center.x + direcao * 1.6f, colisor.bounds.min.y - .1f);
            foreach (Collider2D candidato in Physics2D.OverlapBoxAll(centro, new Vector2(3f, 1.5f), 0))
            {
                QuestElementTarget superficie = candidato.GetComponent<QuestElementTarget>();
                if (superficie != null && superficie.tipo != QuestElementTarget.Tipo.Raizes)
                    superficie.Aplicar(Elemento.Gelo);
            }
        }
        GameObject objeto = Instantiate(prefab, origem, Quaternion.identity);
        ProjetilPoder projetil = objeto.GetComponent<ProjetilPoder>();
        projetil.elemento = reservado;
        projetil.elemental = true;
        projetil.Inicializar(new Vector2(direcao, 0), colisor);
        string arquivo = reservado == Elemento.Ar ? "Ar/01-Rajada-Ofensiva" : reservado == Elemento.Fogo ? "Fogo/01-Bola-Fogo" : "Gelo/01-Projetil-Gelo";
        GameObject visual = QuestMagicArt.Criar(arquivo, origem, new Vector2(1.2f, .8f), 6, true);
        if (visual != null)
        {
            objeto.GetComponent<SpriteRenderer>().enabled = false;
            visual.transform.SetParent(objeto.transform, true);
            visual.GetComponent<SpriteRenderer>().flipX = direcao < 0;
        }
    }

    private void CriarBloco(float direcao)
    {
        Bounds limites = colisor.bounds;
        Vector2 tamanho = new Vector2(.9f, 1.5f);
        Vector2 destino = new Vector2(limites.center.x + direcao * (limites.extents.x + .65f), limites.min.y + tamanho.y * .5f + .03f);
        foreach (Collider2D obstaculo in Physics2D.OverlapBoxAll(destino, tamanho, 0))
        {
            if (!obstaculo.isTrigger && obstaculo.gameObject != bloco)
            {
                Mana = Mathf.Min(manaMaxima, Mana + custos[0]);
                if (QuestSession.Atual != null)
                    QuestSession.Atual.Avisar("Sem espaco para o bloco. Afaste-se da parede.");
                return;
            }
        }
        if (bloco != null)
            Destroy(bloco);
        bloco = QuestMagicArt.Criar("Terra/01-Bloco-Intacto", destino, tamanho, 1, true);
        if (bloco == null)
            return;
        bloco.layer = 3;
        BoxCollider2D colisao = bloco.AddComponent<BoxCollider2D>();
        colisao.size = bloco.GetComponent<SpriteRenderer>().sprite.bounds.size;
        colisao.enabled = false;
        bloco.GetComponent<SpriteRenderer>().enabled = false;
        GameObject surgimento = QuestMagicArt.Criar("Terra/04-Surgimento-Bloco", destino, tamanho);
        if (surgimento != null)
            surgimento.transform.SetParent(bloco.transform, true);
        StartCoroutine(MaterializarBloco(bloco, surgimento, tamanho));
    }

    private IEnumerator MaterializarBloco(GameObject novoBloco, GameObject surgimento, Vector2 tamanho)
    {
        if (surgimento != null)
            yield return new WaitForSeconds(.6f);
        if (novoBloco == null)
            yield break;
        if (surgimento != null)
            surgimento.SetActive(false);
        foreach (Collider2D obstaculo in Physics2D.OverlapBoxAll(novoBloco.transform.position, tamanho * .98f, 0))
        {
            if (obstaculo.gameObject != novoBloco && (!obstaculo.isTrigger || obstaculo.GetComponent<QuestEnemy>() != null))
            {
                Mana = Mathf.Min(manaMaxima, Mana + custos[0]);
                Destroy(novoBloco);
                yield break;
            }
        }
        novoBloco.GetComponent<SpriteRenderer>().enabled = true;
        novoBloco.GetComponent<BoxCollider2D>().enabled = true;
        Destroy(novoBloco, duracaoBloco);
    }

    void OnDisable()
    {
        if (corpo != null)
            corpo.gravityScale = gravidadeOriginal;
        if (vento != null)
            Destroy(vento);
        if (pose != null)
            Destroy(pose);
        if (visualJogador != null)
            visualJogador.forceRenderingOff = false;
    }

    private void MostrarPose(string caminho, float duracao)
    {
        posePlanar = false;
        if (pose != null)
            Destroy(pose);
        pose = QuestMagicArt.Criar("Pato/" + caminho, transform.position, new Vector2(0, tamanhoPose), 6, false, 6f / duracao);
    }

    void LateUpdate()
    {
        visualJogador.forceRenderingOff = pose != null;
        if (pose != null)
        {
            pose.transform.position = transform.position;
            pose.GetComponent<SpriteRenderer>().flipX = visualJogador.flipX;
            pose.GetComponent<SpriteRenderer>().color = visualJogador.color;
        }
    }
}
