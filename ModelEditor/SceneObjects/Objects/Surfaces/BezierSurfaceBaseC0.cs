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
    public abstract class BezierSurfaceBaseC0 : SceneObject,TrimmingSurface
    {
        protected List<List<Vertex>> _controlVertices = new List<List<Vertex>>();
        protected readonly RayCaster _rayCaster;

        public BezierSurfaceBaseC0(RayCaster rayCaster)
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

            var H = HeightPatchCount;
            var W = WidthPatchCount;

            for (int h = 0; h < H; h++)
                for (int w = 0; w < W; w++)
                    data.Add(GetGridForPatch(verts, h, w));

            return data;
        }

        private ObjRenderData GetGridForPatch(List<List<Vector3>> verts, int pIdxH, int pIdxW)
        {
            var data = new ObjRenderData();

            var dh = DrawHeightCount;
            var pw = WidthPatchCount;
            for (int h = 0; h < dh; h++)
            {
                var tu = 1f * h / (dh - 1);

                for (int w = 0; w < pw; w++)
                    data.AddLine(GetWidthSegmentPrimitive(verts, pIdxH * 3, pIdxW * 3, tu));
            }

            var dw = DrawWidthCount;
            var ph = HeightPatchCount;

            for (int w = 0; w < dw; w++)
            {
                var tv = 1f * w / (dw - 1);

                for (int h = 0; h < ph; h++)
                    data.AddLine(GetHeightSegmentPrimitive(verts, pIdxW * 3, pIdxH * 3, tv));
            }

            return data;
        }

        protected List<Vector3> GetHeightSegmentPrimitive(List<List<Vector3>> verts, int idxW, int idxH, float tv)
        {
            var curve = new List<Vector3>();
            int n = DrawPoints;
            for (int i = 0; i < n + 1; i++)
            {
                curve.Add(GetValue(verts, idxH, idxW, 1f * i / n, tv));
            }

            return curve;
        }
        protected List<Vector3> GetWidthSegmentPrimitive(List<List<Vector3>> verts, int idxH, int idxW, float tu)
        {
            var curve = new List<Vector3>();
            int n = DrawPoints;
            for (int i = 0; i < n + 1; i++)
            {
                curve.Add(GetValue(verts, idxH, idxW, tu, 1f * i / n));
            }

            return curve;
        }

        public Vector3 GetValue(List<List<Vector3>> verts, int idxH, int idxW, float tu, float tv)
        {
            var point = Vector3.Zero;
            for (int h = 0; h < 4; h++)
            {
                for (int w = 0; w < 4; w++)
                {
                    point += verts[idxH + h][idxW + w] * GetB(h, tu) * GetB(w, tv);
                }
            }

            return point;
        }
        protected float GetB(int i, float t)
        {
            float c = 1.0f - t;

            if (i == 0)
                return c * c * c;
            if (i == 1)
                return 3 * t * c * c;
            if (i == 2)
                return 3 * t * t * c;
            if (i == 3)
                return t * t * t;

            return 0;
        }

        public int DrawPoints => 3000 / DrawHeightCount / DrawWidthCount / WidthPatchCount / HeightPatchCount;

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

        protected Vertex CreateControlVertex()
        {
            var vert = new Vertex();
            vert.SetParent(this, true);
            return vert;
        }

        public virtual bool WrappedU => false;
        public virtual bool WrappedV => false;

        public float GetBDrv(int i, float t)
        {
            float c = 1.0f - t;

            if (i == 0)
                return -3 * c * c;
            if (i == 1)
                return 3 * (-2 * t + c) * c;
            if (i == 2)
                return 3 * t * (2 - 3 * t);
            if (i == 3)
                return 3 * t * t;

            return 0;
        }

        public Vector3 GetValueDivH(List<List<Vector3>> verts, int idxH, int idxW, float tu, float tv)
        {
            var point = Vector3.Zero;
            for (int h = 0; h < 4; h++)
            {
                for (int w = 0; w < 4; w++)
                {
                    point += verts[idxH + h][idxW + w] * GetBDrv(h, tu) * GetB(w, tv);
                }
            }

            return point;
        }
        public Vector3 GetValueDivW(List<List<Vector3>> verts, int idxH, int idxW, float tu, float tv)
        {
            var point = Vector3.Zero;
            for (int h = 0; h < 4; h++)
            {
                for (int w = 0; w < 4; w++)
                {
                    point += verts[idxH + h][idxW + w] * GetB(h, tu) * GetBDrv(w, tv);
                }
            }

            return point;
        }

        protected abstract List<List<Vector3>> GetPatchVerts(int h, int w);

        public Vector3 Evaluate(Vector2 hw)
        {
            var h = hw.X;
            var w = hw.Y;
            int phc = HeightPatchCount;
            int ph = (int)Math.Floor(h * phc);
            if (ph == phc)
                ph = phc - 1;
            float hh = h * phc - ph;

            int pwc = WidthPatchCount;
            int pw = (int)Math.Floor(w * pwc);
            if (pw == pwc)
                pw = pwc - 1;
            float ww = w * pwc - pw;

            return GetValue(GetPatchVerts(ph, pw), 0, 0, hh, ww);
        }
        public Vector3 EvaluateDU(Vector2 hw)
        {
            var h = hw.X;
            var w = hw.Y;
            int phc = HeightPatchCount;
            int ph = (int)Math.Floor(h * phc);
            if (ph == phc)
                ph = phc - 1;
            float hh = h * phc - ph;

            int pwc = WidthPatchCount;
            int pw = (int)Math.Floor(w * pwc);
            if (pw == pwc)
                pw = pwc - 1;
            float ww = w * pwc - pw;

            return GetValueDivH(GetPatchVerts(ph, pw), 0, 0, hh, ww);
        }
        public Vector3 EvaluateDV(Vector2 hw)
        {
            var h = hw.X;
            var w = hw.Y;
            int phc = HeightPatchCount;
            int ph = (int)Math.Floor(h * phc);
            if (ph == phc)
                ph = phc - 1;
            float hh = h * phc - ph;

            int pwc = WidthPatchCount;
            int pw = (int)Math.Floor(w * pwc);
            if (pw == pwc)
                pw = pwc - 1;
            float ww = w * pwc - pw;

            return GetValueDivW(GetPatchVerts(ph, pw), 0, 0, hh, ww);
        }
    }
}