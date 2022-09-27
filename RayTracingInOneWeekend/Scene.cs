using System.Collections.Generic;
using System.Numerics;

namespace RayTracingInOneWeekend;

public class Scene
{
    public Camera Camera { get; }

    private readonly List<(Sphere sphere, Material material)> _spheres;

    public Scene(Camera camera)
    {
        Camera = camera;
        _spheres = new List<(Sphere sphere, Material material)>();
    }

    public void Add(Sphere sphere, Material material) => _spheres.Add((sphere, material));

    public bool Hit(Ray ray, float tMin, float tMax, out RayHit hit, out Material? material)
    {
        hit = new RayHit(Vector3.Zero, Vector3.Zero, 0);
        var hitAnything = false;
        var closestSoFar = tMax;
        material = null;

        for (var i = 0; i < _spheres.Count; i++)
        {
            if (Sphere.Hit(_spheres[i].sphere, ray, tMin, closestSoFar, out RayHit tempHit))
            {
                hitAnything = true;
                closestSoFar = tempHit.T;
                hit = tempHit;
                material = _spheres[i].material;
            }
        }

        return hitAnything;
    }
}