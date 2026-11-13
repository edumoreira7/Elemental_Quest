using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(250)]
public sealed class QuestStageDetails : MonoBehaviour
{
    readonly List<Material> materials=new List<Material>();
    public int PropsPlaced {get;private set;}
    public int PortalsPlaced {get;private set;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Hook(){SceneManager.sceneLoaded-=Attach;SceneManager.sceneLoaded+=Attach;}
    static void Attach(Scene scene,LoadSceneMode mode)
    {
        foreach(var root in scene.GetRootGameObjects()){
            var campaign=root.GetComponent<QuestCampaignScene>();
            if(campaign!=null&&campaign.region>=1&&campaign.region<=3&&!campaign.epilogue&&root.GetComponent<QuestStageDetails>()==null)root.AddComponent<QuestStageDetails>();
        }
    }
    void Start()
    {
        var campaign=GetComponent<QuestCampaignScene>();int region=campaign.region+1;
        string first=region==2?"Obelisco":region==3?"Pilar":"Estatua";
        string second=region==2?"Bandeira":region==3?"Tronco":"Tocha";
        float[] positions=campaign.arena?new[]{-7.5f,7.2f}:new[]{6f,25f,45f,62f,83f,102f};
        for(int i=0;i<positions.Length;i++) {
            float x=positions[i];float floor=campaign.arena?-2:QuestStageLayout.Ground(region,x);
            DrawProp(i%2==0?first:second,new Vector2(x,floor),i%2==0?2.5f:1.8f,-4,transform);
        }
        foreach(var child in GetComponentsInChildren<Transform>(true)) {
            if(child.name.StartsWith("Entrada da Arena")||child.name.StartsWith("Saida para "))Portal(child);
            if(child.name=="Raizes carbonizadas - Fogo"||child.name=="Raizes sombrias marcadas - Fogo") {
                child.GetComponent<SpriteRenderer>().enabled=false;
                DrawProp("Raizes",new Vector2(child.position.x,QuestStageLayout.Ground(region,child.position.x)),2.4f,2,child);
            }
        }
    }
    void Portal(Transform gate)
    {
        var prefab=Resources.Load<GameObject>("LevelPolish/PortalVisual");if(prefab==null)return;
        var art=Instantiate(prefab,Vector3.zero,Quaternion.identity);art.name="Portal animado - visual F1";
        foreach(var collider in art.GetComponentsInChildren<Collider2D>()){collider.enabled=false;Destroy(collider);}
        foreach(var trigger in art.GetComponentsInChildren<QuestPortal>()){trigger.enabled=false;Destroy(trigger);}
        var sr=art.GetComponent<SpriteRenderer>();
        art.transform.localScale=Vector3.one*(2.6f/sr.sprite.bounds.size.y);
        art.transform.position=new Vector3(gate.position.x,-2-sr.sprite.bounds.min.y*art.transform.localScale.y,0);
        art.transform.SetParent(gate,true);
        var old=gate.GetComponent<SpriteRenderer>();if(old!=null)old.enabled=false;
        PortalsPlaced++;
    }
    void DrawProp(string file,Vector2 feet,float height,int order,Transform parent)
    {
        var source=Resources.Load<Sprite>("LevelPolish/"+file);if(source==null)return;
        // Sources already have transparency. Preserve every branch and glow;
        // measured solid bounds only determine size and where the feet touch ground.
        Rect bounds=SolidBounds(file);
        float scale=height/(bounds.height/source.pixelsPerUnit);
        var obj=new GameObject("Prop - "+file,typeof(SpriteRenderer));
        float xOffset=(bounds.center.x-source.pivot.x)/source.pixelsPerUnit;
        float yOffset=(bounds.y-source.pivot.y)/source.pixelsPerUnit;
        obj.transform.position=new Vector3(feet.x-xOffset*scale,feet.y-yOffset*scale,0);
        obj.transform.localScale=Vector3.one*scale;obj.transform.SetParent(parent,true);
        var material=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));material.mainTexture=source.texture;materials.Add(material);
        var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=source;sr.sharedMaterial=material;sr.sortingOrder=order;PropsPlaced++;
    }
    static Rect SolidBounds(string name)
    {
        switch(name) {
            case "Obelisco":return new Rect(57,8,142,232);
            case "Bandeira":return new Rect(45,9,199,217);
            case "Pilar":return new Rect(46,15,163,223);
            case "Tronco":return new Rect(15,19,231,215);
            case "Raizes":return new Rect(15,26,230,160);
            case "Estatua":return new Rect(60,10,143,228);
            default:return new Rect(78,15,103,231);
        }
    }
    void OnDestroy(){foreach(var material in materials)if(material!=null)Destroy(material);}
}

