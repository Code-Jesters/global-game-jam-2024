using System;

public class IntPair : IEquatable<IntPair>
{
    private int a, b;

    public IntPair(int x, int y)
    {
        // Always store them in a consistent order
        if (x < y)
        {
            a = x; b = y;
        }
        else
        {
            a = y; b = x;
        }
    }

    public bool Equals(IntPair other)
    {
        return (a == other.a && b == other.b) || (a == other.b && b == other.a);
    }

    public override bool Equals(object obj)
    {
        if (obj is IntPair)
        {
            return Equals((IntPair)obj);
        }
        return false;
    }

    public override int GetHashCode()
    {
        // Combine the hash codes in an order-independent manner
        int hash1 = a.GetHashCode();
        int hash2 = b.GetHashCode();
        return hash1 ^ hash2; // XOR is commutative, so order doesn't matter
    }

    public override string ToString()
    {
        return $"({a}, {b})";
    }
}
