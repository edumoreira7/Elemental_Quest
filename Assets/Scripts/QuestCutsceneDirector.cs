using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable] public sealed class QuestStoryPage { public string image, caption; }
[Serializable] public sealed class QuestStorySequence { public string id; public QuestStoryPage[] pages; }
[Serializable] public sealed class QuestStoryCatalog { public QuestStorySequence[] sequences; }

// One visual storyteller. Progress is granted only by the caller's completion callback.
[DefaultExecutionOrder(2000)]
public sealed class QuestCutsceneDirector : MonoBehaviour
{
    static QuestCutsceneDirector instance;
    public static bool IsPlaying => instance != null && instance.current != null;
    public static QuestCutsceneDirector Instance => Ensure();
    public string CurrentId => current == null ? "" : current.id;
    public int PageIndex { get; private set; }
    public int PageCount => current == null ? 0 : current.pages.Length;
    public Sprite CurrentArt => picture == null ? null : picture.sprite;
    public string CurrentCaption => caption == null ? "" : caption.text;
    public Canvas StoryCanvas => screen == null ? null : screen.GetComponent<Canvas>();
    public Text CaptionLabel => caption;
    readonly Dictionary<string, QuestStorySequence> sequences = new Dictionary<string, QuestStorySequence>();
    readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    readonly List<MonoBehaviour> suspended = new List<MonoBehaviour>();
    QuestStorySequence current;
    Action completed;
    GameObject screen, temporaryEvents;
    Image picture;
    Text caption, count;
    Button previous, advance;
    float previousTimeScale, nextInput, fade;
    Scene queuedScene;
    bool hasQueuedScene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() { instance = null; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { Ensure().Queue(SceneManager.GetActiveScene()); }
    static QuestCutsceneDirector Ensure()
    {
        if (instance == null)
        {
            var obj = new GameObject("Cutscenes de Elemental Quest");
            instance = obj.AddComponent<QuestCutsceneDirector>();
            DontDestroyOnLoad(obj);
        }
        return instance;
    }
    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        SceneManager.sceneLoaded += Loaded;
        var json = Resources.Load<TextAsset>("StoryArt/StoryCatalog");
        if (json != null)
            foreach (var sequence in JsonUtility.FromJson<QuestStoryCatalog>(json.text).sequences)
                sequences.Add(sequence.id, sequence);
    }
    void Loaded(Scene scene, LoadSceneMode mode) { Queue(scene); }
    void Queue(Scene scene)
    {
        if (hasQueuedScene && queuedScene == scene) return;
        queuedScene = scene; hasQueuedScene = true;
        StartCoroutine(AfterSceneReady(scene));
    }
    IEnumerator AfterSceneReady(Scene scene)
    {
        yield return null;
        if (SceneManager.GetActiveScene() != scene || IsPlaying) yield break;
        string id = scene.name == "ArenaFloresta" ? "C02.Intro" :
            scene.name == "ArenaCume" ? "C03.Intro" :
            scene.name == "ArenaVulcao" ? "C04.Intro" :
            scene.name == "ArenaEclipse" ? "C05.Intro" : null;
        if (scene.name == "FasePrincipal" && !QuestCampaign.HasEvent("Campaign.Opening"))
            Play("C01", () => QuestCampaign.MarkEvent("Campaign.Opening"));
        else if (id != null && !QuestCampaign.HasEvent(id + ".Shown")) Play(id, null);
    }
    public static bool Play(string id, Action onComplete)
    {
        var d = Ensure();
        if (IsPlaying) return false;
        if (!d.sequences.TryGetValue(id, out var sequence) || sequence.pages == null || sequence.pages.Length == 0)
        { Debug.LogError("Cutscene ausente: " + id); return false; }
        foreach (var page in sequence.pages)
            if (d.Art(page.image) == null) { Debug.LogError("Arte de cutscene ausente: " + page.image); return false; }
        d.BuildScreen();
        d.current = sequence; d.completed = onComplete; d.PageIndex = 0;
        d.previousTimeScale = Time.timeScale;
        // Suspend custom controllers and their HUDs, preserving their enabled state.
        // UI/EventSystem components remain enabled and use the unscaled clock.
        d.suspended.Clear();
        foreach (var behaviour in FindObjectsByType<MonoBehaviour>())
            if (behaviour != d && behaviour.enabled && behaviour.GetType().Assembly == typeof(QuestCutsceneDirector).Assembly)
            { d.suspended.Add(behaviour); behaviour.enabled = false; }
        Time.timeScale = 0;
        if (EventSystem.current == null)
        {
            d.temporaryEvents = new GameObject("Eventos da cutscene", typeof(EventSystem), typeof(StandaloneInputModule));
            d.temporaryEvents.transform.SetParent(d.transform);
        }
        d.screen.SetActive(true);
        d.ShowPage();
        Debug.Log("EQ_STORY_STARTED " + id);
        return true;
    }
    Sprite Art(string file)
    {
        if (!sprites.TryGetValue(file, out var result))
        { result = Resources.Load<Sprite>("StoryArt/" + file); if (result != null) sprites.Add(file, result); }
        return result;
    }
    void Update()
    {
        if (!IsPlaying) return;
        fade = Mathf.Min(1, fade + Time.unscaledDeltaTime / .18f);
        picture.color = new Color(1, 1, 1, fade);
        if (Time.unscaledTime < nextInput) return;
        if (Input.GetKeyDown(KeyCode.Escape)) Skip();
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) Previous();
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightArrow)) Next();
    }
    void ShowPage()
    {
        var page = current.pages[PageIndex];
        picture.sprite = Art(page.image); picture.preserveAspect = true;
        picture.color = new Color(1, 1, 1, 0); fade = 0;
        caption.text = page.caption;
        count.text = (PageIndex + 1) + " / " + PageCount;
        previous.interactable = PageIndex > 0;
        advance.GetComponentInChildren<Text>().text = PageIndex == PageCount - 1 ? "CONTINUAR" : "AVANÇAR";
        nextInput = Time.unscaledTime + .18f;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }
    public void Next()
    {
        if (!IsPlaying || Time.unscaledTime < nextInput) return;
        if (PageIndex + 1 == PageCount) { Finish(false); return; }
        PageIndex++; ShowPage();
    }
    public void Previous()
    {
        if (!IsPlaying || PageIndex == 0 || Time.unscaledTime < nextInput) return;
        PageIndex--; ShowPage();
    }
    public void Skip()
    { if (IsPlaying && Time.unscaledTime >= nextInput) Finish(true); }
    void Finish(bool skipped)
    {
        if (current == null) return;
        string id = current.id;
        var callback = completed;
        current = null; completed = null;
        QuestCampaign.MarkEvent(id + ".Shown");
        screen.SetActive(false);
        if (temporaryEvents != null) { temporaryEvents.SetActive(false); Destroy(temporaryEvents); }
        foreach (var b in suspended) if (b != null) b.enabled = true;
        suspended.Clear(); Time.timeScale = previousTimeScale;
        Debug.Log("EQ_STORY_COMPLETED " + id + (skipped ? " SKIPPED" : " READ"));
        callback?.Invoke();
    }
    static RectTransform Rect(GameObject go, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
    {
        var r = go.GetComponent<RectTransform>(); r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = low; r.offsetMax = high; return r;
    }
    GameObject UI(string name, Transform parent, params Type[] types)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        foreach (var t in types) go.AddComponent(t); return go;
    }
    Text Label(string name, Transform parent, int size, TextAnchor alignment)
    {
        var go = UI(name, parent, typeof(Text)); var t = go.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize = size;
        t.color = Color.white; t.alignment = alignment; t.supportRichText = false; t.raycastTarget = false;
        return t;
    }
    Button Button(string label, float x, float width, Action action)
    {
        var go = UI(label, screen.transform, typeof(Image), typeof(Button));
        go.GetComponent<Image>().color = new Color(.16f, .13f, .27f, 1);
        Rect(go, Vector2.zero, Vector2.zero, new Vector2(x, 12), new Vector2(x + width, 55));
        var text = Label("Legenda", go.transform, 19, TextAnchor.MiddleCenter); text.text = label;
        Rect(text.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var button = go.GetComponent<Button>(); button.onClick.AddListener(() => action());
        var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;
        return button;
    }
    void BuildScreen()
    {
        if (screen != null) return;
        screen = UI("Apresentação narrativa", transform, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(Image));
        var canvas = screen.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 32000;
        var scaler = screen.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
        screen.GetComponent<Image>().color = Color.black;
        var art = UI("Ilustração", screen.transform, typeof(Image)); picture = art.GetComponent<Image>(); picture.raycastTarget = false;
        Rect(art, new Vector2(0, .215f), Vector2.one, Vector2.zero, Vector2.zero);
        caption = Label("Narração", screen.transform, 24, TextAnchor.MiddleCenter);
        Rect(caption.gameObject, Vector2.zero, new Vector2(1, 0), new Vector2(70, 62), new Vector2(-70, 150));
        count = Label("Página", screen.transform, 18, TextAnchor.MiddleRight);
        Rect(count.gameObject, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-125, 15), new Vector2(-25, 52));
        previous = Button("ANTERIOR", 35, 150, Previous);
        advance = Button("AVANÇAR", 205, 170, Next);
        Button("PULAR", 395, 140, Skip);
        var help = Label("Teclas", screen.transform, 15, TextAnchor.MiddleCenter);
        help.text = "Espaço / Enter: avançar     Esc: pular";
        Rect(help.gameObject, Vector2.zero, new Vector2(1, 0), new Vector2(550, 15), new Vector2(-150, 52));
        screen.SetActive(false);
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= Loaded;
        if (instance != this) return;
        if (current != null)
        {
            foreach (var b in suspended) if (b != null) b.enabled = true;
            Time.timeScale = previousTimeScale;
        }
        instance = null;
    }
}
