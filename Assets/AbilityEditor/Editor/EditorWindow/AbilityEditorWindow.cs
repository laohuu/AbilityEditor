using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class AbilityEditorWindow : EditorWindow
{
    public static AbilityEditorWindow Instance;

    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/AbilityEditorWindow")]
    public static void ShowExample()
    {
        AbilityEditorWindow wnd = GetWindow<AbilityEditorWindow>();
        wnd.titleContent = new GUIContent("AbilityEditorWindow");
    }

    public void CreateGUI()
    {
        Instance = this;

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        InitTopMenu();
        InitConsole();
        InitTimerShaft();
        InitContent();

        if (m_SkillConfig)
        {
            m_SkillConfigObjectField.value = m_SkillConfig;
            CurrentFrameCount = m_SkillConfig.FrameCount;
        }
        else
        {
            CurrentFrameCount = 100;
        }

        CurrentSelectFrameIndex = 0;
    }

    #region TopMenu

    private const string m_SkillEditorScenePath = "Assets/Scenes/SampleScene.unity";
    private const string m_PreviewCharacterParentPath = "PreviewCharacterRoot";
    private string m_OldScenePath;

    private Button m_LoadSceneBtn;
    private Button m_SkillBasicBtn;

    private ObjectField m_PreviewCharacterPrefabObjectField;
    private ObjectField m_PreviewCharacterObjectField;
    private ObjectField m_SkillConfigObjectField;

    private GameObject m_CurrentPreviewCharacterPrefab;
    private GameObject m_CurrentPreviewCharacterObj;

    private void InitTopMenu()
    {
        m_LoadSceneBtn = rootVisualElement.Q<Button>("LoadSceneBtn");
        m_LoadSceneBtn.clicked += OnLoadSceneBtnClicked;
        m_SkillBasicBtn = rootVisualElement.Q<Button>("SkillBasicBtn");
        m_SkillBasicBtn.clicked += OnSkillBasicBtnClicked;


        m_PreviewCharacterPrefabObjectField = rootVisualElement.Q<ObjectField>("PreviewCharacter");
        m_PreviewCharacterPrefabObjectField.RegisterValueChangedCallback(PreviewCharacterObjectFieldValueChanged);

        m_SkillConfigObjectField = rootVisualElement.Q<ObjectField>("SkillConfigObjectField");
        m_SkillConfigObjectField.RegisterValueChangedCallback(SkillConfigObjectFieldValueChanged);
    }

    private void OnLoadSceneBtnClicked()
    {
        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        // 当前是编辑器场景，但是玩家依然点击了加载编辑器场景，没有意义
        if (currentScenePath == m_SkillEditorScenePath) return;
        m_OldScenePath = currentScenePath;
        EditorSceneManager.OpenScene(m_SkillEditorScenePath);
    }

    private void OnSkillBasicBtnClicked()
    {
        if (SkillConfig != null)
        {
            Selection.activeObject = SkillConfig;
        }
    }

    private void PreviewCharacterObjectFieldValueChanged(ChangeEvent<Object> evt)
    {
        // 避免在其他场景实例化
        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        if (currentScenePath != m_SkillEditorScenePath)
        {
            m_PreviewCharacterPrefabObjectField.value = null;
            return;
        }

        // 值相等，设置无效
        if (evt.newValue == m_CurrentPreviewCharacterPrefab) return;

        m_CurrentPreviewCharacterPrefab = (GameObject)evt.newValue;

        // 销毁旧的
        if (m_CurrentPreviewCharacterObj != null) DestroyImmediate(m_CurrentPreviewCharacterObj);
        Transform parent = GameObject.Find(m_PreviewCharacterParentPath).transform;
        if (parent != null && parent.childCount > 0)
        {
            DestroyImmediate(parent.GetChild(0).gameObject);
        }

        // 实例化新的
        if (evt.newValue != null)
        {
            m_CurrentPreviewCharacterObj =
                Instantiate(evt.newValue as GameObject, Vector3.zero, Quaternion.identity, parent);
            m_CurrentPreviewCharacterObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            // PreviewCharacterObjectField.value = currentPreviewCharacterObj;
        }
    }

    private void SkillConfigObjectFieldValueChanged(ChangeEvent<Object> evt)
    {
        m_SkillConfig = evt.newValue as SkillConfig;
        // 刷新轨道
        ResetTrack();
        CurrentSelectFrameIndex = 0;
        if (m_SkillConfig == null)
        {
            CurrentFrameCount = 100;
            return;
        }

        CurrentFrameCount = m_SkillConfig.FrameCount;
    }

    #endregion

    #region Config

    private SkillConfig m_SkillConfig;

    public SkillConfig SkillConfig
    {
        get => m_SkillConfig;
    }

    private SkillEditorConfig skillEditorConfig = new SkillEditorConfig();

    public void SaveConfig()
    {
        if (m_SkillConfig != null)
        {
            EditorUtility.SetDirty(m_SkillConfig);
            AssetDatabase.SaveAssetIfDirty(m_SkillConfig);
            ResetTrackData();
        }
    }

    private void ResetTrackData()
    {
        // 重新引用一下数据
        // for (int i = 0; i < trackList.Count; i++)
        // {
        //     // trackList[i].OnConfigChanged();
        // }
    }

    #endregion

    #region TimerShaft

    private IMGUIContainer timerShaft;
    private IMGUIContainer selectLine;
    private VisualElement contentContainer;
    private VisualElement contentViewPort;
    private int currentSelectFrameIndex = -1;

    private int CurrentSelectFrameIndex
    {
        get => currentSelectFrameIndex;
        set
        {
            // 如果超出范围，更新最大帧
            if (value > CurrentFrameCount) CurrentFrameCount = value;
            currentSelectFrameIndex = Mathf.Clamp(value, 0, CurrentFrameCount);
            CurrentFrameField.value = currentSelectFrameIndex;
            UpdateTimerShaftView();
        }
    }

    private int m_CurrentFrameCount;

    public int CurrentFrameCount
    {
        get => m_CurrentFrameCount;
        set
        {
            m_CurrentFrameCount = value;
            FrameCountField.value = value;
            // 同步给SkillConfig
            if (m_SkillConfig != null)
            {
                m_SkillConfig.FrameCount = m_CurrentFrameCount;
                SaveConfig();
            }

            // Content区域的尺寸变化
            UpdateContentSize();
        }
    }


    // 当前内容区域的偏移坐标
    private float contentOffsetPos
    {
        get => Mathf.Abs(contentContainer.transform.position.x);
    }

    private float currentSlectFramePos
    {
        get => currentSelectFrameIndex * skillEditorConfig.frameUnitWidth;
    }

    private bool timerShaftIsMouseEnter = false;

    private void InitTimerShaft()
    {
        ScrollView mainContentView = rootVisualElement.Q<ScrollView>("MainContentView");
        contentContainer = mainContentView.Q<VisualElement>("unity-content-container");
        contentViewPort = mainContentView.Q<VisualElement>("unity-content-viewport");

        timerShaft = rootVisualElement.Q<IMGUIContainer>("TimerShaft");
        timerShaft.onGUIHandler = DrawTimerShaft;
        timerShaft.RegisterCallback<WheelEvent>(TimerShaftWheel);
        timerShaft.RegisterCallback<MouseDownEvent>(TimerShaftMouseDown);
        timerShaft.RegisterCallback<MouseMoveEvent>(TimerShaftMouseMove);
        timerShaft.RegisterCallback<MouseUpEvent>(TimerShaftMouseUp);
        timerShaft.RegisterCallback<MouseOutEvent>(TimerShaftMouseOut);


        selectLine = rootVisualElement.Q<IMGUIContainer>("SelectLine");
        selectLine.onGUIHandler = DrawSelectLine;
    }

    private void DrawTimerShaft()
    {
        Handles.BeginGUI();
        Handles.color = Color.white;
        Rect rect = timerShaft.contentRect;
        // 起始索引
        int index = Mathf.CeilToInt(contentOffsetPos / skillEditorConfig.frameUnitWidth);
        // 计算绘制起点的偏移
        float startOffset = 0;
        if (index > 0)
            startOffset = skillEditorConfig.frameUnitWidth - (contentOffsetPos % skillEditorConfig.frameUnitWidth);

        // 步长
        int tickStep = SkillEditorConfig.maxFrameWidthLV + 1 -
                       (skillEditorConfig.frameUnitWidth / SkillEditorConfig.standFrameUnitWidth);
        tickStep = tickStep / 2; // 可能 1 / 2 = 0的情况
        if (tickStep == 0) tickStep = 1; // 避免为0

        for (float i = startOffset; i < rect.width; i += skillEditorConfig.frameUnitWidth)
        {
            if (index % tickStep == 0)
            {
                Handles.DrawLine(new Vector3(i, rect.height - 10), new Vector3(i, rect.height));
                string indexStr = index.ToString();
                GUI.Label(new Rect(i - indexStr.Length * 4.5f, 0, 35, 20), indexStr);
            }
            else
            {
                Handles.DrawLine(new Vector3(i, rect.height - 5), new Vector3(i, rect.height));
            }

            index += 1;
        }

        Handles.EndGUI();
    }

    private void DrawSelectLine()
    {
        // 判断当前选中帧是否在视图范围内
        if (!(currentSlectFramePos >= contentOffsetPos)) return;
        Handles.BeginGUI();

        Handles.color = Color.white;
        float x = currentSlectFramePos - contentOffsetPos;
        Handles.DrawLine(new Vector3(x, 0),
            new Vector3(x, contentViewPort.contentRect.height + timerShaft.contentRect.height));

        Handles.EndGUI();
    }

    private void TimerShaftWheel(WheelEvent evt)
    {
        int delta = (int)evt.delta.y;
        skillEditorConfig.frameUnitWidth = Mathf.Clamp(skillEditorConfig.frameUnitWidth - delta,
            SkillEditorConfig.standFrameUnitWidth,
            SkillEditorConfig.maxFrameWidthLV * SkillEditorConfig.standFrameUnitWidth);
        UpdateTimerShaftView();
        UpdateContentSize();
    }

    private void TimerShaftMouseDown(MouseDownEvent evt)
    {
        // 让选中线得位置卡在帧的位置上
        timerShaftIsMouseEnter = true;
        IsPlaying = false;
        int newValue = GetFrameIndexByMousePos(evt.localMousePosition.x);
        if (newValue != CurrentSelectFrameIndex)
        {
            CurrentSelectFrameIndex = newValue;
        }
    }

    private void TimerShaftMouseMove(MouseMoveEvent evt)
    {
        if (timerShaftIsMouseEnter)
        {
            int newValue = GetFrameIndexByMousePos(evt.localMousePosition.x);
            if (newValue != CurrentSelectFrameIndex)
            {
                CurrentSelectFrameIndex = newValue;
            }
        }
    }

    private void TimerShaftMouseUp(MouseUpEvent evt)
    {
        timerShaftIsMouseEnter = false;
    }

    private void TimerShaftMouseOut(MouseOutEvent evt)
    {
        timerShaftIsMouseEnter = false;
    }


    /// <summary>
    /// 根据鼠标坐标获取帧索引
    /// </summary>
    private int GetFrameIndexByMousePos(float x)
    {
        return GetFrameIndexByPos(x + contentOffsetPos);
    }

    public int GetFrameIndexByPos(float x)
    {
        return Mathf.RoundToInt(x / skillEditorConfig.frameUnitWidth);
    }

    private void UpdateTimerShaftView()
    {
        // 标志为需要重新绘制的
        timerShaft.MarkDirtyLayout();
        selectLine.MarkDirtyLayout();
    }

    #endregion

    #region Preview

    private bool isPlaying;

    public bool IsPlaying
    {
        get => isPlaying;
        set
        {
            isPlaying = value;
            if (isPlaying)
            {
                startTime = DateTime.Now;
                startFrameIndex = currentSelectFrameIndex;
            }
        }
    }

    private DateTime startTime;
    private int startFrameIndex;

    private void Update()
    {
        if (IsPlaying)
        {
            // 得到时间差
            float time = (float)DateTime.Now.Subtract(startTime).TotalSeconds;

            // 确定时间轴的帧率
            float frameRote;
            if (m_SkillConfig != null) frameRote = m_SkillConfig.FrameRote;
            else frameRote = skillEditorConfig.defaultFrameRote;

            // 根据时间差计算当前的选中帧
            CurrentSelectFrameIndex = (int)((time * frameRote) + startFrameIndex);
            // 到达最后一帧自动暂停
            if (CurrentSelectFrameIndex == CurrentFrameCount)
            {
                IsPlaying = false;
            }
        }
    }

    private void TickSkill()
    {
        // 驱动技能表现
        if (m_SkillConfig != null && m_CurrentPreviewCharacterObj != null)
        {
            // for (int i = 0; i < trackList.Count; i++)
            // {
            //     trackList[i].TickView(currentSelectFrameIndex);
            // }
        }
    }

    #endregion


    #region Console

    private Button PreviouFrameButton;
    private Button PlayButton;
    private Button NextFrameButton;
    private IntegerField CurrentFrameField;
    private IntegerField FrameCountField;

    private void InitConsole()
    {
        PreviouFrameButton = rootVisualElement.Q<Button>("PreviouFrameButton");
        PreviouFrameButton.clicked += PreviouFrameButtonClick;

        PlayButton = rootVisualElement.Q<Button>("PlayButton");
        PlayButton.clicked += PlayButtonClick;

        NextFrameButton = rootVisualElement.Q<Button>("NextFrameButton");
        NextFrameButton.clicked += NextFrameButtonClick;

        CurrentFrameField = rootVisualElement.Q<IntegerField>("CurrentFrameField");
        CurrentFrameField.RegisterValueChangedCallback(CurrentFrameFieldValueChanged);

        FrameCountField = rootVisualElement.Q<IntegerField>("FrameCountField");
        FrameCountField.RegisterValueChangedCallback(FrameCountValueChanged);
    }

    private void PreviouFrameButtonClick()
    {
        IsPlaying = false;
        CurrentSelectFrameIndex -= 1;
    }

    private void PlayButtonClick()
    {
        IsPlaying = !IsPlaying;
    }

    private void NextFrameButtonClick()
    {
        IsPlaying = false;
        CurrentSelectFrameIndex += 1;
    }

    private void CurrentFrameFieldValueChanged(ChangeEvent<int> evt)
    {
        if (CurrentSelectFrameIndex != evt.newValue) CurrentSelectFrameIndex = evt.newValue;
    }

    private void FrameCountValueChanged(ChangeEvent<int> evt)
    {
        if (CurrentFrameCount != evt.newValue) CurrentFrameCount = evt.newValue;
    }

    #endregion

    #region Track

    private VisualElement trackMenuParent;
    private VisualElement contentListView;
    private ScrollView mainContentView;
    private List<SkillTrackBase> trackList = new List<SkillTrackBase>();

    private void InitContent()
    {
        contentListView = rootVisualElement.Q<VisualElement>("ContentListView");
        trackMenuParent = rootVisualElement.Q<VisualElement>("TrackMenuList");
        mainContentView = rootVisualElement.Q<ScrollView>("MainContentView");
        mainContentView.verticalScroller.valueChanged += ContentVerticalScorllerValueChanged;
        UpdateContentSize();
        InitTrack();
    }

    private void ContentVerticalScorllerValueChanged(float value)
    {
        Vector3 pos = trackMenuParent.transform.position;
        pos.y = contentContainer.transform.position.y;
        trackMenuParent.transform.position = pos;
    }


    private void InitTrack()
    {
        InitAnimationTrack();
    }

    private void ResetTrack()
    {
        for (int i = 0; i < trackList.Count; i++)
        {
            trackList[i].RestView(skillEditorConfig.frameUnitWidth);
        }
    }

    private void InitAnimationTrack()
    {
        AnimationTrack animationTrack = new AnimationTrack();
        animationTrack.Init(trackMenuParent, contentListView, skillEditorConfig.frameUnitWidth);
        trackList.Add(animationTrack);
    }

    private void UpdateContentSize()
    {
        contentListView.style.width = skillEditorConfig.frameUnitWidth * CurrentFrameCount;
    }

    #endregion
}