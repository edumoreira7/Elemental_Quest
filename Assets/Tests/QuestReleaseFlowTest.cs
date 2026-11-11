#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor;

public class QuestReleaseFlowTest : MonoBehaviour
{
    int checks;static BindingFlags privado=BindingFlags.Instance|BindingFlags.NonPublic;
    public static void Begin(){EditorApplication.isPlaying=true;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar(){if(!Environment.GetCommandLineArgs().Contains("-questReleaseTest")||FindAnyObjectByType<QuestReleaseFlowTest>()!=null)return;var o=new GameObject("Validacao de menu e fluxo final");DontDestroyOnLoad(o);o.AddComponent<QuestReleaseFlowTest>();}
    IEnumerator Start()
    {
        Application.logMessageReceived+=Falha;Time.captureFramerate=60;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Inicio.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return new WaitForSeconds(.5f);
        var menu=FindAnyObjectByType<QuestMainMenu>();
        Exigir(menu!=null&&FindAnyObjectByType<QuestPlayer>()==null,"Menu tem ilustracoes sem personagem jogavel ativo");
        Exigir(EditorBuildSettings.scenes.Count(s=>s.enabled)==4,"Somente quatro cenas no fluxo final");
        Exigir(EventSystem.current.currentSelectedGameObject==menu.jogar.gameObject,"Jogar recebe foco inicial para teclado");
        Exigir(menu.GetComponentsInChildren<Button>(true).All(b=>b.onClick.GetPersistentEventCount()==1),"Todos os botoes tem acao persistida na cena");
        yield return Capturar("menu-final.png");
        menu.GetComponentsInChildren<Button>().First(b=>b.name=="CONTROLES").onClick.Invoke();yield return null;
        Exigir(menu.controles.activeSelf&&EventSystem.current.currentSelectedGameObject==menu.fecharControles.gameObject,"Controles abrem modal com foco");yield return Capturar("menu-controles.png");
        menu.fecharControles.onClick.Invoke();Exigir(!menu.controles.activeSelf,"Voltar fecha os controles");
        menu.jogar.onClick.Invoke();yield return new WaitForSeconds(.6f);
        Exigir(SceneManager.GetActiveScene().name=="FasePrincipal"&&QuestSession.Vidas==3&&QuestSession.Pontos==0,"Jogar inicia fase principal com estado limpo");
        var pato=FindAnyObjectByType<PatoController>();pato.transform.position=new Vector3(151,-2.35f,0);pato.GetComponent<Rigidbody2D>().position=pato.transform.position;Physics2D.SyncTransforms();yield return new WaitForSeconds(.3f);
        Exigir(QuestForestStage.Retomar,"Checkpoint preparado para testar reinicio de nova partida");
        QuestSession.Vidas=1;QuestSession.Pontos=1234;QuestSession.VoltarAoMenu();yield return new WaitForSeconds(.4f);FindAnyObjectByType<QuestMainMenu>().jogar.onClick.Invoke();yield return new WaitForSeconds(.6f);
        Exigir(FindAnyObjectByType<PatoController>().transform.position.x<1&&QuestSession.Vidas==3&&QuestSession.Pontos==0&&!QuestForestStage.Retomar,"Nova partida nao herda checkpoint nem pontos anteriores");
        QuestSession.VoltarAoMenu();yield return new WaitForSeconds(.4f);menu=FindAnyObjectByType<QuestMainMenu>();menu.GetComponentsInChildren<Button>().First(b=>b.name=="TUTORIAL").onClick.Invoke();yield return new WaitForSeconds(.5f);
        Exigir(SceneManager.GetActiveScene().name=="Tutorial"&&!QuestTutorialFinal.RetomarCheckpoint,"Tutorial acessivel separadamente e sem progresso antigo");
        var final=FindAnyObjectByType<QuestTutorialFinal>();final.SeguirParaAventura();Exigir(SceneManager.GetActiveScene().name=="Tutorial","Continuacao nao pula tutorial ainda incompleto");
        typeof(QuestTutorialFinal).GetField("<Concluiu>k__BackingField",privado).SetValue(final,true);final.SeguirParaAventura();yield return new WaitForSeconds(.5f);
        Exigir(SceneManager.GetActiveScene().name=="FasePrincipal"&&QuestSession.Vidas==3,"Conclusao preparada no teste segue para aventura");
        pato=FindAnyObjectByType<PatoController>();pato.transform.position=new Vector3(151,-2.35f,0);pato.GetComponent<Rigidbody2D>().position=pato.transform.position;Physics2D.SyncTransforms();yield return new WaitForSeconds(.3f);
        var antiga=FindAnyObjectByType<QuestForestStage>();QuestSession.Vidas=1;pato.GetComponent<QuestPlayer>().Ferir();yield return new WaitForSeconds(.8f);
        Exigir(antiga==null&&SceneManager.GetActiveScene().name=="FasePrincipal"&&Mathf.Abs(FindAnyObjectByType<PatoController>().transform.position.x-151)<1,"Morte preserva retorno real ao checkpoint");
        pato=FindAnyObjectByType<PatoController>();pato.enabled=false;float prazo=Time.time+5;
        while(SceneManager.GetActiveScene().name=="FasePrincipal"&&Time.time<prazo){typeof(PatoController).GetField("movimento",privado).SetValue(pato,1f);pato.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
        yield return new WaitForSeconds(.6f);var arena=FindAnyObjectByType<QuestBossArena>();
        Exigir(SceneManager.GetActiveScene().name=="ArenaFloresta"&&arena.boss.Vida==6,"Portal continua carregando boss de seis vidas");
        Exigir(arena.boss.visualPrincipal.sprite.texture!=null&&arena.boss.prefabOnda!=null,"Sprites e efeito do boss preservados");
        QuestSession.Vidas=1;FindAnyObjectByType<QuestPlayer>().Ferir();yield return new WaitForSeconds(.8f);
        Exigir(arena==null&&SceneManager.GetActiveScene().name=="FasePrincipal"&&QuestSession.Vidas==3,"Derrota do boss continua retornando para preparacao");
        Time.timeScale=0;QuestSession.VoltarAoMenu();yield return new WaitForSeconds(.4f);Exigir(Time.timeScale==1&&FindAnyObjectByType<QuestMainMenu>()!=null,"Retorno ao menu restaura o tempo");
        if(Environment.GetCommandLineArgs().Contains("-questReleaseClean")) {
            var cenas=AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).Where(s=>s.StartsWith("Assets/")).ToArray();
            Exigir(cenas.Length==4&&!cenas.Any(s=>s.Contains("SampleScene")||s.EndsWith("Fase1.unity")||s.EndsWith("Fase2.unity")),"Cenas antigas realmente retiradas de Assets");
        }
        Debug.Log("EQ_RELEASE_FLOW_OK: "+checks+" verificacoes; gameplay completo e navegacao por teclado/mouse requerem revisao humana.");EditorApplication.Exit(0);
    }
    void Exigir(bool ok,string m){if(!ok)throw new InvalidOperationException("EQ_RELEASE_FAILED: "+m);checks++;Debug.Log("EQ_RELEASE_CHECK_OK: "+m);}
    void Falha(string m,string p,LogType t){if((t==LogType.Error||t==LogType.Exception||t==LogType.Assert)&&!p.Contains("UnityEditor.Search.")){Application.logMessageReceived-=Falha;File.WriteAllText(Application.dataPath+"/../release-failure.txt",m+"\n"+p);EditorApplication.delayCall+=()=>EditorApplication.Exit(1);}}
    IEnumerator Capturar(string nome)
    {
        yield return new WaitForSeconds(.2f);if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;
        var canvas=FindAnyObjectByType<Canvas>();var camera=Camera.main;var anterior=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var scaler=canvas.GetComponent<CanvasScaler>();float escalaAnterior=canvas.scaleFactor;int ordem=canvas.sortingOrder;
        camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.sortingOrder=1000;scaler.enabled=false;canvas.scaleFactor=1;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
        var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Application.dataPath+"/../"+nome,tex.EncodeToPNG());
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=ordem;canvas.scaleFactor=escalaAnterior;scaler.enabled=true;camera.targetTexture=null;RenderTexture.active=anterior;Canvas.ForceUpdateCanvases();Destroy(rt);Destroy(tex);
    }
}
#endif
