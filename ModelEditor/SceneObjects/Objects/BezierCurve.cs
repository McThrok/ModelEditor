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
        private static int _count = 0;
        public BezierCurve()
        {
            Name = nameof(BezierCurve) + " " + _count++.ToString();
            Holdable = true;

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
            data.Pixels = GetVertices()
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 4)
                .SelectMany(seg => GetSegmentPixels(seg.Select(v => v.Value).ToList()))
                .ToList();

            return data;
        }
        private List<Vector2Int> GetSegmentPixels(List<Vector3> vertices)
        {
            var result = new List<Vector2Int>();

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
