#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class QuestTutorialReviewTest : MonoBehaviour
{
    PatoController controle;
    QuestElementPowers poderes;
    QuestTutorialArea tutorial;
    QuestElementTraining treino;
    Rigidbody2D corpo;
    Collider2D colisor;
    static readonly BindingFlags Privado = BindingFlags.Instance | BindingFlags.NonPublic;
    int checagens;
    public static void Begin() { UnityEditor.EditorApplication.isPlaying = true; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        if (!Environment.GetCommandLineArgs().Contains("-questReviewTest")) return;
        var obj = new GameObject("Revisao integral do tutorial"); DontDestroyOnLoad(obj); obj.AddComponent<QuestTutorialReviewTest>();
    }
    IEnumerator Start()
    {
        Application.logMessageReceived += Falha;
        Time.captureFramerate=60; QuestSession.Vidas=3; QuestSession.Pontos=0; QuestSession.Venceu=false;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return new WaitForSeconds(.5f);
        tutorial=FindAnyObjectByType<QuestTutorialArea>(); treino=FindAnyObjectByType<QuestElementTraining>();
        controle=tutorial.jogador.GetComponent<PatoController>(); controle.enabled=false;
        poderes=controle.GetComponent<QuestElementPowers>(); corpo=controle.GetComponent<Rigidbody2D>(); colisor=controle.GetComponent<Collider2D>();
        Exigir(Application.CanStreamedLevelBeLoaded("Tutorial"),"Tutorial persistida na lista de cenas para recarga");
        Exigir(FindObjectsByType<QuestEnemy>(FindObjectsInactive.Include).Length==3,"Tres encontros: corpo a corpo, atirador e Guardiao");
        Exigir(treino.estacoes.Length==6 && treino.estacoes.All(e=>e.chegada!=null && e.marco!=null && e.alvos.All(a=>a!=null)),"Seis introducoes sem referencias quebradas");
        Exigir(tutorial.faixas.All(f=>tutorial.faixas.Count(o=>o.inicio<f.fim && o.fim>f.inicio)==1),"Uma faixa de orientacao por vez");
        var cards=FindAnyObjectByType<QuestTutorialControls>();
        cards.SendMessage("LateUpdate");
        Exigir(cards.Visivel && tutorial.MensagemAtual=="","Popup inicial substitui instrucao, sem empilhar textos");
        Exigir(cards.cartoes.All(c=>c.tecla.Length<=12 && c.acao.Length<=20),"Popups breves com teclas reais");
        var mapa=FindAnyObjectByType<Tilemap>();
        for(int x=-12;x<232;x++) ExigirSilencioso(mapa.HasTile(new Vector3Int(x,-4,0)),"Buraco visual no chao em " + x);
        Exigir(true,"Tilemap continuo do inicio ao fim");
        Exigir(Mathf.Abs(colisor.bounds.min.y+3)<.1f,"Spawn apoiado no piso");
        yield return CameraEFundo("revisao-01-inicio.png");
        yield return Andar(8); float inicio=Time.time;
        yield return Andar(20,true); Exigir(Time.time-inicio<2.5f,"Corrida usa velocidade existente");
        yield return Andar(21.5f); yield return Pular(26.5f);
        yield return Andar(27); yield return Pular(30); yield return new WaitForSeconds(.2f);
        Exigir(Mathf.Abs(colisor.bounds.min.y+2.4f)<.12f,"Primeira plataforma atingida");
        yield return Andar(31); yield return Pular(34.5f); yield return new WaitForSeconds(.2f);
        Exigir(Mathf.Abs(colisor.bounds.min.y+1.8f)<.12f,"Segunda plataforma atingida");
        yield return Andar(35.5f); yield return Pular(39); yield return new WaitForSeconds(.2f);
        Exigir(Mathf.Abs(colisor.bounds.min.y+1.2f)<.12f,"Terceira plataforma atingida");
        yield return CameraEFundo("revisao-02-expansao.png");
        yield return Andar(51); poderes.Selecionar(QuestElementPowers.Elemento.Terra); yield return null;
        Exigir(treino.estacoes[0].Concluida,"Troca de elemento ensinado antes dos perigos");
        yield return Andar(62.2f); yield return new WaitForSeconds(.3f); yield return Conjurar(QuestElementPowers.Elemento.Terra);
        var bloco=poderes.BlocoAtual; Exigir(bloco!=null && bloco.GetComponent<Collider2D>().enabled,"Terra materializa bloco e colisao");
        yield return Pular(bloco.transform.position.x); yield return new WaitForSeconds(.3f);
        Exigir(treino.estacoes[1].UsouTerra,"Salto fisico utiliza bloco");
        yield return Pular(66.5f); yield return null;
        Exigir(treino.estacoes[1].Concluida,"Terra alcanca marco sem teleporte");
        yield return CameraEFundo("revisao-03-terra.png");
        yield return Andar(75.5f); yield return new WaitForSeconds(.3f);
        yield return Pular(78); yield return new WaitForSeconds(.2f); yield return Pular(80.5f);
        yield return Conjurar(QuestElementPowers.Elemento.Gelo);
        Exigir(!treino.estacoes[2].alvos[0].GetComponent<Collider2D>().isTrigger,"Gelo cria piso real");
        yield return Andar(86); yield return null;
        Exigir(treino.estacoes[2].Concluida,"Gelo atravessado sem reposicionar jogador");
        yield return CameraEFundo("revisao-04-gelo.png");
        yield return Andar(97.5f); yield return new WaitForSeconds(.4f); yield return Pular(101.2f); yield return new WaitForSeconds(.2f);
        poderes.Selecionar(QuestElementPowers.Elemento.Ar); yield return Pular(105.3f,true); yield return null;
        Exigir(treino.estacoes[3].Concluida,"Ar: salto extra real alcanca plataforma alta");
        yield return Andar(106.4f); yield return Pular(112,true); yield return new WaitForSeconds(.4f);
        Exigir(colisor.bounds.min.y < -1.9f && corpo.position.x>=111.9f,"Pouso seguro na secao de planar");
        Debug.Log("EQ_REVIEW_MANUAL: planar segurando J requer teclado; nenhum progresso simulado nesta travessia.");
        yield return CameraEFundo("revisao-05-ar.png");
        yield return Andar(126.5f); yield return new WaitForSeconds(.3f); yield return Conjurar(QuestElementPowers.Elemento.Fogo);
        yield return Andar(132); yield return null;
        Exigir(treino.estacoes[5].Concluida,"Fogo abre raizes durante percurso continuo");
        Exigir(QuestSession.Vidas==3,"Introducao e expansao sem perda de vida");
        yield return Andar(151.5f); yield return new WaitForSeconds(.6f);
        var melee=FindAnyObjectByType<QuestTutorialCombat>(); yield return Conjurar(QuestElementPowers.Elemento.Fogo);
        yield return Esperar(()=>melee.Derrotado);
        Exigir(melee.Derrotado,"Primeiro perigo derrotado pelo poder aprendido");
        yield return CameraEFundo("revisao-06-corpo-a-corpo.png");
        yield return Andar(177.5f); yield return new WaitForSeconds(.5f);
        var ranged=FindAnyObjectByType<QuestTutorialRanged>();
        Exigir(ranged.Visivel(ranged.primeiro),"Atirador enquadrado antes da aproximacao");
        yield return Pular(180); yield return new WaitForSeconds(.4f);
        float prazo=Time.time+5; while(ranged.UltimoDisparo<0 && Time.time<prazo) yield return null;
        Exigir(ranged.UltimoDisparo>0,"Atirador usa disparo existente");
        yield return new WaitForSeconds(1); Exigir(QuestSession.Vidas==3,"Abrigo protege na travessia real");
        yield return CameraEFundo("revisao-07-atirador.png");
        yield return Andar(182.5f); yield return new WaitForSeconds(.5f); yield return Conjurar(QuestElementPowers.Elemento.Fogo);
        yield return Esperar(()=>ranged.PrimeiroDerrotado);
        Exigir(ranged.PrimeiroDerrotado,"Janela de contra-ataque permite derrotar atirador");
        yield return Andar(196); yield return new WaitForSeconds(.3f);
        var final=FindAnyObjectByType<QuestTutorialFinal>();
        Exigir(QuestTutorialFinal.RetomarCheckpoint,"Checkpoint ativado pelo percurso, sem reposicionar");
        final.SendMessage("LateUpdate");
        Exigir(tutorial.MensagemAtual.Contains("salvo"),"Orientacao reconhece checkpoint ativado");
        yield return Andar(200); yield return Pular(204.5f); yield return new WaitForSeconds(.6f);
        yield return CameraEFundo("revisao-08-arena.png");
        Exigir(final.inimigo.gameObject.activeSelf,"Guardiao visivel na entrada da arena");
        yield return Conjurar(QuestElementPowers.Elemento.Fogo);
        yield return Esperar(()=>final.inimigo==null || final.inimigo.vidas<2);
        Exigir(final.inimigo!=null && final.inimigo.vidas==1,"Guardiao resiste ao primeiro acerto");
        yield return Andar(203.5f,true); yield return Conjurar(QuestElementPowers.Elemento.Fogo);
        yield return Esperar(()=>final.Derrotado);
        Exigir(final.Derrotado,"Movimento e contra-ataque vencem inimigo forte");
        yield return new WaitForSeconds(.8f);
        yield return Andar(217); yield return Pular(221); yield return Andar(225);
        yield return new WaitForSeconds(.3f);
        Exigir(final.Concluiu && !controle.enabled,"Portal conclui e encerra controle");
        Exigir(QuestSession.Vidas>0,"Travessia continua terminou sem morte");
        yield return CameraEFundo("revisao-09-portal.png");
        yield return new WaitForSeconds(.5f);
        final.VoltarAoMenu(); yield return new WaitForSeconds(.3f);
        Exigir(SceneManager.GetActiveScene().name=="Inicio" && !QuestTutorialFinal.RetomarCheckpoint,"Conclusao permite voltar ao menu e limpa retomada");
        Debug.Log("EQ_REVIEW_TEST_OK: " + checagens + " checagens; percurso continuo sem teleporte; planar e teclado pendentes.");
        UnityEditor.EditorApplication.Exit(0);
    }
    IEnumerator Andar(float destino,bool correr=false)
    {
        float prazo=Time.time+12; float sentido=destino>=corpo.position.x ? 1 : -1;
        while((destino-corpo.position.x)*sentido>.03f && Time.time<prazo)
        {
            typeof(PatoController).GetField("movimento",Privado).SetValue(controle,sentido);
            typeof(PatoController).GetField("correndo",Privado).SetValue(controle,correr);
            controle.SendMessage("FixedUpdate"); yield return new WaitForFixedUpdate();
        }
        corpo.linearVelocity=new Vector2(0,corpo.linearVelocity.y);
        Exigir((destino-corpo.position.x)*sentido<.1f,"Percurso alcanca x="+destino);
    }
    IEnumerator Pular(float destino,bool extra=false)
    {
        float limite=Time.time+2;
        while(Physics2D.OverlapCircle(controle.groundCheck.position,controle.raioChao,controle.camadaChao)==null && Time.time<limite)
            yield return new WaitForFixedUpdate();
        Exigir(Physics2D.OverlapCircle(controle.groundCheck.position,controle.raioChao,controle.camadaChao)!=null,"Apoio real antes de saltar para "+destino);
        poderes.AtualizarVoo(true); corpo.AddForce(Vector2.up*controle.forcaPulo,ForceMode2D.Impulse);
        yield return new WaitForSeconds(.18f);
        if(extra) { yield return new WaitForSeconds(.1f); Exigir(poderes.SaltarExtra(controle.forcaPulo),"Segundo salto usa sistema original"); }
        yield return Andar(destino); yield return new WaitForSeconds(.3f);
    }
    IEnumerator Conjurar(QuestElementPowers.Elemento elemento)
    {
        float prazo=Time.time+10; while(poderes.Mana<35 && Time.time<prazo) yield return new WaitForSeconds(.2f);
        poderes.Selecionar(elemento); controle.IniciarAtaque(); yield return new WaitForSeconds(1.15f);
    }
    IEnumerator Esperar(Func<bool> condicao)
    {
        float prazo=Time.time+3;
        while(!condicao() && Time.time<prazo) yield return null;
    }
    IEnumerator CameraEFundo(string nome)
    {
        yield return new WaitForSeconds(.5f);
        var camera=Camera.main;
        Exigir(Mathf.Abs(camera.transform.position.x-Mathf.Clamp(corpo.position.x+2,0,223))<.3f,"Camera acompanha em "+corpo.position.x.ToString("F1"));
        var fundos=FindObjectsByType<QuestBackground>();
        Exigir(fundos.Length==3 && fundos.Select(f=>f.fator).Distinct().Count()==3,"Tres camadas de parallax distintas");
        float esquerda=camera.ViewportToWorldPoint(new Vector3(0,.5f,10)).x, direita=camera.ViewportToWorldPoint(new Vector3(1,.5f,10)).x;
        foreach(var fundo in fundos)
        {
            var limites=fundo.paineis.Select(p=>p.GetComponent<SpriteRenderer>().bounds).OrderBy(b=>b.min.x).ToArray();
            float coberto=esquerda;
            foreach(var b in limites) if(b.min.x<=coberto+.02f && b.max.x>coberto) coberto=b.max.x;
            ExigirSilencioso(coberto>=direita,"Fenda no fundo: "+fundo.name+" x="+corpo.position.x);
        }
        Exigir(true,"Background cobre enquadramento sem fendas");
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
        var rt=new RenderTexture(1280,720,24); var anterior=RenderTexture.active; camera.targetTexture=rt; camera.Render(); RenderTexture.active=rt;
        var tex=new Texture2D(1280,720,TextureFormat.RGB24,false); tex.ReadPixels(new Rect(0,0,1280,720),0,0); tex.Apply();
        File.WriteAllBytes(Application.dataPath+"/../"+nome,tex.EncodeToPNG()); camera.targetTexture=null; RenderTexture.active=anterior; Destroy(rt); Destroy(tex);
    }
    void Exigir(bool ok,string mensagem) { ExigirSilencioso(ok,mensagem); checagens++; Debug.Log("EQ_REVIEW_CHECK_OK: "+mensagem); }
    void ExigirSilencioso(bool ok,string mensagem) { if(!ok) throw new InvalidOperationException("EQ_REVIEW_FAILED: "+mensagem+(corpo!=null ? "; pos="+corpo.position : "")); }
    void Falha(string mensagem,string pilha,LogType tipo)
    {
        if((tipo==LogType.Exception || tipo==LogType.Assert || tipo==LogType.Error) && !pilha.Contains("UnityEditor.Search."))
        {
            Application.logMessageReceived-=Falha; File.WriteAllText(Application.dataPath+"/../review-failure.txt",mensagem+"\n"+pilha);
            UnityEditor.EditorApplication.delayCall+=()=>UnityEditor.EditorApplication.Exit(1);
        }
    }
}
#endif
