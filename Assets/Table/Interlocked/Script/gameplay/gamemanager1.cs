using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class gamemanager1 : MonoBehaviour
{
    public PanelResult resultPanel;

    public GameObject pausePanel;
    public GameObject aboutPanel;

    public Sprite[] bg_sprites;
    private static int total = 0;
    private static bool[] unlocked = null;
    private static int std = 0;
    private static int left = 0;
    public static int steps = 0;
    public static Text step_text;
    public Text level_text;
    public Text time_text;

    private int all;

    private AudioSource _audioSource = null;
    public AudioClip background_music = null;
    private bool isMusicPlaying = false;
    private GameObject levelSelectPanel;
    private Button skipButton;
    private bool isSkipping;

    [SerializeField] private float skipMoveDuration = 0.45f;
    [SerializeField] private float skipDistanceScale = 0.1667f;

    public enum STATE
    {
        Normal,
        Pause,
        About,
        Result
    }
    public STATE m_state;
    private LevelInfo levelInfo;

    [HideInInspector]
    public int levelId;
    float _timer;

    void Start()
    {
        Camera.main.orthographic = false;

        level_text = GameObject.Find("levelTxt").GetComponent<Text>();
        level_text.text = "关  卡 " + levelId.ToString();
        step_text = GameObject.Find("stepTxt").GetComponent<Text>();
        time_text = GameObject.Find("timeTxt").GetComponent<Text>();

        Init();

        step_text.text = steps + "  步";

    }
    public static void Setsteps()
    {

        step_text.text = steps + "  步";
    }
    // Update is called once per frame
    void Update()
    {


        switch (m_state)
        {
            case STATE.Normal:
                {
                    step_text.gameObject.SetActive(true);
                    time_text.gameObject.SetActive(true);
                    _timer += Time.deltaTime;
                    UpdateTimeDisplay((int)_timer);
                    updateUnlockStatus();
                }
                break;

            case STATE.Pause:
                break;
            case STATE.About:
                break;
            case STATE.Result:
                {
                    step_text.gameObject.SetActive(false);
                    time_text.gameObject.SetActive(false);
                }
                break;
            default:
                break;
        }

    }
    private void Awake()
    {
        if (DataMgr.Instance().levelData == null)
        {
            DataMgr.Instance().InitLevelData();
        }

        if (DataMgr.Instance().levelId <= 0)
        {
            DataMgr.Instance().levelId = 1;
        }

        if (DataMgr.Instance().levelInfo == null)
        {
            DataMgr.Instance().levelInfo = DataMgr.Instance().levelData.levels[DataMgr.Instance().levelId - 1];
        }

        if (!PlayerPrefs.HasKey("level"))
        {
            PlayerPrefs.SetInt("level", 1);
            for (int i = 1; i <= 18; i++)
            {
                PlayerPrefs.SetString("level" + i + "time", "99 : 99");
                PlayerPrefs.SetInt("level" + i + "step", 9999);
            }
        }

        levelInfo = DataMgr.Instance().levelInfo;
        levelId = DataMgr.Instance().levelId;

        GameObject.Find("PanseBtn").GetComponent<Button>().onClick.AddListener(() => { OnPauseBtn(); });
        GameObject.Find("AudioBtn").GetComponent<Button>().onClick.AddListener(() => { OnAudioBtn(); });
        skipButton = GameObject.Find("AboutBtn").GetComponent<Button>();
        skipButton.onClick.AddListener(() => { OnSkipBtn(); });
    }

    private void destroyAll()
    {
        for (int i = 1; i <= this.all; i++)
        {
            GameObject obj = GameObject.Find("part" + i);
            if (obj != null)
            {
                obj.SetActive(false);
                Destroy(obj);
            }
        }
    }

    void Init()
    {
        Image bg = GameObject.Find("bg").GetComponent<Image>();
        bg.sprite = bg_sprites[levelId - 1];
        std = 0;
        buildAll();
        SwitchState(STATE.Normal);
        _timer = 0;
        steps = 0;
        step_text.text = steps + "  步";

    }

    private void buildAll()
    {
        TextAsset modelAsset = Resources.Load<TextAsset>("models/" + levelId);
        StringReader model = new StringReader(modelAsset.text);
        string outline = model.ReadLine();
        string line;
        int counter = 1;
        total = 0;

        string[] maxesString = outline.Split(' ');
        for (int i = 0; i < 3; i++)
        {
            if (std < Int32.Parse(maxesString[i]))
            {
                std = Int32.Parse(maxesString[i]);
            }
        }


        // partitial counter
        while ((line = model.ReadLine()) != null)
        {
            string[] nums = line.Split(' ');
            int allNum = nums.Length;
            if (allNum % 3 == 0)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                string baseName = "part" + Convert.ToString(counter);
                obj.transform.position = new Vector3(Int32.Parse(nums[0]), Int32.Parse(nums[1]), Int32.Parse(nums[2]));
                obj.AddComponent<Rigidbody>();
                obj.GetComponent<Rigidbody>().useGravity = false;
                obj.GetComponent<Rigidbody>().mass = 0.01F;
                obj.GetComponent<Rigidbody>().drag = 0;
                obj.GetComponent<Rigidbody>().angularDrag = 0;
                obj.GetComponent<Rigidbody>().freezeRotation = true;
                obj.GetComponent<Renderer>().material= Resources.Load("pic/"+Convert.ToString(counter)) as Material;
                obj.GetComponent<Collider>().material.dynamicFriction = 0;
                obj.GetComponent<Collider>().material.staticFriction = 0;
                obj.GetComponent<Rigidbody>().isKinematic = true;
                obj.name = baseName;
                total += 1;

                // particle counter
                for (int i = 1; i < allNum / 3; i++)
                {
                    GameObject derived = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    derived.GetComponent<Renderer>().material = Resources.Load("pic/" + Convert.ToString(counter)) as Material;
                    derived.transform.position = new Vector3(Int32.Parse(nums[i * 3]), Int32.Parse(nums[i * 3 + 1]), Int32.Parse(nums[i * 3 + 2]));
                    derived.GetComponent<Collider>().material.dynamicFriction = 0;
                    derived.GetComponent<Collider>().material.staticFriction = 0;
                    derived.transform.parent = obj.transform;
                }
                if (levelId == 2)
                {
                    if (counter == 1)
                    {
                    
                        Vector3 scale = obj.transform.localScale;
                        scale.x = 1.1f;
                        scale.y = 1.1f;
                        scale.z = 0.8f;
                        obj.transform.localScale = scale;
                    }
                    if (counter == 2)
                    {
                    
                        Vector3 scale = obj.transform.localScale;
                        Vector3 pos = obj.transform.position;
                        scale.z = 1.0f;
                        scale.y = 1.08f;
                        pos.y = -0.925f;
                        obj.transform.localScale = scale;
                    }

                    if (counter == 4)
                    {
                        Vector3 pos = obj.transform.position;
                        pos.y = 2.35f;
                        pos.x = -0.55f;
                        obj.transform.position = pos;


                    }
                    if (counter == 3)
                    {
                        Vector3 pos = obj.transform.position;
                        pos.y = 0.156f;
                        pos.x = -0.55f;
                        obj.transform.position = pos;


                    }
                }
                if (levelId == 3)
                {
                    if (counter == 4 || counter == 3)
                    {

                        Vector3 scale = obj.transform.localScale;
                        Vector3 pos = obj.transform.position;

                        scale.x = 0.85f;

                        scale.z = 1.3f;
                        pos.z = pos.z - 1.5f;
                        obj.transform.localScale = scale;
                        obj.transform.position = pos;
                    }
                    if (counter == 2 || counter == 5)
                    {

                        Vector3 scale = obj.transform.localScale;
                        scale.x = 1.15f;
                        scale.y = 0.85f;
                        obj.transform.localScale = scale;
                    }
                    if (counter == 6 || counter == 7)
                    {

                        Vector3 scale = obj.transform.localScale;
                        scale.x = 0.85f;
                        scale.y = 1.15f;
                        scale.z = 0.85f;
                        obj.transform.localScale = scale;
                    }

                }

                if (levelId == 4)
                {
                    if (counter == 1)
                    {
                        //print("here");
                        Vector3 scale = obj.transform.localScale;
                        scale.x = 0.80f;

                        obj.transform.localScale = scale;
                    }
                    if (counter == 2)
                    {
                        //print("here");
                        Vector3 scale = obj.transform.localScale;
                        scale.z = 0.85f;

                        obj.transform.localScale = scale;
                    }
                    if (counter == 3)
                    {
                        //print("here");
                        Vector3 scale = obj.transform.localScale;
                        scale.y = 0.85f;

                        obj.transform.localScale = scale;
                    }

                }
                if (levelId == 5)
                {
                    if (counter == 2 || counter == 3)
                    {
                        //print("here");
                        Vector3 scale = obj.transform.localScale;
                        scale.x = 1.05f;
                        scale.y = 1.05f;
                        scale.z = 0.95f;
                        obj.transform.localScale = scale;
                    }


                }
                if (levelId == 6)
                {
                    if (counter == 4 || counter == 1)
                    {
                       
                        Vector3 scale = obj.transform.localScale;
                        scale.y = 0.90f;
                        scale.x = 0.90f;
                        scale.z = 1.10f;
                        obj.transform.localScale = scale;
                    }
                    if (counter == 2 || counter == 3)
                    {
                     
                        Vector3 scale = obj.transform.localScale;
                        scale.x = 0.90f;
                        scale.z = 0.90f;
                        obj.transform.localScale = scale;
                    }
                    if (counter == 6 || counter == 5)
                    {
                      
                        Vector3 scale = obj.transform.localScale;
                        Vector3 pos = obj.transform.position;
                        if (counter == 6) { pos.y = pos.y - 0.1f; }
                        else { pos.y = pos.y + 0.1f; }
                        obj.transform.position = pos;
                        scale.y = 0.90f;
                        scale.z = 0.90f;
                        obj.transform.localScale = scale;
                    }
                }

   
            if (levelId == 7)
                {
                    if (counter == 1)
                    {
                        Vector3 scale = obj.transform.localScale;
                        scale.y = 1.05f;

                        scale.z = 0.8f;
                        obj.transform.localScale = scale;
                    }
                    else
                    {
                        Vector3 scale = obj.transform.localScale;
                        scale.x = 0.95f;

                        scale.y = 0.95f;
                        obj.transform.localScale = scale;

                    }
                }
                obj.AddComponent<Demo>();
                obj.AddComponent<Self>();
            }
            else
            {
                System.Console.WriteLine("Error: Dimensions Unmatch");
            }
            counter++;
        }
        unlocked = new bool[total];
        left = total;
        this.all = total;
    }

    public void NextLevel()
    {
        int playableLevelCount = GetPlayableLevelCount();
        if (levelId < playableLevelCount)
        {
            levelId++;
            DataMgr.Instance().levelId = levelId;
            levelInfo = DataMgr.Instance().levelData.levels[levelId - 1];
            Init();
            GameObject.Find("levelTxt").GetComponent<Text>().text = "关  卡 " + levelId.ToString();
        }
        else
        {
            GameObject.Find("NextText").GetComponent<Text>().text = "敬 请 期 待";
        }

    }
    public void RetryLevel()
    {
        destroyAll();
        Init();
    }

    public void OpenLevelSelectPanel()
    {
        if (levelSelectPanel == null)
        {
            CreateLevelSelectPanel();
        }

        if (levelSelectPanel == null)
        {
            return;
        }

        m_state = STATE.Pause;
        resultPanel.gameObject.SetActive(false);
        pausePanel.SetActive(false);
        aboutPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CreateLevelSelectPanel()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogWarning("未找到 Canvas，无法创建选关面板。");
            return;
        }

        Font font = level_text != null ? level_text.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        levelSelectPanel = CreateUIRoot("LevelSelectPanel", canvas.transform, new Color(0f, 0f, 0f, 0.62f));

        GameObject panel = CreateUIRoot("LevelSelectContent", levelSelectPanel.transform, new Color(0.18f, 0.11f, 0.07f, 0.96f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(560f, 300f);
        panelRect.anchoredPosition = Vector2.zero;

        CreateLabel("Title", panel.transform, "选择关卡", font, 36, new Vector2(0f, 105f), new Vector2(500f, 54f), new Color(1f, 0.86f, 0.55f, 1f));

        int levelCount = GetPlayableLevelCount();
        int columns = 4;
        Vector2 start = new Vector2(-180f, 30f);
        for (int i = 0; i < levelCount; i++)
        {
            int level = i + 1;
            int row = i / columns;
            int col = i % columns;
            Vector2 position = start + new Vector2(col * 120f, -row * 70f);
            CreateLevelButton(panel.transform, "LevelBtn" + level, "第 " + level + " 关", font, position, () => SelectLevel(level));
        }

        CreateLevelButton(panel.transform, "CloseLevelSelectBtn", "返回", font, new Vector2(0f, -105f), CloseLevelSelectPanel);
        levelSelectPanel.SetActive(false);
    }

    private GameObject CreateUIRoot(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = obj.GetComponent<Image>();
        image.color = color;
        return obj;
    }

    private void CreateLabel(string name, Transform parent, string text, Font font, int fontSize, Vector2 position, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text label = obj.GetComponent<Text>();
        label.text = text;
        label.font = font;
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        label.raycastTarget = false;
    }

    private void CreateLevelButton(Transform parent, string name, string text, Font font, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(110f, 52f);

        Image image = obj.GetComponent<Image>();
        image.color = new Color(0.72f, 0.38f, 0.14f, 1f);

        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        CreateLabel("Text", obj.transform, text, font, 24, Vector2.zero, rect.sizeDelta, Color.white);
    }

    private void CloseLevelSelectPanel()
    {
        levelSelectPanel.SetActive(false);
        SwitchState(STATE.Normal);
    }

    private void SelectLevel(int selectedLevel)
    {
        if (DataMgr.Instance().levelData == null)
        {
            DataMgr.Instance().InitLevelData();
        }

        int levelCount = GetPlayableLevelCount();
        if (selectedLevel < 1 || selectedLevel > levelCount)
        {
            return;
        }

        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }

        destroyAll();
        levelId = selectedLevel;
        DataMgr.Instance().levelId = selectedLevel;
        DataMgr.Instance().levelInfo = DataMgr.Instance().levelData.levels[selectedLevel - 1];
        levelInfo = DataMgr.Instance().levelInfo;
        Init();
        level_text.text = "关  卡 " + levelId.ToString();
    }

    private int GetPlayableLevelCount()
    {
        int dataCount = DataMgr.Instance().levelData != null ? DataMgr.Instance().levelData.levels.Length : int.MaxValue;
        int spriteCount = bg_sprites != null && bg_sprites.Length > 0 ? bg_sprites.Length : int.MaxValue;
        int resourceCount = 0;

        for (int i = 1; i <= dataCount; i++)
        {
            if (Resources.Load<TextAsset>("models/" + i) == null)
            {
                break;
            }

            resourceCount++;
        }

        if (resourceCount == 0)
        {
            resourceCount = dataCount == int.MaxValue ? 0 : dataCount;
        }

        return Mathf.Min(dataCount, spriteCount, resourceCount);
    }

    public void SwitchState(STATE state)
    {

        m_state = state;
        switch (state)
        {
            case STATE.Normal:
                resultPanel.gameObject.SetActive(false);
                pausePanel.SetActive(false);
                aboutPanel.SetActive(false);
                break;
            case STATE.Pause:
                pausePanel.SetActive(true);
                break;
            case STATE.About:
                break;
            case STATE.Result:
                resultPanel.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
    void UpdateTimeDisplay(int time)
    {
        System.TimeSpan tspan = new System.TimeSpan(0, 0, time);
        string timeStr = string.Format("{0:00}:{1:00}", tspan.Minutes, tspan.Seconds);
        time_text.text = timeStr.Substring(0, 2) + " : " + timeStr.Substring(3, 2);

    }

    public void updateUnlockStatus()
    {
        for (int i = 0; i < total; i++)
        {
            if (unlocked[i] == false)
            {
                unlocked[i] = true;
                GameObject cube1 = GameObject.Find("part" + Convert.ToString(i + 1));
                for (int j = 0; j < total; j++)
                {
                    if (i != j && unlocked[j] == false)
                    {
                        GameObject cube2 = GameObject.Find("part" + Convert.ToString(j + 1));

                        Vector3 diff = cube1.transform.position - cube2.transform.position;
                        if (diff.x > std || diff.x < -std || diff.y > std
                            || diff.y < -std || diff.z > std || diff.z < -std)
                        {
                            continue;
                        }
                        else
                        {
                            unlocked[i] = false;
                            break;
                        }
                    }
                }

                if (unlocked[i])
                {
                    cube1.GetComponent<Self>().setUnlocked(true);
                    left--;
                    if (left == 0)
                    {

                        SwitchState(STATE.Result);
                        resultPanel.MatchResult(true);
                    }
                }
            }
        }
    }


    void OnPauseBtn()
    {
        SwitchState(STATE.Pause);
    }

    void OnAudioBtn()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (background_music == null)
        {
            background_music = Resources.Load<AudioClip>("music/background_music");
        }

        if (background_music == null)
        {
            Debug.LogWarning("未找到 Resources/music/background_music 音频文件。");
            return;
        }

        _audioSource.loop = true;
        _audioSource.clip = background_music;
        _audioSource.volume = 0.25f;

        isMusicPlaying = !isMusicPlaying;
        if (isMusicPlaying)
        {
            _audioSource.Play();
        }
        else
        {
            _audioSource.Pause();
        }

    }

    void OnSkipBtn()
    {
        if (isSkipping)
        {
            return;
        }

        SwitchState(STATE.About);
        Cursor.lockState = CursorLockMode.Locked;
        String para = Path.Combine(Application.dataPath, "Table", "Interlocked", "Script", "solutions", "solution" + levelId + ".wood");
        if (!File.Exists(para))
        {
            Debug.LogWarning("未找到跳过步骤文件：" + para);
            SwitchState(STATE.Normal);
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        destroyAll();
        buildAll();
        StartCoroutine(PlaySkipSolution(para));
    }

    private IEnumerator PlaySkipSolution(string solutionPath)
    {
        isSkipping = true;
        if (skipButton != null)
        {
            skipButton.interactable = false;
        }

        string[] lines = File.ReadAllLines(solutionPath);
        if (lines.Length == 0)
        {
            FinishSkipPlayback(false);
            yield break;
        }

        string[] names = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        List<GameObject> parts = new List<GameObject>(names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            parts.Add(GameObject.Find("part" + names[i]));
        }

        Vector3 initialOffset = new Vector3(0f, 0f, 50f) * skipDistanceScale;
        for (int i = 0; i < parts.Count; i++)
        {
            if (parts[i] != null)
            {
                parts[i].transform.position += initialOffset;
            }
        }

        float elapsed = 0f;
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] act = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int length = act.Length;
            if (length != 5 && length != 2)
            {
                continue;
            }

            float actionTime = float.Parse(act[length - 1]);
            float waitTime = Mathf.Max(0f, actionTime - elapsed);
            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
                elapsed = actionTime;
            }

            int partIndex = int.Parse(act[0]) - 1;
            if (partIndex < 0 || partIndex >= parts.Count || parts[partIndex] == null)
            {
                continue;
            }

            if (length == 5)
            {
                int a = int.Parse(act[1]);
                int b = int.Parse(act[2]);
                int c = int.Parse(act[3]);
                Vector3 offset = new Vector3(a, b, c) * skipDistanceScale;
                yield return MovePartSmoothly(parts[partIndex].transform, offset);
                elapsed += skipMoveDuration;
            }
            else if (length == 2)
            {
                Destroy(parts[partIndex]);
            }
        }

        FinishSkipPlayback(true);
    }

    private IEnumerator MovePartSmoothly(Transform target, Vector3 offset)
    {
        if (target == null)
        {
            yield break;
        }

        Vector3 start = target.position;
        Vector3 end = start + offset;
        float duration = Mathf.Max(0.01f, skipMoveDuration);
        float elapsed = 0f;
        while (elapsed < duration && target != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.position = Vector3.LerpUnclamped(start, end, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        if (target != null)
        {
            target.position = end;
        }
    }

    private void FinishSkipPlayback(bool showResult)
    {
        isSkipping = false;
        Cursor.lockState = CursorLockMode.None;
        if (skipButton != null)
        {
            skipButton.interactable = true;
        }

        if (showResult)
        {
            SwitchState(STATE.Result);
            resultPanel.MatchResult(false);
        }
        else
        {
            SwitchState(STATE.Normal);
        }
    }

    public IEnumerator DelayToInvokeDo(Action action, float delaySeconds)
    {//延时函数
        yield return new WaitForSeconds(delaySeconds);
        action();
    }
}
