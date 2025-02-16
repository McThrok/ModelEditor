﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;

namespace ModelEditor
{
    public abstract class BezierSurfaceBaseC2 : SceneObject, IIntersect
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

            var dh = DrawHeightCount * HeightPatchCount;
            for (int h = 0; h < dh; h++)
            {
                var tu = 1f * h / (dh - 1);
                FillTmpW(verts, tu);
                data.AddLine(GetLine(_tmpW, _knotsW));
            }

            var dw = DrawWidthCount * WidthPatchCount;
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
                _tmpW[i] = GetSplineValue(nodes, tu);
            }
        }
        protected void FillTmpH(List<List<Vector3>> verts, float tv)
        {
            int n = _tmpH.Length;
            for (int i = 0; i < n; i++)
            {
                var nodes = verts[i];
                _tmpH[i] = GetSplineValue(nodes, tv);
            }
        }

        protected List<Vector3> GetLine(Vector3[] points, int[] knots)
        {
            var curve = new List<Vector3>();
            int n = DrawPoints;
            for (int i = 0; i < n + 1; i++)
            {
                curve.Add(GetSplineValue(1f * i / n, points));
            }

            return curve;

        }
        //public Vector3 GetSplineValue(float t, Vector3[] points, int[] knots)
        //{
        //    int degree = 3;
        //    var left = knots[degree];
        //    var right = knots[knots.Length - 1 - degree];
        //    t = t * (right - left) + left;

        //    int s;
        //    for (s = degree; s < knots.Length - 1 - degree; s++)
        //    {
        //        if (t >= knots[s] && t <= knots[s + 1])
        //        {
        //            break;
        //        }
        //    }

        //    var verts = points.ToList();
        //    for (int l = 1; l <= degree + 1; l++)
        //    {
        //        for (int i = s; i > s - degree - 1 + l; i--)
        //        {
        //            float alpha = (t - knots[i]) / (knots[i + degree + 1 - l] - knots[i]);
        //            verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
        //        }
        //    }

        //    return verts[s];
        //}
        //public Vector3 GetSplineValue(float t, List<Vector3> points, int[] knots)
        //{
        //    int degree = 3;
        //    var left = knots[degree];
        //    var right = knots[knots.Length - 1 - degree];
        //    t = t * (right - left) + left;

        //    int s;
        //    for (s = degree; s < knots.Length - 1 - degree; s++)
        //    {
        //        if (t >= knots[s] && t <= knots[s + 1])
        //        {
        //            break;
        //        }
        //    }

        //    var verts = points.ToList();
        //    for (int l = 1; l <= degree + 1; l++)
        //    {
        //        for (int i = s; i > s - degree - 1 + l; i--)
        //        {
        //            float alpha = (t - knots[i]) / (knots[i + degree + 1 - l] - knots[i]);
        //            verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
        //        }
        //    }

        //    return verts[s];
        //}

        public Vector3 GetSplineValue(Vector3[] points, float t)
        {
            return GetSplineValue(t, points);
        }
        public Vector3 GetSplineValue(float t, Vector3[] points)
        {
            int degree = 3;

            var left = degree;
            var right = points.Length;
            t = t * (right - left) + left;

            int s;
            for (s = left; s < right; s++)
            {
                if (t >= s && t <= s + 1)
                {
                    break;
                }
            }

            var verts = points.ToList();
            for (int l = 1; l <= degree + 1; l++)
            {
                for (int i = s; i > s - degree - 1 + l; i--)
                {
                    float alpha = (t - i) / (degree + 1 - l);

                    verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
                }
            }

            var result = verts[s];

            return result;
        }
        public Vector3 GetSplineValue(List<Vector3> points, float t)
        {
            int degree = 3;

            var left = degree;
            var right = points.Count;
            t = t * (right - left) + left;

            int s;
            for (s = left; s < right; s++)
            {
                if (t >= s && t <= s + 1)
                {
                    break;
                }
            }

            var verts = points.ToList();
            for (int l = 1; l <= degree + 1; l++)
            {
                for (int i = s; i > s - degree - 1 + l; i--)
                {
                    float alpha = (t - i) / (degree + 1 - l);

                    verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
                }
            }

            var result = verts[s];

            return result;
        }
        public Vector3 GetSplineDrvValue(List<Vector3> points, float t)
        {
            int degree = 3;

            var left = degree;
            var right = points.Count;
            t = t * (right - left) + left;

            int s;
            for (s = left; s < right; s++)
            {
                if (t >= s && t <= s + 1)
                {
                    break;
                }
            }

            var verts = points.ToList();
            for (int l = 1; l <= degree - 1; l++)
            {
                for (int i = s; i > s - degree - 1 + l; i--)
                {
                    float alpha = (t - i) / (degree + 1 - l);

                    verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
                }
            }

            var result = 3 * (verts[s] - verts[s - 1]);

            return result;
        }
        //public Vector3 GetSplineDrvValue(List<Vector3> points, float t, int[] knots)
        //{
        //    int degree = 3;
        //    var left = knots[degree];
        //    var right = knots[knots.Length - 1 - degree];
        //    t = t * (right - left) + left;

        //    int s;
        //    for (s = degree; s < knots.Length - 1 - degree; s++)
        //    {
        //        if (t >= knots[s] && t <= knots[s + 1])
        //        {
        //            break;
        //        }
        //    }

        //    var verts = points.ToList();
        //    for (int l = 1; l <= degree - 1; l++)
        //    {
        //        for (int i = s; i > s - degree - 1 + l; i--)
        //        {
        //            float alpha = (t - knots[i]) / (knots[i + degree + 1 - l] - knots[i]);
        //            verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
        //        }
        //    }

        //    var result = 3 * (verts[s] - verts[s - 1]);

        //    return result;
        //}

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

        private int _widthPatchCount;
        public int WidthPatchCount
        {
            get => _widthPatchCount;
            set
            {
                var newValue = value;
                newValue = Math.Max(1, newValue);
                if (_widthPatchCount != newValue)
                {
                    _widthPatchCount = newValue;
                    InitVertices();
                    InvokePropertyChanged(nameof(WidthPatchCount));
                }
            }
        }

        private int _heightPatchCount;
        public int HeightPatchCount
        {
            get => _heightPatchCount;
            set
            {
                var newValue = value;
                newValue = Math.Max(1, newValue);

                if (_heightPatchCount != newValue)
                {
                    _heightPatchCount = newValue;
                    InitVertices();
                    InvokePropertyChanged(nameof(HeightPatchCount));
                }
            }
        }

        public int WidthVertexCount
        {
            get => 3 * WidthPatchCount + 1;
        }
        public int HeightVertexCount
        {
            get => 3 * HeightPatchCount + 1;
        }

        protected abstract void InitVertices();


        public bool WrappedU => false;
        public virtual bool WrappedV => false;

        protected Vertex CreateControlVertex()
        {
            var vert = new Vertex();
            vert.SetParent(this, true);
            return vert;
        }

        public abstract List<List<Vector3>> GetGlobalVerts();

        public Vector3 Evaluate(Vector2 hw)
        {
            float h = hw.X;
            float w = hw.Y;

            var tmpW = new List<Vector3>(_tmpW.Length);
            var verts = GetGlobalVerts();
            for (int i = 0; i < _tmpW.Length; i++)
            {
                var nodes = verts.Select(v => v[i]).ToList();
                tmpW.Add(GetSplineValue(nodes, h));
            }

            var result = GetSplineValue(tmpW, w);

            return result;
        }

        public Vector3 EvaluateDU(Vector2 hw)
        {
            float h = hw.X;
            float w = hw.Y;

            var d = 0.00001f;
            if (h + d <= 1)
            {
                var a = Evaluate(hw);
                var b = Evaluate(new Vector2(h+d, w));

                return (b - a) / d;
            }
            else
            {
                var a = Evaluate(hw);
                var b = Evaluate(new Vector2(h-d, w ));

                return (a - b) / d;
            }
        }

        //public Vector3 EvaluateDU(Vector2 hw)
        //{
        //    float h = hw.X;
        //    float w = hw.Y;

        //    var tmpH = new List<Vector3>(_tmpH.Length);
        //    var verts = GetGlobalVerts();
        //    for (int i = 0; i < _tmpH.Length; i++)
        //    {
        //        var nodes = verts[i];
        //        tmpH.Add(GetSplineValue(nodes, w));
        //    }

        //    var result = GetSplineDrvValue(tmpH, h);

        //    return result;
        //}

        public Vector3 EvaluateDV(Vector2 hw)
        {
            float h = hw.X;
            float w = hw.Y;

            var d = 0.00001f;
            if (w + d <= 1)
            {
                var a = Evaluate(hw);
                var b = Evaluate(new Vector2(h, w + d));

                return (b - a) / d;
            }
            else
            {
                var a = Evaluate(hw);
                var b = Evaluate(new Vector2(h, w - d));

                return (a-b) / d;
            }
        }

        //public Vector3 EvaluateDV(Vector2 hw)
        //{
        //    float h = hw.X;
        //    float w = hw.Y;

        //    int n = _tmpW.Length;

        //    var tmpW = new List<Vector3>(n);
        //    var verts = GetGlobalVerts();
        //    for (int i = 0; i < n; i++)
        //    {
        //        var nodes = verts.Select(v => v[i]).ToList();
        //        tmpW.Add(GetSplineValue(nodes, h));
        //    }

        //    var result = GetSplineDrvValue(tmpW, w);

        //    return result;
        //}
    }
}