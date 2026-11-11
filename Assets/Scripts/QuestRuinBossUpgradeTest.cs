#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class QuestRuinBossUpgradeTest : MonoBehaviour
{
    QuestRuinBoss boss;QuestBossArena arena;PatoController controle;QuestElementPowers poderes;Rigidbody2D corpo;Collider2D colisor;
    float avisoAnterior=-1;int checks;
    static BindingFlags privado=BindingFlags.Instance|BindingFlags.NonPublic;
    public static void Begin(){EditorApplication.isPlaying=true;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar(){if(!Environment.GetCommandLineArgs().Contains("-questBossUpgradeTest")||FindAnyObjectByType<QuestRuinBossUpgradeTest>()!=null)return;var obj=new GameObject("Teste do Guardiao integrado e dificil");DontDestroyOnLoad(obj);obj.AddComponent<QuestRuinBossUpgradeTest>();}
    IEnumerator Start()
    {
        Application.logMessageReceived+=Falha;Time.captureFramerate=60;QuestSession.Vidas=3;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/ArenaFloresta.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.4f);Obter();
        Exigir(boss.Vida==6&&boss.vidaMaxima==6,"Vida aumentada de quatro para seis");
        Exigir(AssetDatabase.GetAssetPath(boss.visualPrincipal.sprite).Contains("GuardiaoRuinasVisual/Frames/"),"Sprite do pacote real na batalha");
        Exigir(boss.animator.runtimeAnimatorController.animationClips.Length==11,"Onze estados com frames integrados ao combate");
        Exigir(boss.animator.runtimeAnimatorController.animationClips.All(c=>AnimationUtility.GetCurveBindings(c).Length==0),"Clips nao movem o boss nem alteram sua escala");
        var onda=boss.prefabOnda.GetComponent<QuestBossWave>();Exigir(Mathf.Abs(onda.velocidade-4.2f)<.01f&&onda.alcance==6,"Onda mais rapida e com alcance maior");
        var clipsFx=boss.prefabOnda.GetComponentInChildren<Animator>().runtimeAnimatorController.animationClips;
        var keys=AnimationUtility.GetObjectReferenceCurve(clipsFx[0],AnimationUtility.GetObjectReferenceCurveBindings(clipsFx[0]).Single());
        Exigir(keys.All(k=>k.value!=null&&!k.value.name.EndsWith("_05")),"Onda nunca usa o ultimo frame transparente durante o dano");
        PosicionarParaAtacar();yield return Atacar(QuestElementPowers.Elemento.Fogo);Exigir(boss.Vida==6,"Nucleo fechado continua imune");
        yield return Aguardar(()=>boss.Atual==QuestRuinBoss.Estado.Preparacao,false);
        Exigir(boss.Prazo-Time.time>.8f&&boss.Prazo-Time.time<1,"Preparacao acelerada ainda concede tempo de reacao");
        yield return new WaitForSeconds(.35f);yield return Capturar("boss-integrado-01-ataque.png");EvitarRisco();
        yield return Aguardar(()=>boss.Atual==QuestRuinBoss.Estado.Vulneravel);
        yield return null;Exigir(boss.efeitoNucleo.activeSelf,"FX do nucleo aparece na abertura");
        yield return Capturar("boss-integrado-02-nucleo.png");PosicionarParaAtacar();yield return Atacar(QuestElementPowers.Elemento.Fogo);yield return Aguardar(()=>boss.Vida==5);
        Exigir(boss.Vida==5,"Fogo real causa um ponto na abertura");
        Posicionar(6.5f);yield return Atacar(QuestElementPowers.Elemento.Terra);yield return Aguardar(()=>boss.TerrasAbsorvidas>=1);
        Exigir(poderes.BlocoAtual==null&&boss.Vida==6,"Terra real e absorvida e recupera um ponto");
        yield return new WaitForSeconds(.5f);EvitarRisco();yield return Atacar(QuestElementPowers.Elemento.Terra);yield return Aguardar(()=>boss.TerrasAbsorvidas>=2);
        Exigir(boss.Vida==6,"Absorcao nunca ultrapassa vida maxima");Exigir(Time.time<boss.CuraAte,"Cura fornece feedback visivel");
        bool curouNaSegundaEtapa=false;int acertos=0;
        while(boss.Vida>0&&acertos<9) {
            yield return Aguardar(()=>boss.Atual==QuestRuinBoss.Estado.Vulneravel);
            Exigir(boss.Prazo-Time.time<=2.65f,"Abertura mais curta preserva chance de contra-ataque");
            int antes=boss.Vida;PosicionarParaAtacar();yield return Atacar(QuestElementPowers.Elemento.Fogo);yield return Aguardar(()=>boss.Vida<antes);acertos++;
            Exigir(boss.Vida==antes-1,"Um dano por abertura: "+acertos);
            Exigir(Mathf.Abs(Mathf.Abs(boss.animator.transform.localScale.x)-1.3f)<.01f,"Troca de direcao preserva escala dos sprites");
            if(boss.Vida==3&&!curouNaSegundaEtapa) {
                Exigir(boss.SegundaEtapa,"Segunda etapa comeca na metade da vida");
                Posicionar(6.5f);yield return Atacar(QuestElementPowers.Elemento.Terra);yield return Aguardar(()=>boss.Vida==4);
                Exigir(boss.SegundaEtapa&&boss.TerrasAbsorvidas==3,"Cura por Terra nao desfaz a segunda etapa");curouNaSegundaEtapa=true;
                yield return Aguardar(()=>boss.Atual!=QuestRuinBoss.Estado.Preparacao);
                yield return Aguardar(()=>boss.Atual==QuestRuinBoss.Estado.Preparacao);
                Exigir(boss.Prazo-Time.time>.7f&&boss.Prazo-Time.time<.85f,"Segunda etapa usa aviso de 0,8 segundo");
            }
        }
        Exigir(boss.Vida==0&&acertos==7,"Seis danos mais cura de um exigem sete aberturas nesta sequencia");
        Exigir(QuestSession.Vidas==3,"Posicionamento seguro permite evitar os ataques acelerados");
        yield return new WaitForSeconds(1.3f);Exigir(arena.saida.activeSelf&&!boss.GetComponent<Collider2D>().enabled,"Derrota ainda interrompe dano e libera saida");
        yield return Capturar("boss-integrado-03-vitoria.png");Posicionar(6.6f);yield return new WaitForSeconds(.2f);Exigir(arena.Concluiu,"Saida ainda conclui a fase");
        SceneManager.LoadScene("ArenaFloresta");yield return new WaitForSeconds(.5f);Obter();var antiga=arena;QuestSession.Vidas=1;controle.GetComponent<QuestPlayer>().Ferir();yield return new WaitForSeconds(1);
        Exigir(antiga==null&&SceneManager.GetActiveScene().name=="FasePrincipal","Morte retorna a preparacao da fase");
        controle=FindAnyObjectByType<PatoController>();controle.enabled=false;poderes=controle.GetComponent<QuestElementPowers>();corpo=controle.GetComponent<Rigidbody2D>();colisor=controle.GetComponent<Collider2D>();
        Exigir(QuestSession.Vidas==3&&Mathf.Abs(corpo.position.x-151)<1,"Checkpoint correto com tres vidas");
        poderes.Selecionar(QuestElementPowers.Elemento.Terra);controle.IniciarAtaque();yield return new WaitForSeconds(1.3f);
        Exigir(poderes.BlocoAtual!=null&&poderes.BlocoAtual.GetComponent<Collider2D>().enabled,"Terra continua funcionando normalmente fora da batalha");
        Destroy(poderes.BlocoAtual);yield return new WaitForFixedUpdate();float prazo=Time.time+5;
        while(SceneManager.GetActiveScene().name=="FasePrincipal"&&Time.time<prazo){typeof(PatoController).GetField("movimento",privado).SetValue(controle,1f);controle.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
        yield return new WaitForSeconds(.5f);Obter();Exigir(boss.Vida==6&&!boss.SegundaEtapa,"Nova tentativa restaura seis vidas e ritmo inicial");
        Debug.Log("EQ_BOSS_UPGRADE_TEST_OK: "+checks+" verificacoes; teclado e dificuldade percebida permanecem manuais.");EditorApplication.Exit(0);
    }
    void Obter(){arena=FindAnyObjectByType<QuestBossArena>();boss=arena.boss;controle=boss.jogador.GetComponent<PatoController>();controle.enabled=false;poderes=controle.GetComponent<QuestElementPowers>();corpo=controle.GetComponent<Rigidbody2D>();colisor=controle.GetComponent<Collider2D>();avisoAnterior=-1;}
    void Posicionar(float x){corpo.position=new Vector2(x,-3+colisor.bounds.extents.y+.02f);controle.transform.position=corpo.position;corpo.linearVelocity=Vector2.zero;typeof(PatoController).GetField("olhandoDireita",privado).SetValue(controle,boss.transform.position.x>x);Physics2D.SyncTransforms();}
    void PosicionarParaAtacar(){Posicionar(boss.transform.position.x+(boss.transform.position.x>=0?-3:3));}
    void EvitarRisco(){if(boss.Atual==QuestRuinBoss.Estado.Preparacao&&Mathf.Abs(boss.Prazo-avisoAnterior)>.01f){avisoAnterior=boss.Prazo;Posicionar(boss.Direcao<0?6.5f:-6.5f);}}
    IEnumerator Atacar(QuestElementPowers.Elemento elemento){float prazo=Time.time+12;while(poderes.Mana<35&&Time.time<prazo){EvitarRisco();yield return null;}poderes.Selecionar(elemento);controle.IniciarAtaque();float ate=Time.time+1.2f;while(Time.time<ate){EvitarRisco();yield return null;}}
    IEnumerator Aguardar(Func<bool> pronto,bool esquivar=true){float prazo=Time.time+24;while(!pronto()&&Time.time<prazo){if(esquivar)EvitarRisco();yield return null;}Exigir(pronto(),"Condicao de combate alcancada");}
    void Exigir(bool ok,string mensagem){if(!ok)throw new InvalidOperationException("EQ_BOSS_UPGRADE_FAILED: "+mensagem);checks++;Debug.Log("EQ_BOSS_UPGRADE_CHECK_OK: "+mensagem);}
    void Falha(string mensagem,string pilha,LogType tipo){if((tipo==LogType.Error||tipo==LogType.Exception||tipo==LogType.Assert)&&!pilha.Contains("UnityEditor.Search.")){Application.logMessageReceived-=Falha;File.WriteAllText(Application.dataPath+"/../boss-upgrade-failure.txt",mensagem+"\n"+pilha);EditorApplication.delayCall+=()=>EditorApplication.Exit(1);}}
    IEnumerator Capturar(string nome){yield return null;if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;var c=Camera.main;var rect=c.rect;var anterior=RenderTexture.active;var rt=new RenderTexture(1280,720,24);c.rect=new Rect(0,0,1,1);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Application.dataPath+"/../"+nome,tex.EncodeToPNG());c.targetTexture=null;c.rect=rect;RenderTexture.active=anterior;Destroy(rt);Destroy(tex);}
}
#endif
