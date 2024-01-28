using System;
using UnityEngine;

public struct Vector3Pair : IEquatable<Vector3Pair>
{
    Vector3 v1, v2;

    public Vector3Pair(Vector3 a, Vector3 b)
    {
        // Always store them in a consistent order
        if (a.GetHashCode() < b.GetHashCode())
        {
            v1 = a; v2 = b;
        }
        else
        {
            v1 = b; v2 = a;
        }
    }

    public bool Equals(Vector3Pair other)
    {
        return (v1 == other.v1 && v2 == other.v2) || (v1 == other.v2 && v2 == other.v1);
    }

    public override bool Equals(object obj)
    {
        if (obj is Vector3Pair)
        {
            return Equals((Vector3Pair)obj);
        }
        return false;
    }

    public override int GetHashCode()
    {
        // Combine the hash codes in an order-independent manner
        int hash1 = v1.GetHashCode();
        int hash2 = v2.GetHashCode();
        return hash1 ^ hash2; // XOR is commutative, so order doesn't matter
    }
}
