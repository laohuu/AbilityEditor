using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AnimationTrack : SkillTrackBase
{
    public override string MenuAssetPath =>
        "Assets/AbilityEditor/Editor/Track/Assets/SinglineTrackStyle/SingleLineTrackMenu.uxml";

    public override string TrackAssetPath =>
        "Assets/AbilityEditor/Editor/Track/Assets/SinglineTrackStyle/SingleLineTrackContent.uxml";

    private Dictionary<int, AnimationTrackItem> trackItemDic = new Dictionary<int, AnimationTrackItem>();

    public SkillAnimationData AnimationData
    {
        get => AbilityEditorWindow.Instance.SkillConfig.SkillAnimationData;
    }

    public override void Init(VisualElement menuParent, VisualElement trackParent, int frameUnitWidth)
    {
        base.Init(menuParent, trackParent, frameUnitWidth);
        track.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
        track.RegisterCallback<DragExitedEvent>(DragExited);
        RestView();
    }

    public override void RestView(float frameWdith)
    {
        base.RestView(frameWdith);
        // 销毁当前已有
        foreach (var item in trackItemDic)
        {
            track.Remove(item.Value.root);
        }

        trackItemDic.Clear();
        if (AbilityEditorWindow.Instance.SkillConfig == null) return;

        // 根据数据绘制TrackItem
        foreach (var item in AbilityEditorWindow.Instance.SkillConfig.SkillAnimationData.FrameData)
        {
            AnimationTrackItem trackItem = new AnimationTrackItem();
            trackItem.Init(this, track, item.Key, frameWdith, item.Value);
            trackItemDic.Add(item.Key, trackItem);
        }
    }


    private void OnDragUpdate(DragUpdatedEvent evt)
    {
        // 监听用户拖拽的是否是动画
        UnityEngine.Object[] objs = DragAndDrop.objectReferences;
        AnimationClip clip = objs[0] as AnimationClip;
        if (clip != null)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
    }

    private void DragExited(DragExitedEvent evt)
    {
        // 监听用户拖拽的是否是动画
        UnityEngine.Object[] objs = DragAndDrop.objectReferences;
        AnimationClip clip = objs[0] as AnimationClip;
        if (clip != null)
        {
            // 放置动画资源
            // 当前选中的位置检测能否放置动画
            int selectFrameIndex = AbilityEditorWindow.Instance.GetFrameIndexByPos(evt.localMousePosition.x);
            bool canPlace = true;
            int durationFrame = -1; // -1代表可以用原本AniamtionClip的持续时间
            int clipFrameCount = (int)(clip.length * clip.frameRate);
            int nextTrackItem = -1;
            int currentOffset = int.MaxValue;

            foreach (var item in AbilityEditorWindow.Instance.SkillConfig.SkillAnimationData.FrameData)
            {
                // 不允许选中帧在TrackItem中间（动画事件的起点到他的终点之间）
                if (selectFrameIndex > item.Key && selectFrameIndex < item.Value.DurationFrame + item.Key)
                {
                    // 不能放置
                    canPlace = false;
                    break;
                }

                // 找到右侧的最近的TrakcItem
                if (item.Key > selectFrameIndex)
                {
                    int tempOffset = item.Key - selectFrameIndex;
                    if (tempOffset < currentOffset)
                    {
                        currentOffset = tempOffset;
                        nextTrackItem = item.Key;
                    }
                }
            }

            // 实际的放置
            if (canPlace)
            {
                // 右边有其他TrackItem，要考虑Track不能重叠的问题
                if (nextTrackItem != -1)
                {
                    int offset = clipFrameCount - currentOffset;
                    if (offset < 0) durationFrame = clipFrameCount;
                    else durationFrame = currentOffset;
                }
                // 右边啥都没有
                else durationFrame = clipFrameCount;

                // 构建动画数据
                SkillAnimationEvent animationEvent = new SkillAnimationEvent()
                {
                    AnimationClip = clip,
                    DurationFrame = durationFrame,
                    TransitionTime = 0.25f
                };

                // 保存新增的动画数据
                AbilityEditorWindow.Instance.SkillConfig.SkillAnimationData.FrameData.Add(selectFrameIndex,
                    animationEvent);
                AbilityEditorWindow.Instance.SaveConfig();

                // 同步修改编辑器视图
                RestView();
            }
        }
    }

    public bool CheckFrameIndexOnDrag(int targetIndex, int selfIndex, bool isLeft)
    {
        foreach (var item in AbilityEditorWindow.Instance.SkillConfig.SkillAnimationData.FrameData)
        {
            // 规避拖拽时考虑自身
            if (item.Key == selfIndex) continue;

            // 向左移动 && 原先在其右边 && 目标没有重叠
            if (isLeft && selfIndex > item.Key && targetIndex < item.Key + item.Value.DurationFrame)
            {
                return false;
            }
            // 向右移动 && 原先在其左边 && 目标没有重叠
            else if (!isLeft && selfIndex < item.Key && targetIndex > item.Key)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 将oldIndex的数据变为newIndex
    /// </summary>
    public void SetFrameIndex(int oldIndex, int newIndex)
    {
        if (AnimationData.FrameData.Remove(oldIndex, out SkillAnimationEvent animationEvent))
        {
            AnimationData.FrameData.Add(newIndex, animationEvent);
            trackItemDic.Remove(oldIndex, out AnimationTrackItem animationTrackItem);
            trackItemDic.Add(newIndex, animationTrackItem);
            AbilityEditorWindow.Instance.SaveConfig();
        }
    }

    public override void DeleteTrackItem(int frameIndex)
    {
        AnimationData.FrameData.Remove(frameIndex);
        if (trackItemDic.Remove(frameIndex, out AnimationTrackItem item))
        {
            // trackStyle.DeleteItem(item.itemStyle.root);
            item.Delete();
        }

        AbilityEditorWindow.Instance.SaveConfig();
    }
}