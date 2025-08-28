using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "ScriptableObjects/LevelConfig", order = 0)]
public class LevelConfig : ScriptableObject
{
    //[SerializeField] LevelBase[] m_LevelBases;

    //public LevelBase[] levelBases => m_LevelBases;

    //public LevelBase GetLevelData(int index)
    //{
    //    if (levelBases.Length == 0) return null;

    //    int indexLevel = (index) % (levelBases.Length);
    //    return levelBases[indexLevel];
    //}
}
