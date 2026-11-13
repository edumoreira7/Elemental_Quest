using UnityEngine;

// Swept collision, fixed targets and bounded lifetimes; no homing or hidden damage.
public sealed class QuestEclipseSpell : MonoBehaviour
{
    public QuestEclipseArena arena;
    public QuestEclipseArena.Move element;
    public Vector2 velocity, size;
    public float expires;
    void FixedUpdate()
    {
        if (arena == null || !arena.CombatActive) { Destroy(gameObject); return; }
        if (QuestCutsceneDirector.IsPlaying || QuestSession.Atual != null && QuestSession.Atual.Pausado) return;
        float step = velocity.magnitude * Time.fixedDeltaTime;
        foreach (var hit in Physics2D.BoxCastAll(transform.position, size, 0, velocity.normalized, step))
        {
            var barrier = hit.collider.GetComponent<QuestEclipseBarrier>(); if (barrier != null && barrier.arena == arena) continue;
            var player = hit.collider.GetComponent<QuestPlayer>();
            if (player != null)
            {
                player.Ferir();
                var status = player.GetComponent<QuestEclipseStatus>(); if (status == null) status = player.gameObject.AddComponent<QuestEclipseStatus>();
                if (element == QuestEclipseArena.Move.Ice) status.Chill(.7f);
                if (element == QuestEclipseArena.Move.Air) status.Push(Mathf.Sign(velocity.x), .2f);
                Impact(); return;
            }
            if (!hit.collider.isTrigger && hit.collider.gameObject.layer == 3) { Impact(); return; }
        }
        transform.position += (Vector3)(velocity * Time.fixedDeltaTime);
        if (Time.time >= expires || Mathf.Abs(transform.position.x) > 9 || transform.position.y < QuestEclipseArena.Floor - .5f) Impact();
    }
    void Impact()
    {
        if (arena != null && arena.CombatActive)
        {
            if (element == QuestEclipseArena.Move.Shadow) arena.Fx(arena.gustEnd, transform.position, .35f);
            else arena.SpellEffect(element == QuestEclipseArena.Move.Fire ? "Fogo/03-Impacto" : element == QuestEclipseArena.Move.Ice ? "Gelo/02-Impacto-Congelante" : "Ar/02-Dissipacao-Rajada", transform.position, Vector2.one * .7f, 6);
        }
        Destroy(gameObject);
    }
}

public sealed class QuestEclipseBarrier : MonoBehaviour
{
    public QuestEclipseArena arena;
    public float expires;
    public int Health { get; private set; } = 3;
    public void Receive(Collider2D other)
    {
        var shot = other.GetComponent<ProjetilPoder>(); if (shot == null || Health <= 0) return;
        Health -= shot.elemental && shot.elemento == QuestElementPowers.Elemento.Fogo ? 2 : 1;
        Destroy(shot.gameObject);
        if (Health <= 0)
        {
            arena.SpellEffect("Terra/05-Desmoronamento", transform.position, new Vector2(1.1f, 1.7f), 6);
            arena.BarrierBroken(this); Destroy(gameObject);
        }
        else GetComponent<SpriteRenderer>().color = new Color(1, .72f, .5f);
    }
    void OnTriggerEnter2D(Collider2D other) { Receive(other); }
    void Update() { if (arena == null || !arena.CombatActive || Time.time >= expires) Destroy(gameObject); }
}

[DefaultExecutionOrder(200)]
public sealed class QuestEclipseStatus : MonoBehaviour
{
    float chilledUntil, pushedUntil, pushDirection;
    Rigidbody2D body;
    public bool Chilled => Time.time < chilledUntil;
    void Awake() { body = GetComponent<Rigidbody2D>(); }
    public void Chill(float seconds) { chilledUntil = Time.time + seconds; }
    public void Push(float direction, float seconds) { pushDirection = direction; pushedUntil = Time.time + seconds; }
    public void Clear() { chilledUntil = 0; pushedUntil = 0; }
    void FixedUpdate()
    {
        if (body == null || QuestCutsceneDirector.IsPlaying || QuestSession.Atual != null && QuestSession.Atual.Pausado) return;
        var v = body.linearVelocity;
        if (Chilled) v.x *= .7f;
        if (Time.time < pushedUntil) v.x += pushDirection * 2.5f;
        body.linearVelocity = v;
    }
}
