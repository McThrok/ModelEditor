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
    public class BezierCurve : SceneObject, IRenderableObj, IScreenRenderable
    {
        private readonly RayCaster _rayCaster;
        private static int _count = 0;
        public BezierCurve(RayCaster rayCaster)
        {
            Name = nameof(BezierCurve) + " " + _count++.ToString();
            Holdable = true;

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

        private List<Vector3> GetVertices()
        {
            return Children.Select(x => x.GlobalMatrix.Multiply(Vector3.Zero.ToVector4()).ToVector3()).ToList();
        }

        public ScreenRenderData GetScreenRenderData()
        {
            var data = new ScreenRenderData();
            var verts = GetVertices();

            int i;
            for (i = 0; i + 3 < verts.Count; i += 3)
                data.PixelPositions.AddRange(GetCubicSegment(verts, i));

            var left = verts.Count - i;
            if (left == 3) data.PixelPositions.AddRange(GetQuadratic(verts, verts.Count - 3));
            if (left == 2) data.PixelPositions.AddRange(GetLinear(verts, verts.Count - 2));
            if (left == 1) data.PixelPositions.AddRange(GetPoint(verts, verts.Count - 1));

            return data;
        }
        private List<PixelPosition> GetCubicSegment(List<Vector3> vertices, int idx)
        {
            var result = new List<PixelPosition>();

            var v0 = _rayCaster.GetScreenPositionOf(vertices[idx]);
            var v1 = _rayCaster.GetScreenPositionOf(vertices[idx + 1]);
            var v2 = _rayCaster.GetScreenPositionOf(vertices[idx + 2]);
            var v3 = _rayCaster.GetScreenPositionOf(vertices[idx + 3]);

            if (v0 == Vector2Int.Empty || v1 == Vector2Int.Empty || v2 == Vector2Int.Empty || v3 == Vector2Int.Empty)
                return result;

            var n = GetDivisionCount(v0, v1, v2, v3);

            for (int i = 0; i < n; i++)
            {
                float t = 1f * i / n;
                float c = 1.0f - t;

                float b0 = c * c * c;
                float b1 = 3 * t * c * c;
                float b2 = 3 * t * t * c;
                float b3 = t * t * t;

                var point = vertices[idx] * b0 + vertices[idx + 1] * b1 + vertices[idx + 2] * b2 + vertices[idx + 3] * b3;

                result.Add(new PixelPosition(point));
            }

            return result;
        }
        private List<PixelPosition> GetQuadratic(List<Vector3> vertices, int idx)
        {
            var result = new List<PixelPosition>();

            var v0 = _rayCaster.GetScreenPositionOf(vertices[idx]);
            var v1 = _rayCaster.GetScreenPositionOf(vertices[idx + 1]);
            var v2 = _rayCaster.GetScreenPositionOf(vertices[idx + 2]);

            if (v0 == Vector2Int.Empty || v1 == Vector2Int.Empty || v2 == Vector2Int.Empty)
                return result;

            var n = GetDivisionCount(v0, v1, v2);

            for (int i = 0; i < n; i++)
            {
                float t = 1f * i / n;
                float c = 1.0f - t;

                float b0 = c * c;
                float b1 = 2 * t * c;
                float b2 = t * t;

                var point = vertices[idx] * b0 + vertices[idx + 1] * b1 + vertices[idx + 2] * b2;

                result.Add(new PixelPosition(point));
            }

            return result;

        }
        private List<PixelPosition> GetLinear(List<Vector3> vertices, int idx)
        {
            var result = new List<PixelPosition>();

            var v0 = _rayCaster.GetScreenPositionOf(vertices[idx]);
            var v1 = _rayCaster.GetScreenPositionOf(vertices[idx + 1]);

            if (v0 == Vector2Int.Empty || v1 == Vector2Int.Empty)
                return result;

            var n = GetDivisionCount(v0, v1);

            for (int i = 0; i < n; i++)
            {
                float t = 1f * i / n;
                float c = 1.0f - t;

                float b0 = c;
                float b1 = t;

                var point = vertices[idx] * b0 + vertices[idx + 1] * b1; ;

                result.Add(new PixelPosition(point));
            }

            return result;
        }
        private List<PixelPosition> GetPoint(List<Vector3> vertices, int idx)
        {
            return new List<PixelPosition>() { new PixelPosition(vertices[idx]) };
        }
        private int GetDivisionCount(params Vector2Int[] verts)
        {
            var sum = 0;
            for (int i = 0; i < verts.Length; i++)
            {
                var diff = verts[i] - verts[(i + 1) % verts.Length];
                sum += Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));
            }

            return 2*sum;
        }


        public ObjRenderData GetRenderData()
        {
            //polygon
            var vertices = GetVertices();
            var data = new ObjRenderData
            {
                Vertices = Children.Select(x => x.GlobalMatrix.Multiply(Vector3.Zero.ToVector4()).ToVector3()).ToList(),
                Edges = vertices.Count < 2 || !_showPolygon ? new List<Edge>() : Enumerable.Range(0, vertices.Count - 1).Select(x => new Edge(x, x + 1)).ToList()
            };

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

