using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "ColorManager", menuName = "ScriptableObjects/ColorManager", order = 1)]
public class ColorManager : ScriptableObject
{
    [SerializeField] List<TupleSerialize<Color, Material>> m_SandColors;

    public Color GetSandColor(int index)
    {
        if (index < 0 || index >= m_SandColors.Count)
        {
            Debug.LogWarning($"Index {index} is out of range for sand colors. Returning default color.");
            return Color.white;
        }
        return m_SandColors[index].Value1;
    }

    public int GetSandColorCount()
    {
        return m_SandColors.Count;
    }
}
