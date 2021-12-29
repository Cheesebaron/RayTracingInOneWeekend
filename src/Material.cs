using System;
using System.Numerics;

namespace RayTracingInOneWeekend
{
    public abstract class Material
    {
        protected Material()
        {
        }

        public abstract bool Scatter(Ray rayIn, RayHit hit, out Vector3 attenuation, out Ray scattered);
    }

    public class Metal : Material
    {
        public Vector3 Albedo { get; }
        public float Fuzzyness { get; }

        public Metal(Vector3 albedo, float fuzzyness)
        {
            Albedo = albedo;
            Fuzzyness = fuzzyness < 1f ? fuzzyness : 1f;
        }

        public override bool Scatter(Ray rayIn, RayHit hit, out Vector3 attenuation, out Ray scattered)
        {
            var reflected = Vector3.Reflect(rayIn.Direction.UnitVector(), hit.Normal);
            scattered = new Ray(hit.Point, reflected + Fuzzyness * RandomUtil.RandomInUnitSphere());
            attenuation = Albedo;

            return Vector3.Dot(scattered.Direction, hit.Normal) > 0;
        }
    }

    public class Lambertian : Material
    {
        public Vector3 Albedo { get; }

        public Lambertian(Vector3 albedo)
        {
            Albedo = albedo;
        }

        public override bool Scatter(Ray rayIn, RayHit hit, out Vector3 attenuation, out Ray scattered)
        {
            var target = hit.Point + hit.Normal + RandomUtil.RandomInUnitSphere();
            scattered = new Ray(hit.Point, target - hit.Point);
            attenuation = Albedo;

            return true;
        }
    }

    public class Dielectric : Material
    {
        public float RefractionIndex { get; }

        public Dielectric(float refractionIndex)
        {
            RefractionIndex = refractionIndex;
        }

        public override bool Scatter(Ray rayIn, RayHit hit, out Vector3 attenuation, out Ray scattered)
        {
            Vector3 outwardNormal;
            Vector3 reflected = Vector3.Reflect(rayIn.Direction, hit.Normal);
            float niOverNt;
            attenuation = new Vector3(1, 1, 1);
            float reflectProb;
            float cosine;
            if (Vector3.Dot(rayIn.Direction, hit.Normal) > 0)
            {
                outwardNormal = -hit.Normal;
                niOverNt = RefractionIndex;
                cosine = RefractionIndex * Vector3.Dot(rayIn.Direction, hit.Normal) / rayIn.Direction.Length();
            }
            else
            {
                outwardNormal = hit.Normal;
                niOverNt = 1f / RefractionIndex;
                cosine = -Vector3.Dot(rayIn.Direction, hit.Normal) / rayIn.Direction.Length();
            }

            if (Refract(rayIn.Direction, outwardNormal, niOverNt, out Vector3 refracted))
                reflectProb = Schlick(cosine, RefractionIndex);
            else
                reflectProb = 1f;

            if (RandomUtil.NextFloat() < reflectProb)
                scattered = new Ray(hit.Point, reflected);
            else
                scattered = new Ray(hit.Point, refracted);

            return true;
        }

        private static bool Refract(Vector3 v, Vector3 n, float niOverNt, out Vector3 refracted)
        {
            Vector3 uv = Vector3.Normalize(v);
            var dt = Vector3.Dot(uv, n);
            var discriminant = 1f - niOverNt * niOverNt * (1 - dt * dt);
            if (discriminant > 0)
            {
                refracted = niOverNt * (uv - n * dt) - n * MathF.Sqrt(discriminant);
                return true;
            }
            refracted = Vector3.Zero;
            return false;
        }

        private static float Schlick(float cosine, float refractiveIndex)
        {
            var r0 = (1f - refractiveIndex) / (1f + refractiveIndex);
            r0 *= r0;
            return r0 + (1f - r0) * MathF.Pow(1f - cosine, 5);
        }
    }
}
