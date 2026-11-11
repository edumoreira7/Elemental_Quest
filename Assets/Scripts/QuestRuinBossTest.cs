#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestRuinBossTest : MonoBehaviour
{
    QuestRuinBoss boss;QuestBossArena arena;PatoController controle;Rigidbody2D corpo;Collider2D colisor;
    int verificacoes;
    public static void Begin(){UnityEditor.EditorApplication.isPlaying=true;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar(){if(!Environment.GetCommandLineArgs().Contains("-questBossTest")||FindAnyObjectByType<QuestRuinBossTest>()!=null)return;var o=new GameObject("Validacao do Guardiao das Ruinas");DontDestroyOnLoad(o);o.AddComponent<QuestRuinBossTest>();}
    IEnumerator Start()
    {
        Application.logMessageReceived+=Falha;Time.captureFramerate=60;QuestSession.Vidas=3;QuestSession.Venceu=false;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/ArenaFloresta.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.4f);Obter();
        Exigir(FindAnyObjectByType<QuestCamera>()==null,"Camera de acompanhamento ausente na arena");
        Exigir(boss.Vida==4&&boss.animator.runtimeAnimatorController.animationClips.Length==11,"Quatro aberturas e onze clips provisorios prontos no Animator");
        Exigir(FindObjectsByType<QuestEnemy>(FindObjectsInactive.Include).Length==0,"Sem inimigos menores na batalha");
        Exigir(boss.aviso.sprite.texture!=null&&boss.prefabOnda.GetComponent<SpriteRenderer>().sprite.texture!=null,"Aviso e onda tem textura persistida apos salvar e recarregar");
        if(Environment.GetCommandLineArgs().Contains("-questBossVisual")) {
            var esquerdo=boss.animator.transform.Find("BracoE");var direito=boss.animator.transform.Find("BracoD");
            yield return Aguardar(QuestRuinBoss.Estado.Preparacao,QuestRuinBoss.Ataque.Golpe);yield return new WaitForSeconds(.6f);
            Exigir(Mathf.Abs(direito.localPosition.x-1.05f)<.02f&&direito.localPosition.y>2,"Braco do golpe separado do corpo e levantado");yield return Capturar("boss-01-golpe-aviso.png");
            yield return Aguardar(QuestRuinBoss.Estado.Preparacao,QuestRuinBoss.Ataque.Onda);yield return new WaitForSeconds(.6f);
            Exigir(esquerdo.localPosition.y>2&&direito.localPosition.y>2,"Onda anunciada com os dois bracos");yield return Capturar("boss-04-dois-bracos.png");
            yield return Aguardar(QuestRuinBoss.Estado.Preparacao,QuestRuinBoss.Ataque.Investida);yield return new WaitForSeconds(.6f);
            Exigir(boss.animator.transform.Find("Corpo").localPosition.x<-.1f&&boss.sentido.gameObject.activeSelf,"Investida possui inclinacao e seta");yield return Capturar("boss-05-investida.png");
            Debug.Log("EQ_BOSS_VISUAL_OK: poses, bracos separados e avisos inspecionados; logica de combate coberta em boss-validated.log.");UnityEditor.EditorApplication.Exit(0);yield break;
        }
        var camera=Camera.main;
        Exigir(camera.WorldToViewportPoint(new Vector3(-8,-3,0)).x>0 && camera.WorldToViewportPoint(new Vector3(8,3.5f,0)).x<1,"Toda largura da arena cabe no enquadramento");
        Exigir(Mathf.Abs(Screen.width*camera.rect.width/(Screen.height*camera.rect.height)-16f/9)<.02f,"Enquadramento preserva arena inteira em janela 4:3 por margens");
        Vector3 posicaoCamera=camera.transform.position;Posicionar(1);yield return Atacar();Exigir(boss.Vida==4,"Magia real nao causa dano com nucleo protegido");
        Exigir(Vector3.Distance(camera.transform.position,posicaoCamera)<.001f,"Camera permanece fixa ao mover pato");
        yield return Aguardar(QuestRuinBoss.Estado.Preparacao,QuestRuinBoss.Ataque.Golpe);
        Exigir(boss.aviso.enabled&&boss.Prazo-Time.time>.9f,"Golpe possui marca e preparacao longa");
        Exigir(Mathf.Abs(boss.aviso.bounds.size.x-boss.TamanhoImpacto.x)<.05f&&boss.aviso.bounds.size.y>.1f,"Marca visual corresponde a largura do impacto");yield return new WaitForSeconds(.45f);yield return Capturar("boss-01-golpe-aviso.png");
        Posicionar(-4.5f);yield return Aguardar(QuestRuinBoss.Estado.Vulneravel);
        Exigir(QuestSession.Vidas==3,"Afastar-se evita golpe");yield return Capturar("boss-02-nucleo.png");yield return Atacar();yield return Esperar(()=>boss.Vida==3);
        Exigir(boss.Vida==3,"Primeira abertura aceita projetil existente");
        var duplicado=Instantiate(controle.prefabPoder,boss.transform.position+new Vector3(-1,.8f,0),Quaternion.identity).GetComponent<ProjetilPoder>();duplicado.elemental=true;duplicado.elemento=QuestElementPowers.Elemento.Fogo;duplicado.Inicializar(Vector2.right,colisor);yield return new WaitForSeconds(.2f);
        Exigir(boss.Vida==3,"Um acerto por abertura impede eliminar por rajada");
        yield return Aguardar(QuestRuinBoss.Estado.Preparacao,QuestRuinBoss.Ataque.Onda);Posicionar(0);
        Exigir(boss.aviso.enabled,"Onda possui aviso de direcao");yield return Aguardar(QuestRuinBoss.Estado.Execucao,QuestRuinBoss.Ataque.Onda);yield return Capturar("boss-03-onda.png");
        yield return new WaitForSeconds(.45f);corpo.AddForce(Vector2.up*controle.forcaPulo,ForceMode2D.Impulse);
        yield return new WaitForSeconds(1.4f);Exigir(QuestSession.Vidas==3,"Pulo normal evita onda real");
        Posicionar(0);var onda=Instantiate(boss.prefabOnda,new Vector3(1.5f,-2.62f,0),Quaternion.identity).GetComponent<QuestBossWave>();onda.Inicializar(boss,-1);
        yield return new WaitForSeconds(.7f);Exigir(QuestSession.Vidas==2,"Onda causa uma vida pelo sistema existente");
        yield return Aguardar(QuestRuinBoss.Estado.Preparacao,QuestRuinBoss.Ataque.Investida);float direcao=boss.Direcao,inicio=boss.transform.position.x;
        Posicionar(7);yield return new WaitForSeconds(.25f);Exigir(boss.Direcao==direcao&&boss.sentido.gameObject.activeSelf&&boss.sentido.text==(direcao<0?"<":">"),"Direcao da investida fica travada e exibida por seta");
        yield return Aguardar(QuestRuinBoss.Estado.Vulneravel);Exigir(Mathf.Abs(boss.transform.position.x-inicio)<=3.25f,"Investida curta e limitada");
        Exigir(QuestSession.Vidas==2,"Evitar corredor da investida protege jogador");Posicionar(boss.transform.position.x-3);yield return Atacar();yield return Esperar(()=>boss.Vida==2);
        Exigir(boss.Vida==2&&boss.SegundaEtapa,"Segunda abertura inicia ritmo mais rapido");
        yield return Aguardar(QuestRuinBoss.Estado.Preparacao,QuestRuinBoss.Ataque.Golpe);Exigir(boss.Prazo-Time.time>.8f,"Segunda etapa preserva reacao justa");
        Posicionar(boss.CentroImpacto.x);yield return Aguardar(QuestRuinBoss.Estado.Execucao,QuestRuinBoss.Ataque.Golpe);yield return new WaitForSeconds(.1f);
        Exigir(QuestSession.Vidas==1,"Golpe pesado causa dano real uma vez");Exigir(Mathf.Abs(corpo.position.x-boss.CentroImpacto.x)<.2f,"Dano nao teleporta pato");
        yield return Aguardar(QuestRuinBoss.Estado.Vulneravel);Posicionar(boss.transform.position.x-3);yield return Atacar();yield return Esperar(()=>boss.Vida==1);Exigir(boss.Vida==1,"Terceira abertura causa terceiro dano");
        yield return Aguardar(QuestRuinBoss.Estado.Preparacao,QuestRuinBoss.Ataque.Onda);Posicionar(6.5f);yield return Aguardar(QuestRuinBoss.Estado.Preparacao,QuestRuinBoss.Ataque.Investida);Posicionar(-6.5f);
        yield return Aguardar(QuestRuinBoss.Estado.Vulneravel);Posicionar(boss.transform.position.x-3);yield return Atacar();yield return Esperar(()=>boss.Vida==0);
        Exigir(boss.Atual==QuestRuinBoss.Estado.Derrotado,"Quatro aberturas encerram combate");Exigir(!boss.GetComponent<Collider2D>().enabled&&!boss.aviso.enabled,"Derrota desativa colisao e aviso de ataque");
        Posicionar(boss.transform.position.x);yield return new WaitForSeconds(1.5f);Exigir(QuestSession.Vidas==1&&FindObjectsByType<QuestBossWave>().Length==0,"Boss derrotado nao causa dano nem deixa ondas");
        Exigir(arena.saida.activeSelf,"Animacao de derrota libera portal");yield return Capturar("boss-03-derrotado.png");
        Posicionar(6.6f);yield return new WaitForSeconds(.2f);Exigir(arena.Concluiu&&!QuestSession.Venceu,"Portal conclui fase sem encerrar campanha inteira");
        SceneManager.LoadScene("ArenaFloresta");yield return new WaitForSeconds(.5f);Obter();var arenaAntiga=arena;QuestSession.Vidas=1;controle.GetComponent<QuestPlayer>().Ferir();yield return new WaitForSeconds(1);
        Exigir(arenaAntiga==null&&SceneManager.GetActiveScene().name=="FasePrincipal","Morte realmente sai da arena para fase principal");
        var pato=FindAnyObjectByType<PatoController>();Exigir(QuestSession.Vidas==3&&Mathf.Abs(pato.transform.position.x-151)<1,"Retorno ao checkpoint de preparacao com tres vidas");
        pato.enabled=false;var rb=pato.GetComponent<Rigidbody2D>();float prazo=Time.time+5;
        while(SceneManager.GetActiveScene().name=="FasePrincipal"&&Time.time<prazo){typeof(PatoController).GetField("movimento",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(pato,1f);pato.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
        yield return new WaitForSeconds(.5f);Exigir(SceneManager.GetActiveScene().name=="ArenaFloresta"&&FindAnyObjectByType<QuestRuinBoss>().Vida==4,"Portal da fase principal reinicia batalha com boss completo");
        Debug.Log("EQ_BOSS_TEST_OK: "+verificacoes+" verificacoes; teclado, leitura visual e equilibrio com iniciante permanecem manuais.");UnityEditor.EditorApplication.Exit(0);
    }
    void Obter(){arena=FindAnyObjectByType<QuestBossArena>();boss=arena.boss;controle=boss.jogador.GetComponent<PatoController>();controle.enabled=false;corpo=controle.GetComponent<Rigidbody2D>();colisor=controle.GetComponent<Collider2D>();}
    void Posicionar(float x){corpo.position=new Vector2(x,-3+colisor.bounds.extents.y+.02f);controle.transform.position=corpo.position;corpo.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
    IEnumerator Atacar(){var poderes=controle.GetComponent<QuestElementPowers>();poderes.Selecionar(QuestElementPowers.Elemento.Fogo);controle.IniciarAtaque();yield return new WaitForSeconds(1.15f);}
    IEnumerator Aguardar(QuestRuinBoss.Estado estado,QuestRuinBoss.Ataque? ataque=null){float prazo=Time.time+22;while((boss.Atual!=estado||(ataque.HasValue&&boss.Padrao!=ataque.Value))&&Time.time<prazo)yield return null;Exigir(boss.Atual==estado&&(!ataque.HasValue||boss.Padrao==ataque.Value),"Estado alcancado: "+estado+" "+ataque);}
    IEnumerator Esperar(Func<bool> ok){float prazo=Time.time+3;while(!ok()&&Time.time<prazo)yield return null;}
    void Exigir(bool ok,string texto){if(!ok)throw new InvalidOperationException("EQ_BOSS_FAILED: "+texto);verificacoes++;Debug.Log("EQ_BOSS_CHECK_OK: "+texto);}
    void Falha(string mensagem,string pilha,LogType tipo){if((tipo==LogType.Exception||tipo==LogType.Error||tipo==LogType.Assert)&&!pilha.Contains("UnityEditor.Search.")){Application.logMessageReceived-=Falha;File.WriteAllText(Application.dataPath+"/../boss-failure.txt",mensagem+"\n"+pilha);UnityEditor.EditorApplication.delayCall+=()=>UnityEditor.EditorApplication.Exit(1);}}
    IEnumerator Capturar(string nome){yield return null;if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;var c=Camera.main;var rt=new RenderTexture(1280,720,24);var ant=RenderTexture.active;var rect=c.rect;c.rect=new Rect(0,0,1,1);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(1280,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Application.dataPath+"/../"+nome,t.EncodeToPNG());c.targetTexture=null;c.rect=rect;RenderTexture.active=ant;Destroy(rt);Destroy(t);}
}
#endif
