using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class QuestEclipseArena : MonoBehaviour
{
    public GameObject playerPrefab;
    public Sprite[] walk, purified, acceptWing, bookFloat, bookFall, bookClosed, waveBirth, waveFlight, waveEnd;
    public Sprite[] idle, prepareGust, castGust, prepareDive, dive, landing, vulnerable, hurt, freed;
    public Sprite[] gustBirth, gustFlight, gustEnd, dust, liberation;
    [Header("Batalha final")]
    public int maxHealth = 12;
    public float vulnerabilitySeconds = 1.65f;
    public int maximumProjectiles = 4;
    // Preserve existing state values and scene references.
    public enum State { Introduction, ShotWarning, Shot, WaveWarning, Wave, Landing, Vulnerable, Hurt, Freed,
        EarthWarning, Earth, FireWarning, Fire, IceWarning, Ice, AirWarning, Air, PhaseShift, Reposition }
    public enum Move { Earth, Fire, Ice, Air, Shadow }
    public State Current { get; private set; }
    public int Health { get; private set; }
    public int Phase => Health > maxHealth * 2 / 3 ? 1 : Health > maxHealth / 3 ? 2 : 3;
    public Move LastMove { get; private set; }
    public int CompletedMoves { get; private set; }
    public int DoubleJumps { get; private set; }
    public Vector2 DiveTarget { get; private set; }
    public bool Leaving { get; private set; }
    public QuestEclipseBarrier Barrier { get; private set; }
    public Transform Boss => boss;
    public Transform Player => player;
    public int ActiveSpells { get { spells.RemoveAll(s => s == null); return spells.Count; } }
    public const float Floor = -2f;
    public bool CombatActive => !Leaving && Current != State.Freed && Health > 0;
    bool Paused => QuestSession.Atual != null && QuestSession.Atual.Pausado;
    Transform player, boss;
    SpriteRenderer body, marker, exit, book, aura;
    BoxCollider2D bossCollider;
    Camera cameraView;
    QuestCampaignScene campaign;
    Sprite square;
    Material material;
    float since, deadline, direction, nextShot, hurtUntil, damageAfter;
    bool launched, rewardAttempted, nextWave, barrierStagger;
    int chainLeft, shotsLeft, hitsThisOpening, announcedPhase = 1;
    Vector2 lockedTarget;
    float barrierX, iceX, secondIceX, jumpVelocity, airTargetX, airElapsed, repositionX;
    bool secondJump, airShot;
    readonly float[] readyAt = new float[5];
    readonly List<QuestEclipseSpell> spells = new List<QuestEclipseSpell>();
    readonly List<GameObject> warnings = new List<GameObject>();
    static readonly Move[] Rotation = { Move.Earth, Move.Fire, Move.Ice, Move.Air, Move.Shadow };

    void Awake()
    {
        maxHealth = Mathf.Max(6, maxHealth); Health = maxHealth;
        campaign = GetComponent<QuestCampaignScene>(); campaign.enabled = false;
        square = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), Vector2.one * .5f, Texture2D.whiteTexture.width);
        material = new Material(Shader.Find("Sprites/Default"));
        var session = gameObject.AddComponent<QuestSession>(); session.fase = 4; session.nomeFase = "Arena do Eclipse"; session.avisarColetaCompleta = false;
        var cam = new GameObject("Camera fixa", typeof(Camera), typeof(AudioListener)); cam.tag = "MainCamera";
        cameraView = cam.GetComponent<Camera>(); cameraView.orthographic = true; cameraView.orthographicSize = 5.5f; cameraView.backgroundColor = new Color(.13f, .1f, .22f); FixCamera();
        var floor = Shape("Plataforma da arena", new Vector2(0, Floor - .5f), new Vector2(18, 1), new Color(.5f, .51f, .58f)); floor.gameObject.layer = 3; floor.gameObject.AddComponent<BoxCollider2D>();
        foreach (float x in new[] { -9.3f, 9.3f }) { var wall = Shape("Limite da arena", new Vector2(x, 1), new Vector2(.6f, 7), new Color(.45f, .6f, .68f)); wall.gameObject.layer = 3; wall.gameObject.AddComponent<BoxCollider2D>(); }
        var scaleRoot = new GameObject("Escala do pato - referencia F1").transform; scaleRoot.SetParent(transform, false); scaleRoot.localScale = Vector3.one * .72f;
        var obj = Instantiate(playerPrefab, new Vector3(-5, -1.1f, 0), Quaternion.identity, scaleRoot); player = obj.transform;
        var health = obj.GetComponent<QuestPlayer>(); if (health == null) health = obj.AddComponent<QuestPlayer>(); health.AtivarCheckpoint(player.position);
        body = Shape("Umbrius", new Vector2(5, Floor), Vector2.one, Color.white); boss = body.transform; body.sortingOrder = 6; body.sprite = idle[0];
        bossCollider = body.gameObject.AddComponent<BoxCollider2D>(); bossCollider.isTrigger = true; bossCollider.size = new Vector2(.9f, 1.5f); bossCollider.offset = new Vector2(0, .75f);
        body.gameObject.AddComponent<QuestEclipseHit>().arena = this;
        book = Shape("Grimorio independente", new Vector2(5.7f, Floor + .8f), Vector2.one, Color.white); book.sortingOrder = 7; book.sprite = bookFloat[0];
        marker = Shape("Aviso elemental de Umbrius", Vector2.zero, Vector2.one, Color.white); marker.sortingOrder = 5; marker.enabled = false;
        aura = Shape("Emissao elemental do cajado", Vector2.zero, Vector2.one * .25f, Color.white); aura.sortingOrder = 8; aura.enabled = false;
        if (gustBirth.Length > 0) { aura.sprite = gustBirth[0]; aura.transform.localScale = Vector3.one * (.65f / aura.sprite.bounds.size.x); }
        exit = Shape("Saida para o Epilogo", new Vector2(8, -.7f), new Vector2(.7f, 2.6f), new Color(.55f, .95f, 1)); exit.gameObject.SetActive(false);
        exit.gameObject.AddComponent<BoxCollider2D>().isTrigger = true; exit.gameObject.AddComponent<QuestEclipseExit>().arena = this;
        bool seen = QuestCampaign.HasEvent("C05.Intro"); QuestCampaign.MarkEvent("C05.Intro"); Set(State.Introduction, seen ? .35f : 1.5f);
    }
    SpriteRenderer Shape(string label, Vector2 position, Vector2 size, Color color)
    {
        var obj = new GameObject(label, typeof(SpriteRenderer)); obj.transform.SetParent(transform); obj.transform.position = position; obj.transform.localScale = new Vector3(size.x, size.y, 1);
        var sr = obj.GetComponent<SpriteRenderer>(); sr.sprite = square; sr.sharedMaterial = material; sr.color = color; obj.AddComponent<QuestBossSpriteMaterial>(); return sr;
    }
    void FixCamera()
    {
        cameraView.transform.position = new Vector3(0, .5f, -10); float actual = Screen.width / (float)Mathf.Max(1, Screen.height), target = 16f / 9;
        cameraView.rect = actual > target ? new Rect((1 - target / actual) / 2, 0, target / actual, 1) : new Rect(0, (1 - actual / target) / 2, 1, actual / target);
    }
    void LateUpdate() { FixCamera(); }
    bool Warning => Current == State.EarthWarning || Current == State.FireWarning || Current == State.IceWarning || Current == State.AirWarning || Current == State.ShotWarning || Current == State.WaveWarning;
    public float WarningDuration(Move move) => Mathf.Max(.55f, (move == Move.Ice ? 1f : move == Move.Shadow ? .8f : .9f) - (Phase - 1) * .08f);
    Vector3 StaffOrigin => boss.position + new Vector3(direction * .68f, .85f, 0);
    Color ElementColor => LastMove == Move.Earth ? new Color(.85f, .6f, .28f) : LastMove == Move.Fire ? new Color(1, .38f, .1f) : LastMove == Move.Ice ? new Color(.45f, .9f, 1) : LastMove == Move.Air ? new Color(.8f, 1, 1) : new Color(.85f, .4f, 1);
    void Set(State state, float duration)
    {
        Current = state; since = Time.time; deadline = since + duration; launched = false;
        marker.enabled = false; ClearWarnings();
        if (state == State.Vulnerable) { ClearSpells(); RemoveBarrier(); hitsThisOpening = 0; damageAfter = Time.time; }
        if (state == State.Freed) { ClearSpells(); RemoveBarrier(); aura.enabled = false; }
    }
    void BeginTurn()
    {
        if (Phase > announcedPhase) { announcedPhase = Phase; Set(State.PhaseShift, 1.05f); return; }
        chainLeft = Phase; BeginNextMove();
    }
    Move ChooseMove()
    {
        var desired = Rotation[CompletedMoves % Rotation.Length];
        // The first four casts teach all elements. Later choices react only between casts.
        if (CompletedMoves >= 4)
        {
            var block = player.GetComponent<QuestElementPowers>().BlocoAtual;
            if ((block != null || player.position.y > Floor + 1.9f) && LastMove != Move.Ice && Time.time >= readyAt[(int)Move.Ice]) desired = Move.Ice;
            else if (Mathf.Abs(player.position.x - boss.position.x) < 2 && LastMove != Move.Air && Time.time >= readyAt[(int)Move.Air]) desired = Move.Air;
        }
        if (desired == LastMove && CompletedMoves > 0 || Time.time < readyAt[(int)desired])
            for (int i = 1; i <= Rotation.Length; i++) { var other = Rotation[(CompletedMoves + i) % Rotation.Length]; if (other != LastMove && Time.time >= readyAt[(int)other]) { desired = other; break; } }
        return desired;
    }
    void BeginNextMove()
    {
        var move = ChooseMove(); chainLeft--; BeginMove(move);
    }
    void BeginMove(Move move)
    {
        LastMove = move;
        direction = player.position.x < boss.position.x ? -1 : 1;
        lockedTarget = player.GetComponent<Collider2D>().bounds.center;
        body.flipX = direction < 0;
        float warning = WarningDuration(move);
        if (move == Move.Earth)
        {
            barrierX = Mathf.Clamp(boss.position.x + direction * 1.65f, -7.4f, 7.4f);
            if (Mathf.Abs(barrierX - player.position.x) < 1.45f) barrierX = Mathf.Clamp(boss.position.x - direction * 1.65f, -7.4f, 7.4f);
            Set(State.EarthWarning, warning); Mark(new Vector2(barrierX, Floor + .9f), new Vector2(1.1f, 1.7f), new Color(.85f, .6f, .28f, .35f));
        }
        else if (move == Move.Fire)
        { Set(State.FireWarning, warning); Mark((Vector2)boss.position + new Vector2(direction * 3, .85f), new Vector2(6, .06f), new Color(1, .4f, .1f, .65f)); }
        else if (move == Move.Ice)
        {
            iceX = Mathf.Clamp(lockedTarget.x, -7.3f, 7.3f); secondIceX = Mathf.Clamp(iceX + direction * 1.65f, -7.3f, 7.3f);
            if (Mathf.Abs(iceX - secondIceX) < 1) secondIceX = Mathf.Clamp(iceX - direction * 1.65f, -7.3f, 7.3f);
            Set(State.IceWarning, warning); Mark(new Vector2(iceX, Floor + .04f), new Vector2(1.35f, .08f), new Color(.4f, .9f, 1, .8f));
            if (Phase > 1) ExtraMark(new Vector2(secondIceX, Floor + .04f), new Vector2(1.35f, .08f), new Color(.4f, .9f, 1, .8f));
        }
        else if (move == Move.Air)
        {
            airTargetX = boss.position.x > 0 ? -5.5f : 5.5f;
            Set(State.AirWarning, warning); DiveTarget = new Vector2(airTargetX, Floor);
            Mark(DiveTarget + Vector2.up * .04f, new Vector2(2.1f, .09f), new Color(.75f, 1, 1, .9f));
        }
        else
        {
            nextWave = !nextWave; Set(nextWave ? State.WaveWarning : State.ShotWarning, warning);
            if (nextWave) Mark(new Vector2(0, Floor + .06f), new Vector2(16, .1f), new Color(.8f, .35f, 1, .6f));
            else Mark((Vector2)boss.position + new Vector2(direction * 3, .85f), new Vector2(6, .05f), new Color(.8f, .35f, 1, .65f));
        }
    }
    void Mark(Vector2 center, Vector2 size, Color color) { marker.enabled = true; marker.transform.position = center; marker.transform.localScale = new Vector3(size.x, size.y, 1); marker.color = color; }
    void ExtraMark(Vector2 center, Vector2 size, Color color) { var sr = Shape("Marca de gelo - alvo fixo", center, size, color); sr.sortingOrder = 5; warnings.Add(sr.gameObject); }
    void ClearWarnings() { foreach (var go in warnings) if (go != null) Destroy(go); warnings.Clear(); }
    void Commit()
    {
        CompletedMoves++; readyAt[(int)LastMove] = Time.time + (LastMove == Move.Earth ? 7 : LastMove == Move.Air ? 4 : 2);
        if (LastMove == Move.Earth) { Set(State.Earth, 2f); SpawnBarrier(); }
        else if (LastMove == Move.Fire) { Set(State.Fire, 1.7f); shotsLeft = Mathf.Min(4, Phase + 2); nextShot = Time.time + .12f; SpellEffect("Fogo/02-Emissao-Cajado", StaffOrigin, Vector2.one * .7f, 4); }
        else if (LastMove == Move.Ice)
        {
            Set(State.Ice, 1.35f); SpawnElement(Move.Ice, new Vector2(iceX, 4.1f), Vector2.down * (6 + Phase * .3f));
            if (Phase > 1) SpawnElement(Move.Ice, new Vector2(secondIceX, 4.1f), Vector2.down * (6 + Phase * .3f));
        }
        else if (LastMove == Move.Air)
        {
            Set(State.Air, 2.7f); jumpVelocity = 7.8f; airElapsed = 0; secondJump = false; airShot = false;
            direction = airTargetX < boss.position.x ? -1 : 1; body.flipX = direction < 0;
            Mark(DiveTarget + Vector2.up * .04f, new Vector2(2.1f, .09f), new Color(.75f, 1, 1, .8f));
            SpellEffect("Ar/03-Impulso-Pulo-Duplo", boss.position, Vector2.one, 6);
        }
        else
        {
            Set(nextWave ? State.Wave : State.Shot, 1.6f);
            SpawnShadow(nextWave);
        }
    }
    void Update()
    {
        if (Leaving || Paused || QuestCutsceneDirector.IsPlaying) return;
        float t = Time.time - since;
        AnimateBook(t); AnimateBody(t);
        aura.enabled = Warning || Current == State.PhaseShift;
        if (aura.enabled) { aura.transform.position = StaffOrigin; aura.color = Current == State.PhaseShift ? Color.white : ElementColor; }
        if (Current == State.Freed)
        {
            if (Time.time >= deadline && !rewardAttempted)
            {
                rewardAttempted = true; campaign.NotifyGuardianDefeated();
                campaign.CompletePresentation(() => { exit.gameObject.SetActive(QuestCampaign.HasCrystal(3)); if (!exit.gameObject.activeSelf) QuestSession.Atual.Avisar("Arena isolada: complete as tres regioes para recuperar Gelo na campanha."); });
            }
            return;
        }
        if (Current == State.Air) { TickAir(); return; }
        Settle();
        if (Current == State.Fire && Time.time >= nextShot && shotsLeft > 0 && ActiveSpells < maximumProjectiles)
        { SpawnElement(Move.Fire, StaffOrigin, Vector2.right * direction * (5.8f + Phase * .3f)); shotsLeft--; nextShot = Time.time + .38f - Phase * .035f; }
        if (Current == State.Reposition) boss.position = new Vector3(Mathf.MoveTowards(boss.position.x, repositionX, 3.2f * Time.deltaTime), boss.position.y, 0);
        if (Time.time < deadline) return;
        if (Warning) { Commit(); return; }
        if (Current == State.Introduction || Current == State.PhaseShift || Current == State.Reposition) { BeginTurn(); return; }
        if (Current == State.Vulnerable || Current == State.Hurt) { EndOpening(); return; }
        if (Current == State.Landing) { if (chainLeft > 0) BeginNextMove(); else OpenWindow(); return; }
        Set(State.Landing, .32f);
    }
    void AnimateBook(float t)
    {
        if (Current == State.Freed)
        {
            float fall = Mathf.Clamp01((t - 1) / .6f); book.transform.position = new Vector3(boss.position.x + .7f, Mathf.Lerp(Floor + .8f, Floor + .2f, fall), 0);
            book.sprite = t < 1 ? bookFloat[(int)(t * 6) % bookFloat.Length] : fall < 1 ? bookFall[Mathf.Min(bookFall.Length - 1, (int)(fall * bookFall.Length))] : bookClosed[0];
        }
        else { book.sprite = bookFloat[(int)(Time.time * 6) % bookFloat.Length]; book.transform.position = boss.position + new Vector3(.7f, .8f + Mathf.Sin(Time.time * 3) * .06f, 0); }
    }
    void AnimateBody(float t)
    {
        Sprite[] frames = idle; bool loop = false;
        if (Current == State.Freed) frames = t >= 2 ? purified : freed;
        else if (Current == State.Hurt || Current == State.Vulnerable && Time.time < hurtUntil) frames = hurt;
        else if (Current == State.Vulnerable) { frames = vulnerable; loop = true; }
        else if (Current == State.Landing) frames = landing;
        else if (Current == State.Reposition || Current == State.Air) { frames = walk; loop = true; }
        else if (Warning) frames = LastMove == Move.Earth || LastMove == Move.Air || Current == State.WaveWarning ? prepareDive : prepareGust;
        else if (Current != State.Introduction && Current != State.PhaseShift) frames = Current == State.Wave ? dive : castGust;
        if (frames != null && frames.Length > 0) body.sprite = frames[loop ? (int)(t * 8) % frames.Length : Mathf.Min(frames.Length - 1, (int)(Mathf.Clamp01(t / Mathf.Max(.01f, deadline - since)) * frames.Length))];
        body.color = Current == State.Vulnerable ? new Color(1, .95f, .78f) : Color.white;
    }
    void TickAir()
    {
        float dt = Time.deltaTime; airElapsed += dt;
        if (!secondJump && airElapsed >= .30f) { secondJump = true; DoubleJumps++; jumpVelocity = 7.5f; SpellEffect("Ar/03-Impulso-Pulo-Duplo", boss.position, Vector2.one * 1.3f, 6); }
        if (!airShot && airElapsed >= .68f) { airShot = true; SpawnElement(Move.Air, StaffOrigin, (lockedTarget - (Vector2)StaffOrigin).normalized * 5.6f); }
        jumpVelocity -= 18 * dt;
        if (jumpVelocity < 0 && airElapsed < 1.2f) jumpVelocity = Mathf.Max(jumpVelocity, -1.4f);
        float nextY = boss.position.y + jumpVelocity * dt;
        float nextX = Mathf.MoveTowards(boss.position.x, airTargetX, 6.5f * dt);
        var hit = Physics2D.BoxCast(new Vector2(nextX, boss.position.y + .08f), new Vector2(.6f, .08f), 0, Vector2.down, Mathf.Max(.1f, boss.position.y - nextY + .1f), 1 << 3);
        if (jumpVelocity < 0 && (nextY <= Floor || hit.collider != null && nextY <= hit.point.y + .04f) || Time.time >= deadline)
        {
            boss.position = new Vector3(nextX, Mathf.Max(Floor, hit.collider == null ? Floor : hit.point.y), 0); marker.enabled = false;
            if (barrierStagger) { barrierStagger = false; chainLeft = 0; OpenWindow(1.9f); }
            else { SpawnElement(Move.Air, boss.position + Vector3.up * .3f, Vector2.left * 5.5f, true); SpawnElement(Move.Air, boss.position + Vector3.up * .3f, Vector2.right * 5.5f, true); Set(State.Landing, .4f); }
        }
        else boss.position = new Vector3(nextX, nextY, 0);
    }
    void Settle()
    {
        if (boss.position.y <= Floor) return;
        var hit = Physics2D.BoxCast((Vector2)boss.position + Vector2.up * .1f, new Vector2(.6f, .08f), 0, Vector2.down, .2f, 1 << 3);
        if (hit.collider == null) boss.position = new Vector3(boss.position.x, Mathf.Max(Floor, boss.position.y - 8 * Time.deltaTime), 0);
    }
    void OpenWindow(float seconds = -1) { Set(State.Vulnerable, seconds > 0 ? seconds : Mathf.Max(1.15f, vulnerabilitySeconds - (Phase - 1) * .2f)); }
    void EndOpening()
    {
        if (Phase > announcedPhase) { BeginTurn(); return; }
        repositionX = Mathf.Clamp(boss.position.x + Mathf.Sign(player.position.x - boss.position.x) * (Mathf.Abs(player.position.x - boss.position.x) < 2 ? -2 : 1.5f), -6, 6);
        direction = repositionX < boss.position.x ? -1 : 1; body.flipX = direction < 0; Set(State.Reposition, .5f);
    }
    void SpawnBarrier()
    {
        RemoveBarrier(); Vector2 center = new Vector2(barrierX, Floor + .9f), size = new Vector2(1.1f, 1.7f);
        foreach (var obstacle in Physics2D.OverlapBoxAll(center, size, 0))
            if (obstacle.GetComponent<QuestPlayer>() != null || !obstacle.isTrigger && obstacle.gameObject.layer == 3)
            { chainLeft = 0; OpenWindow(); return; }
        var go = SpellEffect("Terra/01-Bloco-Intacto", center, size, 1, true);
        if (go == null) { chainLeft = 0; OpenWindow(); return; }
        go.name = "Barreira de Terra - Umbrius"; go.layer = 3;
        var c = go.AddComponent<BoxCollider2D>(); c.size = go.GetComponent<SpriteRenderer>().sprite.bounds.size;
        Barrier = go.AddComponent<QuestEclipseBarrier>(); Barrier.arena = this; Barrier.expires = Time.time + 4.3f;
        SpellEffect("Terra/04-Surgimento-Bloco", center, size, 6);
    }
    public void BarrierBroken(QuestEclipseBarrier source)
    {
        if (!CombatActive || Barrier != source) return;
        Barrier = null; ClearSpells();
        if (Current == State.Air) barrierStagger = true;
        else { chainLeft = 0; OpenWindow(1.9f); }
    }
    void RemoveBarrier() { if (Barrier != null) { var old = Barrier; Barrier = null; Destroy(old.gameObject); } }
    public QuestEclipseSpell SpawnElement(Move element, Vector2 position, Vector2 velocity, bool low = false)
    {
        if (!CombatActive || ActiveSpells >= maximumProjectiles) return null;
        string path = element == Move.Fire ? "Fogo/01-Bola-Fogo" : element == Move.Ice ? "Gelo/01-Projetil-Gelo" : "Ar/01-Rajada-Ofensiva";
        Vector2 size = low ? new Vector2(1.3f, .6f) : element == Move.Ice ? new Vector2(.65f, 1f) : new Vector2(1f, .65f);
        var go = SpellEffect(path, position, size, 6, true);
        if (go == null) return null;
        go.name = "Magia de Umbrius - " + element;
        if (element == Move.Ice) go.transform.rotation = Quaternion.Euler(0, 0, -90);
        else if (velocity.x < 0) go.GetComponent<SpriteRenderer>().flipX = true;
        var spell = go.AddComponent<QuestEclipseSpell>(); spell.arena = this; spell.element = element; spell.velocity = velocity; spell.size = low ? new Vector2(.9f, .4f) : new Vector2(.45f, .42f); spell.expires = Time.time + 4;
        spells.Add(spell); return spell;
    }
    void SpawnShadow(bool low)
    {
        if (!CombatActive || ActiveSpells >= maximumProjectiles) return;
        Vector2 position = low ? (Vector2)boss.position + new Vector2(direction * .8f, .28f) : (Vector2)StaffOrigin;
        var frames = low ? waveFlight : gustFlight;
        var go = new GameObject(low ? "Onda de sombra" : "Orbe de sombra", typeof(SpriteRenderer)); go.transform.position = position;
        var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = frames[0]; sr.sortingOrder = 8; sr.flipX = direction < 0; go.AddComponent<QuestBossSpriteMaterial>();
        var anim = go.AddComponent<QuestAnimation>(); anim.quadros = frames; anim.velocidade = 8;
        var spell = go.AddComponent<QuestEclipseSpell>(); spell.arena = this; spell.element = Move.Shadow; spell.velocity = low ? Vector2.right * direction * (5.2f + Phase * .3f) : (lockedTarget - position).normalized * (5.4f + Phase * .3f); spell.size = low ? new Vector2(.9f, .42f) : Vector2.one * .5f; spell.expires = Time.time + 4;
        spells.Add(spell);
    }
    public void ClearSpells() { foreach (var spell in spells) if (spell != null) Destroy(spell.gameObject); spells.Clear(); }
    public GameObject SpellEffect(string path, Vector3 position, Vector2 size, int count, bool loop = false)
    {
        var go = QuestMagicArt.Criar(path, position, size, count, loop);
        if (go != null) go.AddComponent<QuestBossSpriteMaterial>(); return go;
    }
    public void Fx(Sprite[] frames, Vector3 position, float duration)
    {
        if (frames == null || frames.Length == 0) return;
        var go = new GameObject("Efeito sombrio", typeof(SpriteRenderer)); go.transform.position = position;
        var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = frames[0]; sr.sortingOrder = 8; go.AddComponent<QuestBossSpriteMaterial>();
        var anim = go.AddComponent<QuestAnimation>(); anim.quadros = frames; anim.velocidade = frames.Length / duration; Destroy(go, duration);
    }
    public void DamageBox(Vector2 center, Vector2 size)
    { foreach (var c in Physics2D.OverlapBoxAll(center, size, 0)) { var p = c.GetComponent<QuestPlayer>(); if (p != null) p.Ferir(); } }
    public void Receive(Collider2D other)
    {
        var projectile = other.GetComponent<ProjetilPoder>(); if (projectile == null || !CombatActive) return;
        Destroy(projectile.gameObject);
        if (Current != State.Vulnerable || Time.time < damageAfter || hitsThisOpening >= 2) return;
        damageAfter = Time.time + .32f; hitsThisOpening++; hurtUntil = Time.time + .18f; Health = Mathf.Max(0, Health - 1);
        if (Health == 0) { bossCollider.enabled = false; Set(State.Freed, 2f); Fx(liberation, boss.position + Vector3.up, .6f); var status = player.GetComponent<QuestEclipseStatus>(); if (status != null) status.Clear(); }
        else if (hitsThisOpening >= 2) Set(State.Hurt, .26f);
    }
    public void Leave() { if (Leaving || Current != State.Freed || !exit.gameObject.activeSelf || !QuestCampaign.HasCrystal(3)) return; Leaving = true; QuestCampaign.LeaveArena(3); }
    void OnGUI()
    {
        string hint = Current == State.EarthWarning || Current == State.Earth ? "TERRA: Fogo rompe a barreira; voce tambem pode saltar" : Current == State.FireWarning || Current == State.Fire ? "FOGO: salte as rajadas ou use um bloco de Terra" : Current == State.IceWarning || Current == State.Ice ? "GELO: saia das marcas; Terra serve de cobertura" : Current == State.AirWarning || Current == State.Air ? "AR: pulo duplo! Saia da marca de pouso" : Current == State.Vulnerable ? "CONTRA-ATAQUE: ate dois acertos!" : Current == State.PhaseShift ? "O eclipse intensifica os combos. Prepare-se!" : Current == State.Freed ? "Umbrius livre da corrupcao" : "Observe o cajado e prepare a esquiva";
        QuestHudArt.BossBar("UMBRIUS - FASE " + Phase, Health, maxHealth, hint);
    }
    void OnDestroy() { ClearWarnings(); ClearSpells(); RemoveBarrier(); if (material != null) Destroy(material); if (square != null) Destroy(square); }
}

public class QuestEclipseHit : MonoBehaviour { public QuestEclipseArena arena; void OnTriggerEnter2D(Collider2D other) { arena.Receive(other); } }
public class QuestEclipseExit : MonoBehaviour { public QuestEclipseArena arena; void OnTriggerEnter2D(Collider2D other) { if (other.GetComponent<QuestPlayer>() != null) arena.Leave(); } }

// Compatibility for existing editor review tools; new casts use QuestEclipseSpell.
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



