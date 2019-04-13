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
    public class BezierCylinder : SceneObject, IRenderableObj
    {
        private static int _count = 0;
        private List<List<Vertex>> _controlVertices = new List<List<Vertex>>();
        private readonly RayCaster _rayCaster;

        public BezierCylinder(RayCaster rayCaster)
        {
            Holdable = false;
            _rayCaster = rayCaster;
            Name = nameof(BezierCylinder) + " " + _count++.ToString();

            _height = 10;
            _range = 10;
            _heightPatchCount = 2;
            _widthPatchCount = 5;
            DrawHeightCount = 5;
            DrawWidthCount = 5;
            InitVertices();
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
            return _controlVertices.Select(row =>
            {
                var result = row.Select(v => v.Matrix.Translation).ToList();
                if (result.Count > 0)
                    result.Add(result[0]);
                return result;
            }).ToList();

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
                    var idx = h * width + w;
                    data.Edges.Add(new Edge(idx, idx + width));
                }
            }

            return data;

        }
        private ObjRenderData GetGrid(List<List<Vector3>> verts)
        {
            var data = new ObjRenderData();

            for (int h = 0; h < DrawHeightCount; h++)
            {
                var pIdxH = HeightPatchCount * h / (DrawHeightCount - 1);
                if (h == DrawHeightCount - 1)
                    pIdxH -= 1;

                var tu = 1f * HeightPatchCount * h / (DrawHeightCount - 1) - pIdxH;
                var IdxH = pIdxH * 3;

                for (int w = 0; w < WidthPatchCount; w++)
                {
                    var idxW = w * 3;
                    data.Vertices.AddRange(GetWidthSegmentPrimitive(verts, IdxH, idxW, tu));
                }
            }

            for (int w = 0; w < DrawWidthCount; w++)
            {
                var pIdxW = WidthPatchCount * w / (DrawWidthCount - 1);
                if (w == DrawWidthCount - 1)
                    pIdxW -= 1;

                var tv = 1f * WidthPatchCount * w / (DrawWidthCount - 1) - pIdxW;
                var idxW = 3 * pIdxW;

                for (int h = 0; h < HeightPatchCount; h++)
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
            for (int i = 0; i < n + 1; i++)
            {
                curve.Add(GetValue(verts, idxH, idxW, 1f * i / n, tv));
            }

            return curve;
        }
        private List<Vector3> GetWidthSegmentPrimitive(List<List<Vector3>> verts, int idxH, int idxW, float tu)
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

        private float _height;
        public float Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    InitPositions();
                    InvokePropertyChanged(nameof(Height));
                }
            }

        }

        private float _range;
        public float Range
        {
            get => _range;
            set
            {
                if (_range != value)
                {
                    _range = value;
                    InitPositions();
                    InvokePropertyChanged(nameof(Range));
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
            get => 3 * WidthPatchCount + 1 - 1;
        }
        public int HeightVertexCount
        {
            get => 3 * HeightPatchCount + 1;
        }

        private void InitPositions()
        {
            //var startW = -Width / 2;
            var startH = -Height / 2;
            //var stepW = Width / (WidthVertexCount - 1);
            var stepH = Height / (HeightVertexCount - 1);

            for (int h = 0; h < _controlVertices.Count; h++)
            {
                var row = _controlVertices[h];
                for (int w = 0; w < row.Count; w++)
                {
                    var rad = Math.PI * 2 * w / row.Count;
                    var x = (float)(Range * Math.Cos(rad));
                    var z = (float)(Range * Math.Sin(rad));

                    var position = new Vector3(x, startH + h * stepH, z);
                    row[w].Matrix = Matrix4x4.Identity;
                    row[w].MoveLoc(position);
                }
            }
        }
        private void InitVertices()
        {
            HiddenChildren.Clear();
            _controlVertices.Clear();

            _controlVertices.AddRange(
                Enumerable.Range(0, HeightVertexCount).Select(
                    h => Enumerable.Range(0, WidthVertexCount).Select(
                        w => CreateControlVertex()).ToList()).ToList());

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