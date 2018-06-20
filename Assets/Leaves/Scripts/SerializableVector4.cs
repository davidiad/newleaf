using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Since unity doesn't flag the Vector4 as serializable, we
/// need to create our own version. This one will automatically convert
/// between Vector4 and SerializableVector4
/// Can be used to hold Rotations, or Colors
/// </summary>
[System.Serializable]
public struct SerializableVector4
{
    /// <summary>
    /// x component
    /// </summary>
    public float x;

    /// <summary>
    /// y component
    /// </summary>
    public float y;

    /// <summary>
    /// z component
    /// </summary>
    public float z;

    /// <summary>
    /// w component
    /// </summary>
    public float w;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    /// <param name="rZ"></param>
    /// <param name="rW"></param>
    public SerializableVector4(float rX, float rY, float rZ, float rW)
    {
        x = rX;
        y = rY;
        z = rZ;
        w = rW;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
    }

    /// <summary>
    /// Automatic conversion from SerializableVector4 to Vector4
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Vector4(SerializableVector4 rValue)
    {
        return new Vector4(rValue.x, rValue.y, rValue.z, rValue.w);
    }

    /// <summary>
    /// Automatic conversion from Vector4 to SerializableVector4
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableVector4(Vector4 rValue)
    {
        return new SerializableVector4(rValue.x, rValue.y, rValue.z, rValue.w);
    }
}