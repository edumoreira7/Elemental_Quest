using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class QuestCheckpoint : MonoBehaviour
{
    public Vector3 destino;
    private bool ativado;
    private GameObject bandeira;

    void Start()
    {
        AtualizarVisual("01-Bandeira-Inativa");
    }

    private void AtualizarVisual(string arquivo)
    {
        if (bandeira != null)
            Destroy(bandeira);
        bandeira = QuestMagicArt.Criar("Tutorial/Checkpoint/" + arquivo, transform.position, new Vector2(0, 1.4f), 4, true, 6f);
        if (bandeira != null)
        {
            bandeira.transform.SetParent(transform, true);
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        QuestPlayer jogador = outro.GetComponent<QuestPlayer>();
        if (ativado || jogador == null)
            return;
        ativado = true;
        jogador.AtivarCheckpoint(destino);
        AtualizarVisual("03-Bandeira-Ativa");
        QuestMagicArt.Criar("Tutorial/Checkpoint/02-Ativacao", transform.position, new Vector2(1.4f, 1.4f));
        SpriteRenderer visual = GetComponent<SpriteRenderer>();
        if (visual != null)
            visual.color = Color.green;
        if (QuestSession.Atual != null)
            QuestSession.Atual.Avisar("Checkpoint ativado!");
    }
}
