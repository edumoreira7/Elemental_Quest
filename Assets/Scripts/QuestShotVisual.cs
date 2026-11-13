using UnityEngine;

// Resize only the drawing. Projectile colliders and movement keep their world sizes.
public sealed class QuestShotVisual : MonoBehaviour
{
    SpriteRenderer source, drawing;
    Material material;
    public static void Apply(GameObject projectile,float factor)
    {
        if(Mathf.Approximately(factor,1))return;
        var component=projectile.AddComponent<QuestShotVisual>();
        foreach(var renderer in projectile.GetComponentsInChildren<SpriteRenderer>())
            if(renderer.enabled&&renderer.sprite!=null){component.source=renderer;break;}
        if(component.source==null)component.source=projectile.GetComponentInChildren<SpriteRenderer>();
        var child=new GameObject("Tamanho visual do disparo",typeof(SpriteRenderer));
        child.transform.SetParent(component.source.transform,false);
        child.transform.localScale=Vector3.one*factor;
        component.drawing=child.GetComponent<SpriteRenderer>();
        component.material=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        component.drawing.sharedMaterial=component.material;
        component.source.forceRenderingOff=true;
        component.LateUpdate();
    }
    void LateUpdate()
    {
        if(source==null||drawing==null)return;
        drawing.sprite=source.sprite;drawing.color=source.color;drawing.flipX=source.flipX;drawing.flipY=source.flipY;
        drawing.enabled=source.enabled;drawing.sortingLayerID=source.sortingLayerID;drawing.sortingOrder=source.sortingOrder;
        if(source.sprite!=null)material.mainTexture=source.sprite.texture;
    }
    void OnDestroy(){if(material!=null)Destroy(material);}
}
