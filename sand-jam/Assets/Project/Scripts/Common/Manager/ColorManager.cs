using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "ColorManager", menuName = "ScriptableObjects/ColorManager", order = 1)]
public class ColorManager : ScriptableObject
{
    [SerializeField] List<TupleSerialize<Color, Material>> m_BlockColors;
    [SerializeField] List<TupleSerialize<Color, Material>> m_DoorColors;

    [SerializeField] List<Material> m_PortalColor;
    [SerializeField] List<TupleSerialize<Color, Material>> m_LockColor;

    public List<TupleSerialize<Color, Material>> blockColors => m_BlockColors;

    public Color GetColorBlock(int index)
    {
        if (index < 0 || index >= m_BlockColors.Count) return m_BlockColors[0].Value1;

        return m_BlockColors[GetIndex(index)].Value1;
    }

    public Material GetMateralBlock(Color color)
    {

        for (int i = 0; i < m_BlockColors.Count; i++)
        {
            if (m_BlockColors[i].Value1 == color)
            {
                return m_BlockColors[i].Value2;
            };
        }

        return m_BlockColors[0].Value2;
    }

    //only use in editor
    public int GetIndexBlock(Color color)
    {
        for (int i = 0; i < m_BlockColors.Count; i++)
        {
            if (m_BlockColors[i].Value1 == color) return i;
        }

        return 0;
    }

    public Material GetMateralDoor(Color color)
    {
        for (int i = 0; i < m_DoorColors.Count; i++)
        {
            if (m_DoorColors[i].Value1 == color) return m_DoorColors[i].Value2;
        }

        return m_DoorColors[0].Value2;
    }

    public Material GetColorPortal(int index)
    {
        if (index < 0 || index >= m_PortalColor.Count) return m_PortalColor[0];

        return m_PortalColor[index];
    }

    public Material GetMateralBlock(int index)
    {
        if (index >= m_BlockColors.Count) return m_BlockColors[0].Value2;

        return m_BlockColors[index].Value2;
    }

    public TupleSerialize<Color, Material> GetColorLock(int index)
    {
        if (index < 0 || index >= m_LockColor.Count) return m_LockColor[0];

        return m_LockColor[index];
    }

    int GetIndex(int index)
    {
        if (!Application.isPlaying) return index;

        return ((index + randomIndex) % m_BlockColors.Count);
    }

    int randomIndex;
    public void SetRandomValue(int value)
    {
        randomIndex = value;
    }
}
