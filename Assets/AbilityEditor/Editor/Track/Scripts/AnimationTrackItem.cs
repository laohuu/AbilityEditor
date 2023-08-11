using System.Collections;
using System.Collections.Generic;
using AbilityEditor.Editor.Track.Scripts.Style.TrackItem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AnimationTrackItem : TrackItemBase<AnimationTrack>
{
    private SkillAnimationEvent animationEvent;

    public SkillAnimationEvent AnimationEvent
    {
        get => animationEvent;
    }

    private SkillAnimationTrackItemStyle trackItemStyle;

    public void Init(AnimationTrack animationTrack, SkillTrackStyleBase parentTrackStyle, int startFrameIndex,
        float frameUnitWidth, SkillAnimationEvent animationEvent)
    {
        this.frameUnitWidth = frameUnitWidth;
        this.frameIndex = startFrameIndex;
        track = animationTrack;
        this.animationEvent = animationEvent;

        trackItemStyle = new SkillAnimationTrackItemStyle();
        itemStyle = trackItemStyle;
        trackItemStyle.Init(parentTrackStyle, startFrameIndex, frameUnitWidth);

        normalColor = new Color(0.388f, 0.850f, 0.905f, 0.5f);
        selectColor = new Color(0.388f, 0.850f, 0.905f, 1f);
        OnUnSelect();
        // 绑定事件
        trackItemStyle.mainDragArea.RegisterCallback<MouseDownEvent>(MouseDown);
        trackItemStyle.mainDragArea.RegisterCallback<MouseUpEvent>(MouseUp);
        trackItemStyle.mainDragArea.RegisterCallback<MouseOutEvent>(MouseOut);
        trackItemStyle.mainDragArea.RegisterCallback<MouseMoveEvent>(MouseMove);
        ResetView(frameUnitWidth);
    }

    public override void ResetView(float frameUnitWidth)
    {
        this.frameUnitWidth = frameUnitWidth;
        trackItemStyle.SetTitle(animationEvent.AnimationClip.name);
        // 位置计算
        trackItemStyle.SetPosition(frameIndex * frameUnitWidth);
        trackItemStyle.SetWidth(animationEvent.DurationFrame * frameUnitWidth);

        int animationClipFrameCount =
            (int)(animationEvent.AnimationClip.length * animationEvent.AnimationClip.frameRate);
        // 计算动画结束线的位置
        if (animationClipFrameCount > animationEvent.DurationFrame)
        {
            trackItemStyle.animationOverLine.style.display = DisplayStyle.None;
        }
        else
        {
            trackItemStyle.animationOverLine.style.display = DisplayStyle.Flex;
            Vector3 overLinePos = trackItemStyle.animationOverLine.transform.position;
            overLinePos.x = animationClipFrameCount * frameUnitWidth - 1; // 线条自身宽度为2
            trackItemStyle.animationOverLine.transform.position = overLinePos;
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
        Select();
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
    }

    private void MouseMove(MouseMoveEvent evt)
    {
        if (mouseDrag)
        {
            float offsetPos = evt.mousePosition.x - startDargPosX;
            int offsetFrame = Mathf.RoundToInt(offsetPos / frameUnitWidth);
            int targetFrameIndex = startDragFrameIndex + offsetFrame;
            bool checkDrag = false;
            if (targetFrameIndex < 0) return; // 不考虑拖拽到负数的情况
            if (offsetFrame < 0) checkDrag = track.CheckFrameIndexOnDrag(targetFrameIndex, startDragFrameIndex, true);
            else if (offsetFrame > 0)
                checkDrag = track.CheckFrameIndexOnDrag(targetFrameIndex + animationEvent.DurationFrame,
                    startDragFrameIndex, false);
            else return;

            if (checkDrag)
            {
                // 确定修改的数据
                frameIndex = targetFrameIndex;
                // 如果超过右侧边界，拓展边界
                CheckFrameCount();
                // 刷新视图
                ResetView(frameUnitWidth);
            }
        }
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
            track.SetFrameIndex(startDragFrameIndex, frameIndex);
            SkillEditorInspector.Instance.SetTrackItemFrameIndex(frameIndex);
        }
    }

    #endregion

    public override void OnConfigChanged()
    {
        animationEvent = track.AnimationData.FrameData[frameIndex];
    }
}