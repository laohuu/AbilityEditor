using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/SkillConfig", fileName = "SkillConfig")]
public class SkillConfig : ConfigBase
{
    public string SkillName;
    public int FrameCount = 100;
    public int FrameRote = 30;
}