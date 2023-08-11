using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(AbilityEditorWindow))]
public class SkillEditorInspector : Editor
{
    public static SkillEditorInspector Instance;
    private static TrackItemBase currentTrackItem;
    private static SkillTrackBase currentTrack;

    public static void SetTrackItem(TrackItemBase trackItem, SkillTrackBase track)
    {
        if (currentTrackItem != null)
        {
            currentTrackItem.OnUnSelect();
        }

        currentTrackItem = trackItem;
        currentTrack = track;
        currentTrackItem.OnSelect();

        // 避免已经打开了Inspector，不刷新数据
        if (Instance != null) Instance.Show();
    }

    private void OnDestroy()
    {
        // 说明窗口卸载
        if (currentTrackItem != null)
        {
            currentTrackItem.OnUnSelect();
            currentTrackItem = null;
            currentTrack = null;
        }
    }

    private VisualElement root;

    public override VisualElement CreateInspectorGUI()
    {
        Instance = this;
        root = new VisualElement();
        Show();
        return root;
    }

    private void Show()
    {
        Clean();
        if (currentTrackItem == null) return;

        // 目前只有动画这一种情况
        if (currentTrackItem.GetType() == typeof(AnimationTrackItem))
        {
            DrawAnimationTrackItem((AnimationTrackItem)currentTrackItem);
        }
    }

    private int trackItemFrameIndex;

    public void SetTrackItemFrameIndex(int trackItemFrameIndex)
    {
        this.trackItemFrameIndex = trackItemFrameIndex;
    }

    private Label clipFrameLabel;
    private Toggle rootMotionToggle;
    private Label isLoopLable;
    private IntegerField durationField;

    private void DrawAnimationTrackItem(AnimationTrackItem animationTrackItem)
    {
        trackItemFrameIndex = animationTrackItem.FrameIndex;
        // 动画资源
        ObjectField animationClipAssetField = new ObjectField("动画资源");
        animationClipAssetField.objectType = typeof(AnimationClip);
        animationClipAssetField.value = animationTrackItem.AnimationEvent.AnimationClip;
        animationClipAssetField.RegisterValueChangedCallback(AnimationClipAssetFieldValueChanged);
        root.Add(animationClipAssetField);

        // 根运动
        rootMotionToggle = new Toggle("应用根运动");
        rootMotionToggle.value = animationTrackItem.AnimationEvent.ApplyRootMotion;
        rootMotionToggle.RegisterValueChangedCallback(RootMotionToggleValueChanged);
        root.Add(rootMotionToggle);

        // 轨道长度
        durationField = new IntegerField("轨道长度");
        durationField.value = animationTrackItem.AnimationEvent.DurationFrame;
        durationField.RegisterValueChangedCallback(DurationFieldValueChanged);
        root.Add(durationField);

        // 过渡时间
        FloatField transitionTimeField = new FloatField("过渡时间");
        transitionTimeField.value = animationTrackItem.AnimationEvent.TransitionTime;
        transitionTimeField.RegisterValueChangedCallback(TransitionTimeFieldValueChanged);
        root.Add(transitionTimeField);

        // 动画相关的信息
        int clipFrameCount = (int)(animationTrackItem.AnimationEvent.AnimationClip.length *
                                   animationTrackItem.AnimationEvent.AnimationClip.frameRate);
        clipFrameLabel = new Label("动画资源长度:" + clipFrameCount);
        root.Add(clipFrameLabel);
        isLoopLable = new Label("循环动画:" + animationTrackItem.AnimationEvent.AnimationClip.isLooping);
        root.Add(isLoopLable);

        // 删除
        Button deleteButton = new Button(DeleteButtonClick);
        deleteButton.text = "删除";
        deleteButton.style.backgroundColor = new Color(1, 0, 0, 0.5f);
        root.Add(deleteButton);
    }

    private void AnimationClipAssetFieldValueChanged(ChangeEvent<UnityEngine.Object> evt)
    {
        AnimationClip clip = evt.newValue as AnimationClip;
        // 修改自身显示效果
        clipFrameLabel.text = "动画资源长度:" + ((int)(clip.length * clip.frameRate));
        isLoopLable.text = "循环动画:" + clip.isLooping;
        // 保存到配置
        ((AnimationTrackItem)currentTrackItem).AnimationEvent.AnimationClip = clip;
        AbilityEditorWindow.Instance.SaveConfig();
        currentTrackItem.ResetView();
    }

    private void RootMotionToggleValueChanged(ChangeEvent<bool> evt)
    {
        (currentTrackItem as AnimationTrackItem).AnimationEvent.ApplyRootMotion = evt.newValue;
        AbilityEditorWindow.Instance.SaveConfig();
    }

    private void DurationFieldValueChanged(ChangeEvent<int> evt)
    {
        int value = evt.newValue;
        // 安全校验
        if ((currentTrack as AnimationTrack).CheckFrameIndexOnDrag(trackItemFrameIndex + value, trackItemFrameIndex,
                false))
        {
            // 修改数据，刷新视图
            (currentTrackItem as AnimationTrackItem).AnimationEvent.DurationFrame = value;
            (currentTrackItem as AnimationTrackItem).CheckFrameCount();
            AbilityEditorWindow.Instance.SaveConfig();
            currentTrackItem.ResetView();
        }
        else
        {
            durationField.value = evt.previousValue;
        }
    }

    private void TransitionTimeFieldValueChanged(ChangeEvent<float> evt)
    {
        (currentTrackItem as AnimationTrackItem).AnimationEvent.TransitionTime = evt.newValue;
    }

    private void DeleteButtonClick()
    {
        currentTrack.DeleteTrackItem(trackItemFrameIndex); // 此函数提供保存和刷新视图逻辑
        Selection.activeObject = null;
    }

    private void Clean()
    {
        if (root == null) return;
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            root.RemoveAt(i);
        }
    }
}