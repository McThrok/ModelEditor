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

            DerivativeLeft = new Vertex();
            DerivativeRight = new Vertex();
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

                var cv1 = CreateControlVertex();
                cv1.GlobalMatrix = vert.GlobalMatrix;
                cv1.Move(-0.4f, 1, 0);
                bVert.ControlVertex1 = cv1;

                var cv2 = CreateControlVertex();
                cv2.GlobalMatrix = vert.GlobalMatrix;
                cv2.Move(-0.2f, 2, 0);
                bVert.ControlVertex2 = cv2;

                DerivativeRight.SetParent(this, true);
            }

            if (_vertices.Count == 1)
            {
                DerivativeLeft.SetParent(this, true);
            }

            _vertices.Add(bVert);
        }

        private Vertex CreateControlVertex()
        {
            var cv = new Vertex();
            cv.SetParent(this, true);

            return cv;
        }

        private void DeleteBernstein(Vertex vert)
        {
            int idx = _vertices.FindIndex(x => x.Vertex.Id == vert.Id);

            if (idx != 0 || idx == 0 && _vertices.Count > 1)
            {
                var bVert = _vertices[idx != 0 ? idx : 1];

                if (bVert.ControlVertex1 != null)
                    this.HiddenChildren.Remove(bVert.ControlVertex1);

                if (bVert.ControlVertex2 != null)
                    this.HiddenChildren.Remove(bVert.ControlVertex2);
            }

            _vertices.RemoveAt(idx);
        }

        private bool _spline = true;
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

        public override bool CanBeParentOf(SceneObject obj)
        {
            return obj is Vertex;
        }

        public ObjRenderData GetRenderData()
        {
            var data = Spline ? GetSpline() : GetBernstein();
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
        private ObjRenderData GetBernstein()
        {
            var data = new ObjRenderData();


            return GerBernsteinPolygon();
        }
        private ObjRenderData GerBernsteinPolygon()
        {
            var verts = GetBernsteinVertices();
            var data = new ObjRenderData();
            //if (ShowPolygon && verts.Count > 1)
            if (verts.Count > 1)
            {
                data.Vertices = verts.Select(x => x.Matrix.Multiply(Vector3.Zero.ToVector4()).ToVector3()).ToList();
                data.Edges = Enumerable.Range(0, verts.Count - 1).Select(x => new Edge(x, x + 1)).ToList();
            }

            return data;
        }
        private List<Vertex> GetBernsteinVertices()
        {
            var verts = new List<Vertex>();
            foreach (var vert in _vertices)
            {
                if (vert.ControlVertex1 != null)
                    verts.Add(vert.ControlVertex1);

                if (vert.ControlVertex2 != null)
                    verts.Add(vert.ControlVertex2);

                verts.Add(vert.Vertex);
            }

            return verts;
        }
        private ObjRenderData GetSpline()
        {
            var data = new ObjRenderData();
            var verts = Children.Select(x => x.Matrix.Multiply(Vector3.Zero.ToVector4()).ToVector3()).ToList();
            if (verts.Count == 4)
            {
                int n = 10;
                for (int i = 0; i < n; i++)
                {
                    data.Vertices.Add(Interpolate(1f * i / n, 3, verts));
                }
            }

            return data;
        }

        public Vector3 Interpolate(float t, int degree, List<Vector3> points)
        {
            int i, j, s, l;              // function-scoped iteration variables
            var n = points.Count;    // points count

            // build knot vector of length [n + degree + 1]
            var knots = Enumerable.Range(0, n + degree + 1).ToList();

            var domain = new Vector2Int(degree, knots.Count - 1 - degree);

            // remap t to the domain where the spline is defined
            var low = knots[domain.X];
            var high = knots[domain.Y];
            t = t * (high - low) + low;

            if (t < low || t > high) throw new Exception("out of bounds");

            // find s (the spline segment) for the [t] value provided
            for (s = domain.X; s < domain.Y; s++)
            {
                if (t >= knots[s] && t <= knots[s + 1])
                {
                    break;
                }
            }

            // convert points to homogeneous coordinates
            var v = points.ToList();

            // l (level) goes from 1 to the curve degree + 1
            float alpha;
            for (l = 1; l <= degree + 1; l++)
            {
                // build level l of the pyramid
                for (i = s; i > s - degree - 1 + l; i--)
                {
                    alpha = (t - knots[i]) / (knots[i + degree + 1 - l] - knots[i]);

                    // interpolate each component
                    v[i] = (1 - alpha) * v[i - 1] + alpha * v[i];
                }
            }

            // convert back to cartesian and return
            var result = v[s];

            return result;
        }

    }
}