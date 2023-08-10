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

    public override void Init(VisualElement menuParent, VisualElement trackParent, int frameUnitWidth)
    {
        base.Init(menuParent, trackParent, frameUnitWidth);
        track.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
        track.RegisterCallback<DragExitedEvent>(DragExited);
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
            Debug.Log(selectFrameIndex);
            // TODO:检查选中帧不在任何已有的TrackItem之间
        }
    }
}