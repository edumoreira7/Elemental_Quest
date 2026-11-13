using UnityEngine;

// Presentation only: original art recomposes at the entry camera position.
[DefaultExecutionOrder(350)]
public sealed class QuestLayeredParallax : MonoBehaviour
{
    public Sprite[] layers;
    public float[] factors = { .008f, .035f, .085f };
    public Transform[][] Panels { get; private set; }
    Camera view;
    float origin;
    Material[] materials;
    public float LayerWidth { get; private set; }
    void Start()
    {
        view = Camera.main;
        origin = view.transform.position.x;
        Panels = new Transform[layers.Length][];
        materials = new Material[layers.Length];
        for (int l = 0; l < layers.Length; l++)
        {
            if (layers[l] == null) continue;
            materials[l] = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            materials[l].name = "Parallax " + layers[l].name;
            materials[l].mainTexture = layers[l].texture;
            Panels[l] = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                var go = new GameObject("Parallax L" + l + " panel " + i, typeof(SpriteRenderer));
                go.transform.SetParent(transform, false);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = layers[l]; sr.sharedMaterial = materials[l]; sr.sortingOrder = -100 + l;
                Panels[l][i] = go.transform;
            }
        }
        HideComposite();
        UpdatePanels();
    }
    void HideComposite()
    {
        foreach (var sr in FindObjectsByType<SpriteRenderer>())
            if (sr.gameObject.scene == gameObject.scene &&
                (sr.name == "Background regional composto" || sr.name == "Eclipse - panorama")) sr.enabled = false;
    }
    void LateUpdate() { if (view != null) { HideComposite(); UpdatePanels(); } }
    void UpdatePanels()
    {
        float height = view.orthographicSize * 2;
        for (int l = 0; l < layers.Length; l++)
        {
            if (layers[l] == null) continue;
            float scale = Mathf.Max(height / layers[l].bounds.size.y, height * view.aspect / layers[l].bounds.size.x);
            float width = layers[l].bounds.size.x * scale;
            LayerWidth = width;
            float shift = (view.transform.position.x - origin) * factors[l];
            int center = Mathf.FloorToInt(shift / width + .5f);
            for (int i = 0; i < 5; i++)
            {
                int index = center + i - 2;
                var p = Panels[l][i];
                // Alternate reflected copies: outer pixels meet without hard panorama seams.
                p.GetComponent<SpriteRenderer>().flipX = (index & 1) != 0;
                p.localScale = Vector3.one * scale;
                p.position = new Vector3(view.transform.position.x + index * width - shift, view.transform.position.y, 10);
            }
        }
    }
    void OnDestroy() { if (materials != null) foreach (var m in materials) if (m != null) Destroy(m); }
}
