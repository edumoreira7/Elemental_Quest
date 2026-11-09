#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestMagicPlayTest : MonoBehaviour
{
    public static void Begin()
    {
        UnityEditor.EditorApplication.isPlaying = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        if (!Environment.GetCommandLineArgs().Contains("-questMagicTest"))
            return;
        GameObject objeto = new GameObject("Teste de magia");
        DontDestroyOnLoad(objeto);
        objeto.AddComponent<QuestMagicPlayTest>();
    }

    private IEnumerator Start()
    {
        Application.logMessageReceived += Falha;
        QuestSession.NovoJogo();
        yield return new WaitForSeconds(.5f);
        QuestPlayer jogador = FindAnyObjectByType<QuestPlayer>();
        PatoController controle = jogador.GetComponent<PatoController>();
        QuestElementPowers poderes = jogador.GetComponent<QuestElementPowers>();
        Exigir(poderes != null && poderes.Mana == 100f, "Todos os poderes disponiveis e mana inicial");
        foreach (QuestEnemy inimigo in FindObjectsByType<QuestEnemy>())
            inimigo.gameObject.SetActive(false);
        poderes.Selecionar(QuestElementPowers.Elemento.Terra);
        controle.IniciarAtaque();
        Exigir(poderes.Mana == 75f, "Terra consome 25 mana");
        poderes.Selecionar(QuestElementPowers.Elemento.Gelo);
        Exigir(poderes.Atual == QuestElementPowers.Elemento.Terra, "Troca nao altera magia reservada");
        yield return new WaitForSeconds(.6f);
        Collider2D bloco = Physics2D.OverlapPoint(new Vector2(-3.9f, -2.2f), 1 << 3);
        Exigir(bloco != null, "Evento cria bloco com colisao");
        Exigir(!bloco.bounds.Intersects(jogador.GetComponent<Collider2D>().bounds), "Bloco nao aprisiona jogador");
        GameObject inimigoTeste = new GameObject("Inimigo para teste de parede");
        inimigoTeste.AddComponent<SpriteRenderer>().sprite = jogador.GetComponent<SpriteRenderer>().sprite;
        Rigidbody2D corpoInimigo = inimigoTeste.AddComponent<Rigidbody2D>();
        corpoInimigo.bodyType = RigidbodyType2D.Kinematic;
        BoxCollider2D colisaoInimigo = inimigoTeste.AddComponent<BoxCollider2D>();
        colisaoInimigo.size = new Vector2(.7f, .8f);
        colisaoInimigo.isTrigger = true;
        QuestEnemy comportamento = inimigoTeste.AddComponent<QuestEnemy>();
        comportamento.enabled = false;
        Vector2 centroBloco = bloco.bounds.center;
        corpoInimigo.position = centroBloco + Vector2.left * 2;
        Physics2D.SyncTransforms();
        comportamento.SendMessage("Mover", centroBloco.x + 2);
        yield return new WaitForFixedUpdate();
        yield return null;
        Exigir(corpoInimigo.position.x < bloco.bounds.min.x && !colisaoInimigo.bounds.Intersects(bloco.bounds), "Parede bloqueia inimigo vindo da esquerda");
        corpoInimigo.position = centroBloco + Vector2.right * 2;
        Physics2D.SyncTransforms();
        comportamento.tipo = QuestEnemy.Tipo.Guardiao;
        comportamento.SendMessage("Mover", centroBloco.x - 2);
        yield return new WaitForFixedUpdate();
        yield return null;
        Exigir(corpoInimigo.position.x > bloco.bounds.max.x && !colisaoInimigo.bounds.Intersects(bloco.bounds), "Parede bloqueia guardiao vindo da direita");
        comportamento.SendMessage("Start");
        int vidasAntesParede = QuestSession.Vidas;
        comportamento.SendMessage("TocarJogador", jogador.GetComponent<Collider2D>());
        Exigir(QuestSession.Vidas == vidasAntesParede, "Inimigo nao causa dano atraves da parede");
        GameObject tiroInimigo = Instantiate(FindObjectsByType<QuestEnemy>(FindObjectsInactive.Include).First(inimigo => inimigo.prefabDisparo != null).prefabDisparo, centroBloco + Vector2.right * 2, Quaternion.identity);
        tiroInimigo.GetComponent<QuestEnemyShot>().Inicializar(-1);
        yield return new WaitForSeconds(.6f);
        Exigir(tiroInimigo == null, "Parede continua bloqueando tiro inimigo");
        Destroy(inimigoTeste);
        Exigir(FindObjectsByType<QuestElementTarget>().All(alvoAtivo => alvoAtivo.tipo != QuestElementTarget.Tipo.Agua), "Barra de agua provisoria desativada na Fase1");
        Exigir(poderes.Mana > 75f, "Mana regenera");
        poderes.Selecionar(QuestElementPowers.Elemento.Ar);
        Exigir(poderes.SaltarExtra(10f), "Ar permite salto extra");
        Exigir(!poderes.SaltarExtra(10f), "Ar impede saltos extras infinitos");
        GameObject alvo = new GameObject("Alvo de teste");
        BoxCollider2D colisao = alvo.AddComponent<BoxCollider2D>();
        alvo.transform.position = new Vector3(80, 0, 0);
        QuestElementTarget interacao = alvo.AddComponent<QuestElementTarget>();
        interacao.tipo = QuestElementTarget.Tipo.Agua;
        colisao.isTrigger = true;
        Exigir(!interacao.Aplicar(QuestElementPowers.Elemento.Fogo), "Fogo nao congela agua");
        Exigir(interacao.Aplicar(QuestElementPowers.Elemento.Gelo) && !colisao.isTrigger && alvo.layer == 3, "Gelo cria piso na agua");
        interacao.duracaoGelo = .1f;
        interacao.Aplicar(QuestElementPowers.Elemento.Gelo);
        yield return new WaitForSeconds(.2f);
        Exigir(colisao.isTrigger && alvo.layer == 0, "Degelo remove piso");
        interacao.tipo = QuestElementTarget.Tipo.Lava;
        Exigir(interacao.Aplicar(QuestElementPowers.Elemento.Gelo) && !colisao.isTrigger, "Gelo resfria lava");
        interacao.tipo = QuestElementTarget.Tipo.Raizes;
        Destroy(alvo);
        alvo = new GameObject("Raizes de teste");
        colisao = alvo.AddComponent<BoxCollider2D>();
        interacao = alvo.AddComponent<QuestElementTarget>();
        interacao.tipo = QuestElementTarget.Tipo.Raizes;
        Exigir(!interacao.Aplicar(QuestElementPowers.Elemento.Gelo), "Gelo nao abre raizes");
        Exigir(interacao.Aplicar(QuestElementPowers.Elemento.Fogo) && !colisao.enabled, "Fogo abre raizes");
        Debug.Log("EQ_MAGIC_TEST_OK");
        UnityEditor.EditorApplication.Exit(0);
    }

    private static void Exigir(bool condicao, string mensagem)
    {
        if (!condicao)
            throw new InvalidOperationException("EQ_MAGIC_TEST_FAILED: " + mensagem);
        Debug.Log("EQ_MAGIC_CHECK_OK: " + mensagem);
    }

    private void Falha(string mensagem, string pilha, LogType tipo)
    {
        if ((tipo == LogType.Exception || tipo == LogType.Assert) && !pilha.Contains("UnityEditor.Search."))
        {
            Application.logMessageReceived -= Falha;
            System.IO.File.WriteAllText(Application.dataPath + "/../magic-failure.txt", mensagem + "\n" + pilha);
            UnityEditor.EditorApplication.delayCall += () => UnityEditor.EditorApplication.Exit(1);
        }
    }
}
#endif
