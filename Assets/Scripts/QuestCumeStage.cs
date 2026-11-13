using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Local assembly for CumeDosVentos only; does not alter the forest scene or prefabs.
[DefaultExecutionOrder(-100)]
public class QuestCumeStage : MonoBehaviour
{
    public GameObject playerPrefab, meleePrefab, rangedPrefab, heavyPrefab;
    public Sprite terrainTop, terrainFill;
    public Sprite skyBackground, mountainsBackground, ruinsBackground;
    static readonly Vector2[] FragmentPositions = {
new Vector2(5.0f,-0.9f),new Vector2(9.0f,-0.9f),new Vector2(14.0f,1.0f),new Vector2(17.0f,1.4f),new Vector2(21.0f,0.5f),new Vector2(26.0f,0.5f),new Vector2(29.0f,0.5f),new Vector2(33.0f,0.8f),new Vector2(37.0f,0.1f),new Vector2(42.0f,-0.9f),new Vector2(48.0f,-0.9f),new Vector2(56.0f,-0.9f),new Vector2(58.0f,1.9f),new Vector2(60.0f,1.9f),new Vector2(65.0f,-0.9f),new Vector2(71.0f,0.9f),new Vector2(75.0f,1.1f),new Vector2(81.0f,0.4f),new Vector2(86.0f,0.4f),new Vector2(101.0f,-0.9f)
    };
    public Transform Player { get; private set; }
    public int PlatformCount { get; private set; }
    public int FragmentCount { get; private set; }
    public readonly List<QuestEnemy> Enemies = new List<QuestEnemy>();
    readonly List<Vector2> zones = new List<Vector2>();
    Camera view;
    QuestPlayer health;
    Sprite square;
    Material unlit;
    bool leaving;
    string message;
    float messageUntil;
    static Vector3 checkpoint;
    static bool resume;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetProgress() { resume=false;checkpoint=Vector3.zero; }
    public static void ReturnToPreparation()
    {
        resume=true;checkpoint=new Vector3(100,-1.1f,0);
        Time.timeScale=1;SceneManager.LoadScene("CumeDosVentos");
    }
    void Awake()
    {
        var shell=GetComponent<QuestCampaignScene>();if(shell!=null)shell.enabled=false;
        square=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),new Vector2(.5f,.5f),Texture2D.whiteTexture.width);
        unlit=new Material(Shader.Find("Sprites/Default"));
        var session=gameObject.AddComponent<QuestSession>();
        session.fase=2;session.nomeFase="Cume dos Ventos Partidos";session.avisarColetaCompleta=false;
        var cam=new GameObject("Camera do Cume",typeof(Camera),typeof(AudioListener));cam.tag="MainCamera";
        view=cam.GetComponent<Camera>();view.orthographic=true;view.orthographicSize=5.5f;
        view.backgroundColor=new Color(.32f,.53f,.67f);cam.transform.position=new Vector3(5,.5f,-10);
        Terrain();
        var boundary=Block("Limite esquerdo",new Vector2(-.6f,0),new Vector2(1,12),Color.clear);boundary.layer=3;boundary.AddComponent<BoxCollider2D>();
        var scaleRoot=new GameObject("Escala do pato - referencia da fase 1").transform;scaleRoot.SetParent(transform,false);scaleRoot.localScale=Vector3.one*.72f;
        var player=Instantiate(playerPrefab,new Vector3(2,-1.1f,0),Quaternion.identity,scaleRoot);
        player.name="Quackerius - Cume";Player=player.transform;
        player.GetComponent<QuestElementPowers>().Selecionar(QuestElementPowers.Elemento.Ar);
        health=player.GetComponent<QuestPlayer>();if(health==null)health=player.AddComponent<QuestPlayer>();
        health.AtivarCheckpoint(resume?checkpoint:Player.position);
        if(resume){Player.position=checkpoint;player.GetComponent<Rigidbody2D>().position=checkpoint;}
        var follow=cam.AddComponent<QuestCamera>();follow.alvo=Player;follow.minimoX=5;follow.maximoX=104;follow.antecipacao=3.8f;
        Enemy(meleePrefab,26,23,29,19,31);
        Enemy(rangedPrefab,48,48,48,34,53);
        Enemy(heavyPrefab,65,61,67,55,70);
        // Short combination follows three isolated archetypes.
        Enemy(meleePrefab,80,78,82,74,89);Enemy(rangedPrefab,87,87,87,73,97);
        Checkpoint(21);Checkpoint(56);Checkpoint(100);
        Sign(3,"CUME DOS VENTOS\nLiberte a guardia do cume.");
        Sign(9,"2: AR | Shift: correr | Espaco: salto extra\nSegure J ao descer para planar ate o pouso.");
        Sign(22,"Ruinas claras indicam piso seguro.\nJ: magia. Cada inimigo deixa espaco para recuar.");
        Sign(56,"Respiro: recupere mana aqui.\n1: Terra cria apoio temporario para rotas altas.");
        Sign(66,"TRAVESSIA LONGA\nCorrida + pulo duplo. Plane na descida.");
        Sign(97,"CHECKPOINT - PREPARACAO\nSiga para a arena. Fragmentos sao opcionais.");
        foreach(var point in FragmentPositions) Fragment(point.x,point.y);
        session.totalItens=FragmentCount;
        var gate=Block("Entrada da ArenaCume",new Vector2(107,-.5f),new Vector2(.8f,3),new Color(.65f,.88f,1));
        var trigger=gate.AddComponent<BoxCollider2D>();trigger.isTrigger=true;
        gate.AddComponent<QuestCumeGate>().stage=this;
        message="Cume dos Ventos Partidos - os pousos claros mostram o caminho.";messageUntil=Time.unscaledTime+6;
    }
    void Terrain()
    {
        // Each ledge has a single continuous collider, avoiding tile seams.
        foreach(var floor in QuestStageLayout.Floors(2))Platform(floor.x,floor.y,floor.z);
        foreach(float x in new[]{5f,20,40,60,78,101}) {
            var ruin=Block("Coluna de ruina clara",new Vector2(x,-.5f),new Vector2(.65f,3),new Color(.65f,.73f,.72f));ruin.GetComponent<SpriteRenderer>().sortingOrder=-3;
            var cap=Block("Capitel",new Vector2(x,1),new Vector2(1.2f,.3f),new Color(.79f,.83f,.78f));cap.GetComponent<SpriteRenderer>().sortingOrder=-3;
        }
    }
    void BackgroundLayer(Sprite art,string label,float factor,int order,float width)
    {
        if(art==null)return;
        var root=new GameObject(label);root.transform.SetParent(transform);
        var panels=new Transform[3];
        for(int i=0;i<3;i++) {
            var go=new GameObject(label+" "+i,typeof(SpriteRenderer));go.transform.SetParent(root.transform);
            go.transform.position=new Vector3(i*width,.5f,5);
            go.transform.localScale=Vector3.one*(width/art.bounds.size.x);
            var sr=go.GetComponent<SpriteRenderer>();sr.sprite=art;sr.sharedMaterial=unlit;sr.sortingOrder=order;
            panels[i]=go.transform;
        }
        var parallax=root.AddComponent<QuestBackground>();parallax.cameraAlvo=view.transform;parallax.paineis=panels;parallax.fator=factor;
    }
    GameObject Block(string name,Vector2 center,Vector2 size,Color tint)
    {
        var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(transform);
        go.transform.position=center;go.transform.localScale=new Vector3(size.x,size.y,1);
        var sr=go.GetComponent<SpriteRenderer>();sr.sprite=square;sr.sharedMaterial=unlit;sr.color=tint;return go;
    }
    void Platform(float left,float right,float top)
    {
        var go=Block("Piso continuo - pouso claro",new Vector2((left+right)/2,top-.55f),new Vector2(right-left,1.1f),new Color(.57f,.64f,.65f));
        go.layer=3;go.AddComponent<BoxCollider2D>();PlatformCount++;
        for(float x=left+1;x<right;x+=2) {
            var joint=Block("Junta da pedra clara",new Vector2(x,top-.55f),new Vector2(.035f,.9f),new Color(.43f,.53f,.58f));
            joint.GetComponent<SpriteRenderer>().sortingOrder=1;
        }
        var edge=Block("Borda de pouso",new Vector2((left+right)/2,top-.04f),new Vector2(right-left,.08f),new Color(.94f,.95f,.8f));edge.GetComponent<SpriteRenderer>().sortingOrder=2;
    }
    void Enemy(GameObject prefab,float x,float left,float right,float zoneStart,float zoneEnd)
    {
        var go=Instantiate(prefab,new Vector3(x,-1.4f,0),Quaternion.identity);go.name="Encontro do Cume - "+prefab.name;
        var enemy=go.GetComponent<QuestEnemy>();enemy.limiteEsquerdo=left;enemy.limiteDireito=right;enemy.combatePermitido=false;
        if(enemy.tipo==QuestEnemy.Tipo.Atirador){enemy.atiradorFixo=true;enemy.direcaoFixa=-1;enemy.cadenciaTiro=2.4f;}
        // Ground by collider bounds rather than assuming all archetypes have the same pivot.
        var bounds=go.GetComponent<Collider2D>().bounds;go.transform.position+=Vector3.up*(QuestStageLayout.Ground(2,x)-bounds.min.y+.03f);
        Enemies.Add(enemy);zones.Add(new Vector2(zoneStart,zoneEnd));
    }
    void Checkpoint(float x)
    {
        var go=Block("Checkpoint - respiro",new Vector2(x,QuestStageLayout.Ground(2,x)+.75f),new Vector2(.35f,1.5f),new Color(.4f,1,.75f));
        var box=go.AddComponent<BoxCollider2D>();box.isTrigger=true;
        go.AddComponent<QuestCheckpoint>().destino=new Vector3(x,QuestStageLayout.Ground(2,x)+.9f,0);
    }
    void Fragment(float x,float y)
    {
        // Keep fragments over safe floors, except the explicitly signposted Air gaps.
        var go=Block("Fragmento local - orientacao",new Vector2(x,y),new Vector2(.22f,.32f),new Color(.6f,1,1));
        go.transform.rotation=Quaternion.Euler(0,0,45);go.GetComponent<SpriteRenderer>().sortingOrder=4;
        var collider=go.AddComponent<BoxCollider2D>();collider.isTrigger=true;go.AddComponent<QuestCrystal>();FragmentCount++;
    }
    void Sign(float x,string text)
    {
        var go=Block("Placa - "+text.Split('\n')[0],new Vector2(x,-.8f),new Vector2(.8f,.65f),new Color(.21f,.32f,.37f));
        var label=new GameObject("Texto da placa",typeof(TextMesh));label.transform.SetParent(transform);label.transform.position=new Vector3(x,QuestStageLayout.Ground(2,x)+2.25f,-.2f);
        var tm=label.GetComponent<TextMesh>();tm.text=text;tm.fontSize=38;tm.characterSize=.065f;tm.anchor=TextAnchor.MiddleCenter;tm.color=Color.white;
        label.GetComponent<MeshRenderer>().sortingOrder=5;
    }
    void Update()
    {
        if(Player==null||leaving)return;
        for(int i=0;i<Enemies.Count;i++) {
            var enemy=Enemies[i];if(enemy==null)continue;
            var screen=view.WorldToViewportPoint(enemy.transform.position);
            enemy.combatePermitido=Player.position.x>=zones[i].x&&Player.position.x<zones[i].y&&screen.x>.06f&&screen.x<.94f&&screen.z>0;
        }
        // Scene-local checkpoint state never shares the forest's static destination.
        if(health.pontoSeguro.x>2){checkpoint=health.pontoSeguro;resume=true;}
    }
    public void EnterArena()
    {
        if(leaving||QuestSession.Atual.Pausado)return;
        leaving=true;resume=true;checkpoint=new Vector3(100,-1.1f,0);
        Time.timeScale=1;SceneManager.LoadScene("ArenaCume");
    }
    void OnGUI()
    {
        if(Time.unscaledTime<messageUntil)GUI.Box(new Rect(20,175,Mathf.Min(Screen.width-40,760),40),message);
    }
    void OnDestroy(){if(unlit!=null)Destroy(unlit);if(square!=null)Destroy(square);}
}

