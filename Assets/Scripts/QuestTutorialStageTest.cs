#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestTutorialStageTest : MonoBehaviour
{
    private PatoController controle;
    private Rigidbody2D corpo;
    private Collider2D colisor;

    public static void Begin()
    {
        UnityEditor.EditorApplication.isPlaying = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        if (!Environment.GetCommandLineArgs().Contains("-questTutorialTest"))
            return;
        GameObject teste = new GameObject("Validacao da primeira area tutorial");
        DontDestroyOnLoad(teste);
        teste.AddComponent<QuestTutorialStageTest>();
    }

    private IEnumerator Start()
    {
        Application.logMessageReceived += Falha;
        QuestSession.Vidas = 3;
        QuestSession.Pontos = 0;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return new WaitForSeconds(.6f);
        QuestPlayer jogador = FindAnyObjectByType<QuestPlayer>();
        controle = jogador.GetComponent<PatoController>();
        corpo = jogador.GetComponent<Rigidbody2D>();
        colisor = jogador.GetComponent<Collider2D>();
        controle.enabled = false;
        Exigir(Mathf.Abs(colisor.bounds.min.y + 3) < .08f, "Nascimento sobre chao seguro");
        Exigir(FindObjectsByType<QuestEnemy>(FindObjectsInactive.Include).All(inimigo => inimigo.transform.position.x >= 143), "Area inicial continua sem inimigos");
        Exigir(FindObjectsByType<QuestElementTarget>(FindObjectsInactive.Include).All(alvo => alvo.transform.position.x >= 49), "Primeira area continua sem desafios elementais");
        Exigir(FindObjectsByType<UnityEngine.Tilemaps.Tilemap>().Length > 0, "Terreno usa Tilemap");
        Exigir(FindObjectsByType<QuestBackground>().Length == 3, "Background existente com tres camadas");
        QuestTutorialArea area = FindAnyObjectByType<QuestTutorialArea>();
        Exigir(area.MensagemEm(1).Contains("A / D") && area.MensagemEm(12).Contains("Shift") && area.MensagemEm(22).Contains("Espaço"), "Instrucoes contextuais dos controles reais");
        Exigir(area.faixas.All(faixa => faixa.mensagem.Length <= 24), "Dicas curtas sem frases longas");
        Exigir(area.faixas.All(faixa => area.faixas.Count(outra => outra.inicio < faixa.fim && outra.fim > faixa.inicio) == 1), "Faixas nao sobrepoem mensagens");
        Capturar("tutorial-inicio.png");
        yield return AndarAte(8, false);
        Exigir(Mathf.Abs(corpo.linearVelocity.x) < .01f, "Movimento encerra ao soltar direcao");
        float inicioCorrida = Time.time;
        yield return AndarAte(20, true);
        Exigir(Time.time - inicioCorrida < 2.5f, "Trecho de corrida funciona com velocidade existente");
        yield return AndarAte(21.5f, false);
        corpo.AddForce(Vector2.up * controle.forcaPulo, ForceMode2D.Impulse);
        yield return AndarAte(26.5f, false);
        Exigir(corpo.position.x >= 26.5f, "Obstaculo baixo ultrapassado com pulo comum");
        yield return AndarAte(27, false);
        yield return new WaitForSeconds(.3f);
        corpo.AddForce(Vector2.up * controle.forcaPulo, ForceMode2D.Impulse);
        yield return AndarAte(30, false);
        yield return new WaitForSeconds(.2f);
        Exigir(Mathf.Abs(colisor.bounds.min.y + 2.4f) < .1f, "Primeira plataforma alcancavel");
        yield return AndarAte(31, false);
        corpo.AddForce(Vector2.up * controle.forcaPulo, ForceMode2D.Impulse);
        yield return AndarAte(34.5f, false);
        yield return new WaitForSeconds(.2f);
        Exigir(Mathf.Abs(colisor.bounds.min.y + 1.8f) < .1f, "Segunda plataforma alcancavel");
        yield return AndarAte(35.5f, false);
        corpo.AddForce(Vector2.up * controle.forcaPulo, ForceMode2D.Impulse);
        yield return AndarAte(39, false);
        yield return new WaitForSeconds(.2f);
        Exigir(Mathf.Abs(colisor.bounds.min.y + 1.2f) < .1f, "Terceira plataforma alcancavel");
        Capturar("tutorial-plataformas.png");
        yield return AndarAte(48, false);
        yield return new WaitForSeconds(.3f);
        Exigir(area.Concluiu && QuestSession.Vidas == 3, "Percurso concluido sem morte nem perda de vida");
        Exigir(QuestSession.Atual.Coletados > 0, "Trilha de cristais pode ser coletada");
        Debug.Log("EQ_TUTORIAL_STAGE1_OK");
        UnityEditor.EditorApplication.Exit(0);
    }

    private IEnumerator AndarAte(float destino, bool correr)
    {
        typeof(PatoController).GetField("movimento", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controle, 1f);
        typeof(PatoController).GetField("correndo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controle, correr);
        float limite = Time.time + 8f;
        while (corpo.position.x < destino && Time.time < limite)
        {
            controle.SendMessage("FixedUpdate");
            yield return new WaitForFixedUpdate();
        }
        corpo.linearVelocity = new Vector2(0, corpo.linearVelocity.y);
        Exigir(corpo.position.x >= destino, "Deslocamento ate x=" + destino);
    }

    private static void Exigir(bool condicao, string mensagem)
    {
        if (!condicao)
            throw new InvalidOperationException("EQ_TUTORIAL_FAILED: " + mensagem);
        Debug.Log("EQ_TUTORIAL_CHECK_OK: " + mensagem);
    }

    private void Falha(string mensagem, string pilha, LogType tipo)
    {
        if ((tipo == LogType.Exception || tipo == LogType.Assert) && !pilha.Contains("UnityEditor.Search."))
        {
            Application.logMessageReceived -= Falha;
            File.WriteAllText(Application.dataPath + "/../tutorial-failure.txt", mensagem + "\n" + pilha);
            UnityEditor.EditorApplication.delayCall += () => UnityEditor.EditorApplication.Exit(1);
        }
    }

    private void Capturar(string nome)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            return;
        Camera camera = Camera.main;
        RenderTexture destino = new RenderTexture(1280, 720, 24);
        RenderTexture anterior = RenderTexture.active;
        camera.targetTexture = destino;
        camera.Render();
        RenderTexture.active = destino;
        Texture2D imagem = new Texture2D(1280, 720, TextureFormat.RGB24, false);
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
