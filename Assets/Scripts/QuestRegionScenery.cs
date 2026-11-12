using UnityEngine;

// Visual replacement only: existing terrain colliders and encounters remain authoritative.
[DefaultExecutionOrder(-90)]
public class QuestRegionScenery : MonoBehaviour
{
    public Sprite background,topLeft,topCenter,topRight,fill,propColumn,propRock,propSign,crystal;
    public Sprite platformLeft,platformCenter,platformRight;
    public Rect topLeftRect,topCenterRect,topRightRect,fillRect,platformLeftRect,platformCenterRect,platformRightRect;
    public bool cume;
    public int DecoratedFloors { get; private set; }
    Material artMaterial;
    readonly System.Collections.Generic.Dictionary<Texture,Material> textureMaterials=new System.Collections.Generic.Dictionary<Texture,Material>();
    readonly System.Collections.Generic.List<Sprite> trimmedSprites=new System.Collections.Generic.List<Sprite>();
    void Awake()
    {
        if (gameObject.scene.name == "VulcaoCinzento" || gameObject.scene.name == "ArenaVulcao") { VolcanoVisuals(); return; }
        topLeft=Trim(topLeft,topLeftRect);topCenter=Trim(topCenter,topCenterRect);topRight=Trim(topRight,topRightRect);fill=Trim(fill,fillRect);
        platformLeft=Trim(platformLeft,platformLeftRect);platformCenter=Trim(platformCenter,platformCenterRect);platformRight=Trim(platformRight,platformRightRect);
        artMaterial=new Material(Shader.Find("Sprites/Default"));
        if(cume) { BuildCume(); return; }
        var camera=Camera.main;
        var backdrop=Art("Background regional composto",background,camera.transform.position+new Vector3(0,0,20),22,-100);
        backdrop.transform.SetParent(camera.transform,true);
        foreach(var sr in FindObjectsByType<SpriteRenderer>()) {
            if(sr.gameObject.layer!=3||sr.GetComponent<BoxCollider2D>()==null||!sr.name.Contains("Piso continuo")&&!sr.name.Contains("Plataforma da arena"))continue;
            var bounds=sr.GetComponent<BoxCollider2D>().bounds;
            sr.enabled=false;DecoratedFloors++;
            // Hide the old visual joints; colliders are deliberately untouched.
            foreach(var joint in FindObjectsByType<SpriteRenderer>())if(joint.name=="Junta da pedra clara"&&joint.bounds.center.x>=bounds.min.x&&joint.bounds.center.x<=bounds.max.x&&Mathf.Abs(joint.bounds.center.y-bounds.center.y)<.2f)joint.enabled=false;
            int count=Mathf.RoundToInt(bounds.size.x);
            for(int i=0;i<count;i++) {
                bool suspended=bounds.max.y>0;
                var sprite=suspended?(i==0?platformLeft:i==count-1?platformRight:platformCenter):(i==0?topLeft:i==count-1?topRight:topCenter);
                Tile(sprite,new Vector3(bounds.min.x+i+.5f,bounds.max.y-(suspended?.125f:.5f),0),suspended?.25f:1f);
                if(!suspended)Tile(fill,new Vector3(bounds.min.x+i+.5f,bounds.max.y-1.5f,0));
            }
        }
        bool arena=GetComponent<QuestCampaignScene>().arena;
        foreach(float x in arena?new[]{-7f,7f}:new[]{5f,20f,40f,60f,78f,101f})
            Art("Prop regional sem colisao",propColumn,new Vector3(x,-.65f,0),2.6f,-4);
        foreach(float x in arena?new[]{-7.8f,6.8f}:new[]{7f,24f,45f,64f,83f,100f})
            Art("Pedra regional sem colisao",propRock,new Vector3(x,-1.65f,0),1.2f,-3);
        if(!arena&&propSign!=null)Art("Placa regional",propSign,new Vector3(3,-1,0),1.7f,-2);
        if(arena&&crystal!=null) {
            foreach(var root in gameObject.scene.GetRootGameObjects())foreach(var child in root.GetComponentsInChildren<Transform>(true))
                if(child.name==(cume?"Saida para o Vulcao":"Saida para o Castelo")) {var reward=Art(cume?"Cristal do Ar - recompensa visual":"Cristal do Fogo - recompensa visual",crystal,child.position+Vector3.up*.7f,1.1f,5);reward.transform.SetParent(child,true);}
        }
    }
    // The Cume has its own presentation; the other regions keep their existing art.
    Camera cumeCamera;
    SpriteRenderer cumeBackdrop;
    void BuildCume()
    {
        cumeCamera=Camera.main;
        cumeBackdrop=Art("Background regional composto",background,cumeCamera.transform.position+new Vector3(0,0,20),22,-100);
        cumeBackdrop.transform.SetParent(cumeCamera.transform,true);
        FitCumeBackground();
        var renderers=GetComponentsInChildren<SpriteRenderer>();
        foreach(var sr in renderers) {
            // Remove prototype columns, caps and bright lines left over from assembly.
            if(sr.name=="Coluna de ruina clara"||sr.name=="Capitel"||sr.name=="Junta da pedra clara"||sr.name=="Borda de pouso"||sr.name=="Limite da arena"||sr.name.StartsWith("Placa -"))sr.enabled=false;
            var collider=sr.GetComponent<BoxCollider2D>();
            if(collider==null||sr.gameObject.layer!=3||(!sr.name.Contains("Piso continuo")&&!sr.name.Contains("Plataforma da arena")))continue;
            var bounds=collider.bounds;
            sr.enabled=false;DecoratedFloors++;
            int count=Mathf.RoundToInt(bounds.size.x);
            bool suspended=bounds.max.y>0;
            for(int i=0;i<count;i++) {
                float x=bounds.min.x+i+.5f;
                if(suspended) {
                    // These tiles contain 24 pixels of ledge, so retain their native aspect.
                    Tile(i==0?platformLeft:i==count-1?platformRight:platformCenter,new Vector3(x,bounds.max.y-.1875f,0),.375f);
                } else {
                    Tile(i==0?topLeft:i==count-1?topRight:topCenter,new Vector3(x,bounds.max.y-.5f,0));
                    // Continue the cliff below the viewport instead of leaving a floating strip.
                    for(int row=1;row<=6;row++)Tile(fill,new Vector3(x,bounds.max.y-row-.5f,0));
                }
            }
        }
        // Regional prop PNGs have opaque baked backgrounds. Do not place rectangular
        // pictures over the landscape; transparent props can be added independently.
    }
    void LateUpdate(){if(cume)FitCumeBackground();if(volcanoCamera!=null)FitVolcanoBackground();}
    void FitCumeBackground()
    {
        if(cumeCamera==null||cumeBackdrop==null||background==null)return;
        float height=cumeCamera.orthographicSize*2;
        float width=height*cumeCamera.aspect;
        float scale=Mathf.Max(width/background.bounds.size.x,height/background.bounds.size.y);
        cumeBackdrop.transform.localScale=Vector3.one*scale;
        cumeBackdrop.transform.localPosition=new Vector3(0,0,20);
    }
    Camera volcanoCamera;
    SpriteRenderer volcanoBackdrop;
    // The supplied PNGs contain opaque mattes. Use only the interior stone pixels,
    // never scale the entire image (including its painted background) into a tile.
    void VolcanoVisuals()
    {
        artMaterial=new Material(Shader.Find("Sprites/Default"));
        volcanoCamera=Camera.main;
        volcanoBackdrop=Art("Background regional composto",background,Vector3.zero,22,-100);
        volcanoBackdrop.transform.SetParent(volcanoCamera.transform,false);
        volcanoBackdrop.transform.localPosition=new Vector3(0,0,20);
        var surface=Trim(topCenter,new Rect(40,18,184,180));
        var interior=Trim(fill,new Rect(38,18,180,180));
        var ledge=Trim(platformCenter,new Rect(36,131,192,34));
        foreach(var sr in FindObjectsByType<SpriteRenderer>()) {
            if(sr.gameObject.scene!=gameObject.scene)continue;
            // Remove obsolete construction marks, opaque sign blocks and arena walls.
            // The original objects, text, triggers and colliders remain in place.
            if(sr.name=="Junta da pedra clara" || sr.name=="Borda de pouso" || sr.name=="Limite da arena" || sr.name.StartsWith("Placa -"))sr.enabled=false;
            if(sr.gameObject.layer!=3 || sr.GetComponent<BoxCollider2D>()==null ||
               (!sr.name.Contains("Piso continuo") && !sr.name.Contains("Plataforma da arena")))continue;
            var bounds=sr.GetComponent<BoxCollider2D>().bounds;
            sr.enabled=false;DecoratedFloors++;
            bool suspended=bounds.max.y>0;
            const float cell=1.5f;
            int columns=Mathf.CeilToInt(bounds.size.x/cell);
            int rows=suspended?1:5;
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++) {
                float left=bounds.min.x+col*cell;
                float width=Mathf.Min(cell,bounds.max.x-left);
                float height=suspended?.265625f:cell;
                var sprite=suspended?ledge:row==0?surface:interior;
                // Clip the final cell instead of squeezing it to fit the floor length.
                if(width<cell-.001f) sprite=Trim(sprite,new Rect(sprite.rect.x,sprite.rect.y,sprite.rect.width*width/cell,sprite.rect.height));
                var tile=Art("Basalto continuo - visual F3",sprite,new Vector3(left+width*.5f,bounds.max.y-row*cell-height*.5f,0),width,1);
                tile.transform.localScale=new Vector3(width/sprite.bounds.size.x,height/sprite.bounds.size.y,1);
            }
        }
        // The composed backdrop already contains ruins. Omit matte-backed props
        // rather than introducing floating rectangular paintings into the level.
        bool arena=GetComponent<QuestCampaignScene>().arena;
        if(arena && crystal!=null) {
            // Do not add the opaque crystal PNG; the existing exit remains readable.
            foreach(var sr in FindObjectsByType<SpriteRenderer>())
                if(sr.gameObject.scene==gameObject.scene && sr.name=="Saida para o Castelo")sr.color=new Color(1f,.65f,.22f,.85f);
        }
        FitVolcanoBackground();
    }

    void FitVolcanoBackground()
    {
        if(volcanoBackdrop==null || background==null)return;
        float height=volcanoCamera.orthographicSize*2;
        float width=height*volcanoCamera.aspect;
        float scale=Mathf.Max(width/background.bounds.size.x,height/background.bounds.size.y);
        volcanoBackdrop.transform.localScale=Vector3.one*scale;
    }
    SpriteRenderer Art(string label,Sprite sprite,Vector3 position,float width,int order)
    {
        var obj=new GameObject(label,typeof(SpriteRenderer));obj.transform.SetParent(transform);obj.transform.position=position;
        var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=sprite;sr.sharedMaterial=(cume||volcanoCamera!=null)&&sprite!=null?MaterialForTexture(sprite.texture):artMaterial;sr.sortingOrder=order;
        if(sprite!=null)obj.transform.localScale=Vector3.one*(width/sprite.bounds.size.x);return sr;
    }
    Material MaterialForTexture(Texture texture)
    {
        // Keep the backdrop and every tile texture in separate material batches.
        // Match the URP sprite shader already used by the first region's terrain.
        if(textureMaterials.TryGetValue(texture,out var existing))return existing;
        var material=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        material.name=(cume?"Cume - ":"Vulcao - ")+texture.name;
        material.mainTexture=texture;
        textureMaterials.Add(texture,material);
        return material;
    }
    Sprite Trim(Sprite sprite,Rect rect){if(rect.width<=0)return sprite;var result=Sprite.Create(sprite.texture,rect,Vector2.one*.5f,64);trimmedSprites.Add(result);return result;}
    void Tile(Sprite sprite,Vector3 position,float height=1)
    {var sr=Art("Tile regional - visual",sprite,position,1,1);sr.transform.localScale=new Vector3(1/sprite.bounds.size.x,height/sprite.bounds.size.y,1);}
    void OnDestroy(){foreach(var sprite in trimmedSprites)if(sprite!=null)Destroy(sprite);foreach(var material in textureMaterials.Values)if(material!=null)Destroy(material);if(artMaterial!=null)Destroy(artMaterial);}
}



