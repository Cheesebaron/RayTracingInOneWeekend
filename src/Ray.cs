using System.Numerics;
using System.Runtime.InteropServices;

namespace RayTracingInOneWeekend
{
    [StructLayout(LayoutKind.Sequential)]
    public record struct Ray(Vector3 Origin, Vector3 Direction)
    {
        public Vector3 PointAt(float t) => Origin + (Direction * t);
    }
}
