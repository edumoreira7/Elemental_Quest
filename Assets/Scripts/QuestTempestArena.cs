using UnityEngine;

[DefaultExecutionOrder(-100)]
public class QuestTempestArena : MonoBehaviour
{
    public GameObject playerPrefab;
    public Sprite[] idle, prepareGust, castGust, prepareDive, dive, landing, vulnerable, hurt, freed;
    public Sprite[] gustBirth, gustFlight, gustEnd, dust, liberation;
    public enum State { Introduction, GustWarning, Gust, DiveWarning, Dive, Landing, Vulnerable, Hurt, Freed }
    public State Current { get; private set; }
    public int Health { get; private set; } = 6;
    public Vector2 DiveTarget { get; private set; }
    public bool Leaving { get; private set; }
    Transform player, boss;
    SpriteRenderer body, marker, exit;
    BoxCollider2D bossCollider;
    Camera cameraView;
    QuestCampaignScene campaign;
    QuestTempestGust gust;
    Sprite square;
    Material material;
    float since, deadline;
    bool launched, rewardAttempted;
    Vector3 diveStart;
    Vector3 liftStart;
    float direction;
    public const float Floor=-2f;
    public bool CombatActive => !Leaving && Current!=State.Freed;
    bool Paused => QuestSession.Atual!=null && QuestSession.Atual.Pausado;
    void Awake()
    {
        campaign=GetComponent<QuestCampaignScene>();campaign.enabled=false;
        square=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one*.5f,Texture2D.whiteTexture.width);
        material=new Material(Shader.Find("Sprites/Default"));
        var session=gameObject.AddComponent<QuestSession>();session.fase=2;session.nomeFase="Arena do Cume";session.avisarColetaCompleta=false;
        var cam=new GameObject("Camera fixa",typeof(Camera),typeof(AudioListener));cam.tag="MainCamera";
        cameraView=cam.GetComponent<Camera>();cameraView.orthographic=true;cameraView.orthographicSize=5.5f;cameraView.backgroundColor=new Color(.28f,.45f,.59f);FixCamera();
        var floor=Shape("Plataforma da arena",new Vector2(0,Floor-.5f),new Vector2(18,1),new Color(.66f,.74f,.76f));floor.gameObject.layer=3;floor.gameObject.AddComponent<BoxCollider2D>();
        foreach(float x in new[]{-9.3f,9.3f}) {var wall=Shape("Limite da arena",new Vector2(x,1),new Vector2(.6f,7),new Color(.45f,.6f,.68f));wall.gameObject.layer=3;wall.gameObject.AddComponent<BoxCollider2D>();}
        var scaleRoot=new GameObject("Escala do pato - referencia da fase 1").transform;scaleRoot.SetParent(transform,false);scaleRoot.localScale=Vector3.one*.72f;
        var obj=Instantiate(playerPrefab,new Vector3(-5,-1.1f,0),Quaternion.identity,scaleRoot);player=obj.transform;
        var health=obj.GetComponent<QuestPlayer>();if(health==null)health=obj.AddComponent<QuestPlayer>();health.AtivarCheckpoint(player.position);
        body=Shape("Gra-Coruja das Tempestades",new Vector2(5,Floor),Vector2.one,Color.white);boss=body.transform;body.enabled=false;
        var art=new GameObject("Visual da Coruja",typeof(SpriteRenderer));art.transform.SetParent(boss,false);
        body=art.GetComponent<SpriteRenderer>();body.sharedMaterial=material;body.sortingOrder=6;body.sprite=idle[0];art.AddComponent<QuestBossSpriteMaterial>();
        bossCollider=boss.gameObject.AddComponent<BoxCollider2D>();bossCollider.isTrigger=true;bossCollider.size=new Vector2(1.5f,1.8f);bossCollider.offset=new Vector2(0,.9f);
        boss.gameObject.AddComponent<QuestTempestHit>().arena=this;
        marker=Shape("Posicao marcada - mergulho",new Vector2(0,Floor+.05f),new Vector2(2.4f,.1f),new Color(1,.65f,.18f));marker.enabled=false;
        exit=Shape("Saida para o Vulcao",new Vector2(8,-.7f),new Vector2(.7f,2.6f),new Color(.55f,.95f,1));exit.gameObject.SetActive(false);
        exit.gameObject.AddComponent<BoxCollider2D>().isTrigger=true;exit.gameObject.AddComponent<QuestTempestExit>().arena=this;
        bool seen=QuestCampaign.HasEvent("C03.Intro");QuestCampaign.MarkEvent("C03.Intro");Set(State.Introduction,seen?.35f:1.5f);
    }
    SpriteRenderer Shape(string label,Vector2 position,Vector2 size,Color color)
    {
        var obj=new GameObject(label,typeof(SpriteRenderer));obj.transform.SetParent(transform);obj.transform.position=position;obj.transform.localScale=Vector3.one*1f;obj.transform.localScale=new Vector3(size.x,size.y,1);
        var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=square;sr.sharedMaterial=material;sr.color=color;obj.AddComponent<QuestBossSpriteMaterial>();return sr;
    }
    void FixCamera()
    {
        cameraView.transform.position=new Vector3(0,.5f,-10);float actual=Screen.width/(float)Mathf.Max(1,Screen.height),target=16f/9;
        cameraView.rect=actual>target?new Rect((1-target/actual)/2,0,target/actual,1):new Rect(0,(1-actual/target)/2,1,actual/target);
    }
    void LateUpdate(){FixCamera();}
    void Set(State state,float duration)
    {
        Current=state;since=Time.time;deadline=since+duration;launched=false;
        marker.enabled=state==State.DiveWarning;
        if(state==State.GustWarning)direction=player.position.x<boss.position.x?-1:1;
        if(state==State.DiveWarning) {
            DiveTarget=new Vector2(Mathf.Clamp(player.position.x,-6.8f,6.8f),Floor);
            marker.transform.position=new Vector3(DiveTarget.x,Floor+.05f,0);
            liftStart=boss.position;direction=DiveTarget.x<boss.position.x?-1:1;
        }
        if(state==State.Dive)diveStart=boss.position;
        body.flipX=direction<0;
    }
    void Update()
    {
        if(Leaving||Paused)return;
        float t=Time.time-since;
        Sprite[] frames=idle;bool loop=false;
        switch(Current) {
            case State.GustWarning:frames=prepareGust;break;
            case State.Gust:frames=castGust;break;
            case State.DiveWarning:frames=prepareDive;break;
            case State.Dive:frames=dive;break;
            case State.Landing:frames=landing;break;
            case State.Vulnerable:frames=vulnerable;loop=true;break;
            case State.Hurt:frames=hurt;break;
            case State.Freed:frames=freed;break;
            default:loop=true;break;
        }
        int frame=loop?(int)(t*6)%frames.Length:Mathf.Min(frames.Length-1,(int)(Mathf.Clamp01(t/Mathf.Max(.01f,deadline-since))*frames.Length));body.sprite=frames[frame];
        // Keep physics on a feet anchor; compensate the centered flight sprite separately.
        body.transform.localPosition=Current==State.Dive?Vector3.up*0.70714286f:Vector3.zero;
        bossCollider.offset=new Vector2(0,.9f);
        if(Current==State.DiveWarning)boss.position=Vector3.Lerp(liftStart,new Vector3(liftStart.x,1.8f,0),Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.45f)));
        if(Current==State.Gust && !launched && t>=.3f){launched=true;SpawnGust();}
        if(Current==State.Dive) {
            boss.position=Vector3.Lerp(diveStart,new Vector3(DiveTarget.x,Floor,0),Mathf.Clamp01(t/.65f));
            DamageBox((Vector2)boss.position+Vector2.up*.9f,new Vector2(1.5f,1.2f));
        }
        if(Current==State.Freed) {
            if(Time.time>=deadline&&!rewardAttempted){rewardAttempted=true;campaign.NotifyGuardianDefeated();campaign.CompletePresentation(() => {exit.gameObject.SetActive(QuestCampaign.HasCrystal(1));if(!exit.gameObject.activeSelf)QuestSession.Atual.Avisar("Arena isolada: complete a floresta para recuperar Ar na campanha.");});}
            return;
        }
        if(Time.time<deadline)return;
        switch(Current) {
            case State.Introduction:Set(State.GustWarning,1f);break;
            case State.GustWarning:Set(State.Gust,3.2f);break;
            case State.Gust:if(gust==null)Set(State.DiveWarning,1.1f);break;
            case State.DiveWarning:Set(State.Dive,.65f);break;
            case State.Dive:
                boss.position=new Vector3(DiveTarget.x,Floor,0);Fx(dust,boss.position,.34f);Set(State.Landing,.6f);break;
            case State.Landing:Set(State.Vulnerable,2.4f);break;
            case State.Vulnerable:case State.Hurt:Set(State.GustWarning,1f);break;
        }
    }
    void SpawnGust()
    {
        Vector3 position=new Vector3(boss.position.x+direction*1.2f,Floor+.35f,0);
        Fx(gustBirth,position,.4f);
        var obj=new GameObject("Rajada horizontal",typeof(SpriteRenderer));obj.transform.position=position;obj.transform.localScale=Vector3.one*1f;
        var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=gustFlight[0];sr.sharedMaterial=material;sr.sortingOrder=7;sr.flipX=direction<0;
        obj.AddComponent<QuestBossSpriteMaterial>();gust=obj.AddComponent<QuestTempestGust>();gust.arena=this;gust.direction=direction;gust.frames=gustFlight;
    }
    public void DamageBox(Vector2 center,Vector2 size)
    {foreach(var c in Physics2D.OverlapBoxAll(center,size,0)){var health=c.GetComponent<QuestPlayer>();if(health!=null)health.Ferir();}}
    public void Fx(Sprite[] frames,Vector3 position,float duration)
    {
        var obj=new GameObject("Efeito da Coruja",typeof(SpriteRenderer));obj.transform.position=position;if(frames==gustBirth||frames==gustEnd)obj.transform.localScale=Vector3.one*1f;
        var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=frames[0];sr.sharedMaterial=material;sr.sortingOrder=8;sr.flipX=direction<0;
        obj.AddComponent<QuestBossSpriteMaterial>();var animation=obj.AddComponent<QuestAnimation>();animation.quadros=frames;animation.velocidade=frames.Length/duration;Destroy(obj,duration);
    }
    public void Receive(Collider2D other)
    {
        var projectile=other.GetComponent<ProjetilPoder>();if(projectile==null||Current==State.Freed||Leaving)return;
        Destroy(projectile.gameObject);if(Current!=State.Vulnerable)return;
        Health=Mathf.Max(0,Health-1);
        if(Health>0)Set(State.Hurt,.3f);
        else {if(gust!=null)Destroy(gust.gameObject);bossCollider.enabled=false;Set(State.Freed,1.2f);Fx(liberation,boss.position+Vector3.up, .6f);}
    }
    public void Leave()
    {if(Leaving||Current!=State.Freed||!exit.gameObject.activeSelf||!QuestCampaign.HasCrystal(1))return;Leaving=true;QuestCampaign.LeaveArena(1);}
    void OnGUI()
    {
        string hint=Current==State.GustWarning?"RAJADA: prepare o pulo":Current==State.DiveWarning?"MERGULHO: saia da marca no chao":Current==State.Vulnerable?"Guardia no chao: ataque com magia":Current==State.Freed?"Guardia liberta - Cristal do Ar":"Esquive e espere a recuperacao";
        QuestHudArt.BossBar("GRA-CORUJA DAS TEMPESTADES",Health,6,hint);
    }
    void OnDestroy(){if(gust!=null)Destroy(gust.gameObject);if(material!=null)Destroy(material);if(square!=null)Destroy(square);}
}

