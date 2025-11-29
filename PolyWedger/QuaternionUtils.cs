using System.Collections.Specialized;
using System.Numerics;

namespace PolyWedger;

public static class QuaternionUtils
{
    public const float Rad2deg = 180f / (float)Math.PI;

    public static Vector3 GetNormalizedEulerAngles(Vector3 v)
    {
        Quaternion q = FromToRotation(Vector3.UnitY, v);
        Vector3 e = ToEulerAngles(q) * Rad2deg;
        return NormalizeEulerDegrees(e);
    }

    public static Vector3 GetNormalizedEulerAngles(Vector3 v, Vector3 up)
    {
        Quaternion q = FromToRotation(up, v);
        Vector3 e = ToEulerAngles(q) * Rad2deg;
        return NormalizeEulerDegrees(e);
    }

    public static Quaternion FromToRotation(Vector3 from, Vector3 to)
    {
        Vector3 f = Vector3.Normalize(from);
        Vector3 t = Vector3.Normalize(to);
        float dot = Vector3.Dot(f, t);
        const float eps = 1e-6f;

        switch (dot)
        {
            case > 1f - eps:
                return Quaternion.Identity;
            case < -1f + eps:
            {
                Vector3 ortho = Vector3.Cross(Vector3.UnitX, f);
                if (ortho.LengthSquared() < eps)
                    ortho = Vector3.Cross(Vector3.UnitY, f);
                ortho = Vector3.Normalize(ortho);
                Quaternion q180 = Quaternion.CreateFromAxisAngle(ortho, (float)Math.PI);
                // canonical sign
                if (q180.W < 0f) q180 = new Quaternion(-q180.X, -q180.Y, -q180.Z, -q180.W);
                return q180;
            }
        }

        Vector3 cross = Vector3.Cross(f, t);
        Quaternion q = new Quaternion(cross.X, cross.Y, cross.Z, 1f + dot);

        // Normalize safely
        float lenSq = q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
        if (lenSq > 0f)
        {
            float inv = 1f / (float)Math.Sqrt(lenSq);
            q.X *= inv; q.Y *= inv; q.Z *= inv; q.W *= inv;
        }

        // Canonicalize quaternion sign so W >= 0 to avoid Euler sign flips
        if (q.W < 0f)
            q = new Quaternion(-q.X, -q.Y, -q.Z, -q.W);

        return q;
    }

    public static Vector3 NormalizeEulerDegrees(Vector3 deg)
    {
        static float Wrap(float a)
        {
            float w = (a + 180f) % 360f;
            if (w < 0f) w += 360f;
            return w - 180f;
        }

        float x = Wrap(deg.X);
        float y = Wrap(deg.Y);
        float z = Wrap(deg.Z);

        const float gimbalEps = 1e-3f;
        // If pitch is near ±90°, roll and yaw are coupled; choose a stable canonicalization.
        if (Math.Abs(y - 90f) < gimbalEps || Math.Abs(y + 90f) < gimbalEps)
        {
            x = 0f;
            z = 0f;
        }

        // Clean tiny noise
        const float noiseEps = 1e-4f;
        if (Math.Abs(x) < noiseEps) x = 0f;
        if (Math.Abs(y) < noiseEps) y = 0f;
        if (Math.Abs(z) < noiseEps) z = 0f;

        return new Vector3(x, y, z);
    }

    public static Vector3 ToEulerAngles(Quaternion q)
    {
        // Normalize quaternion first to reduce floating-point drift
        float mag = (float)Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
        if (mag > 0f)
        {
            q.X /= mag; q.Y /= mag; q.Z /= mag; q.W /= mag;
        }

        Vector3 angles = new();

        // roll (x)
        double sinrCosp = 2.0 * (q.W * q.X + q.Y * q.Z);
        double cosrCosp = 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
        angles.X = (float)Math.Atan2(sinrCosp, cosrCosp);

        // pitch (y) with clamping to avoid NaN
        double sinp = 2.0 * (q.W * q.Y - q.Z * q.X);
        sinp = Math.Max(-1.0, Math.Min(1.0, sinp));
        if (Math.Abs(sinp) >= 1.0)
        {
            angles.Y = (float)(Math.PI / 2.0 * Math.Sign(sinp)); // use 90 degrees if out of range
        }
        else
        {
            angles.Y = (float)Math.Asin(sinp);
        }

        // yaw (z)
        double sinyCosp = 2.0 * (q.W * q.Z + q.X * q.Y);
        double cosyCosp = 1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
        angles.Z = (float)Math.Atan2(sinyCosp, cosyCosp);

        return angles;
    }
}