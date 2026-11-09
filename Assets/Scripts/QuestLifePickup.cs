using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class QuestLifePickup : MonoBehaviour
{
    private bool coletado;

    void Start()
    {
        GameObject visual = QuestMagicArt.Criar("Tutorial/Recursos/01-Gota-Vital", transform.position, new Vector2(.55f, .7f), 6, true, 8f);
        if (visual != null)
        {
            visual.transform.SetParent(transform, true);
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (coletado || outro.GetComponent<QuestPlayer>() == null || QuestSession.Atual == null)
            return;
        if (!QuestSession.Atual.ColetarVida())
            return;
        coletado = true;
        QuestMagicArt.Criar("Tutorial/Recursos/03-Coleta", transform.position, Vector2.one);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