public class QuestTempestHit : MonoBehaviour
{public QuestTempestArena arena;void OnTriggerEnter2D(Collider2D other){arena.Receive(other);}}
public class QuestTempestExit : MonoBehaviour
{public QuestTempestArena arena;void OnTriggerEnter2D(Collider2D other){if(other.GetComponent<QuestPlayer>()!=null)arena.Leave();}}
public class QuestTempestGust : MonoBehaviour
{
    public QuestTempestArena arena;public float direction;public Sprite[] frames;
    float started;
    void Start(){started=Time.time;GetComponent<SpriteRenderer>().enabled=false;}
    void FixedUpdate()
    {
        if(arena==null||!arena.CombatActive){Destroy(gameObject);return;}
        if(QuestSession.Atual!=null&&QuestSession.Atual.Pausado)return;
        // Birth is visual only; moving/damaging flight begins after 0.4s.
        if(Time.time-started<.4f)return;
        GetComponent<SpriteRenderer>().enabled=true;
        GetComponent<SpriteRenderer>().sprite=frames[(int)((Time.time-started)*12)%frames.Length];
        float step=6*Time.fixedDeltaTime;
        var cast=Physics2D.BoxCast(transform.position,new Vector2(.8f,.45f),0,Vector2.right*direction,step,1<<3);
        if(cast.collider!=null||Mathf.Abs(transform.position.x)>8.7f||Time.time-started>3.5f){arena.Fx(arena.gustEnd,transform.position,.34f);Destroy(gameObject);return;}
        arena.DamageBox((Vector2)transform.position+Vector2.right*direction*step/2,new Vector2(.8f+step,.45f));
        transform.position+=Vector3.right*direction*step;
    }
}


