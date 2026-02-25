using System;

/// <summary>
/// Double-precision 3D vector for astronomical calculations.
/// Unity's Vector3 uses float (7 digits precision) which is insufficient
/// for solar system scales. This struct provides ~15 digits of precision.
/// 
/// Reference: Standard approach used in n-body simulators like Universe Sandbox,
/// OpenSpace, and Sebastian Lague's solar system simulation.
/// </summary>
[Serializable]
public struct DoubleVector3
{
    public double x;
    public double y;
    public double z;

    public static readonly DoubleVector3 zero = new DoubleVector3(0, 0, 0);
    public static readonly DoubleVector3 one = new DoubleVector3(1, 1, 1);
    public static readonly DoubleVector3 up = new DoubleVector3(0, 1, 0);
    public static readonly DoubleVector3 right = new DoubleVector3(1, 0, 0);
    public static readonly DoubleVector3 forward = new DoubleVector3(0, 0, 1);

    public DoubleVector3(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    /// <summary>
    /// Squared magnitude - use this instead of magnitude when possible
    /// to avoid expensive sqrt operation.
    /// </summary>
    public double sqrMagnitude => x * x + y * y + z * z;

    /// <summary>
    /// Vector magnitude (length). Uses sqrt - prefer sqrMagnitude for comparisons.
    /// </summary>
    public double magnitude => Math.Sqrt(sqrMagnitude);

    /// <summary>
    /// Returns a unit vector in the same direction.
    /// </summary>
    public DoubleVector3 normalized
    {
        get
        {
            double mag = magnitude;
            if (mag > 1e-15)
                return this / mag;
            return zero;
        }
    }

    // ==================== Operator Overloads ====================

    public static DoubleVector3 operator +(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static DoubleVector3 operator -(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static DoubleVector3 operator -(DoubleVector3 a)
    {
        return new DoubleVector3(-a.x, -a.y, -a.z);
    }

    public static DoubleVector3 operator *(DoubleVector3 a, double d)
    {
        return new DoubleVector3(a.x * d, a.y * d, a.z * d);
    }

    public static DoubleVector3 operator *(double d, DoubleVector3 a)
    {
        return new DoubleVector3(a.x * d, a.y * d, a.z * d);
    }

    public static DoubleVector3 operator /(DoubleVector3 a, double d)
    {
        double inv = 1.0 / d;
        return new DoubleVector3(a.x * inv, a.y * inv, a.z * inv);
    }

    // ==================== Utility Methods ====================

    public static double Dot(DoubleVector3 a, DoubleVector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static DoubleVector3 Cross(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );
    }

    public static double Distance(DoubleVector3 a, DoubleVector3 b)
    {
        return (a - b).magnitude;
    }

    public static double SqrDistance(DoubleVector3 a, DoubleVector3 b)
    {
        return (a - b).sqrMagnitude;
    }

    /// <summary>
    /// Convert to Unity's float Vector3 (for transform.position).
    /// Precision loss is acceptable for rendering only.
    /// </summary>
    public UnityEngine.Vector3 ToVector3()
    {
        return new UnityEngine.Vector3((float)x, (float)y, (float)z);
    }

    /// <summary>
    /// Create from Unity's float Vector3.
    /// </summary>
    public static DoubleVector3 FromVector3(UnityEngine.Vector3 v)
    {
        return new DoubleVector3(v.x, v.y, v.z);
    }

    public override string ToString()
    {
        return $"({x:F6}, {y:F6}, {z:F6})";
    }
}

