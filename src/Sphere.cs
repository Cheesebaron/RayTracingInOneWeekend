using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RayTracingInOneWeekend;

[StructLayout(LayoutKind.Sequential)]
public record struct Sphere(Vector3 Center, float Radius)
{
    public static bool Hit(Sphere sphere, Ray ray, float tMin, float tMax, out RayHit hit)
    {
        Vector3 center = sphere.Center;
        Vector3 oc = ray.Origin - center;
        Vector3 rayDir = ray.Direction;
        float a = Vector3.Dot(rayDir, rayDir);
        float b = Vector3.Dot(oc, rayDir);
        float radius = sphere.Radius;
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - a * c;
        if (discriminant > 0)
        {
            float tmp = MathF.Sqrt(b * b - a * c);
            float t = (-b - tmp) / a;
            if (t < tMax && t > tMin)
            {
                hit = GetRayHit(t, ray, center, radius);
                return true;
            }
            t = (-b + tmp) / a;
            if (t < tMax && t > tMin)
            {
                hit = GetRayHit(t, ray, center, radius);
                return true;
            }
        }

        hit = new RayHit();
        return false;
    }

    private static RayHit GetRayHit(float t, Ray ray, Vector3 center, float radius)
    {
        Vector3 position = ray.PointAt(t);
        Vector3 normal = (position - center) / radius;
        return new RayHit(position, normal, t);
    }
}