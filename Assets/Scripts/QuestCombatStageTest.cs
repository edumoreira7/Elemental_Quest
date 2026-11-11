#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class QuestCombatStageTest : MonoBehaviour
{
    private QuestTutorialCombat encontro;
    private PatoController controle;
    private Rigidbody2D corpo;
    private Collider2D colisor;

    public static void Begin() { UnityEditor.EditorApplication.isPlaying = true; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        if (!Environment.GetCommandLineArgs().Contains("-questCombatTest")) return;
        GameObject objeto = new GameObject("Teste do primeiro combate");
        DontDestroyOnLoad(objeto);
        objeto.AddComponent<QuestCombatStageTest>();
    }

    private IEnumerator Start()
    {
        Application.logMessageReceived += Falha;
        Time.captureFramerate = 60;
        QuestSession.Vidas = 3;
        QuestSession.Pontos = 0;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return new WaitForSeconds(.5f);
        encontro = FindAnyObjectByType<QuestTutorialCombat>();
        controle = encontro.jogador.GetComponent<PatoController>();
        controle.enabled = false;
        corpo = encontro.jogador.GetComponent<Rigidbody2D>();
        colisor = encontro.jogador.GetComponent<Collider2D>();
        QuestEnemy inimigo = encontro.inimigo;
        Exigir(FindObjectsByType<QuestEnemy>(FindObjectsInactive.Include).Count(alvo => alvo.transform.position.x < 270) == 1, "Somente um inimigo na area de corpo a corpo");
        Exigir(inimigo.tipo == QuestEnemy.Tipo.Guaxinim, "Corpo a corpo existente reutilizado");
        Exigir(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ElementalQuest/TutorialEtapa3/GuaxinimCorpoACorpo.prefab") != null, "Prefab reutilizavel salvo a partir da configuracao existente");
        Tilemap mapa = FindAnyObjectByType<Tilemap>();
        Tile tile = (Tile)mapa.GetTile(new Vector3Int(230, -4, 0));
        Exigir(tile.sprite.texture.name == "Terreno-Continuo-128" && tile.sprite.rect.width == 128, "Tiles novos em grade correta de 128px");
        Exigir(tile.sprite.texture.filterMode == FilterMode.Point, "Filtro Point preservado");
        Exigir(FindObjectsByType<BoxCollider2D>().All(caixa => caixa.isTrigger || caixa.gameObject.layer != 3 || caixa.bounds.center.x <= 223 || caixa.bounds.center.x >= 268 || caixa.bounds.max.y <= -2.95f), "Corredor de combate plano sem obstaculos extras");
        Posicionar(231.5f);
        float inicioPatrulha = inimigo.transform.position.x;
        yield return new WaitForSeconds(.9f);
        Exigir(Mathf.Abs(inimigo.transform.position.x - inicioPatrulha) > .3f, "Guaxinim patrulha com IA original");
        Exigir(inimigo.transform.position.x >= inimigo.limiteEsquerdo - .1f && inimigo.transform.position.x <= inimigo.limiteDireito + .1f, "Patrulha fica longe do trecho seguro");
        Exigir(QuestSession.Vidas == 3, "Observar de longe nao causa dano");
        Vector3 tela = Camera.main.WorldToViewportPoint(inimigo.transform.position);
        Exigir(tela.x > 0 && tela.x < 1, "Ameaca visivel a partir do ponto de observacao");
        Capturar("combate-observacao.png");
        Posicionar(inimigo.transform.position.x - .98f);
        yield return new WaitForSeconds(.18f);
        float ataqueInicio = (float)typeof(QuestEnemy).GetField("ataqueInicio", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(inimigo);
        QuestPlayer alvoCache = (QuestPlayer)typeof(QuestEnemy).GetField("jogador", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(inimigo);
        RaycastHit2D linha = Physics2D.Linecast(inimigo.GetComponent<Collider2D>().bounds.center, colisor.bounds.center, 1 << 3);
        Debug.Log("COMBAT_DIAG: jogador=" + encontro.jogador.position + "; corpo=" + corpo.position + "; inimigo=" + inimigo.transform.position + "; camada=" + inimigo.gameObject.layer + "; alvoCorreto=" + (alvoCache == encontro.jogador.GetComponent<QuestPlayer>()) + "; linha=" + (linha.collider == null ? "livre" : linha.collider.name) + "; ataqueInicio=" + ataqueInicio + "; agora=" + Time.time);
        Exigir(ataqueInicio >= 0 && Time.time - ataqueInicio < .8f, "Aproximacao inicia aviso de ataque existente");
        Posicionar(inimigo.transform.position.x - 4);
        yield return new WaitForSeconds(.6f);
        Exigir(QuestSession.Vidas == 3, "Recuar evita o golpe");
        Posicionar(inimigo.transform.position.x);
        yield return new WaitForSeconds(.1f);
        Exigir(QuestSession.Vidas == 2, "Contato do inimigo retira uma vida");
        Exigir(Mathf.Abs(corpo.position.x - inimigo.transform.position.x) < 2, "Dano nao teleporta ao checkpoint");
        bool piscou = false;
        for (int indice = 0; indice < 8; indice++)
        {
            piscou |= encontro.jogador.GetComponent<SpriteRenderer>().color.a < .9f;
            yield return new WaitForSeconds(.03f);
        }
        Exigir(piscou && QuestSession.Vidas == 2, "Piscar e invulnerabilidade temporaria preservados");
        Posicionar(inimigo.transform.position.x - 5);
        QuestElementPowers poderes = encontro.jogador.GetComponent<QuestElementPowers>();
        poderes.Selecionar(QuestElementPowers.Elemento.Fogo);
        int pontosAntes = QuestSession.Pontos;
        int vidaInimigoAntes = inimigo.vidas;
        controle.IniciarAtaque();
        float limite = Time.time + 3.5f;
        while (!encontro.Derrotado && Time.time < limite) yield return null;
        Exigir(encontro.Derrotado && (inimigo == null || inimigo.vidas < vidaInimigoAntes), "Magia real atinge e derrota o corpo a corpo");
        Exigir(QuestSession.Pontos == pontosAntes + 50, "Pontuacao de derrota existente preservada");
        yield return new WaitForSeconds(.8f);
        Exigir(FindObjectsByType<QuestEnemy>().Length == 0, "Morte remove o unico inimigo");
        float limiteSaida = Time.time + 9;
        while (corpo.position.x < 254 && Time.time < limiteSaida)
        {
            typeof(PatoController).GetField("movimento", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controle, 1f);
            typeof(PatoController).GetField("correndo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controle, false);
            controle.SendMessage("FixedUpdate");
            yield return new WaitForFixedUpdate();
        }
        corpo.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(.2f);
        Exigir(encontro.Concluiu && encontro.marco.color.g > encontro.marco.color.r, "Saida registra encontro concluido");
        Exigir(QuestSession.Vidas == 2 && SceneManager.GetActiveScene().name == "Tutorial", "Transicao segura sem nova area ou inimigo");
        Capturar("combate-concluido.png");
        Debug.Log("EQ_COMBAT_TEST_OK");
        UnityEditor.EditorApplication.Exit(0);
    }

    private void Posicionar(float posicaoX)
    {
        corpo.position = new Vector2(posicaoX, -3 + colisor.bounds.extents.y + .02f);
        encontro.jogador.position = corpo.position;
        corpo.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }

    private static void Exigir(bool condicao, string mensagem)
    {
        if (!condicao) throw new InvalidOperationException("EQ_COMBAT_FAILED: " + mensagem);
        Debug.Log("EQ_COMBAT_CHECK_OK: " + mensagem);
    }

    private void Falha(string mensagem, string pilha, LogType tipo)
    {
        if ((tipo == LogType.Exception || tipo == LogType.Assert) && !pilha.Contains("UnityEditor.Search."))
        {
            Application.logMessageReceived -= Falha;
            File.WriteAllText(Application.dataPath + "/../combat-failure.txt", mensagem + "\n" + pilha);
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
