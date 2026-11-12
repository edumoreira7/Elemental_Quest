using UnityEngine;

public static class QuestHudArt
{
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
