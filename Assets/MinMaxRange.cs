using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MinMaxRange
{
    public float minimum;
    public float maximum;

    public override string ToString() => $"[{minimum}, {maximum}]";
}
