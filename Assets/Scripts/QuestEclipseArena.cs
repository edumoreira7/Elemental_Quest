using UnityEngine;

[DefaultExecutionOrder(-100)]
public class QuestEclipseArena : MonoBehaviour
{
    public GameObject playerPrefab;
    public Sprite[] walk, purified, acceptWing, bookFloat, bookFall, bookClosed, waveBirth, waveFlight, waveEnd;
    public Sprite[] idle, prepareGust, castGust, prepareDive, dive, landing, vulnerable, hurt, freed;
    public Sprite[] gustBirth, gustFlight, gustEnd, dust, liberation;
    public enum State { Introduction, ShotWarning, Shot, WaveWarning, Wave, Landing, Vulnerable, Hurt, Freed }
    public State Current { get; private set; }
    public int Health { get; private set; } = 6;
    public Vector2 DiveTarget { get; private set; }
    public bool Leaving { get; private set; }
    Transform player, boss;
    SpriteRenderer body, marker, exit;
    BoxCollider2D bossCollider;
    Camera cameraView;
    QuestCampaignScene campaign;
    QuestEclipseGust gust;
    Sprite square;
    Material material;
    float since, deadline;
    bool launched, rewardAttempted;
    Vector3 diveStart;
    float direction;
    bool nextWave;
    SpriteRenderer book;
    public const float Floor=-2f;
    public bool CombatActive => !Leaving && Current!=State.Freed;
    bool Paused => QuestSession.Atual!=null && QuestSession.Atual.Pausado;
    void Awake()
    {
        campaign=GetComponent<QuestCampaignScene>();campaign.enabled=false;
        square=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one*.5f,Texture2D.whiteTexture.width);
        material=new Material(Shader.Find("Sprites/Default"));
        var session=gameObject.AddComponent<QuestSession>();session.fase=4;session.nomeFase="Arena do Eclipse";session.avisarColetaCompleta=false;
        var cam=new GameObject("Camera fixa",typeof(Camera),typeof(AudioListener));cam.tag="MainCamera";
        cameraView=cam.GetComponent<Camera>();cameraView.orthographic=true;cameraView.orthographicSize=5.5f;cameraView.backgroundColor=new Color(.13f,.1f,.22f);FixCamera();
        var floor=Shape("Plataforma da arena",new Vector2(0,Floor-.5f),new Vector2(18,1),new Color(.5f,.51f,.58f));floor.gameObject.layer=3;floor.gameObject.AddComponent<BoxCollider2D>();
        foreach(float x in new[]{-9.3f,9.3f}) {var wall=Shape("Limite da arena",new Vector2(x,1),new Vector2(.6f,7),new Color(.45f,.6f,.68f));wall.gameObject.layer=3;wall.gameObject.AddComponent<BoxCollider2D>();}
        var scaleRoot=new GameObject("Escala do pato - referencia F1").transform;scaleRoot.SetParent(transform,false);scaleRoot.localScale=Vector3.one*.72f;
        var obj=Instantiate(playerPrefab,new Vector3(-5,-1.1f,0),Quaternion.identity,scaleRoot);player=obj.transform;
        var health=obj.GetComponent<QuestPlayer>();if(health==null)health=obj.AddComponent<QuestPlayer>();health.AtivarCheckpoint(player.position);
        body=Shape("Umbrius",new Vector2(5,Floor),Vector2.one,Color.white);boss=body.transform;body.sortingOrder=6;body.sprite=idle[0];
        bossCollider=body.gameObject.AddComponent<BoxCollider2D>();bossCollider.isTrigger=true;bossCollider.size=new Vector2(.9f,1.5f);bossCollider.offset=new Vector2(0,.75f);
        body.gameObject.AddComponent<QuestEclipseHit>().arena=this;
        book=Shape("Grimorio independente",new Vector2(5.7f,Floor+.8f),Vector2.one,Color.white);book.sortingOrder=7;book.sprite=bookFloat[0];
        marker=Shape("Faixa anunciada da onda",new Vector2(0,Floor+.05f),new Vector2(2.4f,.1f),new Color(1,.65f,.18f));marker.enabled=false;
        exit=Shape("Saida para o Epilogo",new Vector2(8,-.7f),new Vector2(.7f,2.6f),new Color(.55f,.95f,1));exit.gameObject.SetActive(false);
        exit.gameObject.AddComponent<BoxCollider2D>().isTrigger=true;exit.gameObject.AddComponent<QuestEclipseExit>().arena=this;
        bool seen=QuestCampaign.HasEvent("C05.Intro");QuestCampaign.MarkEvent("C05.Intro");Set(State.Introduction,seen?.35f:1.5f);
    }
    SpriteRenderer Shape(string label,Vector2 position,Vector2 size,Color color)
    {
        var obj=new GameObject(label,typeof(SpriteRenderer));obj.transform.SetParent(transform);obj.transform.position=position;obj.transform.localScale=new Vector3(size.x,size.y,1);
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
        marker.enabled=state==State.WaveWarning;
        if(state==State.ShotWarning)direction=player.position.x<boss.position.x?-1:1;
        if(state==State.WaveWarning) {
            direction=player.position.x<boss.position.x?-1:1;
            marker.transform.position=new Vector3(0,Floor+.06f,0);marker.transform.localScale=new Vector3(16,.12f,1);
        }
        body.flipX=direction<0;
    }
    void Update()
    {
        if(Leaving||Paused)return;
        float t=Time.time-since;
        if(Current==State.Freed) {
            float fall=Mathf.Clamp01((t-1f)/.6f);
            book.transform.position=new Vector3(boss.position.x+.7f,Mathf.Lerp(Floor+.8f,Floor+.2f,fall),0);
            book.sprite=t<1?bookFloat[(int)(t*6)%bookFloat.Length]:fall<1?bookFall[Mathf.Min(bookFall.Length-1,(int)(fall*bookFall.Length))]:bookClosed[0];
        } else {book.sprite=bookFloat[(int)(Time.time*6)%bookFloat.Length];book.transform.position=new Vector3(boss.position.x+.7f,Floor+.8f+Mathf.Sin(Time.time*3)*.06f,0);}

        Sprite[] frames=idle;bool loop=false;
        switch(Current) {
            case State.ShotWarning:frames=prepareGust;break;
            case State.Shot:frames=castGust;break;
            case State.WaveWarning:frames=prepareDive;break;
            case State.Wave:frames=dive;break;
            case State.Landing:frames=landing;break;
            case State.Vulnerable:frames=vulnerable;loop=true;break;
            case State.Hurt:frames=hurt;break;
            case State.Freed:frames=t>=2?purified:freed;break;
            default:loop=true;break;
        }
        int frame=loop?(int)(t*6)%frames.Length:Mathf.Min(frames.Length-1,(int)(Mathf.Clamp01(t/Mathf.Max(.01f,deadline-since))*frames.Length));body.sprite=frames[frame];
        bossCollider.offset=new Vector2(0,.75f);
        if((Current==State.Shot || Current==State.Wave) && !launched && t>=.3f){launched=true;SpawnGust();}
        if(Current==State.Freed) {
            if(Time.time>=deadline&&!rewardAttempted){rewardAttempted=true;campaign.NotifyGuardianDefeated();campaign.CompletePresentation();exit.gameObject.SetActive(QuestCampaign.HasCrystal(3));if(!exit.gameObject.activeSelf)QuestSession.Atual.Avisar("Arena isolada: complete as tres regioes para recuperar Gelo na campanha.");}
            return;
        }
        if(Time.time<deadline)return;
        switch(Current) {
            case State.Introduction:Set(State.ShotWarning,1f);break;
            case State.ShotWarning:Set(State.Shot,3.4f);break;
            case State.WaveWarning:Set(State.Wave,3.4f);break;
            case State.Shot:case State.Wave:
                if(gust==null){nextWave=Current==State.Shot;Set(State.Landing,.4f);}break;
            case State.Landing:Set(State.Vulnerable,2.2f);break;
            case State.Vulnerable:case State.Hurt:Set(nextWave?State.WaveWarning:State.ShotWarning,1f);break;
        }
    }
    void SpawnGust()
    {
        bool low=Current==State.Wave;
        Vector3 position=new Vector3(boss.position.x+direction*1.2f,low?Floor:Floor+.85f,0);
        Fx(low?waveBirth:gustBirth,position,.34f);
        var obj=new GameObject(low?"Onda rasteira":"Orbe sombrio",typeof(SpriteRenderer));obj.transform.position=position;obj.transform.localScale=Vector3.one*(low?1.2f:1.25f);
        var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=(low?waveFlight:gustFlight)[0];sr.sharedMaterial=material;sr.sortingOrder=7;sr.flipX=direction<0;sr.enabled=false;
        obj.AddComponent<QuestBossSpriteMaterial>();gust=obj.AddComponent<QuestEclipseGust>();gust.arena=this;gust.direction=direction;gust.frames=low?waveFlight:gustFlight;gust.ending=low?waveEnd:gustEnd;gust.low=low;
    }
    public void DamageBox(Vector2 center,Vector2 size)
    {foreach(var c in Physics2D.OverlapBoxAll(center,size,0)){var health=c.GetComponent<QuestPlayer>();if(health!=null)health.Ferir();}}
    public void Fx(Sprite[] frames,Vector3 position,float duration)
    {
        var obj=new GameObject("Efeito sombrio",typeof(SpriteRenderer));obj.transform.position=position;if(frames==gustBirth||frames==gustEnd)obj.transform.localScale=Vector3.one*1.25f;else if(frames==waveBirth||frames==waveEnd)obj.transform.localScale=Vector3.one*1.2f;
        var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=frames[0];sr.sharedMaterial=material;sr.sortingOrder=8;
        obj.AddComponent<QuestBossSpriteMaterial>();var animation=obj.AddComponent<QuestAnimation>();animation.quadros=frames;animation.velocidade=frames.Length/duration;Destroy(obj,duration);
    }
    public void Receive(Collider2D other)
    {
        var projectile=other.GetComponent<ProjetilPoder>();if(projectile==null||Current==State.Freed||Leaving)return;
        Destroy(projectile.gameObject);if(Current!=State.Vulnerable)return;
        Health=Mathf.Max(0,Health-1);
        if(Health>0)Set(State.Hurt,.3f);
        else {if(gust!=null)Destroy(gust.gameObject);bossCollider.enabled=false;Set(State.Freed,2f);Fx(liberation,boss.position+Vector3.up, .6f);}
    }
    public void Leave()
    {if(Leaving||Current!=State.Freed||!exit.gameObject.activeSelf||!QuestCampaign.HasCrystal(3))return;Leaving=true;QuestCampaign.LeaveArena(3);}
    void OnGUI()
    {
        string hint=Current==State.ShotWarning?"DISPARO SOMBRIO: desvie do orbe":Current==State.WaveWarning?"ONDA RASTEIRA: pule a faixa marcada":Current==State.Vulnerable?"Umbrius em recuperacao: ataque com magia":Current==State.Freed?"Umbrius livre: aproxime-se. Cristal do Gelo.":"Esquive e espere a recuperacao";
        QuestHudArt.BossHint("UMBRIUS  ".Trim(),Health,hint);
    }
    void OnDestroy(){if(gust!=null)Destroy(gust.gameObject);if(material!=null)Destroy(material);if(square!=null)Destroy(square);}
}

