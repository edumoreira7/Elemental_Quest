using System.Collections.Generic;
using UnityEngine;

// Presentation only. All original colliders, triggers and combat remain authoritative.
[DefaultExecutionOrder(200)]
public sealed class QuestCastleScenery : MonoBehaviour
{
    public Sprite backdrop, surface, masonry, ledge;
    public int DecoratedFloors { get; private set; }
    Camera view;
    SpriteRenderer sky;
    readonly Dictionary<Texture, Material> materials = new Dictionary<Texture, Material>();
    readonly List<Sprite> cuts = new List<Sprite>();
    void Start()
    {
        view = Camera.main;
        sky = Draw("Eclipse - panorama", backdrop, Vector3.zero, Vector2.one, -100);
        sky.transform.SetParent(view.transform, false);
        sky.transform.localPosition = new Vector3(0, 0, 20);
        // Crop inside the painted stones: supplied images have opaque purple margins.
        var top = Cut(surface, new Rect(26, 1, 208, 194));
        var fill = Cut(masonry, new Rect(31, 1, 192, 194));
        var platform = Cut(ledge, new Rect(25, 145, 208, 25));
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            string label = sr.name;
            if (label == "Junta da pedra clara" || label == "Borda de pouso" ||
                label == "Limite da arena" || label == "Pedra fria de fundo - sem colisao" ||
                label == "Moldura do vitral apagado" || label == "Vitral apagado - decoracao" ||
                label == "Travessa do vitral" || label == "Coluna fria - decoracao" ||
                label.StartsWith("Placa -")) sr.enabled = false;
            var box = sr.GetComponent<BoxCollider2D>();
            if (box == null || sr.gameObject.layer != 3 ||
                (!label.Contains("Piso continuo") && label != "Plataforma da arena")) continue;
            var bounds = box.bounds;
            sr.enabled = false;
            DecoratedFloors++;
            bool suspended = bounds.max.y > 0;
            const float cell = 2f;
            int columns = Mathf.CeilToInt(bounds.size.x / cell);
            for (int row = 0; row < (suspended ? 1 : 5); row++)
                for (int col = 0; col < columns; col++)
                {
                    float left = bounds.min.x + col * cell;
                    float width = Mathf.Min(cell, bounds.max.x - left);
                    var sprite = suspended ? platform : row == 0 ? top : fill;
                    float height = suspended ? cell * 25 / 208 : cell * 194 / 208;
                    // Trim the last column instead of compressing the stone pattern.
                    if (width < cell - .001f)
                        sprite = Cut(sprite, new Rect(sprite.rect.x, sprite.rect.y,
                            sprite.rect.width * width / cell, sprite.rect.height));
                    Draw(suspended ? "Eclipse - plataforma" : "Eclipse - alvenaria", sprite,
                        new Vector3(left + width / 2, bounds.max.y - row * height - height / 2, 0),
                        new Vector2(width, height), 1);
                }
        }
        Fit();
    }
    void LateUpdate() { Fit(); }
    void Fit()
    {
        if (sky == null || view == null || backdrop == null) return;
        float height = view.orthographicSize * 2;
        float scale = Mathf.Max(height / backdrop.bounds.size.y,
            height * view.aspect / backdrop.bounds.size.x);
        sky.transform.localScale = Vector3.one * scale;
    }
    Sprite Cut(Sprite source, Rect rect)
    {
        var sprite = Sprite.Create(source.texture, rect, Vector2.one * .5f, 64, 0, SpriteMeshType.FullRect);
        cuts.Add(sprite);
        return sprite;
    }
    SpriteRenderer Draw(string label, Sprite sprite, Vector3 position, Vector2 size, int order)
    {
        var obj = new GameObject(label, typeof(SpriteRenderer));
        obj.transform.SetParent(transform, false);
        obj.transform.position = position;
        obj.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1);
        var sr = obj.GetComponent<SpriteRenderer>();
        if (!materials.TryGetValue(sprite.texture, out var material))
        {
            material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            material.name = "Eclipse - " + sprite.texture.name;
            material.mainTexture = sprite.texture;
            materials.Add(sprite.texture, material);
        }
        sr.sprite = sprite; sr.sharedMaterial = material; sr.sortingOrder = order;
        return sr;
    }
    void OnDestroy()
    {
        foreach (var sprite in cuts) if (sprite != null) Destroy(sprite);
        foreach (var material in materials.Values) if (material != null) Destroy(material);
    }
}
