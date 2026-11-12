using UnityEngine;

// Visual feedback only; QuestEnemyShot retains collision, damage and lifetime.
public class QuestCumeArrowImpact : MonoBehaviour
{
    public Sprite[] frames;
    bool emitted;
    void OnTriggerEnter2D(Collider2D other)
    {
        if(emitted || frames==null || frames.Length==0)return;
        if(other.GetComponent<QuestPlayer>()==null && (other.gameObject.layer!=3 || other.isTrigger))return;
        emitted=true;
        var fx=new GameObject("Dissipacao da flecha",typeof(SpriteRenderer));
        fx.transform.position=transform.position;fx.transform.localScale=transform.lossyScale;
        var sr=fx.GetComponent<SpriteRenderer>();var source=GetComponent<SpriteRenderer>();
        sr.sharedMaterial=source.sharedMaterial;sr.sprite=frames[0];sr.flipX=source.flipX;sr.sortingOrder=source.sortingOrder;
        var animation=fx.AddComponent<QuestAnimation>();animation.quadros=frames;animation.velocidade=12;
        Destroy(fx,frames.Length/12f);
    }
}
