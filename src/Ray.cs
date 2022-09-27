using System.Numerics;
using System.Runtime.InteropServices;

namespace RayTracingInOneWeekend;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Ray(Vector3 Origin, Vector3 Direction)
{
    public Vector3 PointAt(float t) => Vector3.Add(Origin, Vector3.Multiply(Direction, t));
}