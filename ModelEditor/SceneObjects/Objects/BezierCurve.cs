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
    public class BezierCurve : SceneObject, IRenderableObj
    {
        private readonly RayCaster _rayCaster;
        private static int _count = 0;
        public BezierCurve(RayCaster rayCaster)
        {
            Name = nameof(BezierCurve) + " " + _count++.ToString();
            Holdable = false;

            _rayCaster = rayCaster;

            GlobalMatrixChange += OnMatrixChange;
            Children.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (SceneObject child in e.NewItems)
                    child.GlobalMatrixChange += OnMatrixChange;

            if (e.OldItems != null)
                foreach (SceneObject child in e.OldItems)
                    child.GlobalMatrixChange -= OnMatrixChange;
        }
        public void OnMatrixChange(object sender, ChangeMatrixEventArgs e)
        {
            Recalculate();
        }
        public void Recalculate()
        {
        }

        private List<Vector3> GetVerts()
        {
            return Children.Select(x => x.Matrix.Multiply(Vector3.Zero.ToVector4()).ToVector3()).ToList();
        }
        private List<Vector3> GetSegment(List<Vector3> verts, int idx, int length)
        {
            var result = new List<Vector3>();
            if (length == 0) return new List<Vector3>() { };
            if (length == 1) return new List<Vector3>() { verts[idx] };

            float step = 1;
            Vector2Int prev = Vector2Int.Empty;
            float prevt = 0;
            float t = 0;

            while (t <= 1)
            {
                var point = GetSegmentValue(verts, idx, length, t);
                var pos = _rayCaster.GetScreenPositionOf(point);

                if (pos == Vector2Int.Empty)
                {
                    return new List<Vector3>();
                }

                if (t == 0 || Dist(prev, pos) <= 1)
                {
                    result.Add(point);

                    prevt = t;
                    prev = pos;

                    t += step;
                    step *= 2;
                }
                else
                {
                    step /= 2;
                    t = prevt + step;
                }
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
        private int Dist(Vector2Int a, Vector2Int b)
        {

            var diff = a - b;
            return Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));
        }
       
        public ObjRenderData GetRenderData()
        {
            var verts = GetVerts();
            var data = new ObjRenderData();

            //polygon
            if (ShowPolygon && verts.Count > 1)
            {
                data.Vertices.AddRange(verts);
                data.Edges.AddRange(Enumerable.Range(0, verts.Count - 1).Select(x => new Edge(x, x + 1)).ToList());
            }

            //curve
            int i;
            for (i = 0; i + 3 < verts.Count; i += 3)
                data.Vertices.AddRange(GetSegment(verts, i, 4));

            var left = verts.Count - i;
            data.Vertices.AddRange(GetSegment(verts, i, left));

            return data;
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

