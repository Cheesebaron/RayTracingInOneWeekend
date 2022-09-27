using System.Numerics;
using System.Runtime.InteropServices;

namespace RayTracingInOneWeekend;

[StructLayout(LayoutKind.Sequential)]
public record struct RayHit(Vector3 Point, Vector3 Normal, float T);