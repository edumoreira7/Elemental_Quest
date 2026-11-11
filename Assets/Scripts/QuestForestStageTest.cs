#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestForestStageTest : MonoBehaviour
{
    QuestForestStage fase; PatoController controle; QuestElementPowers poderes; Rigidbody2D corpo; Collider2D colisor;
    static BindingFlags privado=BindingFlags.Instance|BindingFlags.NonPublic;
    int checagens;
    public static void Begin(){UnityEditor.EditorApplication.isPlaying=true;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar(){if(!Environment.GetCommandLineArgs().Contains("-questForestTest") || FindAnyObjectByType<QuestForestStageTest>()!=null)return;var o=new GameObject("Teste da floresta");DontDestroyOnLoad(o);o.AddComponent<QuestForestStageTest>();}
    IEnumerator Start()
    {
        Application.logMessageReceived+=Falha;Time.captureFramerate=60;QuestSession.Vidas=3;QuestSession.Venceu=false;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/FasePrincipal.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.5f);Obter();
        if(Environment.GetCommandLineArgs().Contains("-questForestVisualOnly")) {
            foreach(var ponto in new[]{new Vector2(32.5f,-.8f),new Vector2(126,-2),new Vector2(151,-3)}) {
                corpo.position=new Vector2(ponto.x,ponto.y+colisor.bounds.extents.y+.02f);controle.transform.position=corpo.position;corpo.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();yield return new WaitForSeconds(1);
                yield return Capturar("floresta-acabamento-"+ponto.x.ToString("F0")+".png");
            }
            Debug.Log("EQ_FOREST_VISUAL_OK: acabamento das emendas; capturas com posicionamento apenas para inspecao visual.");UnityEditor.EditorApplication.Exit(0);yield break;
        }
        Exigir(Application.CanStreamedLevelBeLoaded("FasePrincipal")&&Application.CanStreamedLevelBeLoaded("ArenaFloresta"),"Duas cenas registradas para recarga e transicao");
        Exigir(fase.encontros.Length==5&&fase.encontros.All(e=>e.inimigo!=null&&e.inimigo.prefabDisparo!=null),"Cinco encontros com Prefabs existentes completos");
        Exigir(FindObjectsByType<QuestCheckpoint>().Length==3,"Tres checkpoints em trechos seguros");
        Exigir(FindObjectsByType<QuestCrystal>().Length>=25,"Trilha e rotas opcionais com cristais");
        Exigir(FindObjectsByType<UnityEngine.Tilemaps.Tilemap>().Length>=15,"Terreno variado em Tilemaps");
        yield return Capturar("floresta-01-inicio.png");
        yield return Andar(10,true);yield return Pular(13);yield return Pular(16);yield return Andar(23);yield return new WaitForSeconds(.3f);
        yield return Andar(25);yield return Pular(28.4f);yield return Pular(32.5f);yield return Capturar("floresta-02-exploracao.png");
        yield return Andar(34.7f);yield return new WaitForSeconds(.5f);yield return Magia(QuestElementPowers.Elemento.Fogo);
        yield return Esperar(()=>Morto(0));Exigir(Morto(0),"Corpo a corpo derrotado na rota de exploracao");
        yield return Andar(46);yield return new WaitForSeconds(.2f);Exigir(QuestForestStage.Retomar&&Mathf.Abs(QuestForestStage.Destino.x-46)<.1f,"Primeiro checkpoint ativado por contato");
        yield return Andar(48);yield return Pular(50);yield return new WaitForSeconds(3);
        Exigir(FindObjectsByType<QuestEnemyShot>().Length>0 || (float)typeof(QuestEnemy).GetField("ataqueInicio",privado).GetValue(fase.encontros[1].inimigo)>0,"Atirador utiliza ataque existente diante do abrigo");
        yield return Capturar("floresta-03-abrigo.png");yield return Andar(52);yield return new WaitForSeconds(.4f);yield return Magia(QuestElementPowers.Elemento.Fogo);yield return Esperar(()=>Morto(1));Exigir(Morto(1),"Contra-ataque real derrota atirador");
        yield return Andar(65.2f);yield return Magia(QuestElementPowers.Elemento.Terra);Exigir(poderes.BlocoAtual!=null,"Terra cria degrau na floresta");
        yield return Pular(poderes.BlocoAtual.transform.position.x);yield return Pular(70);Exigir(colisor.bounds.min.y>-.9f,"Terra permite rota alta de coleta");
        yield return Capturar("floresta-04-terra.png");yield return Andar(75);yield return new WaitForSeconds(.3f);yield return Magia(QuestElementPowers.Elemento.Gelo);
        var agua=FindObjectsByType<QuestElementTarget>().First(a=>a.tipo==QuestElementTarget.Tipo.Agua&&a.transform.position.x<80);
        Exigir(!agua.GetComponent<Collider2D>().isTrigger,"Gelo cria ponte de agua");yield return Andar(80);yield return Capturar("floresta-05-gelo.png");
        yield return Andar(83);yield return Pular(85.5f);yield return Andar(86.2f);poderes.Selecionar(QuestElementPowers.Elemento.Ar);yield return Pular(90,true);
        Exigir(colisor.bounds.min.y>-.1f,"Salto extra real permite explorar a copa");yield return Andar(91.4f);yield return Pular(95.8f,true);
        yield return Andar(98.5f);yield return new WaitForSeconds(.4f);yield return Magia(QuestElementPowers.Elemento.Fogo);
        Exigir(!FindObjectsByType<QuestElementTarget>().Any(a=>a.tipo==QuestElementTarget.Tipo.Raizes&&a.transform.position.x<105&&a.GetComponent<Collider2D>().enabled),"Fogo abre primeira passagem");
        yield return Andar(105);yield return new WaitForSeconds(.2f);Exigir(Mathf.Abs(QuestForestStage.Destino.x-105)<.1f,"Checkpoint intermediario ativado");
        yield return Pular(107);yield return Pular(108.5f);yield return Magia(QuestElementPowers.Elemento.Fogo);yield return Esperar(()=>Morto(2));Exigir(Morto(2),"Combate em terreno alto reutiliza corpo a corpo");
        yield return Andar(115);yield return Magia(QuestElementPowers.Elemento.Fogo);yield return Andar(120);yield return Magia(QuestElementPowers.Elemento.Gelo);yield return Andar(124);
        yield return Pular(126);yield return Capturar("floresta-06-combinacao.png");yield return Andar(128);yield return new WaitForSeconds(.4f);yield return Magia(QuestElementPowers.Elemento.Fogo);yield return Esperar(()=>Morto(3));Exigir(Morto(3),"Fogo, Gelo, pulo e ataque combinados");
        yield return Andar(134.5f);yield return new WaitForSeconds(.5f);yield return Andar(137.5f);yield return Magia(QuestElementPowers.Elemento.Fogo);yield return Esperar(()=>fase.encontros[4].inimigo==null||fase.encontros[4].inimigo.vidas<2);
        yield return Andar(135.5f,true);yield return Magia(QuestElementPowers.Elemento.Fogo);yield return Esperar(()=>Morto(4));Exigir(Morto(4),"Guardiao existente derrotado antes das ruinas");
        yield return Andar(151);yield return new WaitForSeconds(.3f);Exigir(Mathf.Abs(QuestForestStage.Destino.x-151)<.1f,"Checkpoint antes da arena salvo");yield return Capturar("floresta-07-preparacao.png");
        yield return new WaitForSeconds(2);var antiga=fase;QuestSession.Vidas=1;controle.GetComponent<QuestPlayer>().Ferir();yield return new WaitForSeconds(2);Obter();
        Exigir(antiga==null&&QuestSession.Vidas==3&&Mathf.Abs(corpo.position.x-151)<1,"Morte realmente recarrega junto ao checkpoint");
        yield return Andar(160.5f);float limitePortal=Time.time+4;
        while(SceneManager.GetActiveScene().name=="FasePrincipal"&&Time.time<limitePortal){typeof(PatoController).GetField("movimento",privado).SetValue(controle,1f);controle.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
        yield return new WaitForSeconds(.6f);Exigir(SceneManager.GetActiveScene().name=="ArenaFloresta","Portal leva a cena propria de arena");
        Exigir(FindObjectsByType<QuestEnemy>(FindObjectsInactive.Include).Length==0,"Arena sem inimigos menores adicionais");
        Exigir(FindAnyObjectByType<QuestBossArena>()!=null,"Chegada na arena do Guardiao das Ruinas");
        yield return Capturar("floresta-08-arena-reservada.png");QuestForestStage.VoltarAPreparacao();yield return new WaitForSeconds(.6f);Obter();
        Exigir(Mathf.Abs(corpo.position.x-151)<1,"Retorno da arena preserva preparacao");
        Debug.Log("EQ_FOREST_TEST_OK: "+checagens+" checagens; controle por roteiro, entrada real de teclado e planar permanecem manuais.");UnityEditor.EditorApplication.Exit(0);
    }
    void Obter(){fase=FindAnyObjectByType<QuestForestStage>();controle=fase.jogador.GetComponent<PatoController>();controle.enabled=false;poderes=controle.GetComponent<QuestElementPowers>();corpo=controle.GetComponent<Rigidbody2D>();colisor=controle.GetComponent<Collider2D>();}
    bool Morto(int i){return fase.encontros[i].inimigo==null||fase.encontros[i].inimigo.vidas<=0;}
    IEnumerator Andar(float x,bool correr=false){float sentido=x>=corpo.position.x?1:-1,prazo=Time.time+12;while((x-corpo.position.x)*sentido>.03f&&Time.time<prazo){typeof(PatoController).GetField("movimento",privado).SetValue(controle,sentido);typeof(PatoController).GetField("correndo",privado).SetValue(controle,correr);controle.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}corpo.linearVelocity=new Vector2(0,corpo.linearVelocity.y);Exigir((x-corpo.position.x)*sentido<.1f,"Rota alcanca "+x);}
    IEnumerator Pular(float x,bool extra=false)
    {
        float prazo=Time.time+2;
        while((Physics2D.OverlapCircle(controle.groundCheck.position,controle.raioChao,controle.camadaChao)==null||Mathf.Abs(corpo.linearVelocity.y)>.1f)&&Time.time<prazo)yield return new WaitForFixedUpdate();
        Exigir(Physics2D.OverlapCircle(controle.groundCheck.position,controle.raioChao,controle.camadaChao)!=null&&Mathf.Abs(corpo.linearVelocity.y)<.1f,"Apoio estavel antes do salto");
        poderes.AtualizarVoo(true);corpo.AddForce(Vector2.up*controle.forcaPulo,ForceMode2D.Impulse);yield return new WaitForSeconds(.18f);
        if(extra) {
            float ate=Time.time+.1f;
            typeof(PatoController).GetField("movimento",privado).SetValue(controle,1f);typeof(PatoController).GetField("correndo",privado).SetValue(controle,false);
            while(Time.time<ate){controle.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
            Exigir(poderes.SaltarExtra(controle.forcaPulo),"Salto extra existente");
        }
        yield return Andar(x);yield return new WaitForSeconds(.4f);
    }
    IEnumerator Magia(QuestElementPowers.Elemento e){float prazo=Time.time+12;while(poderes.Mana<35&&Time.time<prazo)yield return new WaitForSeconds(.2f);poderes.Selecionar(e);controle.IniciarAtaque();yield return new WaitForSeconds(1.5f);}
    IEnumerator Esperar(Func<bool> ok){float prazo=Time.time+3;while(!ok()&&Time.time<prazo)yield return null;}
    void Exigir(bool ok,string texto){if(!ok)throw new InvalidOperationException("EQ_FOREST_FAILED: "+texto+(corpo!=null?" pos="+corpo.position:""));checagens++;Debug.Log("EQ_FOREST_CHECK_OK: "+texto);}
    void Falha(string mensagem,string pilha,LogType tipo){if((tipo==LogType.Exception||tipo==LogType.Error||tipo==LogType.Assert)&&!pilha.Contains("UnityEditor.Search.")){Application.logMessageReceived-=Falha;File.WriteAllText(Application.dataPath+"/../forest-failure.txt",mensagem+"\n"+pilha);UnityEditor.EditorApplication.delayCall+=()=>UnityEditor.EditorApplication.Exit(1);}}
    IEnumerator Capturar(string nome){yield return new WaitForSeconds(.4f);if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;var camera=Camera.main;var rt=new RenderTexture(1280,720,24);var anterior=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Application.dataPath+"/../"+nome,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=anterior;Destroy(rt);Destroy(tex);}
}
#endif
