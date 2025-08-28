using System;
using UnityEngine;

[Serializable]
public class TupleSerialize<T1, T2>
{
    [SerializeField] private T1 value1;
    [SerializeField] private T2 value2;

    public TupleSerialize()
    {
    }

    public TupleSerialize(T1 value1, T2 value2)
    {
        this.value1 = value1;
        this.value2 = value2;
    }

    public T1 Value1 => value1;
    public T2 Value2 => value2;
}

[Serializable]
public class TupleSerialize<T1, T2, T3>
{
    [SerializeField] private T1 value1;
    [SerializeField] private T2 value2;
    [SerializeField] private T3 value3;

    public TupleSerialize()
    {
    }

    public TupleSerialize(T1 value1, T2 value2, T3 value3)
    {
        this.value1 = value1;
        this.value2 = value2;
        this.value3 = value3;
    }

    public T1 Value1 => value1;
    public T2 Value2 => value2;
    public T3 Value3 => value3;
}

