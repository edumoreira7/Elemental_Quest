using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuestLiquidsReview
{
    const string Output="C:/Users/morei/Documents/Jogos Digitais/LiquidsIntegration/";
    static int scene,step,firstFrame;
    static double next;
    static Bounds initialBounds;
    static Vector2 initialSize;
    static QuestAnimatedLiquid selected;
    static QuestElementTarget target;
    static readonly StringBuilder report=new StringBuilder();
    [MenuItem("Tools/Elemental Quest/Conferir agua e lava animadas")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)return;
        scene=0;step=0;report.Clear();File.WriteAllText(Output+"play-review.txt","RUNNING");
        EditorSceneManager.OpenScene("Assets/Scenes/FasePrincipal.unity");
        next=EditorApplication.timeSinceStartup+5;
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        try {
            string prefix=scene==0?"agua":"lava";
            next=EditorApplication.timeSinceStartup+.4;
            if(step==0){
                var liquids=UnityEngine.Object.FindObjectsByType<QuestAnimatedLiquid>();
                if(liquids.Length!=2)throw new Exception("Expected two "+prefix+" surfaces");
                Array.Sort(liquids,(a,b)=>a.transform.position.x.CompareTo(b.transform.position.x));selected=liquids[0];
                foreach(var liquid in liquids){
                    if(!liquid.Ready||liquid.TileCount!=liquid.Rows*liquid.Columns||liquid.CurrentFrame!=selected.CurrentFrame)throw new Exception("Liquid tiles missing or out of sync");
                    foreach(var drawing in liquid.GetComponentsInChildren<SpriteRenderer>())
                        if(drawing.name.Contains(" animado - ")&&(drawing.sharedMaterial.mainTexture!=drawing.sprite.texture||drawing.GetComponent<Collider2D>()!=null))throw new Exception("Liquid material/collider mismatch");
                }
                target=selected.GetComponent<QuestElementTarget>();var collider=target.GetComponent<BoxCollider2D>();initialBounds=collider.bounds;initialSize=collider.size;
                firstFrame=selected.CurrentFrame;
                if(Camera.main.GetComponent<QuestCamera>()!=null)Camera.main.GetComponent<QuestCamera>().enabled=false;
                var p=Camera.main.transform.position;p.x=target.transform.position.x;Camera.main.transform.position=p;
                step++;return;
            }
            if(step==1){Capture(prefix+"-animado");step++;return;}
            if(step==2){
                if(selected.CurrentFrame==firstFrame)throw new Exception("Liquid animation did not advance");
                if(!target.Aplicar(QuestElementPowers.Elemento.Gelo))throw new Exception("Freeze rejected");step++;return;
            }
            if(step==3){
                var collider=target.GetComponent<BoxCollider2D>();
                if(!target.Congelado||collider.isTrigger||target.gameObject.layer!=3||collider.size!=initialSize||collider.bounds!=initialBounds)throw new Exception("Freeze mechanics changed");
                foreach(var drawing in selected.GetComponentsInChildren<SpriteRenderer>())if(drawing.name.Contains(" animado - 0:")&&drawing.sortingOrder>=1)throw new Exception("Liquid covers the ice bridge");
                Capture(prefix+"-congelado");step++;next=EditorApplication.timeSinceStartup+8.1;return;
            }
            if(step==4){
                var collider=target.GetComponent<BoxCollider2D>();
                if(target.Congelado||!collider.isTrigger||target.gameObject.layer!=0||collider.size!=initialSize||collider.bounds!=initialBounds)throw new Exception("Thaw mechanics changed");
                foreach(var drawing in selected.GetComponentsInChildren<SpriteRenderer>())if(drawing.name.Contains(" animado - 0:")&&drawing.sortingOrder!=2)throw new Exception("Liquid surface did not return");
                Capture(prefix+"-descongelado");report.AppendLine(prefix+": two animated surfaces; synchronized frames and matching textures; no visual colliders; freeze and 8-second thaw preserve original collider bounds.");step++;return;
            }
            if(scene==0){scene=1;step=0;SceneManager.LoadScene("VulcaoCinzento");next=EditorApplication.timeSinceStartup+4;}
            else{report.AppendLine("PASS. Visual and freeze/thaw integration checked in Play; no complete traversal.");File.WriteAllText(Output+"play-review.txt",report.ToString());EditorApplication.update-=Tick;EditorApplication.isPlaying=false;}
        }catch(Exception ex){report.AppendLine(ex.ToString());File.WriteAllText(Output+"play-review.txt",report.ToString());Debug.LogException(ex);EditorApplication.update-=Tick;EditorApplication.isPlaying=false;}
    }
    static void Capture(string name){ScreenCapture.CaptureScreenshot(Output+name+".png");}
}
