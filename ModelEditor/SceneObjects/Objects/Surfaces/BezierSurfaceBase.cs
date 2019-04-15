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
    public abstract class BezierSurfaceBase : SceneObject
    {
        protected List<List<Vertex>> _controlVertices = new List<List<Vertex>>();
        protected readonly RayCaster _rayCaster;

        public BezierSurfaceBase(RayCaster rayCaster)
        {
            Holdable = false;
            ShowGrid = true;
            _rayCaster = rayCaster;
        }

        protected ObjRenderData GetControlGrid(List<List<Vector3>> verts)
        {
            var data = new ObjRenderData();

            var width = WidthVertexCount;
            var height = HeightVertexCount;

            data.Vertices.AddRange(verts.SelectMany(x => x));

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width - 1; w++)
                {
                    var idx = h * width + w;
                    data.Edges.Add(new Edge(idx, idx + 1));
                }
            }

            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height - 1; h++)
                {
                    var idx = h * width + w;
                    data.Edges.Add(new Edge(idx, idx + width));
                }
            }

            return data;

        }
        protected ObjRenderData GetGrid(List<List<Vector3>> verts)
        {
            var data = new ObjRenderData();

            for (int h = 0; h < HeightPatchCount; h++)
                for (int w = 0; w < WidthPatchCount; w++)
                    data.Add(GetGridForPatch(verts, h, w));

            return data;
        }
        private ObjRenderData GetGridForPatch(List<List<Vector3>> verts, int pIdxH, int pIdxW)
        {
            var data = new ObjRenderData();

            for (int h = 0; h < DrawHeightCount; h++)
            {
                var tu = 1f * h / (DrawHeightCount - 1);

                for (int w = 0; w < WidthPatchCount; w++)
                {
                    var dataPart = new ObjRenderData();
                    dataPart.Vertices.AddRange(GetWidthSegmentPrimitive(verts, pIdxH * 3, pIdxW * 3, tu));

                    for (int i = 0; i < dataPart.Vertices.Count - 1; i++)
                        dataPart.Edges.Add(new Edge(i, i + 1));

                    data.Add(dataPart);
                }
            }

            for (int w = 0; w < DrawWidthCount; w++)
            {
                var tv = 1f * w / (DrawWidthCount - 1);

                for (int h = 0; h < HeightPatchCount; h++)
                {
                    var dataPart = new ObjRenderData();
                    dataPart.Vertices.AddRange(GetHeightSegmentPrimitive(verts, pIdxW * 3, pIdxH * 3, tv));
                    for (int i = 0; i < dataPart.Vertices.Count - 1; i++)
                        dataPart.Edges.Add(new Edge(i, i + 1));

                    data.Add(dataPart);
                }
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

        private float[] _u = new float[4];
        private float[] _v = new float[4];
        protected Vector3 GetValue(List<List<Vector3>> verts, int idxH, int idxW, float tu, float tv)
        {
            float cu = 1.0f - tu;
            _u[0] = cu * cu * cu;
            _u[1] = 3 * tu * cu * cu;
            _u[2] = 3 * tu * tu * cu;
            _u[3] = tu * tu * tu;

            float cv = 1.0f - tv;
            _v[0] = cv * cv * cv;
            _v[1] = 3 * tv * cv * cv;
            _v[2] = 3 * tv * tv * cv;
            _v[3] = tv * tv * tv;

            var point = Vector3.Zero;
            for (int h = 0; h < 4; h++)
            {
                for (int w = 0; w < 4; w++)
                {
                    point += verts[idxH + h][idxW + w] * _u[h] * _v[w];
                }
            }


            return point;
        }

        public int DrawPoints { get; set; } = 100;

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

        protected bool WrapLast { get; set; }

        protected abstract void InitPositions();
        protected void InitVertices()
        {
            HiddenChildren.Clear();
            _controlVertices.Clear();

            var widthCount = WrapLast ? WidthVertexCount - 1 : WidthVertexCount;

            _controlVertices.AddRange(
                Enumerable.Range(0, HeightVertexCount).Select(
                    h => Enumerable.Range(0, widthCount).Select(
                        w => CreateControlVertex()).ToList()).ToList());

            InitPositions();
        }

        protected Vertex CreateControlVertex()
        {
            var vert = new Vertex();
            vert.SetParent(this, true);
            return vert;
        }
    }
}