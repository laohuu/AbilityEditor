using AbilityEditor.Editor.Track.Scripts.Style.TrackItem;
using UnityEditor;
using UnityEngine.UIElements;


namespace AbilityEditor.Editor.Track.Scripts.Style.Track
{
    public class SkillSingleLineTrackStyle : SkillTrackStyleBase
    {
        private const string MenuAssetPath =
            "Assets/AbilityEditor/Editor/Track/Assets/SinglineTrackStyle/SingleLineTrackMenu.uxml";

        private const string TrackAssetPath =
            "Assets/AbilityEditor/Editor/Track/Assets/SinglineTrackStyle/SingleLineTrackContent.uxml";

        public void Init(VisualElement menuParent, VisualElement contentParent, string title)
        {
            this.menuParent = menuParent;
            this.menuParent = contentParent;
            menuRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MenuAssetPath).Instantiate().Query().ToList()[1];
            titleLabel = (Label)menuRoot;
            titleLabel.text = title;
            contentRoot =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TrackAssetPath).Instantiate().Query().ToList()[1];
            menuParent.Add(menuRoot);
            contentParent.Add(contentRoot);
        }
    }
}