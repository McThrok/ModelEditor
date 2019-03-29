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

            DerivativeLeft = new Vertex() { IsVisible = false };
            DerivativeRight = new Vertex() { IsVisible = false };
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    if (item is Vertex vert && vert.IsVisible)
                        AddBernstein(vert);

            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    if (item is Vertex vert && vert.IsVisible)
                        DeleteBernstein(vert);
        }

        private void AddBernstein(Vertex vert)
        {
            var bVert = new BernsteinVertex() { Vertex = vert };

            if (_vertices.Count != 0)
            {
                var last = _vertices[_vertices.Count - 1];

                var cv1 = CreateControlVertex();
                cv1.GlobalMatrix = vert.GlobalMatrix;
                cv1.Move(0, 1, 0);
                bVert.ControlVertex1 = cv1;

                var cv2 = CreateControlVertex();
                cv2.GlobalMatrix = vert.GlobalMatrix;
                cv2.Move(0, 2, 0);
                bVert.ControlVertex2 = cv2;

                DerivativeRight.SetParent(this, false);
            }

            if (_vertices.Count == 1)
            {
                //DerivativeLeft.SetParent(this, false);
            }

            _vertices.Add(bVert);
        }

        private Vertex CreateControlVertex()
        {
            var cv = new Vertex();
            cv.IsVisible = false;
            cv.SetParent(this, false);

            return cv;
        }

        private void DeleteBernstein(Vertex vert)
        {
            int idx = _vertices.FindIndex(x => x.Vertex.Id == vert.Id);
                
            if (idx != 0 || idx == 0 && _vertices.Count > 1)
            {
                var bVert = _vertices[idx != 0 ? idx : 1];

                if (bVert.ControlVertex1 != null)
                    //this.Children.RemoveWithoutNotification(bVert.ControlVertex1);
                    this.Children.Remove(bVert.ControlVertex1);

                if (bVert.ControlVertex2 != null)
                    //this.Children.RemoveWithoutNotification(bVert.ControlVertex2);
                    this.Children.Remove(bVert.ControlVertex2);
            }

            _vertices.RemoveAt(idx);
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
            var data = new ObjRenderData();

            ////polygon
            //if (ShowPolygon && verts.Count > 1)
            //{
            //    data.Vertices.AddRange(verts);
            //    data.Edges.AddRange(Enumerable.Range(0, verts.Count - 1).Select(x => new Edge(x, x + 1)).ToList());
            //}

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

