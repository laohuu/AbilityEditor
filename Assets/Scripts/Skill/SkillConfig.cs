using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Sirenix.Serialization;


[CreateAssetMenu(menuName = "Config/SkillConfig", fileName = "SkillConfig")]
public class SkillConfig : ConfigBase
{
    [LabelText("技能名称")] public string SkillName;
    [LabelText("帧数上限")] public int FrameCount = 100;
    [LabelText("帧率")] public int FrameRote = 30;

    [NonSerialized, OdinSerialize] public SkillAnimationData SkillAnimationData = new SkillAnimationData();

#if UNITY_EDITOR
    private static Action skillConfigValidate;

    public static void SetValidateAction(Action action)
    {
        skillConfigValidate = action;
    }

    private void OnValidate()
    {
        skillConfigValidate?.Invoke();
    }
#endif
}


/// <summary>
/// 技能动画数据
/// </summary>
[Serializable]
public class SkillAnimationData
{
    /// <summary>
    /// 动画帧事件
    /// Key:帧数
    /// Value:事件数据
    /// </summary>
    [NonSerialized, OdinSerialize]
    public Dictionary<int, SkillAnimationEvent> FrameData = new Dictionary<int, SkillAnimationEvent>();
}


/// <summary>
/// 帧事件基类
/// </summary>
[Serializable]
public abstract class SkillFrameEventBase
{
}

/// <summary>
/// 动画帧事件
/// </summary>
public class SkillAnimationEvent : SkillFrameEventBase
{
    public AnimationClip AnimationClip;
    public bool ApplyRootMotion;
    public float TransitionTime = 0.25f;
#if UNITY_EDITOR
    public int DurationFrame;
#endif
}