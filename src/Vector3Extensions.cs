using SkiaSharp;
using System;
using System.Numerics;

namespace RayTracingInOneWeekend
{
    public static class Vector3Extensions
    {
        public static Vector3 UnitVector(this Vector3 vector) => vector / vector.Length();

        public static SKColor GetColor(this Vector3 vector, int samplesPerPixel)
        {
            var r = vector.X;
            var g = vector.Y;
            var b = vector.Z;

            var scale = 1.0f / samplesPerPixel;

            r = MathF.Sqrt(scale * r);
            g = MathF.Sqrt(scale * g);
            b = MathF.Sqrt(scale * b);

            var ir = (byte)(256f * Math.Clamp(r, 0.0f, 0.999f));
            var ig = (byte)(256f * Math.Clamp(g, 0.0f, 0.999f));
            var ib = (byte)(256f * Math.Clamp(b, 0.0f, 0.999f));
            return new SKColor(ir, ig, ib);
        }
    }
}
