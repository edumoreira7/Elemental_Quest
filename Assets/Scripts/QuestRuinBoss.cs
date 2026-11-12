using System.Collections;
using UnityEngine;

public class QuestRuinBoss : MonoBehaviour
{
    public enum Estado { Idle, Preparacao, Execucao, Recuperacao, Vulneravel, RecebendoDano, Derrotado }
    public enum Ataque { Golpe, Onda, Investida, OndaAlta }
    public Estado Atual { get; private set; }
    public Ataque Padrao { get; private set; }
    public int Vida { get; private set; } = 6;
    public int vidaMaxima = 6;
    public bool SegundaEtapa => segundaEtapaAtivada;
    bool segundaEtapaAtivada;
    public float escalaVisual = 1.3f;
    public SpriteRenderer visualPrincipal;
    public GameObject efeitoNucleo;
    public Transform pontoNucleo;
    public int TerrasAbsorvidas { get; private set; }
    public float CuraAte { get; private set; }
    public Vector3 CentroNucleo => pontoNucleo != null ? pontoNucleo.position : transform.position + Vector3.up * 1.25f;
    QuestElementPowers poderes;
    public Transform jogador;
    public Animator animator;
    public SpriteRenderer nucleo, halo, aviso;
    public TextMesh sentido;
    public QuestBossArena arena;
    public GameObject prefabOnda;
    public Vector2 CentroImpacto { get; private set; }
    public Vector2 TamanhoImpacto { get; private set; }
    public float Direcao { get; private set; }
    public float Prazo { get; private set; }
    float inicio, inicioInvestida, fimInvestida, duracaoInvestidaAtual;
    int proximo;
    int ataquesNaSequencia;
    int tamanhoSequencia;
    static readonly Ataque[] sequencia = { Ataque.Onda, Ataque.OndaAlta, Ataque.Investida, Ataque.Golpe, Ataque.OndaAlta, Ataque.Onda };
    public int AtaquesNaSequencia => ataquesNaSequencia;
    public int TamanhoSequencia => tamanhoSequencia;
    public bool AtaqueAlto => Padrao==Ataque.OndaAlta;
    bool abriu;
    public QuestBossWave OndaAtual { get; private set; }

