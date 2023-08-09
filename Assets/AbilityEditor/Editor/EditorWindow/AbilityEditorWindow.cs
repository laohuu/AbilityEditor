using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class AbilityEditorWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/AbilityEditorWindow")]
    public static void ShowExample()
    {
        AbilityEditorWindow wnd = GetWindow<AbilityEditorWindow>();
        wnd.titleContent = new GUIContent("AbilityEditorWindow");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        InitTopMenu();
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
        // ResetTrack();
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
        // // 重新引用一下数据
        // for (int i = 0; i < trackList.Count; i++)
        // {
        //     trackList[i].OnConfigChanged();
        // }
    }

    #endregion

    #region TimerShaft

    private int m_CurrentSelectFrameIndex = -1;

    private int CurrentSelectFrameIndex
    {
        get => m_CurrentSelectFrameIndex;
        set
        {
            // 如果超出范围，更新最大帧
            if (value > CurrentFrameCount) CurrentFrameCount = value;
            m_CurrentSelectFrameIndex = Mathf.Clamp(value, 0, CurrentFrameCount);
        }
    }

    private int m_CurrentFrameCount;

    public int CurrentFrameCount
    {
        get => m_CurrentFrameCount;
        set { m_CurrentFrameCount = value; }
    }

    #endregion
}