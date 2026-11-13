using UnityEngine;

// Combat stays in the existing arena. This component samples only the defeated sprite.
[DefaultExecutionOrder(400)]
public sealed class QuestBossDefeatPresentation : MonoBehaviour
{
    public AnimationClip defeat;
    public Sprite[] frames;
    public bool Playing { get; private set; }
    public SpriteRenderer Body { get; private set; }
    public float Elapsed => Playing ? Time.time - started : 0;
    QuestTempestArena tempest;
    QuestMagmaArena magma;
    QuestEclipseArena eclipse;
    float started;
    void Start()
    {
        tempest = GetComponent<QuestTempestArena>();
        magma = GetComponent<QuestMagmaArena>();
        eclipse = GetComponent<QuestEclipseArena>();
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            if ((tempest != null && sr.name == "Visual da Coruja") ||
                (magma != null && sr.name == "Salamandra de Magma") ||
                (eclipse != null && sr.name == "Umbrius")) Body = sr;
    }
    void LateUpdate()
    {
        bool down = tempest != null && tempest.Current == QuestTempestArena.State.Freed ||
                    magma != null && magma.Current == QuestMagmaArena.State.Freed ||
                    eclipse != null && eclipse.Current == QuestEclipseArena.State.Freed;
        if (!down || frames == null || frames.Length == 0 || Body == null) return;
        if (!Playing) { Playing = true; started = Time.time; }
        // Arenas already drive sprites directly in Update. Assign after them explicitly.
        Body.sprite = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(Elapsed * 8))];
        // The existing material updater runs before this final visual override.
        if (Body.sharedMaterial != null && Body.sprite != null) Body.sharedMaterial.mainTexture = Body.sprite.texture;
    }
}
