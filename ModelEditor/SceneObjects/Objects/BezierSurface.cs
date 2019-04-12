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
    public class BezierSurface : SceneObject, IRenderableObj
    {
        private static int _count = 0;
        private List<List<Vertex>> _controlVertices = new List<List<Vertex>>();
        private readonly RayCaster _rayCaster;

        public BezierSurface(RayCaster rayCaster)
        {
            Holdable = false;
            _rayCaster = rayCaster;
            Name = nameof(BezierSurface) + " " + _count++.ToString();
        }


        public ObjRenderData GetRenderData()
        {
            var verts = GetVerts();

            var data = new ObjRenderData();
            if (ShowControlGrid)
                data.Add(GetControlGrid(verts));
            data.Add(GetGrid(verts));

            return data;
        }
        private List<List<Vector3>> GetVerts()
        {
            return _controlVertices.Select(row => row.Select(v => v.Matrix.Translation).ToList()).ToList();
        }
        private ObjRenderData GetControlGrid(List<List<Vector3>> verts)
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
                    var idx = w * width + h;
                    data.Edges.Add(new Edge(idx, idx + w));
                }
            }

            return data;

        }
        private ObjRenderData GetGrid(List<List<Vector3>> verts)
        {
            var data = new ObjRenderData();

            for (int h = 0; h < DrawHeightCount; h++)
            {
                var idxH = HeightPatchCount * h / (DrawHeightCount - 1);
                var tu = 1f * HeightPatchCount * h / (DrawHeightCount - 1) - idxH;

                for (int w = 0; w < HeightPatchCount; w++)
                {
                    var idxW = w * 3;
                    data.Vertices.AddRange(GetWidthSegmentPrimitive(verts, idxH, idxW, tu));
                }
            }

            for (int w = 0; w < DrawWidthCount; w++)
            {
                var idxW = WidthPatchCount * w / (DrawWidthCount - 1);
                var tv = 1f * WidthPatchCount * w / (DrawWidthCount - 1) - idxW;

                for (int h = 0; h < WidthPatchCount; h++)
                {
                    var idxH = h * 3;
                    data.Vertices.AddRange(GetHeightSegmentPrimitive(verts, idxW, idxH, tv));
                }
            }

            return data;
        }

        private List<Vector3> GetHeightSegmentPrimitive(List<List<Vector3>> verts, int idxW, int idxH, float tv)
        {
            var curve = new List<Vector3>();
            int n = DrawPoints;
            for (int i = 0; i < n; i++)
            {
                curve.Add(GetValue(verts, idxH, idxW, 1f * i / n, tv));
            }

            return curve;
        }
        private List<Vector3> GetWidthSegmentPrimitive(List<List<Vector3>> verts, int idxH, int idxW, float tu)
        {
            var curve = new List<Vector3>();
            int n = DrawPoints;
            for (int i = 0; i < n; i++)
            {
                curve.Add(GetValue(verts, idxH, idxW, tu, 1f * i / n));
            }

            return curve;
        }

        private float[] _u = new float[4];
        private float[] _v = new float[4];
        private Vector3 GetValue(List<List<Vector3>> verts, int idxH, int idxW, float tu, float tv)
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
            _v[3] = tv * tv * tu;

            var point = Vector3.Zero;
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    point += verts[idxH + x][idxW + y] * _u[x] * _v[y];
                }
            }


            return point;
        }

        public int DrawPoints { get; set; } = 1000;
        public int DrawWidthCount { get; set; } = 5;
        public int DrawHeightCount { get; set; } = 5;

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

        private float _height;
        public float Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    InitVertices();
                    InvokePropertyChanged(nameof(Height));
                }
            }

        }

        private float _width;
        public float Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    InitVertices();
                    InvokePropertyChanged(nameof(Width));
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
                    InitPositions();
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
                    InitPositions();
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

        private void InitPositions()
        {
            var startW = -Width / 2;
            var startH = -Height / 2;
            var stepW = Width / WidthVertexCount;
            var stepH = Height / HeightVertexCount;

            for (int h = 0; h < _controlVertices.Count; h++)
            {
                var row = _controlVertices[h];
                for (int w = 0; w < row.Count; w++)
                {
                    var position = new Vector3(startH + h * stepH, startW + w * stepW, 0);
                    row[w].Matrix = Matrix4x4.Identity;
                    row[w].MoveLoc(position);
                }
            }
        }
        private void InitVertices()
        {
            _controlVertices.Clear();

            _controlVertices.AddRange(
                Enumerable.Range(0, HeightVertexCount).Select(
                    x => Enumerable.Range(0, WidthVertexCount).Select(
                        y => CreateControlVertex()).ToList()).ToList());

            InitPositions();
        }

        private Vertex CreateControlVertex()
        {
            var vert = new Vertex();
            vert.SetParent(this, true);
            return vert;
        }
    }
}