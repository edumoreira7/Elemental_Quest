using System;
using System.Collections.Generic;
using UnityEngine;

// Visual tiles only: the existing QuestElementTarget owns damage and freezing.
public sealed class QuestAnimatedLiquid : MonoBehaviour
{
    sealed class Sequence
    {
        public Sprite[] frames;
        public Material material;
        public readonly List<SpriteRenderer> tiles=new List<SpriteRenderer>();
    }
    readonly Sequence[] sequences=new Sequence[4];
    QuestElementTarget target;
    int previousFrame=-1;
    bool previousFrozen;
    public int CurrentFrame {get;private set;}
    public int Columns {get;private set;}
    public int Rows {get;private set;}
    public int TileCount {get;private set;}
    public bool Ready {get;private set;}
    public const float FramesPerSecond=8;
    void Start()
    {
        target=GetComponent<QuestElementTarget>();
        string folder=target.tipo==QuestElementTarget.Tipo.Lava?"Lava":"Water";
        string[] types={"Surface","Fill","Surface_Left","Surface_Right"};
        // Validate the entire set before replacing any existing drawing.
        for(int i=0;i<types.Length;i++){
            var frames=Resources.LoadAll<Sprite>("AnimatedLiquids/"+folder+"/"+types[i]);
            Array.Sort(frames,(a,b)=>string.CompareOrdinal(a.name,b.name));
            if(frames.Length!=8){Debug.LogError("Liquid sequence missing: "+folder+"/"+types[i],this);return;}
            sequences[i]=new Sequence{frames=frames};
        }
        foreach(var renderer in GetComponentsInChildren<Renderer>())renderer.enabled=false;
        var bounds=GetComponent<BoxCollider2D>().bounds;
        Columns=Mathf.Max(1,Mathf.CeilToInt(bounds.size.x/.5f));
        // Uniform square cells fit fractional pool widths without stretched end caps.
        float cell=bounds.size.x/Columns;
        float bottom=Camera.main==null?bounds.max.y-3:Camera.main.transform.position.y-Camera.main.orthographicSize-.5f;
        Rows=Mathf.Max(2,Mathf.CeilToInt(Mathf.Max(3,bounds.max.y-bottom)/cell));
        for(int i=0;i<sequences.Length;i++){
            sequences[i].material=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            sequences[i].material.name=folder+" - "+types[i]+" - animado";
        }
        for(int row=0;row<Rows;row++)for(int column=0;column<Columns;column++){
            int type=row>0?1:Columns==1?0:column==0?2:column==Columns-1?3:0;
            var sequence=sequences[type];
            var tile=new GameObject(folder+" animado - "+row+":"+column,typeof(SpriteRenderer));
            tile.transform.position=new Vector3(bounds.min.x+(column+.5f)*cell,bounds.max.y-(row+.5f)*cell,0);
            tile.transform.localScale=Vector3.one*cell;
            tile.transform.SetParent(transform,true);
            var drawing=tile.GetComponent<SpriteRenderer>();drawing.sharedMaterial=sequence.material;
            drawing.sortingOrder=row==0?2:0;sequence.tiles.Add(drawing);TileCount++;
        }
        Ready=true;previousFrozen=!target.Congelado;Update();
    }
    void Update()
    {
        if(!Ready)return;
        CurrentFrame=Mathf.FloorToInt(Time.time*FramesPerSecond)%8;
        if(CurrentFrame!=previousFrame){
            foreach(var sequence in sequences){
                var sprite=sequence.frames[CurrentFrame];sequence.material.mainTexture=sprite.texture;
                foreach(var tile in sequence.tiles)tile.sprite=sprite;
            }
            previousFrame=CurrentFrame;
        }
        if(previousFrozen!=target.Congelado){
            // Ice uses order 1. Keep the animated surface underneath while frozen.
            for(int i=0;i<sequences.Length;i++)if(i!=1)
                foreach(var tile in sequences[i].tiles)tile.sortingOrder=target.Congelado?0:2;
            previousFrozen=target.Congelado;
        }
    }
    void OnDestroy(){foreach(var sequence in sequences)if(sequence!=null&&sequence.material!=null)Destroy(sequence.material);}
}
