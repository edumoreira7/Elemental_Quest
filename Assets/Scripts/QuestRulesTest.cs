#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;

public class QuestRulesTest : MonoBehaviour
{
    [UnityEditor.MenuItem("Elemental Quest/Testes/Validar reinicio da fase")]
    public static void Executar()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Entre no Play antes de executar este teste.");
            return;
        }
        GameObject teste = new GameObject("Teste de regras de partida");
        DontDestroyOnLoad(teste);
        teste.AddComponent<QuestRulesTest>();
    }

    private IEnumerator Start()
    {
        QuestSession.NovoJogo();
        yield return null;
        yield return null;
        QuestSession sessao = QuestSession.Atual;
        Assert.AreEqual("Fase1", SceneManager.GetActiveScene().name);
        QuestPlayer jogador = FindFirstObjectByType<QuestPlayer>();
        Assert.IsNotNull(jogador);
        Vector3 checkpointTeste = new Vector3(-3f, -2.3f, 0f);
        jogador.AtivarCheckpoint(checkpointTeste);
        Vector2 posicaoAntesDano = jogador.GetComponent<Rigidbody2D>().position;
        jogador.Ferir();
        jogador.Ferir();
        Assert.AreEqual(2, QuestSession.Vidas);
        Assert.IsTrue(Vector2.Distance(jogador.GetComponent<Rigidbody2D>().position, posicaoAntesDano) < .01f);
        jogador.transform.position = new Vector3(-3f, -9f, 0f);
        yield return null;
        yield return null;
        Assert.IsTrue(Vector2.Distance(jogador.GetComponent<Rigidbody2D>().position, checkpointTeste) < .2f);
        Assert.IsTrue(sessao.ColetarVida());
        Assert.AreEqual(3, QuestSession.Vidas);
        Assert.IsFalse(sessao.ColetarVida());
        sessao.PerderVida();
        sessao.Coletar();
        sessao.PerderVida();
        sessao.PerderVida();
        yield return null;
        yield return null;
        Assert.AreEqual("Fase1", SceneManager.GetActiveScene().name);
        Assert.AreEqual(3, QuestSession.Vidas);
        Assert.AreEqual(0, QuestSession.Pontos);
        Assert.AreEqual(0, QuestSession.Atual.Coletados);
        Assert.IsFalse(QuestSession.Venceu);
        QuestSession.Pontos = 700;
        SceneManager.LoadScene("Fase2");
        yield return null;
        yield return null;
        sessao = QuestSession.Atual;
        sessao.Coletar();
        sessao.PerderVida();
        sessao.PerderVida();
        sessao.PerderVida();
        yield return null;
        yield return null;
        Assert.AreEqual("Fase2", SceneManager.GetActiveScene().name);
        Assert.AreEqual(3, QuestSession.Vidas);
        Assert.AreEqual(700, QuestSession.Pontos);
        Assert.AreEqual(0, QuestSession.Atual.Coletados);
        Debug.Log("EQ_RULES_OK: dano sem teleporte e invulnerabilidade; queda retorna ao checkpoint; coleta de vida respeita maximo; reinicio de Fase1 e Fase2; vidas restauradas; cristais restaurados; pontos anteriores preservados sem duplicacao.");
        QuestSession.NovoJogo();
        Destroy(gameObject);
    }
}
#endif
