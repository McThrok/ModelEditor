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
    public abstract class BezierSurfaceBaseC2 : SceneObject
    {
        protected List<List<Vertex>> _controlVertices = new List<List<Vertex>>();
        protected readonly RayCaster _rayCaster;
        protected Vector3[] _tmpH;
        protected Vector3[] _tmpW;
        protected int[] _knotsH;
        protected int[] _knotsW;

        public BezierSurfaceBaseC2(RayCaster rayCaster)
        {
            Holdable = false;
            ShowGrid = true;
            _rayCaster = rayCaster;
        }

        protected ObjRenderData GetControlGrid(List<List<Vector3>> verts)
        {
            var data = new ObjRenderData();
            data.Vertices = verts.SelectMany(x => x).ToList();

            var count = 0;
            for (int h = 0; h < verts.Count; h++)
            {
                var row = verts[h];
                for (int w = 0; w < row.Count - 1; w++)
                {
                    var idx = count + w;
                    data.Edges.Add(new Edge(idx, idx + 1));
                }
                count += row.Count;
            }

            count = 0;
            for (int h = 0; h < verts.Count - 1; h++)
            {
                var row = verts[h];
                for (int w = 0; w < row.Count; w++)
                {
                    var idx = count + w;
                    data.Edges.Add(new Edge(idx, idx + row.Count));
                }
                count += row.Count;
            }

            return data;
        }
        protected ObjRenderData GetGrid(List<List<Vector3>> verts)
        {
            var data = new ObjRenderData();

            var dh = DrawHeightCount;
            for (int h = 0; h < dh; h++)
            {
                var tu = 1f * h / (dh - 1);
                FillTmpW(verts, tu);
                data.AddLine(GetLine(_tmpW, _knotsW));
            }

            var dw = DrawWidthCount;
            for (int w = 0; w < dw; w++)
            {
                var tv = 1f * w / (dw - 1);
                FillTmpH(verts, tv);
                data.AddLine(GetLine(_tmpH, _knotsH));
            }

            return data;
        }

        protected void FillTmpW(List<List<Vector3>> verts, float tu)
        {
            int n = _tmpW.Length;
            for (int i = 0; i < n; i++)
            {
                var nodes = verts.Select(v => v[i]).ToList();
                _tmpW[i] = GetSplineValue(tu, nodes, _knotsH);
            }
        }
        protected void FillTmpH(List<List<Vector3>> verts, float tv)
        {
            int n = _tmpH.Length;
            for (int i = 0; i < n; i++)
            {
                var nodes = verts[i];
                _tmpH[i] = GetSplineValue(tv, nodes, _knotsW);
            }
        }

        protected List<Vector3> GetLine(Vector3[] points, int[] knots)
        {
            var curve = new List<Vector3>();
            int n = DrawPoints;
            for (int i = 0; i < n + 1; i++)
            {
                curve.Add(GetSplineValue(1f * i / n, points, knots));
            }

            return curve;

        }
        public Vector3 GetSplineValue(float t, Vector3[] points, int[] knots)
        {
            int degree = 3;
            var left = knots[degree];
            var right = knots[knots.Length - 1 - degree];
            t = t * (right - left) + left;

            int s;
            for (s = degree; s < knots.Length - 1 - degree; s++)
            {
                if (t >= knots[s] && t <= knots[s + 1])
                {
                    break;
                }
            }

            var verts = points.ToList();
            for (int l = 1; l <= degree + 1; l++)
            {
                for (int i = s; i > s - degree - 1 + l; i--)
                {
                    float alpha = (t - knots[i]) / (knots[i + degree + 1 - l] - knots[i]);
                    verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
                }
            }

            return verts[s];
        }
        public Vector3 GetSplineValue(float t, List<Vector3> points, int[] knots)
        {
            int degree = 3;
            var left = knots[degree];
            var right = knots[knots.Length - 1 - degree];
            t = t * (right - left) + left;

            int s;
            for (s = degree; s < knots.Length - 1 - degree; s++)
            {
                if (t >= knots[s] && t <= knots[s + 1])
                {
                    break;
                }
            }

            var verts = points.ToList();
            for (int l = 1; l <= degree + 1; l++)
            {
                for (int i = s; i > s - degree - 1 + l; i--)
                {
                    float alpha = (t - knots[i]) / (knots[i + degree + 1 - l] - knots[i]);
                    verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
                }
            }

            return verts[s];
        }
        //public Vector3 GetSplineValue(float t, Vector3[] points, int[] knots)
        ////public Vector3 GetSplineValue(Vector3[] points, float t)
        //{
        //    int degree = 3;

        //    var left = degree;
        //    var right = points.Length;
        //    t = t * (right - left) + left;

        //    int s;
        //    for (s = left; s < right; s++)
        //    {
        //        if (t >= s && t <= s + 1)
        //        {
        //            break;
        //        }
        //    }

        //    var verts = points.ToList();
        //    for (int l = 1; l <= degree + 1; l++)
        //    {
        //        for (int i = s; i > s - degree - 1 + l; i--)
        //        {
        //            float alpha = (t - i) / (degree + 1 - l);

        //            verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
        //        }
        //    }

        //    var result = verts[s];

        //    return result;
        //}
        //public Vector3 GetSplineValue(float t, List<Vector3> points, int[] knots)
        ////public Vector3 GetSplineValue(List<Vector3> points, float t)
        //{
        //    int degree = 3;

        //    var left = degree;
        //    var right = points.Count;
        //    t = t * (right - left) + left;

        //    int s;
        //    for (s = left; s < right; s++)
        //    {
        //        if (t >= s && t <= s + 1)
        //        {
        //            break;
        //        }
        //    }

        //    var verts = points.ToList();
        //    for (int l = 1; l <= degree + 1; l++)
        //    {
        //        for (int i = s; i > s - degree - 1 + l; i--)
        //        {
        //            float alpha = (t - i) / (degree + 1 - l);

        //            verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
        //        }
        //    }

        //    var result = verts[s];

        //    return result;
        //}

        public int DrawPoints => 3000 / (int)Math.Sqrt(DrawHeightCount * DrawWidthCount);

        private bool _showControlGrid;
        public bool ShowControlGrid
        {
            get => _showControlGrid;
            set
            {
                if (_showControlGrid != value)
                {
                    _showControlGrid = value;
                    InvokePropertyChanged(nameof(ShowControlGrid));
                }
            }

        }

        private bool _showGrid;
        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                if (_showGrid != value)
                {
                    _showGrid = value;
                    InvokePropertyChanged(nameof(ShowGrid));
                }
            }

        }

        private int _drawHeightCount;
        public int DrawHeightCount
        {
            get => _drawHeightCount;
            set
            {
                if (_drawHeightCount != value)
                {
                    _drawHeightCount = value;
                    InvokePropertyChanged(nameof(DrawHeightCount));
                }
            }

        }

        private int _drawWidthCount;
        public int DrawWidthCount
        {
            get => _drawWidthCount;
            set
            {
                if (_drawWidthCount != value)
                {
                    _drawWidthCount = value;
                    InvokePropertyChanged(nameof(DrawWidthCount));
                }
            }

        }

        private int _widthCount;
        public int WidthCount
        {
            get => _widthCount;
            set
            {
                var newValue = value;
                newValue = Math.Max(1, newValue);
                if (_widthCount != newValue)
                {
                    _widthCount = newValue;
                    InitVertices();
                    InvokePropertyChanged(nameof(WidthCount));
                }
            }

        }

        private int _heightCount;
        public int HeightCount
        {
            get => _heightCount;
            set
            {
                var newValue = value;
                newValue = Math.Max(1, newValue);

                if (_heightCount != newValue)
                {
                    _heightCount = newValue;
                    InitVertices();
                    InvokePropertyChanged(nameof(HeightCount));
                }
            }

        }


        protected abstract void InitPositions();
        protected abstract void InitKnots();
        protected void InitVertices()
        {
            HiddenChildren.Clear();
            _controlVertices.Clear();

            _controlVertices.AddRange(
                Enumerable.Range(0, HeightCount).Select(
                    h => Enumerable.Range(0, WidthCount).Select(
                        w => CreateControlVertex()).ToList()).ToList());

            InitPositions();
            InitKnots();
        }

        protected Vertex CreateControlVertex()
        {
            var vert = new Vertex();
            vert.SetParent(this, true);
            return vert;
        }
    }
}