using UnityEngine;

public class QuestAnimation : MonoBehaviour
{
    public Sprite[] quadros;
    public float velocidade = 8f;
    public float escalaVisual = 1f;
    private SpriteRenderer visual;
    private Material materialQuadros;

    void Awake()
    {
        visual = GetComponent<SpriteRenderer>();
        materialQuadros = visual.material;
    }

    void Update()
    {
        if (quadros != null && quadros.Length > 0)
        {
            visual.sprite = quadros[Mathf.FloorToInt(Time.time * velocidade) % quadros.Length];
            materialQuadros.mainTexture = visual.sprite.texture;
            if(escalaVisual!=1f) {
                visual.drawMode=SpriteDrawMode.Sliced;
                visual.size=visual.sprite.bounds.size*Mathf.Max(.1f,escalaVisual);
            }
        }
    }

    void OnDestroy()
    {
        if (materialQuadros != null)
            Destroy(materialQuadros);
    }
}
