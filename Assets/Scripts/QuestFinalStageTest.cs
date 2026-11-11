#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestFinalStageTest : MonoBehaviour
{
    QuestTutorialFinal area;
    PatoController controle;
    Rigidbody2D corpo;
    Collider2D colisor;
    public static void Begin() { UnityEditor.EditorApplication.isPlaying = true; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        if (!Environment.GetCommandLineArgs().Contains("-questFinalTest") || FindAnyObjectByType<QuestFinalStageTest>() != null) return;
        var obj = new GameObject("Validacao tutorial final"); DontDestroyOnLoad(obj); obj.AddComponent<QuestFinalStageTest>();
    }
    IEnumerator Start()
    {
        Application.logMessageReceived += Falha;
        Time.captureFramerate = 60; QuestSession.Vidas = 3;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return new WaitForSeconds(.5f);
        Obter();
        Exigir(Application.CanStreamedLevelBeLoaded("Tutorial"), "Cena pode ser recarregada pela lista de cenas persistida");
        Exigir(FindAnyObjectByType<QuestElementTraining>().estacoes.Length == 6, "Introducoes preservadas; repeticoes removidas");
        Exigir(FindObjectsByType<QuestEnemy>(FindObjectsInactive.Include).Count(x => x.tipo == QuestEnemy.Tipo.Atirador) == 1, "Apenas uma introducao ao atirador");
        Exigir(GameObject.Find("Piso seguro sem abismos").GetComponent<BoxCollider2D>().bounds.max.x == 232, "Percurso reduzido de 344 para 232");
        Exigir(area.inimigo.tipo == QuestEnemy.Tipo.Guardiao && area.inimigo.vidas == 2, "Guardiao original de duas vidas; sem boss novo");
        Exigir(!area.portal.gameObject.activeSelf, "Objetivo final aguarda derrota");
        QuestSession.Atual.Portal();
        Exigir(!area.Concluiu, "Portal nao conclui com inimigo vivo");
        yield return EncontrosCompactos();
        Posicionar(196,-3);
        yield return new WaitForSeconds(.3f);
        Exigir(Vector3.Distance(area.tutorial.jogador.GetComponent<QuestPlayer>().pontoSeguro, area.checkpoint.destino) < .1f, "Contato ativa checkpoint existente");
        Exigir(QuestTutorialFinal.RetomarCheckpoint, "Checkpoint registrado para retomada local");
        Exigir(!area.inimigo.gameObject.activeSelf, "Preparacao antes da arena segura");
        Posicionar(204.5f,-3);
        yield return new WaitForSeconds(1.3f);
        Exigir(area.inimigo.gameObject.activeSelf, "Guardiao aparece enquadrado antes do contato");
        Exigir(QuestSession.Vidas == 3, "Observacao nao causa dano");
        Capturar("tutorial-final-observacao.png");
        Posicionar(207,-3);
        float inicio = area.inimigo.transform.position.x;
        yield return new WaitForSeconds(.4f);
        Exigir(area.inimigo.transform.position.x < inicio, "Perseguicao existente funciona na arena");
        Posicionar(area.inimigo.transform.position.x,-3);
        yield return new WaitForSeconds(.1f);
        Exigir(QuestSession.Vidas == 2, "Guardiao causa dano real");
        Exigir(corpo.position.x > 204, "Dano nao teleporta ao checkpoint");
        Posicionar(196,-3);
        yield return new WaitForSeconds(2);
        area.tutorial.jogador.GetComponent<QuestPlayer>().Ferir();
        yield return new WaitForSeconds(2);
        QuestTutorialFinal tentativaAnterior = area;
        area.tutorial.jogador.GetComponent<QuestPlayer>().Ferir();
        yield return new WaitForSeconds(.8f);
        Obter();
        Exigir(tentativaAnterior == null, "Zerar vidas realmente descarrega tentativa anterior");
        Exigir(QuestSession.Vidas == 3 && corpo.position.x > 195 && corpo.position.x < 197, "Zerar vidas reinicia junto ao checkpoint");
        Exigir(area.inimigo.vidas == 2 && !area.portal.gameObject.activeSelf, "Tentativa restaura inimigo e objetivo bloqueado");
        Posicionar(196,-9);
        yield return new WaitForSeconds(.2f);
        Exigir(corpo.position.x > 195 && corpo.position.y > -4, "Queda retorna ao checkpoint original");
        Posicionar(207,-3);
        yield return new WaitForSeconds(1);
        corpo.AddForce(Vector2.up * controle.forcaPulo, ForceMode2D.Impulse);
        yield return new WaitForSeconds(.2f);
        Exigir(colisor.bounds.min.y > -2, "Pulo existente permite esquiva vertical");
        Posicionar(206,-3);
        var poderes = area.tutorial.jogador.GetComponent<QuestElementPowers>();
        poderes.Selecionar(QuestElementPowers.Elemento.Fogo);
        controle.IniciarAtaque();
        float prazo = Time.time + 2;
        while (area.inimigo != null && area.inimigo.vidas == 2 && Time.time < prazo) yield return null;
        Exigir(area.inimigo != null && area.inimigo.vidas == 1, "Primeira magia fere sem eliminar inimigo forte");
        Posicionar(203,-3);
        yield return new WaitForSeconds(.7f);
        controle.IniciarAtaque();
        prazo = Time.time + 3;
        while (!area.Derrotado && Time.time < prazo) yield return null;
        Exigir(area.Derrotado, "Segundo contra-ataque derrota Guardiao");
        yield return new WaitForSeconds(.8f);
        Exigir(area.portal.gameObject.activeSelf, "Derrota libera portal animado existente");
        Posicionar(225,-3);
        yield return new WaitForSeconds(.3f);
        Exigir(area.Concluiu && !controle.enabled, "Contato no portal conclui tutorial claramente");
        Exigir(SceneManager.GetActiveScene().name == "Tutorial" && !QuestSession.Venceu, "Conclusao nao altera outros niveis nem vence campanha");
        Capturar("tutorial-final-concluido.png");
        Debug.Log("EQ_FINAL_TEST_OK"); UnityEditor.EditorApplication.Exit(0);
    }
    void Obter()
    {
        area = FindAnyObjectByType<QuestTutorialFinal>(); controle = area.tutorial.jogador.GetComponent<PatoController>(); controle.enabled = false;
        corpo = controle.GetComponent<Rigidbody2D>(); colisor = controle.GetComponent<Collider2D>();
    }
    IEnumerator EncontrosCompactos()
    {
        var treino = FindAnyObjectByType<QuestElementTraining>();
        Exigir(treino.estacoes.All(e => e.chegada != null && e.marco != null && e.alvos.All(a => a != null)), "Referencias das seis introducoes intactas");
        var magia = controle.GetComponent<QuestElementPowers>(); magia.Selecionar(QuestElementPowers.Elemento.Fogo);
        var corpoACorpo = FindAnyObjectByType<QuestTutorialCombat>();
        Posicionar(152,-3); controle.IniciarAtaque();
        float prazo = Time.time + 4;
        while (!corpoACorpo.Derrotado && Time.time < prazo) yield return null;
        Exigir(corpoACorpo.Derrotado, "Corpo a corpo continua derrotavel na area encurtada");
        var atirador = FindAnyObjectByType<QuestTutorialRanged>();
        Posicionar(180,-2);
        yield return new WaitForSeconds(1.5f);
        prazo = Time.time + 4;
        while (atirador.UltimoDisparo < 0 && Time.time < prazo) yield return null;
        Exigir(atirador.UltimoDisparo > 0, "Atirador real dispara na nova posicao");
        int vida = QuestSession.Vidas;
        yield return new WaitForSeconds(1);
        Exigir(QuestSession.Vidas == vida, "Abrigo preserva protecao apos encurtar");
        Posicionar(182.5f,-3); controle.IniciarAtaque();
        prazo = Time.time + 4;
        while (!atirador.PrimeiroDerrotado && Time.time < prazo) yield return null;
        Exigir(atirador.PrimeiroDerrotado, "Contra-ataque conclui encontro compacto");
        QuestSession.Vidas = 3;
    }
    void Posicionar(float x, float piso)
    {
        corpo.position = new Vector2(x,piso + colisor.bounds.extents.y + .02f); controle.transform.position = corpo.position;
        corpo.linearVelocity = Vector2.zero; Physics2D.SyncTransforms();
    }
    static void Exigir(bool ok, string mensagem) { if (!ok) throw new InvalidOperationException("EQ_FINAL_FAILED: " + mensagem); Debug.Log("EQ_FINAL_CHECK_OK: " + mensagem); }
    void Falha(string mensagem,string pilha,LogType tipo)
    {
        if ((tipo == LogType.Exception || tipo == LogType.Assert || tipo == LogType.Error) && !pilha.Contains("UnityEditor.Search."))
        {
            Application.logMessageReceived -= Falha;
            File.WriteAllText(Application.dataPath + "/../final-failure.txt", mensagem + "\n" + pilha);
            UnityEditor.EditorApplication.delayCall += () => UnityEditor.EditorApplication.Exit(1);
        }
    }
    void Capturar(string nome)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var cam = Camera.main; var rt = new RenderTexture(1280,720,24); var anterior = RenderTexture.active;
        cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var tex = new Texture2D(1280,720,TextureFormat.RGB24,false); tex.ReadPixels(new Rect(0,0,1280,720),0,0); tex.Apply();
        File.WriteAllBytes(Application.dataPath + "/../" + nome, tex.EncodeToPNG()); cam.targetTexture = null; RenderTexture.active = anterior; Destroy(rt); Destroy(tex);
    }
}
#endif
