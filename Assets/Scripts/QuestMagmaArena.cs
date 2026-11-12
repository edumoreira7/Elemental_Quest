using UnityEngine;

[DefaultExecutionOrder(-100)]
public class QuestMagmaArena : MonoBehaviour
{
    public GameObject playerPrefab;
    public Sprite[] walk;
    public Sprite[] idle, prepareGust, castGust, prepareDive, dive, landing, vulnerable, hurt, freed;
    public Sprite[] gustBirth, gustFlight, gustEnd, dust, liberation;
    public enum State { Introduction, SpitWarning, Spit, AdvanceWarning, Advance, Landing, Vulnerable, Hurt, Freed }
    public State Current { get; private set; }
    public int Health { get; private set; } = 6;
    public Vector2 AdvanceTarget { get; private set; }
    public bool Leaving { get; private set; }
    Transform player, boss;
    SpriteRenderer body, marker, exit;
    BoxCollider2D bossCollider;
    Camera cameraView;
    QuestCampaignScene campaign;
    QuestMagmaGust gust;
    Sprite square;
    Material material;
    float since, deadline;
    bool launched, rewardAttempted;
    Vector3 advanceStart;
    float direction;
    Vector2 magmaTarget;
    public const float Floor=-2f;
    public bool CombatActive => !Leaving && Current!=State.Freed;
    bool Paused => QuestSession.Atual!=null && QuestSession.Atual.Pausado;
    void Awake()
    {
        campaign=GetComponent<QuestCampaignScene>();campaign.enabled=false;
        square=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one*.5f,Texture2D.whiteTexture.width);
        material=new Material(Shader.Find("Sprites/Default"));
        var session=gameObject.AddComponent<QuestSession>();session.fase=3;session.nomeFase="Arena do Vulcao";session.avisarColetaCompleta=false;
        var cam=new GameObject("Camera fixa",typeof(Camera),typeof(AudioListener));cam.tag="MainCamera";
        cameraView=cam.GetComponent<Camera>();cameraView.orthographic=true;cameraView.orthographicSize=5.5f;cameraView.backgroundColor=new Color(.18f,.13f,.16f);FixCamera();
        var floor=Shape("Plataforma da arena",new Vector2(0,Floor-.5f),new Vector2(18,1),new Color(.42f,.43f,.45f));floor.gameObject.layer=3;floor.gameObject.AddComponent<BoxCollider2D>();
        foreach(float x in new[]{-9.3f,9.3f}) {var wall=Shape("Limite da arena",new Vector2(x,1),new Vector2(.6f,7),new Color(.45f,.6f,.68f));wall.gameObject.layer=3;wall.gameObject.AddComponent<BoxCollider2D>();}
        var obj=Instantiate(playerPrefab,new Vector3(-5,-1.1f,0),Quaternion.identity);player=obj.transform;
        var health=obj.GetComponent<QuestPlayer>();if(health==null)health=obj.AddComponent<QuestPlayer>();health.AtivarCheckpoint(player.position);
        body=Shape("Salamandra de Magma",new Vector2(5,Floor),Vector2.one,Color.white);boss=body.transform;body.sortingOrder=6;body.sprite=idle[0];
        bossCollider=body.gameObject.AddComponent<BoxCollider2D>();bossCollider.isTrigger=true;bossCollider.size=new Vector2(2.6f,1.2f);bossCollider.offset=new Vector2(0,.6f);
        body.gameObject.AddComponent<QuestMagmaHit>().arena=this;
        marker=Shape("Aviso de magma ou avanco",new Vector2(0,Floor+.05f),new Vector2(2.4f,.1f),new Color(1,.65f,.18f));marker.enabled=false;
        exit=Shape("Saida para o Castelo",new Vector2(8,-.7f),new Vector2(.7f,2.6f),new Color(.55f,.95f,1));exit.gameObject.SetActive(false);
        exit.gameObject.AddComponent<BoxCollider2D>().isTrigger=true;exit.gameObject.AddComponent<QuestMagmaExit>().arena=this;
        bool seen=QuestCampaign.HasEvent("C04.Intro");QuestCampaign.MarkEvent("C04.Intro");Set(State.Introduction,seen?.35f:1.5f);
    }
    SpriteRenderer Shape(string label,Vector2 position,Vector2 size,Color color)
    {
        var obj=new GameObject(label,typeof(SpriteRenderer));obj.transform.SetParent(transform);obj.transform.position=position;obj.transform.localScale=new Vector3(size.x,size.y,1);
        var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=square;sr.sharedMaterial=material;sr.color=color;return sr;
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
        marker.enabled=state==State.AdvanceWarning || state==State.SpitWarning;
        if(state==State.SpitWarning) {
            direction=player.position.x<boss.position.x?-1:1;
            magmaTarget=new Vector2(Mathf.Clamp(player.position.x,-7,7),Floor+.3f);
            marker.transform.position=new Vector3(magmaTarget.x,Floor+.05f,0);marker.transform.localScale=new Vector3(1.4f,.1f,1);
        }
        if(state==State.AdvanceWarning) {
            direction=player.position.x<boss.position.x?-1:1;
            AdvanceTarget=new Vector2(Mathf.Clamp(boss.position.x+direction*3.2f,-6,6),Floor);
            marker.transform.position=new Vector3(AdvanceTarget.x,Floor+.05f,0);
            marker.transform.localScale=new Vector3(2.4f,.1f,1);
        }
        if(state==State.Advance)advanceStart=boss.position;
        body.flipX=direction<0;
    }
    void Update()
    {
        if(Leaving||Paused)return;
        float t=Time.time-since;
        Sprite[] frames=idle;bool loop=false;
        switch(Current) {
            case State.SpitWarning:frames=prepareGust;break;
            case State.Spit:frames=castGust;break;
            case State.AdvanceWarning:frames=prepareDive;break;
            case State.Advance:frames=dive;break;
            case State.Landing:frames=landing;break;
            case State.Vulnerable:frames=vulnerable;loop=true;break;
            case State.Hurt:frames=hurt;break;
            case State.Freed:frames=freed;break;
            default:loop=true;break;
        }
        int frame=loop?(int)(t*6)%frames.Length:Mathf.Min(frames.Length-1,(int)(Mathf.Clamp01(t/Mathf.Max(.01f,deadline-since))*frames.Length));body.sprite=frames[frame];
        // All Salamandra poses share one grounded anchor.
        bossCollider.offset=new Vector2(0,.6f);
        if(Current==State.Spit && !launched && t>=.3f){launched=true;SpawnGust();}
        if(Current==State.Advance) {
            boss.position=Vector3.Lerp(advanceStart,new Vector3(AdvanceTarget.x,Floor,0),Mathf.Clamp01(t/.75f));
            DamageBox((Vector2)boss.position+Vector2.up*.55f,new Vector2(2.1f,1.1f));
        }
        if(Current==State.Freed) {
            if(Time.time>=deadline&&!rewardAttempted){rewardAttempted=true;campaign.NotifyGuardianDefeated();campaign.CompletePresentation();exit.gameObject.SetActive(QuestCampaign.HasCrystal(2));if(!exit.gameObject.activeSelf)QuestSession.Atual.Avisar("Arena isolada: complete floresta e cume para recuperar Fogo na campanha.");}
            return;
        }
        if(Time.time<deadline)return;
        switch(Current) {
            case State.Introduction:Set(State.SpitWarning,1f);break;
            case State.SpitWarning:Set(State.Spit,2.2f);break;
            case State.Spit:if(gust==null)Set(State.AdvanceWarning,1.1f);break;
            case State.AdvanceWarning:Set(State.Advance,.75f);break;
            case State.Advance:
                boss.position=new Vector3(AdvanceTarget.x,Floor,0);Fx(dust,boss.position,.34f);Set(State.Landing,.6f);break;
            case State.Landing:Set(State.Vulnerable,2.4f);break;
            case State.Vulnerable:case State.Hurt:Set(State.SpitWarning,1f);break;
        }
    }
    void SpawnGust()
    {
        Vector3 position=new Vector3(boss.position.x+direction*1.5f,Floor+.9f,0);
        Fx(gustBirth,position,.4f);
        var obj=new GameObject("Cuspe de magma",typeof(SpriteRenderer));obj.transform.position=position;
        var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=gustFlight[0];sr.sharedMaterial=material;sr.sortingOrder=7;sr.flipX=direction<0;sr.enabled=false;
        gust=obj.AddComponent<QuestMagmaGust>();gust.arena=this;gust.frames=gustFlight;gust.origin=position;gust.target=magmaTarget;
    }
    public void DamageBox(Vector2 center,Vector2 size)
    {foreach(var c in Physics2D.OverlapBoxAll(center,size,0)){var health=c.GetComponent<QuestPlayer>();if(health!=null)health.Ferir();}}
    public void Fx(Sprite[] frames,Vector3 position,float duration)
    {
        var obj=new GameObject("Efeito da Coruja",typeof(SpriteRenderer));obj.transform.position=position;
        var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=frames[0];sr.sharedMaterial=material;sr.sortingOrder=8;
        var animation=obj.AddComponent<QuestAnimation>();animation.quadros=frames;animation.velocidade=frames.Length/duration;Destroy(obj,duration);
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
    {if(Leaving||Current!=State.Freed||!exit.gameObject.activeSelf||!QuestCampaign.HasCrystal(2))return;Leaving=true;QuestCampaign.LeaveArena(2);}
    void OnGUI()
    {
        string hint=Current==State.SpitWarning?"CUSPE: observe a trajetoria e saia da marca":Current==State.AdvanceWarning?"AVANCO: pule ou recue da marca":Current==State.Vulnerable?"Salamandra cansada: ataque com magia":Current==State.Freed?"Salamandra liberta - Cristal do Fogo":"Esquive e espere a recuperacao";
        GUI.Box(new Rect(20,170,Mathf.Min(Screen.width-40,700),70),"SALAMANDRA DE MAGMA  "+Health+" / 6\n"+hint);
    }
    void OnDestroy(){if(gust!=null)Destroy(gust.gameObject);if(material!=null)Destroy(material);if(square!=null)Destroy(square);}
}

