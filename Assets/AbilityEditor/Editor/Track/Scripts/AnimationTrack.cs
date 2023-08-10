using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrack : SkillTrackBase
{
    public override string MenuAssetPath =>
        "Assets/AbilityEditor/Editor/Track/Assets/SinglineTrackStyle/SingleLineTrackContent.uxml";

    public override string TrackAssetPath =>
        "Assets/AbilityEditor/Editor/Track/Assets/AnimationTrack/AnimationTrackItem.uxml";
}