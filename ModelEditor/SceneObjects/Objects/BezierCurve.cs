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
        private void OnMatrixChange(object sender, ChangeMatrixEventArgs e)
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

            for (int i = 0; i < verts.Count; i += 4)
                data.PixelPositions.AddRange(GetCubicSegment(verts, i));

            var left = verts.Count - data.PixelPositions.Count;
            if (left == 3) data.PixelPositions.AddRange(GetQuadratic(verts, verts.Count - 3));
            if (left == 2) data.PixelPositions.AddRange(GetLinear(verts, verts.Count - 2));
            if (left == 1) data.PixelPositions.Add(GetPoint(verts, verts.Count - 1));

            return data;
        }
        private List<PixelPosition> GetCubicSegment(List<Vector3> vertices, int idx)
        {
            return null;
        }
        private List<PixelPosition> GetQuadratic(List<Vector3> vertices, int idx)
        {
            return null;

        }
        private List<PixelPosition> GetLinear(List<Vector3> vertices, int idx)
        {
            return null;

        }
        private PixelPosition GetPoint(List<Vector3> vertices, int idx)
        {
            return new PixelPosition();
        }

        private List<Vector2Int> GetSegmentPixels(List<Vector3> vertices)
        {
            var result = new List<Vector2Int>();
            if (vertices.Count > 3)
            {
                var center = _rayCaster.GetScreenPositionOf(this);
                var n = 500;
                for (int i = 0; i < n; i++)
                {
                    var point = GetPoint(1f * i / n, vertices);
                    result.Add(_rayCaster.GetScreenPositionOf(point) - center);
                }
            }
            return result;
        }


        public Vector3 GetPoint(float t, List<Vector3> verts)                            // t E [0, 1].
        {
            float c = 1.0f - t;

            float bb0 = c * c * c;
            float bb1 = 3 * t * c * c;
            float bb2 = 3 * t * t * c;
            float bb3 = t * t * t;

            Vector3 point = verts[0] * bb0 + verts[1] * bb1 + verts[2] * bb2 + verts[3] * bb3;
            return point;
        }
        private List<Vector2Int> GetLinePixels(Vector2Int a, Vector2Int b)
        {
            var center = _rayCaster.GetScreenPositionOf(this);
            var result = new List<Vector2Int>();
            if (a != Vector2Int.Empty && b != Vector2Int.Empty)
            {
                a -= center;
                b -= center;
                var xDiff = a.X - b.X;
                var yDiff = a.Y - b.Y;

                var count = Math.Max(Math.Abs(xDiff), Math.Abs(yDiff));

                for (int i = 0; i < count; i++)
                {
                    result.Add(new Vector2Int(
                        Convert.ToInt32(b.X + i * xDiff / count),
                        Convert.ToInt32(b.Y + i * yDiff / count)
                        ));
                }
            }

            return result;
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

