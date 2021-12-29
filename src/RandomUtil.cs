using System;
using System.Numerics;

namespace RayTracingInOneWeekend
{
    public static class RandomUtil
    {
        private static readonly Random _random = new();

        public static Vector3 RandomVector(float min, float max) =>
            new(NextFloat(min, max), NextFloat(min, max), NextFloat(min, max));

        public static Vector3 RandomInUnitSphere()
        {
            Vector3 ret;
            do
            {
                ret = RandomVector(-1, 1);
            } while (ret.LengthSquared() >= 1f);
            return ret;
        }

        public static Vector3 RandomInUnitDisk()
        {
            Vector3 p;
            do
            {
                p = 2f * new Vector3(NextFloat(), NextFloat(), 0) - new Vector3(1, 1, 0);
            } while (Vector3.Dot(p, p) >= 1f);
            return p;
        }

        public static float NextFloat() => _random.NextSingle();

        private static float NextFloat(float min, float max) =>
            _random.NextSingle() * (max - min) + min;
    }
}
