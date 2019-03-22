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

            var n = 500;

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

            var n = 500;

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

            var n = 500;

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

