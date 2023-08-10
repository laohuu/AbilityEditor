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
        root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(trackItemAssetPath).Instantiate().Query<Label>();
        mainDragArea = root.Q<VisualElement>("Main");
        animationOverLine = root.Q<VisualElement>("OverLline");
        parent.Add(root);
        RestView(frameUnitWidth);
    }

    public void RestView(float frameUnitWidth)
    {
        Debug.Log(frameIndex);
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
}