using UnityEngine;

public static class QuestHudArt
{
    public static void BossHint(string name, int health, string hint)
    {
        // Use the HUD's reference canvas instead of a large box in physical pixels.
        var previousMatrix = GUI.matrix;
        var previousColor = GUI.color;
        // Clear the unused viewport margins, including pixels left by the last scene.
        GUI.matrix = Matrix4x4.identity;
        GUI.color = Color.black;
        var camera = Camera.main;
        if (camera != null)
        {
            Rect viewport = camera.pixelRect;
            float top = Screen.height - viewport.yMax;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, top), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, Screen.height - viewport.yMin, Screen.width, viewport.yMin), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, top, viewport.xMin, viewport.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(viewport.xMax, top, Screen.width - viewport.xMax, viewport.height), Texture2D.whiteTexture);
        }
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2,
            (Screen.height - 720 * scale) / 2), Quaternion.identity, Vector3.one * scale);
        GUI.color = new Color(.025f, .045f, .09f, .85f);
        GUI.DrawTexture(new Rect(360, 174, 560, 62), Texture2D.whiteTexture);
        GUI.color = Color.white;
        var title = new GUIStyle(GUI.skin.label) { fontSize = 17, alignment = TextAnchor.MiddleCenter };
        var text = new GUIStyle(title) { fontSize = 19, wordWrap = true };
        GUI.Label(new Rect(370, 177, 540, 22), name + "  " + health + " / 6", title);
        GUI.Label(new Rect(370, 200, 540, 32), hint, text);
        GUI.color = previousColor;
        GUI.matrix = previousMatrix;
    }

    public static void Desenhar(Rect area, string arquivo)
    {
        Sprite[] quadros = QuestMagicArt.Quadros("Tutorial/Interface/" + arquivo, 1);
        if (quadros.Length == 0)
            return;
        Sprite sprite = quadros[0];
        Rect recorte = sprite.rect;
        float escala = Mathf.Min(area.width / recorte.width, area.height / recorte.height);
        Rect destino = new Rect(area.center.x - recorte.width * escala * .5f, area.center.y - recorte.height * escala * .5f, recorte.width * escala, recorte.height * escala);
        GUI.DrawTextureWithTexCoords(destino, sprite.texture, new Rect(recorte.x / sprite.texture.width, recorte.y / sprite.texture.height, recorte.width / sprite.texture.width, recorte.height / sprite.texture.height));
    }

    public static string RegionTitle(int region)
    {
        string[] names={"Floresta das Raizes Perdidas","Cume dos Ventos Partidos","Vulcao Cinzento","Castelo do Eclipse"};
        return region>=1&&region<=4?region+" / 4 - "+names[region-1]:"Tutorial";
    }
    public static void CampaignCrystals()
    {
        string[] names={"TERRA","AR","FOGO","GELO"};
        GUI.Label(new Rect(835,78,390,22),"CRISTAIS DA CAMPANHA  "+QuestCampaign.CrystalCount+" / 4");
        for(int i=0;i<4;i++) {
            var previous=GUI.color;GUI.color=QuestCampaign.HasCrystal(i)?new Color(.6f,1,.8f):new Color(.6f,.62f,.7f);
            GUI.Label(new Rect(835+i*95,108,95,35),(QuestCampaign.HasCrystal(i)?"[+] ":"[ ] ")+names[i]);GUI.color=previous;
        }
    }
    public static void Mostrar(QuestElementPowers poderes)
    {
        for (int indice = 0; indice < 3; indice++)
            Desenhar(new Rect(450 + indice * 46, 33, 40, 34), QuestSession.Vidas > indice ? "05-Coracao-Cheio" : "06-Coracao-Vazio");
        Desenhar(new Rect(38, 94, 30, 34), "07-Icone-Mana");
        Rect barra = new Rect(82, 103, 210, 17);
        Color anterior = GUI.color;
        GUI.color = new Color(.08f, .12f, .2f);
        GUI.DrawTexture(barra, Texture2D.whiteTexture);
        GUI.color = new Color(.15f, .65f, 1f);
        GUI.DrawTexture(new Rect(barra.x, barra.y, barra.width * Mathf.Clamp01(poderes.Mana / Mathf.Max(1, poderes.manaMaxima)), barra.height), Texture2D.whiteTexture);
        GUI.color = anterior;
        GUI.Label(new Rect(305, 98, 120, 30), "MANA " + Mathf.FloorToInt(poderes.Mana));
        string[] icones = { "01-Icone-Terra", "02-Icone-Ar", "03-Icone-Fogo", "04-Icone-Gelo" };
        for (int indice = 0; indice < 4; indice++)
        {
            Rect area = new Rect(470 + indice * 76, 88, 48, 48);
            if ((int)poderes.Atual == indice)
                Desenhar(new Rect(area.x - 5, area.y - 5, 58, 58), "12-Elemento-Selecionado");
            Desenhar(area, icones[indice]);
            GUI.Label(new Rect(area.x + 17, 137, 24, 22), (indice + 1).ToString());
        }
    }
}

// A material belongs to one renderer so animation frames cannot borrow another
// sprite's texture from a shared legacy material. Runtime-only; no asset edits.
public sealed class QuestBossSpriteMaterial : MonoBehaviour
{
    SpriteRenderer rendererSprite;
    Material material;
    void Awake()
    {
        rendererSprite = GetComponent<SpriteRenderer>();
        material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        material.name = "Visual do boss - " + name;
        rendererSprite.sharedMaterial = material;
        LateUpdate();
    }
    void LateUpdate()
    {
        if (rendererSprite.sprite != null) material.mainTexture = rendererSprite.sprite.texture;
    }
    void OnDestroy() { if (material != null) Destroy(material); }
}