public class QuestEclipseHit : MonoBehaviour
{public QuestEclipseArena arena;void OnTriggerEnter2D(Collider2D other){arena.Receive(other);}}
public class QuestEclipseExit : MonoBehaviour
{public QuestEclipseArena arena;void OnTriggerEnter2D(Collider2D other){if(other.GetComponent<QuestPlayer>()!=null)arena.Leave();}}
public class QuestEclipseGust : MonoBehaviour
{
    public QuestEclipseArena arena;public float direction;public Sprite[] frames,ending;public bool low;
    float started;
    void Start(){started=Time.time;}
    void FixedUpdate()
    {
        if(arena==null||!arena.CombatActive){Destroy(gameObject);return;}
        if(QuestSession.Atual!=null&&QuestSession.Atual.Pausado)return;
        if(Time.time-started<.34f)return;
        var sr=GetComponent<SpriteRenderer>();sr.enabled=true;sr.sprite=frames[(int)((Time.time-started)*12)%frames.Length];
        float step=(low?5.5f:6f)*Time.fixedDeltaTime;
        Vector2 center=(Vector2)transform.position+(low?Vector2.up*.25f:Vector2.zero);
        Vector2 size=low?new Vector2(.9f,.45f):new Vector2(.5f,.5f);
        var wall=Physics2D.BoxCast(center,size,0,Vector2.right*direction,step,1<<3);
        if(wall.collider!=null||Mathf.Abs(transform.position.x)>8.7f||Time.time-started>3.6f){arena.Fx(ending,transform.position,.5f);Destroy(gameObject);return;}
        arena.DamageBox(center+Vector2.right*direction*step/2,new Vector2(size.x+step,size.y));transform.position+=Vector3.right*direction*step;
    }
}