    void Start() { Vida=vidaMaxima; poderes=jogador.GetComponent<QuestElementPowers>(); animator.transform.localScale=new Vector3(-escalaVisual,escalaVisual,1); NovaSequencia(); Trocar(Estado.Idle,1.6f); }
    void NovaSequencia(){proximo=0;ataquesNaSequencia=0;tamanhoSequencia=SegundaEtapa?6:4;}
    void Trocar(Estado estado,float duracao)
    {
        Atual=estado; inicio=Time.time; Prazo=inicio+duracao;
        int animacaoAtaque=Padrao==Ataque.OndaAlta?1:(int)Padrao;
        string animacao=estado==Estado.Preparacao || estado==Estado.Execucao ? estado+animacaoAtaque.ToString() : estado.ToString();
        animator.speed=1;
        foreach(var clip in animator.runtimeAnimatorController.animationClips)
            if(clip.name==animacao) {animator.speed=estado==Estado.Idle||estado==Estado.Vulneravel?1:duracao>0?clip.length/duracao:1;break;}
        animator.Play(animacao,0,0);
        aviso.enabled=estado==Estado.Preparacao;
        sentido.gameObject.SetActive(estado==Estado.Preparacao);
    }
    void Preparar()
    {
        Padrao=sequencia[proximo++]; Direcao=jogador.position.x<transform.position.x?-1:1;
        animator.transform.localScale=new Vector3(Direcao*escalaVisual,escalaVisual,1);
        sentido.text=Padrao==Ataque.Onda?"PULE":AtaqueAlto?"NO CHAO":Direcao<0?"<":">";sentido.transform.position=transform.position+new Vector3(Direcao*1.7f,2.2f,0);
        inicioInvestida=transform.position.x; fimInvestida=Mathf.Clamp(inicioInvestida+Direcao*3.2f,-5.5f,5.5f);
        bool onda=Padrao==Ataque.Onda || AtaqueAlto;
        TamanhoImpacto=new Vector2(Padrao==Ataque.Golpe?2.8f:onda?Mathf.Abs(Direcao*7.8f-transform.position.x):Mathf.Max(.5f,Mathf.Abs(fimInvestida-inicioInvestida)+1.9f),.4f);
        float x=Padrao==Ataque.Golpe?transform.position.x+Direcao*2.05f:onda?(transform.position.x+Direcao*7.8f)/2:(inicioInvestida+fimInvestida)/2;
        CentroImpacto=new Vector2(x,AtaqueAlto?-1.05f:-2.65f);
        aviso.transform.position=new Vector3(x,AtaqueAlto?-1.05f:-2.89f,0); aviso.transform.localScale=new Vector3(TamanhoImpacto.x,.18f,1);
        Trocar(Estado.Preparacao,SegundaEtapa?.6f:.75f);
    }
    void Executar()
    {
        duracaoInvestidaAtual=DuracaoInvestida;
        bool onda=Padrao==Ataque.Onda || AtaqueAlto;
        float velocidadeOnda=SegundaEtapa?7.2f:6f;
        Trocar(Estado.Execucao,Padrao==Ataque.Golpe?.32f:onda?16f/velocidadeOnda+.1f:duracaoInvestidaAtual);
        if(onda) {
            var obj=Instantiate(prefabOnda,new Vector3(transform.position.x+Direcao*1.2f,AtaqueAlto?-1.05f:-2.62f,0),Quaternion.identity);
            OndaAtual=obj.GetComponent<QuestBossWave>(); OndaAtual.velocidade=velocidadeOnda;OndaAtual.alcance=16;OndaAtual.Inicializar(this,Direcao);
            var desenho=obj.GetComponentInChildren<SpriteRenderer>();if(desenho!=null)desenho.color=AtaqueAlto?new Color(.3f,.9f,1):new Color(1,.65f,.15f);
        }
    }
    void Update()
    {
        if(arena.Saindo || (QuestSession.Atual!=null && QuestSession.Atual.Pausado))return;
        bool aberto=Atual==Estado.Vulneravel;
        if(nucleo!=null)nucleo.color=aberto?new Color(.25f,1,.2f):Atual==Estado.RecebendoDano?Color.white:new Color(.22f,.38f,.23f);
        if(halo!=null)halo.color=new Color(.2f,1,.25f,aberto?.3f+.15f*Mathf.Sin(Time.time*10):.06f);
        if(efeitoNucleo!=null)efeitoNucleo.SetActive(aberto || Time.time<CuraAte);
        if(visualPrincipal!=null)visualPrincipal.color=Atual==Estado.RecebendoDano?new Color(.6f,1,.65f):Color.white;
        if(Atual==Estado.Preparacao) aviso.color=AtaqueAlto?new Color(.3f,.9f,1,.65f):new Color(1,.6f,.12f,.55f+.25f*Mathf.Sin(Time.time*12));
        if(Atual==Estado.Derrotado) return;
        if(poderes!=null && poderes.BlocoAtual!=null)AbsorverTerra(poderes.BlocoAtual);
        if(Atual==Estado.Execucao) {
            if(Padrao==Ataque.Golpe && Time.time>=inicio+.08f && Time.time<inicio+.24f) Atingir(CentroImpacto,new Vector2(2.8f,.65f));
            if(Padrao==Ataque.Investida) {
                float x=Mathf.Lerp(inicioInvestida,fimInvestida,Mathf.Clamp01((Time.time-inicio)/duracaoInvestidaAtual));
                transform.position=new Vector3(x,-3,0);
                Atingir(new Vector2(x,-2.65f),new Vector2(1.9f,.7f));
            }
        }
        // A onda termina antes do proximo aviso, sem sobrepor ataques de alturas opostas.
        if(Atual==Estado.Execucao && (Padrao==Ataque.Onda || AtaqueAlto) && OndaAtual==null) Prazo=Mathf.Min(Prazo,Time.time);
        if(Time.time<Prazo)return;
        switch(Atual) {
            case Estado.Idle: Preparar();break;
            case Estado.Preparacao: Executar();break;
            case Estado.Execucao:
                if(OndaAtual!=null)Destroy(OndaAtual.gameObject);
                ataquesNaSequencia++;abriu=ataquesNaSequencia>=tamanhoSequencia; Trocar(Estado.Recuperacao,SegundaEtapa?.12f:.18f);break;
            case Estado.Recuperacao:
                if(abriu){abriu=false;Trocar(Estado.Vulneravel,SegundaEtapa?1.8f:2.2f);}else Preparar();break;
            case Estado.Vulneravel: abriu=false;NovaSequencia();Preparar();break;
            case Estado.RecebendoDano: abriu=false;NovaSequencia();Preparar();break;
        }
    }
    void Atingir(Vector2 centro,Vector2 tamanho)
    {
        foreach(var outro in Physics2D.OverlapBoxAll(centro,tamanho,0)) {
            var vida=outro.GetComponent<QuestPlayer>();if(vida!=null)vida.Ferir();
        }
    }
    void OnTriggerEnter2D(Collider2D outro)
    {
        var projetil=outro.GetComponent<ProjetilPoder>(); if(projetil==null || Atual==Estado.Derrotado)return;
        Destroy(projetil.gameObject);
        if(Atual!=Estado.Vulneravel)return;
        Vida=Mathf.Max(0,Vida-1);
        if(Vida<=vidaMaxima/2)segundaEtapaAtivada=true;
        QuestMagicArt.Criar("Tutorial/Recursos/03-Coleta",CentroNucleo,Vector2.one);
        if(Vida==0) {
            if(OndaAtual!=null)Destroy(OndaAtual.gameObject);
            aviso.enabled=false;GetComponent<Collider2D>().enabled=false;
            Trocar(Estado.Derrotado,1.2f);arena.BossDerrotado();
        } else Trocar(Estado.RecebendoDano,.25f);
    }
    float DuracaoInvestida => SegundaEtapa?.45f:.52f;
    GameObject terraEmAbsorcao;
    void AbsorverTerra(GameObject bloco)
    {
        if(bloco==terraEmAbsorcao)return;
        terraEmAbsorcao=bloco;
        var colisor=bloco.GetComponent<Collider2D>();if(colisor!=null)colisor.enabled=false;
        var origem=bloco.GetComponent<SpriteRenderer>();
        var fragmento=new GameObject("Terra absorvida pelo Guardiao");fragmento.transform.SetParent(transform,true);fragmento.transform.position=bloco.transform.position;fragmento.transform.localScale=bloco.transform.lossyScale;
        var desenho=fragmento.AddComponent<SpriteRenderer>();desenho.sprite=origem.sprite;desenho.sharedMaterial=origem.sharedMaterial;desenho.sortingOrder=10;desenho.color=new Color(.8f,1,.35f,.9f);
        Destroy(bloco);StartCoroutine(ConcluirAbsorcao(fragmento));
    }
    IEnumerator ConcluirAbsorcao(GameObject fragmento)
    {
        Vector3 origem=fragmento.transform.position,escala=fragmento.transform.localScale;float inicioEfeito=Time.time;
        while(Time.time<inicioEfeito+.35f) {
            if(Atual==Estado.Derrotado || arena.Saindo){Destroy(fragmento);yield break;}
            float t=(Time.time-inicioEfeito)/.35f;fragmento.transform.position=Vector3.Lerp(origem,CentroNucleo,t);fragmento.transform.localScale=escala*(1-t*.85f);yield return null;
        }
        Destroy(fragmento);
        if(Atual==Estado.Derrotado || arena.Saindo)yield break;
        Vida=Mathf.Min(vidaMaxima,Vida+1);TerrasAbsorvidas++;CuraAte=Time.time+1.8f;
        QuestMagicArt.Criar("Tutorial/Recursos/03-Coleta",CentroNucleo,Vector2.one*1.5f);
    }
    public void Interromper()
    {if(OndaAtual!=null)Destroy(OndaAtual.gameObject);aviso.enabled=false;sentido.gameObject.SetActive(false);}
}
