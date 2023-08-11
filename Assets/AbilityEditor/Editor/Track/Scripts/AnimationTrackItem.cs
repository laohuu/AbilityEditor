using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AnimationTrackItem : TrackItemBase
{
    private const string trackItemAssetPath =
        "Assets/AbilityEditor/Editor/Track/Assets/AnimationTrack/AnimationTrackItem.uxml";

    private AnimationTrack animationTrack;
    private int frameIndex;
    private float frameUnitWidth;
    private SkillAnimationEvent animationEvent;

    public Label root { get; private set; }
    private VisualElement mainDragArea;
    private VisualElement animationOverLine;

    public void Init(AnimationTrack animationTrack, VisualElement parent, int startFrameIndex, float frameUnitWidth,
        SkillAnimationEvent animationEvent)
    {
        this.frameUnitWidth = frameUnitWidth;
        this.frameIndex = startFrameIndex;
        this.animationTrack = animationTrack;
        this.animationEvent = animationEvent;

        normalColor = new Color(0.388f, 0.850f, 0.905f, 0.5f);
        selectColor = new Color(0.388f, 0.850f, 0.905f, 1f);

        root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(trackItemAssetPath).Instantiate().Query<Label>();
        mainDragArea = root.Q<VisualElement>("Main");
        animationOverLine = root.Q<VisualElement>("OverLline");
        parent.Add(root);

        // 绑定事件
        mainDragArea.RegisterCallback<MouseDownEvent>(MouseDown);
        mainDragArea.RegisterCallback<MouseUpEvent>(MouseUp);
        mainDragArea.RegisterCallback<MouseOutEvent>(MouseOut);
        mainDragArea.RegisterCallback<MouseMoveEvent>(MouseMove);

        ResetView(frameUnitWidth);
    }

    public void ResetView(float frameUnitWidth)
    {
        this.frameUnitWidth = frameUnitWidth;
        root.text = animationEvent.AnimationClip.name;
        // 位置计算
        Vector3 mainPos = root.transform.position;
        mainPos.x = frameIndex * frameUnitWidth;
        root.transform.position = mainPos;
        root.style.width = animationEvent.DurationFrame * frameUnitWidth;

        int animationClipFrameCount =
            (int)(animationEvent.AnimationClip.length * animationEvent.AnimationClip.frameRate);
        // 计算动画结束线的位置
        if (animationClipFrameCount > animationEvent.DurationFrame)
        {
            animationOverLine.style.display = DisplayStyle.None;
        }
        else
        {
            animationOverLine.style.display = DisplayStyle.Flex;
            Vector3 overLinePos = animationOverLine.transform.position;
            overLinePos.x = animationClipFrameCount * frameUnitWidth - 1; // 线条自身宽度为2
            animationOverLine.transform.position = overLinePos;
        }
    }

    #region 鼠标交互

    private bool mouseDrag = false;
    private float startDargPosX;
    private int startDragFrameIndex;

    private void MouseDown(MouseDownEvent evt)
    {
        startDargPosX = evt.mousePosition.x;
        startDragFrameIndex = frameIndex;
        mouseDrag = true;
        // Select();
        root.style.backgroundColor = selectColor;
    }

    private void MouseUp(MouseUpEvent evt)
    {
        if (mouseDrag) ApplyDrag();
        mouseDrag = false;
    }

    private void MouseOut(MouseOutEvent evt)
    {
        if (mouseDrag) ApplyDrag();
        mouseDrag = false;
        root.style.backgroundColor = normalColor;
    }

    private void MouseMove(MouseMoveEvent evt)
    {
        if (!mouseDrag) return;
        float offsetPos = evt.mousePosition.x - startDargPosX;
        int offsetFrame = Mathf.RoundToInt(offsetPos / frameUnitWidth);
        int targetFrameIndex = startDragFrameIndex + offsetFrame;
        bool checkDrag = false;
        if (targetFrameIndex < 0) return; // 不考虑拖拽到负数的情况

        if (offsetFrame < 0)
            checkDrag = animationTrack.CheckFrameIndexOnDrag(targetFrameIndex, startDragFrameIndex, true);
        else if (offsetFrame > 0)
            checkDrag = animationTrack.CheckFrameIndexOnDrag(targetFrameIndex + animationEvent.DurationFrame,
                startDragFrameIndex, false);
        else return;

        if (!checkDrag) return;
        // 确定修改的数据
        frameIndex = targetFrameIndex;
        // 如果超过右侧边界，拓展边界
        CheckFrameCount();
        // 刷新视图
        ResetView(frameUnitWidth);
    }

    public void CheckFrameCount()
    {
        // 如果超过右侧边界，拓展边界
        if (frameIndex + animationEvent.DurationFrame > AbilityEditorWindow.Instance.SkillConfig.FrameCount)
        {
            // 保存配置导致对象无效，重新引用
            AbilityEditorWindow.Instance.CurrentFrameCount = frameIndex + animationEvent.DurationFrame;
        }
    }

    private void ApplyDrag()
    {
        if (startDragFrameIndex != frameIndex)
        {
            animationTrack.SetFrameIndex(startDragFrameIndex, frameIndex);
        }
    }

    #endregion
}