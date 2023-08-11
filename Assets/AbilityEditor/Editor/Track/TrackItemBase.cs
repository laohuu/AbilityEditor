using System.Collections;
using System.Collections.Generic;
using AbilityEditor.Editor.Track.Scripts.Style.TrackItem;
using UnityEngine;
using UnityEngine.UIElements;


public abstract class TrackItemBase
{
    protected float frameUnitWidth;
    public abstract void Select();
    public abstract void OnSelect();
    public abstract void OnUnSelect();

    public virtual void OnConfigChanged()
    {
    }

    public virtual void ResetView()
    {
        ResetView(frameUnitWidth);
    }

    public virtual void ResetView(float frameUnitWidth)
    {
        this.frameUnitWidth = frameUnitWidth;
    }
}

public abstract class TrackItemBase<T> : TrackItemBase where T : SkillTrackBase
{
    protected T track;
    protected Color normalColor;
    protected Color selectColor;
    public SkillTrackItemStyleBase itemStyle { get; protected set; }
    protected int frameIndex;

    public int FrameIndex
    {
        get => frameIndex;
    }

    public override void Select()
    {
        AbilityEditorWindow.Instance.ShowTrackItemOnInspector(this, track);
    }

    public override void OnSelect()
    {
        itemStyle.SetBGColor(selectColor);
    }

    public override void OnUnSelect()
    {
        itemStyle.SetBGColor(normalColor);
    }
}