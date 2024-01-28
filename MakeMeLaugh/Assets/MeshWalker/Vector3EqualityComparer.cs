using System.Collections.Generic;
using UnityEngine;

public class Vector3EqualityComparer : IEqualityComparer<Vector3>
{
    readonly float tolerance;

    public Vector3EqualityComparer(float tolerance)
    {
        this.tolerance = tolerance;
    }

    public bool Equals(Vector3 v1, Vector3 v2)
    {
        return Mathf.Abs(v1.x - v2.x) < tolerance &&
            Mathf.Abs(v1.y - v2.y) < tolerance &&
            Mathf.Abs(v1.z - v2.z) < tolerance;
    }

    public int GetHashCode(Vector3 v)
    {
        // Adjust the components based on the tolerance
        int hashX = Mathf.RoundToInt(v.x / tolerance);
        int hashY = Mathf.RoundToInt(v.y / tolerance);
        int hashZ = Mathf.RoundToInt(v.z / tolerance);

        return hashX.GetHashCode() ^ hashY.GetHashCode() << 2 ^ hashZ.GetHashCode() >> 2;
    }
}
