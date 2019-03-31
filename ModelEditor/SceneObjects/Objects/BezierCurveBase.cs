using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;

namespace ModelEditor
{
    public class BezierCurveBase : SceneObject
    {
        protected readonly RayCaster _rayCaster;
        public BezierCurveBase(RayCaster rayCaster)
        {
            Holdable = false;
            _rayCaster = rayCaster;
        }

        protected List<Vector3> GetSegment(List<Vector3> verts, int idx, int length)
        {
            //return GetSegmentPrimitive(verts, idx, length);
            if (length == 0) return new List<Vector3>() { };
            if (length == 1) return new List<Vector3>() { verts[idx] };

            return GetSegmentRec(verts, idx, length, 0, 0, 1);
        }
        private List<Vector3> GetSegmentPrimitive(List<Vector3> verts, int idx, int length)
        {
            if (length == 0) return new List<Vector3>() { };
            if (length == 1) return new List<Vector3>() { verts[idx] };

            int n = 50;
            var result = new List<Vector3>();
            for (int i = 0; i < n; i++)
            {

                var pointA = GetSegmentValue(verts, idx, length, 1f * i / n);
                var screenPosA = _rayCaster.GetExScreenPositionOf(pointA);
                result.Add(pointA);

            }

            return result;
        }
        private List<Vector3> GetSegmentRec(List<Vector3> verts, int idx, int length, int level, float start, float end)
        {
            var pointA = GetSegmentValue(verts, idx, length, start);
            var pointB = GetSegmentValue(verts, idx, length, end);
            var screenPosA = _rayCaster.GetExScreenPositionOf(pointA);
            var screenPosB = _rayCaster.GetExScreenPositionOf(pointB);

            var result = new List<Vector3>();

            if (Dist(screenPosA, screenPosB) <= 1 || level > 10)
            {
                if (screenPosA != Vector2Int.Empty)
                    result.Add(pointA);
                if (screenPosB != Vector2Int.Empty)
                    result.Add(pointB);
            }
            else
            {
                float mid = (start + end) / 2;
                var left = GetSegmentRec(verts, idx, length, level + 1, start, mid);
                var right = GetSegmentRec(verts, idx, length, level + 1, mid, end);
                result.AddRange(left);

                if (left.Count > 0 && right.Count > 0 && left[left.Count - 1] == right[0])
                    result.AddRange(right.Skip(1));
                else
                    result.AddRange(right);
            }

            return result;
        }
        private Vector3 GetSegmentValue(List<Vector3> verts, int idx, int length, float t)
        {
            if (length == 2)
                return GetLinear(verts, idx, t);

            if (length == 3)
                return GetQuadratic(verts, idx, t);

            return GetCubic(verts, idx, t);
        }
        private Vector3 GetCubic(List<Vector3> verts, int idx, float t)
        {
            float c = 1.0f - t;

            float b0 = c * c * c;
            float b1 = 3 * t * c * c;
            float b2 = 3 * t * t * c;
            float b3 = t * t * t;

            var point = verts[idx] * b0 + verts[idx + 1] * b1 + verts[idx + 2] * b2 + verts[idx + 3] * b3;
            return point;
        }
        private Vector3 GetQuadratic(List<Vector3> verts, int idx, float t)
        {
            float c = 1.0f - t;

            float b0 = c * c;
            float b1 = 2 * t * c;
            float b2 = t * t;

            var point = verts[idx] * b0 + verts[idx + 1] * b1 + verts[idx + 2] * b2;
            return point;
        }
        private Vector3 GetLinear(List<Vector3> verts, int idx, float t)
        {
            float c = 1.0f - t;

            float b0 = c;
            float b1 = t;

            var point = verts[idx] * b0 + verts[idx + 1] * b1;
            return point;
        }
        protected int Dist(Vector2Int a, Vector2Int b)
        {

            var diff = a - b;
            return Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));
        }

        private bool _showPolygon;
        public bool ShowPolygon
        {
            get => _showPolygon;
            set
            {
                if (_showPolygon != value)
                {
                    _showPolygon = value;
                    InvokePropertyChanged(nameof(ShowPolygon));
                }
            }
        }
    }
}

