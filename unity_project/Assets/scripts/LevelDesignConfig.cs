using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDesignConfig", menuName = "NutsBolts/LevelDesignConfig")]
public class LevelDesignConfig : ScriptableObject
{
    [Range(3, 10)] public int            max_stack = 3;
    public                ConfigPillar[] pillars;

    public int get_max_score()
    {
        var result = 0;
        foreach (var pillar in pillars)
        {
            result += pillar.color_ids.Length;
        }

       return result;
    }
}

[Serializable]
public class ConfigPillar
{
    public ColorId[] color_ids;
}