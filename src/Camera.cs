using System;
using System.Numerics;

namespace RayTracingInOneWeekend
{
    public record struct Camera
    {
        private Vector3 _origin;
        private Vector3 _horizontal;
        private Vector3 _vertical;
        private Vector3 _lowerLeftCorner;
        private float _lensRadius;
        private Vector3 _w;
        private Vector3 _u;
        private Vector3 _v;

        public float AspectRatio { get; }
        public float VerticalFieldOfView { get; }

        public Camera(Vector3 lookFrom, Vector3 lookAt, Vector3 vUp, float verticalFieldOfView, float aspectRatio, float aperture, float focusDistance)
        {
            VerticalFieldOfView = verticalFieldOfView;
            AspectRatio = aspectRatio;

            var theta = MathUtils.DegreesToRadians(verticalFieldOfView);
            var h = MathF.Tan(theta / 2);

            var viewPortHeight = 2f * h;
            var viewPortWidth = aspectRatio * viewPortHeight;

            _w = (lookFrom - lookAt).UnitVector();
            _u = Vector3.Cross(vUp, _w).UnitVector();
            _v = Vector3.Cross(_w, _u);

            _origin = lookFrom;
            _horizontal = focusDistance * viewPortWidth * _u;
            _vertical = focusDistance * viewPortHeight * _v;
            _lowerLeftCorner = _origin - _horizontal / 2 - _vertical / 2 - focusDistance*_w;

            _lensRadius = aperture / 2f;
        }

        public Ray GetRay(float s, float t)
        {
            var rd = _lensRadius * RandomUtil.RandomInUnitDisk();
            var offset = _u * rd.X + _v * rd.Y;

            return new(_origin + offset, _lowerLeftCorner + s * _horizontal + t * _vertical - _origin - offset);
        }
    }
}
