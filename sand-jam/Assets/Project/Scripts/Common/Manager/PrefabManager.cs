using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabManager", menuName = "ScriptableObjects/PrefabManager", order = 1)]
public class PrefabManager : ScriptableObject
{
    [Header("UI")]
    [SerializeField] GameObject m_PrefabTemp;

    public GameObject PrefabsTemp => m_PrefabTemp;

}

