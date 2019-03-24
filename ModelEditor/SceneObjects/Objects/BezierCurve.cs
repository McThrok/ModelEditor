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

        private List<Vector3> Getverts()
        {
            return Children.Select(x => x.GlobalMatrix.Multiply(Vector3.Zero.ToVector4()).ToVector3()).ToList();
        }
        private List<Vector3> GetvertsLocal()
        {
            return Children.Select(x => x.Matrix.Multiply(Vector3.Zero.ToVector4()).ToVector3()).ToList();
        }

        public ScreenRenderData GetScreenRenderData()
        {
            var data = new ScreenRenderData();
            var verts = GetvertsLocal();

            int i;
            for (i = 0; i + 3 < verts.Count; i += 3)
                data.PixelPositions.AddRange(GetCubicSegment(verts, i));

            var left = verts.Count - i;
            if (left == 3) data.PixelPositions.AddRange(GetQuadratic(verts, verts.Count - 3));
            if (left == 2) data.PixelPositions.AddRange(GetLinear(verts, verts.Count - 2));
            if (left == 1) data.PixelPositions.AddRange(GetPoint(verts, verts.Count - 1));

            return data;
        }
        private List<PixelPosition> GetCubicSegment(List<Vector3> verts, int idx)
        {
            var result = new List<PixelPosition>();
            var n = GetDivisionCount(verts, idx, 4);

            if (n == -1)
                return result;

            for (int i = 0; i < n; i++)
            {
                float t = 1f * i / n;
                float c = 1.0f - t;

                float b0 = c * c * c;
                float b1 = 3 * t * c * c;
                float b2 = 3 * t * t * c;
                float b3 = t * t * t;

                var point = verts[idx] * b0 + verts[idx + 1] * b1 + verts[idx + 2] * b2 + verts[idx + 3] * b3;

                result.Add(new PixelPosition(point));
            }

            return result;
        }
        private List<PixelPosition> GetQuadratic(List<Vector3> verts, int idx)
        {
            var result = new List<PixelPosition>();
            var n = GetDivisionCount(verts, idx, 3);

            if (n == -1)
                return result;

            for (int i = 0; i < n; i++)
            {
                float t = 1f * i / n;
                float c = 1.0f - t;

                float b0 = c * c;
                float b1 = 2 * t * c;
                float b2 = t * t;

                var point = verts[idx] * b0 + verts[idx + 1] * b1 + verts[idx + 2] * b2;

                result.Add(new PixelPosition(point));
            }

            return result;

        }
        private List<PixelPosition> GetLinear(List<Vector3> verts, int idx)
        {
            var result = new List<PixelPosition>();
            var n = GetDivisionCount(verts, idx, 2);

            if (n == -1)
                return result;

            for (int i = 0; i < n; i++)
            {
                float t = 1f * i / n;
                float c = 1.0f - t;

                float b0 = c;
                float b1 = t;

                var point = verts[idx] * b0 + verts[idx + 1] * b1; ;

                result.Add(new PixelPosition(point));
            }

            return result;
        }
        private List<PixelPosition> GetPoint(List<Vector3> verts, int idx)
        {
            return new List<PixelPosition>() { new PixelPosition(verts[idx]) };
        }
        private int GetDivisionCount(List<Vector3> verts, int idx, int length)
        {
            var mtx = GlobalMatrix;
            double max = -1;
            for (int i = 0; i < length - 1; i++)
            {
                var a = _rayCaster.GetScreenPositionOf(mtx.Multiply(verts[idx + i].ToVector4()).ToVector3());
                var b = _rayCaster.GetScreenPositionOf(mtx.Multiply(verts[idx + i + 1].ToVector4()).ToVector3());
                if (a == Vector2Int.Empty || b == Vector2Int.Empty)
                    return -1;

                var diff = a - b;
                var x = Math.Abs(diff.X);
                var y = Math.Abs(diff.Y);
                var dist = Math.Sqrt(x * x + y * y);
                max = dist > max ? dist : max;
            }

            return (int)Math.Ceiling((length - 1) * max);
        }


        public ObjRenderData GetRenderData()
        {
            //polygon
            var verts = Getverts();
            var data = new ObjRenderData
            {
                Vertices = Children.Select(x => x.GlobalMatrix.Multiply(Vector3.Zero.ToVector4()).ToVector3()).ToList(),
                Edges = verts.Count < 2 || !_showPolygon ? new List<Edge>() : Enumerable.Range(0, verts.Count - 1).Select(x => new Edge(x, x + 1)).ToList()
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

