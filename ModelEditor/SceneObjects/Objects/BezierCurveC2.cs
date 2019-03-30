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
    public class BezierCurveC2 : BezierCurveBase, IRenderableObj
    {
        private static int _count = 0;
        private List<Vertex> _vertices = new List<Vertex>();
        private List<Vertex> _controlVertices = new List<Vertex>();
        private Vertex _lastChanged;

        public BezierCurveC2(RayCaster rayCaster) : base(rayCaster)
        {
            Name = nameof(BezierCurveC2) + " " + _count++.ToString();
            Children.CollectionChanged += Children_CollectionChanged;
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
            if (_vertices.Count != 0)
            {
                var last = _vertices[_vertices.Count - 1];

                var cv1 = CreateControlVertex();
                cv1.Matrix = _vertices[_vertices.Count - 1].Matrix;
                cv1.Move(0, 1, 0);

                var cv2 = CreateControlVertex();
                cv2.Matrix = vert.Matrix;
                cv2.Move(0, 1, 0);


                if (_vertices.Count == 1)
                {
                    _lastChanged = cv1;
                }
            }

            _vertices.Add(vert);


        }

        private Vertex CreateControlVertex()
        {
            var cv = new Vertex();
            cv.SetParent(this, true);
            _controlVertices.Add(cv);

            return cv;
        }

        private void RecalculateBernstein()
        {
            int idx = 0;
            if (_lastChanged != null)
                idx = _controlVertices.FindIndex(x => x.Id == _lastChanged.Id) / 2;

            for (int i = idx + 1; i < _vertices.Count - 1; i++)
            {
                RecalculateBernsteinRight(i);
            }

            for (int i = idx - 1; i > 0 - 1; i--)
            {
                RecalculateBernsteinLeft(i);
            }

        }
        private void RecalculateBernsteinRight(int idx)
        {
            //previous - abcd 
            //this - defg

            var b = _controlVertices[2 * (idx - 1)];
            var c = _controlVertices[2 * (idx - 1) + 1];
            var d = _vertices[idx];
            var e = _controlVertices[2 * idx];
            var f = _controlVertices[2 * idx + 1];

            e.Matrix = d.Matrix;
            e.MoveLoc(d.Matrix.Translation - c.Matrix.Translation);

            f.Matrix = e.Matrix;
            var deBoor = c.Matrix.Translation + (c.Matrix.Translation - b.Matrix.Translation);
            f.MoveLoc(e.Matrix.Translation - deBoor);

        }
        private void RecalculateBernsteinLeft(int idx)
        {
            //this - abcd 
            //next - defg

            var b = _controlVertices[2 * idx];
            var c = _controlVertices[2 * idx + 1];
            var d = _vertices[idx + 1];
            var e = _controlVertices[2 * (idx + 1)];
            var f = _controlVertices[2 * (idx + 1) + 1];

            c.Matrix = d.Matrix;
            c.MoveLoc(d.Matrix.Translation - e.Matrix.Translation);

            b.Matrix = c.Matrix;
            var deBoor = e.Matrix.Translation + (e.Matrix.Translation - f.Matrix.Translation);
            b.MoveLoc(c.Matrix.Translation - deBoor);
        }


        private void DeleteBernstein(Vertex vert)
        {
            int idx = _vertices.FindIndex(x => x.Id == vert.Id);

            if (idx == 0 && _vertices.Count > 1)
            {
                _controlVertices.RemoveAt(1);
                _controlVertices.RemoveAt(0);
            }

            if (idx > 0)
            {
                _controlVertices.RemoveAt(2 * (idx - 1) + 1);
                _controlVertices.RemoveAt(2 * (idx - 1));
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
            RecalculateBernstein();

            var data = GerBernsteinCurve();
            data.Add(GerBernsteinPolygon());

            return data;
        }
        private ObjRenderData GerBernsteinCurve()
        {
            var verts = GetBernsteinVertices();
            var data = new ObjRenderData();
            for (int i = 0; i + 3 < verts.Count; i += 3)
                data.Vertices.AddRange(GetSegment(verts, i, 4));

            return data;
        }
        private ObjRenderData GerBernsteinPolygon()
        {
            var verts = GetBernsteinVertices();
            var data = new ObjRenderData();
            if (ShowPolygon && verts.Count > 1)
            {
                data.Vertices = verts;
                data.Edges = Enumerable.Range(0, verts.Count - 1).Select(x => new Edge(x, x + 1)).ToList();
            }

            return data;
        }
        private List<Vector3> GetBernsteinVertices()
        {
            var verts = new List<Vector3>();
            for (int i = 0; i < _vertices.Count; i++)
            {
                verts.Add(_vertices[i].Matrix.Translation);

                if (i < _vertices.Count - 1)
                {
                    verts.Add(_controlVertices[2 * i].Matrix.Translation);
                    verts.Add(_controlVertices[2 * i + 1].Matrix.Translation);
                }
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