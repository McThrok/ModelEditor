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
    public class BernsteinVertex
    {
        public Vertex Vertex { get; set; }
        public Vertex ControlVertex1 { get; set; }
        public Vertex ControlVertex2 { get; set; }
    }
    public class BezierCurveC2 : BezierCurveBase, IRenderableObj
    {
        private static int _count = 0;
        private List<BernsteinVertex> _vertices = new List<BernsteinVertex>();
        private Vertex DerivativeLeft { get; set; }
        private Vertex DerivativeRight { get; set; }

        public BezierCurveC2(RayCaster rayCaster) : base(rayCaster)
        {
            Name = nameof(BezierCurveC2) + " " + _count++.ToString();
            Children.CollectionChanged += Children_CollectionChanged;

            DerivativeLeft = new Vertex() { Name = "dl" };//{ IsVisible = false };
            DerivativeRight = new Vertex() { Name = "dr" };// { IsVisible = false };
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    if (item is Vertex vert)
                        AddBernstein(vert);

            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    if (item is Vertex vert)
                        DeleteBernstein(vert);
        }

        private void AddBernstein(Vertex vert)
        {
            var bVert = new BernsteinVertex() { Vertex = vert };

            if (_vertices.Count != 0)
            {
                var last = _vertices[_vertices.Count - 1];

                var cv1 = new Vertex();
                cv1.IsVisible = false;
                cv1.Parent = this;
                this.Children.AddItemWithoutNotification(cv1);
                cv1.GlobalMatrix = vert.GlobalMatrix;
                cv1.Move(0, 1, 0);
                bVert.ControlVertex1 = cv1;

                var cv2 = new Vertex();
                cv2.IsVisible = false;
                cv2.Parent = this;
                this.Children.AddItemWithoutNotification(cv2);
                cv2.GlobalMatrix = vert.GlobalMatrix;
                cv2.Move(0, 1, 0);
                bVert.ControlVertex2 = cv2;

                DerivativeRight.SetParent(vert);
            }

            if (_vertices.Count == 1)
            {
                DerivativeLeft.SetParent(_vertices[0].Vertex);
            }

            _vertices.Add(bVert);
        }

        private void DeleteBernstein(Vertex vert)
        {

        }

        private bool _spline = false;
        public bool Spline
        {
            get => _spline;
            set
            {
                if (_spline != value)
                {
                    _spline = value;
                    InvokePropertyChanged(nameof(Spline));
                }
            }
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

            ////curve
            //int i;
            //for (i = 0; i + 3 < verts.Count; i += 3)
            //    data.Vertices.AddRange(GetSegment(verts, i, 4));

            //var left = verts.Count - i;
            //data.Vertices.AddRange(GetSegment(verts, i, left));

            return data;
        }

    }
}

