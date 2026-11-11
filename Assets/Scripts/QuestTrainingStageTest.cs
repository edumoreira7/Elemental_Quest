#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestTrainingStageTest : MonoBehaviour
{
    private QuestElementTraining treino;
    private PatoController controle;
    private QuestElementPowers poderes;
    private Rigidbody2D corpo;
    private Collider2D colisor;

    public static void Begin() { UnityEditor.EditorApplication.isPlaying = true; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        if (!Environment.GetCommandLineArgs().Contains("-questTrainingTest")) return;
        GameObject objeto = new GameObject("Validacao do treino elemental");
        DontDestroyOnLoad(objeto);
        objeto.AddComponent<QuestTrainingStageTest>();
    }

    private IEnumerator Start()
    {
        Application.logMessageReceived += Falha;
        QuestSession.Vidas = 3;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return new WaitForSeconds(.5f);
        treino = FindAnyObjectByType<QuestElementTraining>();
        controle = treino.jogador.GetComponent<PatoController>();
        poderes = treino.jogador.GetComponent<QuestElementPowers>();
        corpo = treino.jogador.GetComponent<Rigidbody2D>();
        colisor = treino.jogador.GetComponent<Collider2D>();
        controle.enabled = false;
        Exigir(treino.estacoes.Length == 6, "Seis introducoes do mapa curto presentes");
        Exigir(treino.estacoes.Count(estacao => estacao.pontoTerra > 0) == 1, "Marca de posicionamento da Terra preservada");
        Exigir(FindObjectsByType<QuestEnemy>(FindObjectsInactive.Include).All(inimigo => inimigo.transform.position.x >= 143), "Treino elemental permanece sem inimigos");
        Exigir(FindObjectsByType<QuestElementTarget>().All(alvo => alvo.transform.position.x >= 49), "Area inicial preservada sem alvos elementais");
        Exigir(!QuestSession.Atual.avisarColetaCompleta, "Tutorial nao manda procurar portal inexistente");
        Exigir(treino.tutorial.faixas.All(faixa => faixa.mensagem.Length <= 24), "Dicas continuam curtas");
        Posicionar(51, -3);
        yield return null;
        poderes.Selecionar(QuestElementPowers.Elemento.Terra);
        yield return new WaitForSeconds(.1f);
        Exigir(treino.estacoes[0].Concluida, "Troca de elemento reconhecida");
        yield return Terra(1, 62.2f, 66.5f);
        Capturar("treino-terra.png");
        QuestElementTarget agua = treino.estacoes[2].alvos[0];
        Posicionar(83, -1.7f);
        yield return new WaitForSeconds(.5f);
        Exigir(QuestSession.Vidas == 3, "Agua de treino nao causa dano sem gelo");
        yield return Gelo(2, 80.5f, -1.4f, 86);
        Capturar("treino-gelo.png");
        agua.duracaoGelo = .1f;
        agua.Aplicar(QuestElementPowers.Elemento.Gelo);
        Posicionar(83, -1.4f);
        yield return new WaitForSeconds(.5f);
        Exigir(QuestSession.Vidas == 3 && agua.GetComponent<Collider2D>().isTrigger, "Degelo tambem permanece seguro");
        agua.duracaoGelo = 8;
        Posicionar(101.2f, -2.3f);
        yield return new WaitForSeconds(.2f);
        poderes.Selecionar(QuestElementPowers.Elemento.Ar);
        yield return PularAte(105.3f, true);
        Exigir(treino.estacoes[3].Concluida, "Ar alcanca marco alto com salto extra real");
        Posicionar(112, -2.2f);
        poderes.AtualizarVoo(true);
        yield return null;
        yield return null;
        Exigir(!treino.estacoes[4].Concluida, "Pouso sem planar nao conclui o treino");
        typeof(QuestElementPowers).GetField("tempoPlanar", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(poderes, .25f);
        yield return null;
        yield return null;
        Exigir(treino.estacoes[4].Concluida, "Progresso reconhece telemetria de planar simulada; tecla J requer teste manual");
        yield return Fogo(5, 126.5f, 132);
        Capturar("treino-fogo.png");
        yield return null;
        yield return null;
        Exigir(treino.Concluiu && treino.Concluidas == 6, "Seis marcos do mapa curto concluidos; planar teve telemetria simulada");
        Exigir(QuestSession.Vidas == 3, "Treino completo sem perder vida");
        Capturar("treino-final.png");
        Debug.Log("EQ_TRAINING_TEST_OK: planar fisico e teclado permanecem verificacao manual.");
        UnityEditor.EditorApplication.Exit(0);
    }

    private IEnumerator Terra(int indice, float partida, float destino)
    {
        Posicionar(partida, -3);
        yield return new WaitForSeconds(.2f);
        yield return Conjurar(QuestElementPowers.Elemento.Terra);
        GameObject bloco = GameObject.Find("Terra/01-Bloco-Intacto");
        Exigir(bloco != null && bloco.GetComponent<Collider2D>().enabled, "Bloco pronto na estacao " + indice);
        yield return PularAte(bloco.transform.position.x, false);
        yield return new WaitForSeconds(.3f);
        Exigir(treino.estacoes[indice].UsouTerra, "Degrau de Terra reconhecido na estacao " + indice);
        yield return PularAte(destino, false);
        Exigir(treino.estacoes[indice].Concluida, "Marco de Terra alcancavel na estacao " + indice);
    }

    private IEnumerator Gelo(int indice, float partida, float topo, float destino)
    {
        Posicionar(partida, topo);
        yield return new WaitForSeconds(.2f);
        yield return Conjurar(QuestElementPowers.Elemento.Gelo);
        Collider2D agua = treino.estacoes[indice].alvos[0].GetComponent<Collider2D>();
        Exigir(!agua.isTrigger && !Physics2D.GetIgnoreCollision(agua, colisor), "Gelo cria piso utilizavel na estacao " + indice);
        yield return AndarAte(destino);
        yield return null;
        Exigir(treino.estacoes[indice].Concluida, "Travessia de Gelo conclui estacao " + indice);
    }

    private IEnumerator Fogo(int indice, float partida, float destino)
    {
        Posicionar(partida, -3);
        yield return new WaitForSeconds(.2f);
        yield return Conjurar(QuestElementPowers.Elemento.Fogo);
        yield return new WaitForSeconds(.3f);
        Exigir(treino.estacoes[indice].UsouFogo, "Fogo abre passagem na estacao " + indice);
        yield return AndarAte(destino);
        yield return null;
        Exigir(treino.estacoes[indice].Concluida, "Passagem de Fogo alcancavel na estacao " + indice);
    }

    private IEnumerator Conjurar(QuestElementPowers.Elemento elemento)
    {
        while (poderes.Mana < 35) yield return new WaitForSeconds(.2f);
        poderes.Selecionar(elemento);
        controle.IniciarAtaque();
        yield return new WaitForSeconds(1.15f);
    }

    private void Posicionar(float posicaoX, float piso)
    {
        corpo.position = new Vector2(posicaoX, piso + colisor.bounds.extents.y + .03f);
        treino.jogador.position = corpo.position;
        corpo.linearVelocity = Vector2.zero;
        poderes.AtualizarVoo(true);
        Physics2D.SyncTransforms();
    }

    private IEnumerator PularAte(float destino, bool extra)
    {
        poderes.AtualizarVoo(true);
        corpo.AddForce(Vector2.up * controle.forcaPulo, ForceMode2D.Impulse);
        float inicio = Time.time;
        bool saltou = false;
        yield return new WaitForSeconds(.18f);
        float limite = Time.time + 7;
        while (corpo.position.x < destino && Time.time < limite)
        {
            if (extra && !saltou && Time.time - inicio >= .28f)
            {
                Exigir(poderes.SaltarExtra(controle.forcaPulo), "Salto extra executado pelo sistema existente");
                saltou = true;
            }
            Mover();
            yield return new WaitForFixedUpdate();
        }
        corpo.linearVelocity = new Vector2(0, corpo.linearVelocity.y);
        yield return new WaitForSeconds(.3f);
        Exigir(corpo.position.x >= destino, "Salto ate x=" + destino + "; posicao=" + corpo.position);
    }

    private IEnumerator AndarAte(float destino)
    {
        float limite = Time.time + 8;
        while (corpo.position.x < destino && Time.time < limite)
        {
            Mover();
            yield return new WaitForFixedUpdate();
        }
        corpo.linearVelocity = new Vector2(0, corpo.linearVelocity.y);
        Exigir(corpo.position.x >= destino, "Caminho ate x=" + destino);
    }

    private void Mover()
    {
        typeof(PatoController).GetField("movimento", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controle, 1f);
        typeof(PatoController).GetField("correndo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controle, false);
        controle.SendMessage("FixedUpdate");
    }

    private static void Exigir(bool condicao, string mensagem)
    {
        if (!condicao) throw new InvalidOperationException("EQ_TRAINING_FAILED: " + mensagem);
        Debug.Log("EQ_TRAINING_CHECK_OK: " + mensagem);
    }

    private void Falha(string mensagem, string pilha, LogType tipo)
    {
        if ((tipo == LogType.Exception || tipo == LogType.Assert) && !pilha.Contains("UnityEditor.Search."))
        {
            Application.logMessageReceived -= Falha;
            File.WriteAllText(Application.dataPath + "/../training-failure.txt", mensagem + "\n" + pilha);
            UnityEditor.EditorApplication.delayCall += () => UnityEditor.EditorApplication.Exit(1);
        }
    }

    private void Capturar(string nome)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
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
