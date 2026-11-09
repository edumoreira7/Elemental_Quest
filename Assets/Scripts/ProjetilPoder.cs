
using UnityEngine;

public class ProjetilPoder : MonoBehaviour
{
    [Header("Configuracoes")]
    public float velocidade = 10f;
    public float tempoDeVida = 3f;
    public bool elemental;
    public QuestElementPowers.Elemento elemento;

    private Rigidbody2D rb;
    private Collider2D colisor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colisor = GetComponent<Collider2D>();
    }

    public void Inicializar(
        Vector2 direcao,
        Collider2D colisorPato)
    {
        // Evita que o poder acerte o proprio pato.
        if (colisorPato != null && colisor != null)
        {
            Physics2D.IgnoreCollision(
                colisor,
                colisorPato
            );
        }

        rb.linearVelocity = direcao.normalized * velocidade;

        // Vira o projetil quando necessario.
        if (direcao.x < 0)
        {
            Vector3 escala = transform.localScale;
            escala.x = -Mathf.Abs(escala.x);
            transform.localScale = escala;
        }
    }

    void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        QuestElementTarget alvo = outro.GetComponent<QuestElementTarget>();
        if (elemental && alvo != null && alvo.Aplicar(elemento))
        {
            Impacto();
            Destroy(gameObject);
            return;
        }
        // Ignora outros triggers, como GroundCheck.
        if (outro.isTrigger)
            return;

        // Por enquanto, desaparece ao atingir
        // o chao ou outro objeto com Collider.
        Impacto();
        Destroy(gameObject);
    }

    private void Impacto()
    {
        if (!elemental)
            return;
        string caminho = elemento == QuestElementPowers.Elemento.Fogo ? "Fogo/03-Impacto" : elemento == QuestElementPowers.Elemento.Gelo ? "Gelo/02-Impacto-Congelante" : "Ar/02-Dissipacao-Rajada";
        QuestMagicArt.Criar(caminho, transform.position, Vector2.one);
    }
}
