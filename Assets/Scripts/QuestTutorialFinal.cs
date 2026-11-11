using UnityEngine;

[DefaultExecutionOrder(-90)]
public class QuestTutorialFinal : MonoBehaviour
{
    public QuestTutorialArea tutorial;
    public QuestCheckpoint checkpoint;
    public QuestEnemy inimigo;
    public QuestPortal portal;
    public bool Concluiu { get; private set; }
    public bool Derrotado => inimigo == null || inimigo.vidas <= 0;
    public static bool RetomarCheckpoint { get; private set; }
    private PatoController controle;
    private QuestPlayer jogador;
    private Camera cameraJogo;
    private bool danoVisto;
    private int vidas;
    private float avisoAte;
    private bool saindo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ReiniciarSessao() { RetomarCheckpoint = false; }

    void Start()
    {
        jogador = tutorial.jogador.GetComponent<QuestPlayer>();
        controle = jogador.GetComponent<PatoController>();
        cameraJogo = Camera.main;
        vidas = QuestSession.Vidas;
        if (RetomarCheckpoint)
        {
            jogador.transform.position = checkpoint.destino;
            jogador.GetComponent<Rigidbody2D>().position = checkpoint.destino;
            jogador.AtivarCheckpoint(checkpoint.destino);
            cameraJogo.transform.position = new Vector3(checkpoint.destino.x + 2, cameraJogo.transform.position.y, cameraJogo.transform.position.z);
        }
        portal.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Concluiu && Input.GetKeyDown(KeyCode.Return)) { VoltarAoMenu(); return; }
        if (Concluiu || (QuestSession.Atual != null && QuestSession.Atual.Pausado)) return;
        if (inimigo != null && inimigo.vidas > 0)
        {
            Vector3 tela = cameraJogo.WorldToViewportPoint(inimigo.transform.position + Vector3.up * .6f);
            bool atuar = jogador.transform.position.x >= 202 && tela.x > .08f && tela.x < .92f && tela.z > 0;
            if (atuar && !inimigo.gameObject.activeSelf) inimigo.gameObject.SetActive(true);
            inimigo.enabled = atuar;
        }
        if (Derrotado && !portal.gameObject.activeSelf)
        {
            portal.gameObject.SetActive(true);
            QuestSession.Atual.Avisar("Passagem livre!");
        }
    }

    void LateUpdate()
    {
        if (jogador == null || saindo) return;
        if (Vector3.Distance(jogador.pontoSeguro, checkpoint.destino) < .1f) RetomarCheckpoint = true;
        if (QuestSession.Vidas < vidas) { danoVisto = true; avisoAte = Time.time + 1.8f; }
        vidas = QuestSession.Vidas;
        if (jogador.transform.position.x < 193) return;
        tutorial.DefinirDica(Concluiu ? "" : Derrotado ? "Entre no portal" : Time.time < avisoAte && danoVisto ? "Recue enquanto pisca" : jogador.transform.position.x < 200 ? RetomarCheckpoint ? "Checkpoint salvo: avance" : "Ative o checkpoint" : jogador.transform.position.x < 205 ? "Observe o Guardiao" : "Pule, recue e use magia");
    }

    public void TentarConcluir()
    {
        if (!Derrotado || Concluiu) return;
        Concluiu = true;
        controle.enabled = false;
        jogador.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        QuestMagicArt.Criar("Tutorial/Recursos/03-Coleta", portal.transform.position, Vector2.one * 2);
    }

    void OnGUI()
    {
        if (!Concluiu || saindo) return;
        float largura = Mathf.Min(440, Screen.width - 24);
        Rect area = new Rect((Screen.width - largura) * .5f, Screen.height * .26f, largura, 172);
        GUI.Box(area, "");
        var estilo = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 26, wordWrap = true };
        GUI.Label(new Rect(area.x+12,area.y+8,area.width-24,100), "Tutorial concluido!\nMovimento, elementos e combate", estilo);
        if (GUI.Button(new Rect(area.x+30,area.y+124,area.width-60,32), "Enter: voltar ao menu")) VoltarAoMenu();
    }
    public void VoltarAoMenu()
    {
        if (!Concluiu || saindo) return;
        saindo = true;
        RetomarCheckpoint = false;
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Inicio");
    }
}