public class QuestMagmaHit : MonoBehaviour
{public QuestMagmaArena arena;void OnTriggerEnter2D(Collider2D other){arena.Receive(other);}}
public class QuestMagmaExit : MonoBehaviour
{public QuestMagmaArena arena;void OnTriggerEnter2D(Collider2D other){if(other.GetComponent<QuestPlayer>()!=null)arena.Leave();}}
public class QuestMagmaGust : MonoBehaviour
{
    public QuestMagmaArena arena;public Sprite[] frames;public Vector2 origin,target;
    float started;
    void Start(){started=Time.time;}
    void FixedUpdate()
    {
        if(arena==null||!arena.CombatActive){Destroy(gameObject);return;}
        if(QuestSession.Atual!=null&&QuestSession.Atual.Pausado)return;
        float t=(Time.time-started-.4f)/1.15f;if(t<0)return;
        var sr=GetComponent<SpriteRenderer>();sr.enabled=true;sr.sprite=frames[(int)((Time.time-started)*12)%frames.Length];
        Vector2 next=Vector2.Lerp(origin,target,Mathf.Clamp01(t))+Vector2.up*(1.8f*4*Mathf.Clamp01(t)*(1-Mathf.Clamp01(t)));
        Vector2 delta=next-(Vector2)transform.position;
        var wall=Physics2D.BoxCast(transform.position,new Vector2(.6f,.6f),0,delta.normalized,delta.magnitude,1<<3);
        if(wall.collider!=null||t>=1){arena.Fx(arena.gustEnd,next,.5f);Destroy(gameObject);return;}
        foreach(var hit in Physics2D.BoxCastAll(transform.position,new Vector2(.6f,.6f),0,delta.normalized,delta.magnitude)) {
            var health=hit.collider.GetComponent<QuestPlayer>();if(health!=null)health.Ferir();
        }
        transform.position=next;
    }
}
