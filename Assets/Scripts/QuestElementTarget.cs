using UnityEngine;

public class QuestElementTarget : MonoBehaviour
{
    public enum Tipo { Raizes, Agua, Lava }
    public Tipo tipo;
    public float duracaoGelo = 8f;
    private bool ativo;
    private float descongelar;
    private BoxCollider2D colisor;
    private GameObject visualGelo;

    void Awake()
    {
        colisor = GetComponent<BoxCollider2D>();
    }

    public bool Aplicar(QuestElementPowers.Elemento elemento)
    {
        if (tipo == Tipo.Raizes)
        {
            if (elemento != QuestElementPowers.Elemento.Fogo || ativo)
                return false;
            ativo = true;
            colisor.enabled = false;
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
                renderer.enabled = false;
            QuestMagicArt.Criar("Fogo/05-Barreira-Queimando", transform.position, new Vector2(1.4f, 2f), 8);
            Destroy(gameObject, 1f);
            return true;
        }
        if (elemento != QuestElementPowers.Elemento.Gelo)
            return false;
        ativo = true;
        descongelar = Time.time + duracaoGelo;
        colisor.isTrigger = false;
        gameObject.layer = 3;
        if (visualGelo == null)
        {
            visualGelo = QuestMagicArt.Criar("Gelo/06-Ponte-Centro-Repetivel", transform.position, colisor.size, 1, true);
            if (visualGelo != null)
                visualGelo.transform.SetParent(transform, true);
            QuestMagicArt.Criar("Gelo/08-Formacao-Ponte", transform.position, colisor.size, 8);
        }
        return true;
    }

    void Update()
    {
        if (!ativo || tipo == Tipo.Raizes || Time.time < descongelar)
            return;
        ativo = false;
        colisor.isTrigger = true;
        gameObject.layer = 0;
        Destroy(visualGelo);
        QuestMagicArt.Criar("Gelo/10-Dissolucao-Ponte", transform.position, colisor.size, 8);
    }

    void OnTriggerStay2D(Collider2D outro)
    {
        if (tipo == Tipo.Raizes || ativo)
            return;
        QuestPlayer jogador = outro.GetComponent<QuestPlayer>();
        if (jogador != null)
            jogador.Ferir();
    }
}
