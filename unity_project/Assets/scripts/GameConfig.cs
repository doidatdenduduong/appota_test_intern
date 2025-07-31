using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "NutsBolts/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Range(10, 1000)] public float     ray_distance = 1000f;
    public                   LayerMask ray_mask;

    public ColorAndMat[]       color_and_mats;
    public float               offset_y_stack       = 0.5f;
    public Vector3             nut_scale            = Vector3.one;
    public float               nut_scale_multiplier = 1.5f;
    public LevelDesignConfig[] level_designs;
    public float               move_duration   = .5f;
    public float               rotate_duration = .5f;
    public float               rotate_speed = 720f;

    public Nut    nut_prefab;
    public Pillar pillar_prefab;


    public bool try_get_data(ColorId colorId, out ColorAndMat data)
    {
        foreach (var colorAndMat in color_and_mats)
        {
            if (colorAndMat.color_id == colorId)
            {
                data = colorAndMat;
                return true;
            }
        }

        Debug.LogError($"ColorId {colorId} not found in GameConfig.");
        data = null;
        return false;
    }
}

public enum ColorId
{
    None,
    Red,
    Blue,
    Green,
    Yellow,
    Purple
}

[Serializable]
public class ColorAndMat
{
    public ColorId  color_id;
    public Material material;
}