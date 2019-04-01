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
        private List<Vertex> _controlVertices = new List<Vertex>();
        private Vertex _lastChanged;
        public BezierCurveC2(RayCaster rayCaster) : base(rayCaster)
        {
            Spline = true;
            Name = nameof(BezierCurveC2) + " " + _count++.ToString();
            base.Children.CollectionChanged += Children_CollectionChanged;
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

                    if (value)
                        ConvertToSpline();
                    else
                        ConvertToBezier();

                    InvokePropertyChanged(nameof(Spline));
                }
            }

        }
        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Spline)
                return;

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
            if (Children.Count != 1)
            {
                var cv1 = CreateBezierControlVertex();
                var cv2 = CreateBezierControlVertex();

                if (Children.Count == 2)
                {
                    cv1.Matrix = Children[Children.Count - 2].Matrix;
                    cv1.Move(0, 1, 0);

                    cv2.Matrix = vert.Matrix;
                    cv2.Move(0, 1, 0);
                }
            }

            vert.MatrixChange += BezierVertexChange;

            _lastChanged = null;
        }
        private void DeleteBernstein(Vertex vert)
        {
            var idx = Children.IndexOf(vert);

            if (idx == 0 && Children.Count > 0)
            {
                RemoveBezierControlVertex(_controlVertices[1]);
                RemoveBezierControlVertex(_controlVertices[0]);
            }

            if (idx > 0)
            {
                RemoveBezierControlVertex(_controlVertices[2 * (idx - 1) + 1]);
                RemoveBezierControlVertex(_controlVertices[2 * (idx - 1)]);
            }

            vert.MatrixChange -= BezierVertexChange;
        }
        private void BezierVertexChange(object sender, ChangeMatrixEventArgs e)
        {
            _lastChanged = sender as Vertex;
        }
        private Vertex CreateBezierControlVertex()
        {
            var vert = new Vertex();
            vert.SetParent(this, true);
            vert.MatrixChange += BezierVertexChange;
            _controlVertices.Add(vert);

            return vert;
        }
        private void RemoveBezierControlVertex(Vertex vert)
        {
            _controlVertices.Remove(vert);
            vert.MatrixChange -= BezierVertexChange;
            vert.Parent.HiddenChildren.Remove(vert);
        }

        private int GetConstBezierSegmentIndex()
        {
            int idx = 0;
            if (_lastChanged != null)
            {
                var idxVisible = Children.IndexOf(_lastChanged);
                if (idxVisible != -1)
                {
                    idx = idxVisible;
                }
                else
                {
                    var idxHidden = _controlVertices.FindIndex(x => x.Id == _lastChanged.Id);
                    if (idxHidden != -1)
                    {
                        idx = idxHidden / 2;
                    }

                }
            }

            return idx;
        }
        private void RecalculateBernstein()
        {
            int idx = GetConstBezierSegmentIndex();

            for (int i = idx + 1; i < Children.Count - 1; i++)
            {
                RecalculateBernsteinRight(i);
            }

            if (idx != Children.Count - 1)
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
            var d = Children[idx];
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
            var d = Children[idx + 1];
            var e = _controlVertices[2 * (idx + 1)];
            var f = _controlVertices[2 * (idx + 1) + 1];

            c.Matrix = d.Matrix;
            c.MoveLoc(d.Matrix.Translation - e.Matrix.Translation);

            b.Matrix = c.Matrix;
            var deBoor = e.Matrix.Translation + (e.Matrix.Translation - f.Matrix.Translation);
            b.MoveLoc(c.Matrix.Translation - deBoor);
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
            for (int i = 0; i < Children.Count; i++)
            {
                verts.Add(Children[i].Matrix.Translation);

                if (i < Children.Count - 1)
                {
                    verts.Add(_controlVertices[2 * i].Matrix.Translation);
                    verts.Add(_controlVertices[2 * i + 1].Matrix.Translation);
                }
            }

            return verts;
        }

        private ObjRenderData GetSpline()
        {
            var data = GerSplineCurve();
            data.Add(GerSplinePolygon());

            return data;
        }
        private ObjRenderData GerSplineCurve()
        {
            var data = new ObjRenderData();
            var verts = Children.Select(x => x.Matrix.Translation).ToList();
            if (verts.Count > 3)
                data.Vertices = GetSplineRec(verts, 0, 0, 1);

            return data;
        }
        private List<Vector3> GetSplineRec(List<Vector3> verts, int level, float start, float end)
        {
            var pointA = GetSplineValue(verts, start);
            var pointB = GetSplineValue(verts, end);
            var screenPosA = _rayCaster.GetExScreenPositionOf(pointA);
            var screenPosB = _rayCaster.GetExScreenPositionOf(pointB);

            var result = new List<Vector3>();

            if (Dist(screenPosA, screenPosB) <= 1 || level > 10)
            {
                if (screenPosA != Vector2Int.Empty)
                    result.Add(pointA);
                if (screenPosB != Vector2Int.Empty)
                    result.Add(pointB);
            }
            else
            {
                float mid = (start + end) / 2;
                var left = GetSplineRec(verts, level + 1, start, mid);
                var right = GetSplineRec(verts, level + 1, mid, end);
                result.AddRange(left);

                if (left.Count > 0 && right.Count > 0 && left[left.Count - 1] == right[0])
                    result.AddRange(right.Skip(1));
                else
                    result.AddRange(right);
            }

            return result;
        }
        private ObjRenderData GerSplinePolygon()
        {
            var verts = Children.Select(x => x.Matrix.Translation).ToList();
            var data = new ObjRenderData();
            if (ShowPolygon && verts.Count > 1)
            {
                data.Vertices = verts;
                data.Edges = Enumerable.Range(0, verts.Count - 1).Select(x => new Edge(x, x + 1)).ToList();
            }

            return data;
        }
        public Vector3 GetSplineValue(List<Vector3> points, float t)
        {
            int degree = 3;


            // remap t to the domain where the spline is defined
            var left = degree;
            var right = points.Count;
            t = t * (right - left) + left;

            // find s (the spline segment) for the [t] value provided
            int s;
            for (s = left; s < right; s++)
            {
                if (t >= s && t <= s + 1)
                {
                    break;
                }
            }

            var verts = points.ToList();
            // l (level) goes from 1 to the curve degree + 1
            for (int l = 1; l <= degree + 1; l++)
            {
                // build level l of the pyramid
                for (int i = s; i > s - degree - 1 + l; i--)
                {
                    float alpha = (t - i) / (degree + 1 - l);

                    // interpolate each component
                    verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
                }
            }

            var result = verts[s];

            return result;
        }

        private void ConvertToSpline()
        {
            var verts = GetBernsteinVertices();
            base.Children.CollectionChanged -= Children_CollectionChanged;

            Children.Clear();
            HiddenChildren.Clear();
            _controlVertices.Clear();

            var n = verts.Count;
            if (n > 3)
            {
                var b2 = verts[1] + (verts[1] - verts[2]);
                var b1 = b2 + 3 * (verts[0] + (verts[0] - verts[1]) - b2);
                AddDeBoor(b1);
                AddDeBoor(b2);

                int i;
                for (i = 3; i < n - 3; i += 3)
                {
                    AddDeBoor(verts[i-1] + (verts[i-1] - verts[i - 2]));
                }

                var b3 = verts[i - 1] + (verts[i - 1] - verts[i - 2]);
                var b4 = b3 + 3 * (verts[i] + (verts[i] - verts[i - 1]) - b3);
                AddDeBoor(b3);
                AddDeBoor(b4);
            }
        }
        private void AddDeBoor(Vector3 position)
        {
            var vert = new Vertex();
            vert.SetParent(this);
            vert.MoveLoc(position);
        }

        private void ConvertToBezier()
        {
            var verts = Children.Select(x => x.Matrix.Translation).ToList();

            if (verts.Count == 0)
                return;

            base.Children.CollectionChanged -= Children_CollectionChanged;
            Children.Clear();

            var n = verts.Count;

            if (n > 3)
            {
                AddBernstein((verts[0] + 4 * verts[1] + verts[2]) / 6);

                int i;
                for (i = 2; i < n - 1; i++)
                {
                    AddBernsteinControl((2 * verts[i - 1] + verts[i]) / 3);
                    AddBernsteinControl((verts[i - 1] + 2 * verts[i]) / 3);
                    AddBernstein((verts[i - 1] + 4 * verts[i] + verts[i + 1]) / 6);
                }
            }

            base.Children.CollectionChanged += Children_CollectionChanged;
        }
        private Vertex AddBernstein(Vector3 position)
        {
            var vert = new Vertex();
            vert.SetParent(this);
            vert.MoveLoc(position);

            return vert;
        }
        private Vertex AddBernsteinControl(Vector3 position)
        {
            var vert = new Vertex();
            vert.SetParent(this, true);
            vert.MoveLoc(position);
            _controlVertices.Add(vert);

            return vert;
        }
    }
}