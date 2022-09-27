using System;
using System.Numerics;

namespace RayTracingInOneWeekend.Scenes;

public enum SceneType
{
    None,
    BookScene
}

public static class SceneFactory
{
    public const float DefaultAspectRatio = 16.0f / 9.0f;
    private const int DefaultImageWidth = 1280;
    private const int DefaultImageHeight = (int)(DefaultImageWidth / DefaultAspectRatio);
    private const int DefaultMaxDepth = 50;

    public static Scene CreateScene(SceneType sceneType, float aspectRatio) =>
        sceneType switch
        {
            SceneType.BookScene => CreateBookScene(aspectRatio),
            _ => throw new ArgumentOutOfRangeException(nameof(sceneType))
        };

    private static Scene CreateBookScene(float aspectRatio = DefaultAspectRatio)
    {
        var lookFrom = new Vector3(13, 2, 3);
        var lookAt = new Vector3(0, 0, 0);
        var vUp = Vector3.UnitY;
        var distanceToFocus = 10f;
        var aperture = 0.1f;
        var camera = new Camera(lookFrom, lookAt, vUp, 20, aspectRatio, aperture, distanceToFocus);

        var scene = new Scene(camera);

        // ground
        scene.Add(new Sphere(new Vector3(0, -1000, 0), 1000), new Lambertian(new Vector3(0.5f, 0.5f, 0.5f)));

        for(var a = -11; a < 11; a++)
        {
            for(var b = -11; b < 11; b++)
            {
                var chooseMat = RandomUtil.NextFloat();
                var center = new Vector3(a + 0.9f * RandomUtil.NextFloat(), 0.15f, b + 0.9f * RandomUtil.NextFloat());

                if ((center - new Vector3(4f, 0.2f, 0f)).Length() > 0.9f)
                {
                    float randOffset = RandomUtil.NextFloat() * 0.15f;
                    var sphere = new Sphere(center + Vector3.UnitY * randOffset, 0.15f + randOffset);

                    if (chooseMat < 0.8f)
                    {
                        // diffuse
                        var albedo = new Vector3(
                            RandomUtil.NextFloat() * RandomUtil.NextFloat(),
                            RandomUtil.NextFloat() * RandomUtil.NextFloat(),
                            RandomUtil.NextFloat() * RandomUtil.NextFloat());

                        scene.Add(
                            sphere,
                            new Lambertian(albedo));
                    }
                    else if (chooseMat < 0.95f)
                    {
                        var albedo = new Vector3(
                            0.5f * (1 + RandomUtil.NextFloat()),
                            0.5f * (1 + RandomUtil.NextFloat()),
                            0.5f * (1 + RandomUtil.NextFloat()));

                        var fuzz = 0.5f * (1 + RandomUtil.NextFloat());

                        scene.Add(
                            sphere,
                            new Metal(albedo, fuzz));
                    }
                    else
                    {
                        scene.Add(sphere, new Dielectric(1.5f));
                    }
                }
            }
        }

        scene.Add(new Sphere(new Vector3(0, 1, 0), 1), new Dielectric(1.5f));
        scene.Add(new Sphere(new Vector3(-4, 1, 0), 1), new Lambertian(new Vector3(0.4f, 0.2f, 0.1f)));
        scene.Add(new Sphere(new Vector3(4, 1, 0), 1), new Metal(new Vector3(0.7f, 0.6f, 0.5f), 0f));

        return scene;
    }
}