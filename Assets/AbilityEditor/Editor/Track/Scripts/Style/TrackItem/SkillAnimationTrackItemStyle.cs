using UnityEditor;
using UnityEngine.UIElements;

namespace AbilityEditor.Editor.Track.Scripts.Style.TrackItem
{
    public class SkillAnimationTrackItemStyle : SkillTrackItemStyleBase
    {
        private const string trackItemAssetPath =
            "Assets/AbilityEditor/Editor/Track/Assets/AnimationTrack/AnimationTrackItem.uxml";

        private Label titleLabel;
        public VisualElement mainDragArea { get; private set; }
        public VisualElement animationOverLine { get; private set; }

        public void Init(SkillTrackStyleBase tracStyle, int startFrameIndex, float frameUnitWidth)
        {
            titleLabel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(trackItemAssetPath).Instantiate()
                .Query<Label>();
            root = titleLabel;
            mainDragArea = root.Q<VisualElement>("Main");
            animationOverLine = root.Q<VisualElement>("OverLline");
            tracStyle.AddItem(root);
        }

        public void SetTitle(string title)
        {
            titleLabel.text = title;
        }
    }
}