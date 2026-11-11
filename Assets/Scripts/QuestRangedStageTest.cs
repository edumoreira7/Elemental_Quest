#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestRangedStageTest : MonoBehaviour
{
    QuestTutorialRanged area;
    PatoController controle;
    Rigidbody2D corpo;
    Collider2D colisor;
    public static void Begin() { UnityEditor.EditorApplication.isPlaying = true; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        if (!Environment.GetCommandLineArgs().Contains("-questRangedTest")) return;
        var objeto = new GameObject("Validacao do atirador");
        DontDestroyOnLoad(objeto);
        objeto.AddComponent<QuestRangedStageTest>();
    }
    IEnumerator Start()
    {
        Application.logMessageReceived += Falha;
        Time.captureFramerate = 60;
        QuestSession.Vidas = 3;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return new WaitForSeconds(.5f);
        area = FindAnyObjectByType<QuestTutorialRanged>();
        controle = area.jogador.GetComponent<PatoController>();
        controle.enabled = false;
        corpo = area.jogador.GetComponent<Rigidbody2D>();
        colisor = area.jogador.GetComponent<Collider2D>();
        Exigir(!area.primeiro.gameObject.activeSelf && !area.segundo.gameObject.activeSelf, "Inimigos aguardam aproximacao");
        Exigir(area.primeiro.tipo == QuestEnemy.Tipo.Atirador && area.primeiro.prefabDisparo != null, "Atirador e projetil existentes referenciados");
        Posicionar(275.5f, -3);
        yield return new WaitForSeconds(1.5f);
        Exigir(area.Visivel(area.primeiro) && area.primeiro.enabled, "Ameaca visivel antes do alcance");
        Exigir(FindObjectsByType<QuestEnemyShot>().Length == 0 && QuestSession.Vidas == 3, "Observacao inicial segura");
        Capturar("atirador-observacao.png");
        Posicionar(278, -2);
        float prazo = Time.time + 5;
        while (area.UltimoDisparo < 0 && Time.time < prazo) yield return null;
        Exigir(area.UltimoDisparo > 0, "IA original dispara ao ver jogador no abrigo");
        Capturar("atirador-disparo.png");
        yield return new WaitForSeconds(4);
        Exigir(QuestSession.Vidas == 3, "Abrigo bloqueia projeteis sem alterar dano");
        Exigir(!area.segundo.gameObject.activeSelf, "Expansao aguarda derrota do primeiro");
        QuestCamera seguir = Camera.main.GetComponent<QuestCamera>();
        seguir.enabled = false;
        Camera.main.transform.position += Vector3.left * 30;
        yield return new WaitForSeconds(.1f);
        Exigir(!area.primeiro.enabled, "Atirador suspenso fora da camera");
        seguir.enabled = true;
        Posicionar(280.5f, -3);
        prazo = Time.time + 5;
        while (QuestSession.Vidas == 3 && Time.time < prazo) yield return null;
        Exigir(QuestSession.Vidas == 2, "Disparo real causa uma vida de dano");
        Exigir(corpo.position.x > 279, "Dano preserva posicao sem voltar ao checkpoint");
        var poderes = area.jogador.GetComponent<QuestElementPowers>();
        poderes.Selecionar(QuestElementPowers.Elemento.Fogo);
        controle.IniciarAtaque();
        prazo = Time.time + 4;
        while (area.primeiro != null && Time.time < prazo) yield return null;
        Exigir(area.primeiro == null, "Contra-ataque real derrota primeiro atirador");
        Posicionar(298, -2);
        yield return new WaitForSeconds(1.5f);
        Exigir(area.segundo.gameObject.activeSelf && area.Visivel(area.segundo), "Segundo encontro ativado em sequencia e visivel");
        Exigir(FindObjectsByType<QuestEnemy>().Count(alvo => alvo.tipo == QuestEnemy.Tipo.Atirador) == 1, "Somente um atirador ativo");
        int vidasAntes = QuestSession.Vidas;
        yield return new WaitForSeconds(4);
        Exigir(QuestSession.Vidas == vidasAntes, "Abrigo da expansao protege jogador");
        Posicionar(301, -3);
        controle.IniciarAtaque();
        prazo = Time.time + 4;
        while (area.segundo != null && Time.time < prazo) yield return null;
        Exigir(area.segundo == null, "Magia existente derrota atirador da expansao");
        Posicionar(323, -3);
        yield return new WaitForSeconds(1);
        Exigir(area.Concluiu && area.marco.color.g > .9f, "Marco confirma conclusao");
        Exigir(SceneManager.GetActiveScene().name == "Tutorial", "Transicao permanece na entrada do desafio final");
        Capturar("atirador-concluido.png");
        Debug.Log("EQ_RANGED_TEST_OK");
        UnityEditor.EditorApplication.Exit(0);
    }
    void Posicionar(float x, float piso)
    {
        corpo.position = new Vector2(x, piso + colisor.bounds.extents.y + .02f);
        area.jogador.position = corpo.position;
        corpo.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }
    static void Exigir(bool valor, string mensagem)
    {
        if (!valor) throw new InvalidOperationException("EQ_RANGED_FAILED: " + mensagem);
        Debug.Log("EQ_RANGED_CHECK_OK: " + mensagem);
    }
    void Falha(string mensagem, string pilha, LogType tipo)
    {
        if ((tipo == LogType.Exception || tipo == LogType.Assert) && !pilha.Contains("UnityEditor.Search."))
        {
            Application.logMessageReceived -= Falha;
            File.WriteAllText(Application.dataPath + "/../ranged-failure.txt", mensagem + "\n" + pilha);
            UnityEditor.EditorApplication.delayCall += () => UnityEditor.EditorApplication.Exit(1);
        }
    }
    void Capturar(string nome)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var camera = Camera.main;
        var destino = new RenderTexture(1280, 720, 24);
        var anterior = RenderTexture.active;
        camera.targetTexture = destino;
        camera.Render();
        RenderTexture.active = destino;
        var imagem = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        imagem.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        imagem.Apply();
        File.WriteAllBytes(Application.dataPath + "/../" + nome, imagem.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = anterior;
        Destroy(destino);
        Destroy(imagem);
    }
}
#endif
