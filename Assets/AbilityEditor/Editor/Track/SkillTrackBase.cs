using UnityEditor;
using UnityEngine.UIElements;

public abstract class SkillTrackBase
{
    protected float frameWidth;
    protected VisualElement menuParent;
    protected VisualElement trackParent;
    protected VisualElement menu;
    protected VisualElement track;


    public abstract string MenuAssetPath { get; }
    public abstract string TrackAssetPath { get; }

    public virtual void Init(VisualElement menuParent, VisualElement trackParent, int frameUnitWidth)
    {
        this.menuParent = menuParent;
        this.trackParent = trackParent;
        this.frameWidth = frameUnitWidth;
        menu = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MenuAssetPath).Instantiate().Query().ToList()[1];
        menuParent.Add(menu);
        track = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TrackAssetPath).Instantiate().Query().ToList()[1];
        trackParent.Add(track);
    }

    public virtual void ResetView()
    {
        ResetView(frameWidth);
    }

    public virtual void ResetView(float frameWdith)
    {
        this.frameWidth = frameWdith;
    }

    public virtual void DeleteTrackItem(int frameIndex)
    {
    }

    public virtual void OnConfigChanged()
    {
    }

    public virtual void TickView(int frameIndex)
    {
    }
}