using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabManager", menuName = "ScriptableObjects/PrefabManager", order = 1)]
public class PrefabManager : ScriptableObject
{
    [SerializeField, PoolingObject] private SpriteElement m_SpriteElement;
    public List<object> GetAllSerializeField()
    {
        Type type = this.GetType();

        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var serializedFields = new List<object>();

        foreach (FieldInfo field in fields)
        {
            if (Attribute.IsDefined(field, typeof(PoolingObjectAttribute)))
            {
                object value = field.GetValue(this);
                serializedFields.Add(value);
            }
        }

        return serializedFields;
    }
}

