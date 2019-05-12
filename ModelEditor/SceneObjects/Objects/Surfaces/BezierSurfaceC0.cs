using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Diagnostics;

namespace ModelEditor
{
    public class BezierSurfaceC0 : BezierSurfaceBaseC0, IRenderableObj
    {
        private static int _count = 0;
        public Dictionary<Vertex, Vertex> Linked { get; private set; } = new Dictionary<Vertex, Vertex>();

        public BezierSurfaceC0(RayCaster rayCaster) : base(rayCaster)
        {
            Name = nameof(BezierSurfaceC0) + " " + _count++.ToString();

            _height = 10;
            _width = 10;
            HeightPatchCount = 2;
            WidthPatchCount = 2;
            DrawHeightCount = 4;
            DrawWidthCount = 4;
            InitVertices();
        }
        public BezierSurfaceC0(RayCaster rayCaster, string data) : base(rayCaster)
        {
            DrawHeightCount = 4;
            DrawWidthCount = 4;

            var parts = data.Split(' ');
            Name = parts[0];
            HeightPatchCount = int.Parse(parts[1]);
            WidthPatchCount = int.Parse(parts[2]);
            int h = HeightVertexCount;
            int w = WidthVertexCount;

            InitVertices();

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    var vert = _controlVertices[i][j];
                    vert.StringToPosition(parts[i * w + j + 3]);
                }
            }
        }

        public ObjRenderData GetRenderData()
        {
            var verts = GetVerts();

            var data = new ObjRenderData();
            if (ShowControlGrid)
                data.Add(GetControlGrid(verts));
            if (ShowGrid)
                data.Add(GetGrid(verts));

            return data;
        }
        private List<List<Vector3>> GetVerts()
        {
            return _controlVertices.Select(row => row.Select(v => v.Matrix.Translation).ToList()).ToList();
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

        private float _width;
        public float Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    InitPositions();
                    InvokePropertyChanged(nameof(Width));
                }
            }

        }

        protected override void InitVertices()
        {
            HiddenChildren.Clear();
            _controlVertices.Clear();

            _controlVertices.AddRange(
                Enumerable.Range(0, HeightVertexCount).Select(
                    h => Enumerable.Range(0, WidthVertexCount).Select(
                        w => CreateControlVertex()).ToList()).ToList());

            InitPositions();
        }
        protected void InitPositions()
        {
            var startW = -Width / 2;
            var startH = -Height / 2;
            var stepW = Width / (WidthVertexCount - 1);
            var stepH = Height / (HeightVertexCount - 1);

            for (int h = 0; h < _controlVertices.Count; h++)
            {
                var row = _controlVertices[h];
                for (int w = 0; w < row.Count; w++)
                {
                    var position = new Vector3(startW + w * stepW, startH + h * stepH, 0);
                    row[w].Matrix = Matrix4x4.Identity;
                    row[w].MoveLoc(position);
                }
            }
        }

        public override string[] GetData()
        {
            var data = new string[2];
            data[0] = "surfaceC0 1";
            data[1] = Name.Replace(' ', '_');
            data[1] += " " + HeightPatchCount;
            data[1] += " " + WidthPatchCount;
            for (int i = 0; i < _controlVertices.Count; i++)
            {
                var row = _controlVertices[i];
                for (int j = 0; j < row.Count; j++)
                {
                    var vert = row[j];
                    data[1] += " " + vert.PositionToString();
                }
            }

            return data;
        }

        public Vector2Int GetIndices(Vertex vert)
        {
            for (int i = 0; i < _controlVertices.Count; i++)
            {
                var row = _controlVertices[i];
                for (int j = 0; j < row.Count; j++)
                {
                    if (row[j].Id == vert.Id)
                        return new Vector2Int(i, j);
                }
            }

            return Vector2Int.Empty;
        }
        public bool IsOnEdge(Vertex vert)
        {
            var idx = GetIndices(vert);
            return idx.X % 3 == 0 && idx.Y % 3 == 0;
        }

        public static float GetMatrixDiff(Matrix4x4 a, Matrix4x4 b)
        {
            var mat = b - a;
            var diff = Math.Abs(mat.M11) + Math.Abs(mat.M12) + Math.Abs(mat.M13) + Math.Abs(mat.M14)
                    + Math.Abs(mat.M21) + Math.Abs(mat.M22) + Math.Abs(mat.M23) + Math.Abs(mat.M24)
                    + Math.Abs(mat.M31) + Math.Abs(mat.M32) + Math.Abs(mat.M33) + Math.Abs(mat.M34)
                    + Math.Abs(mat.M41) + Math.Abs(mat.M42) + Math.Abs(mat.M43) + Math.Abs(mat.M44);

            return diff;
        }
        public static void LinkVertices(Vertex a, Vertex b)
        {
            if (!(a.Parent is BezierSurfaceC0 surfA) || !(b.Parent is BezierSurfaceC0 surfB))
                return;

            if (surfA.Id == surfB.Id)
                return;

            if (surfA.Linked.Keys.Any(k => k.Id == a.Id) || surfB.Linked.Keys.Any(k => k.Id == b.Id))
                return;

            if (surfA.IsOnEdge(a) && surfB.IsOnEdge(b))
                return;

            surfA.Linked.Add(a, b);
            surfB.Linked.Add(b, a);

            var eps = 0.000001;

            a.GlobalMatrixChange += (s, e) =>
            {
                var diff = GetMatrixDiff(b.GlobalMatrix, e.NewMatrix);
                if (diff > eps)
                    b.GlobalMatrix = e.NewMatrix;
            };

            b.GlobalMatrixChange += (s, e) =>
            {
                var diff = GetMatrixDiff(a.GlobalMatrix, e.NewMatrix);
                if (diff > eps)
                    a.GlobalMatrix = e.NewMatrix;
            };

            var pos = (a.GlobalMatrix.Translation + b.GlobalMatrix.Translation) / 2;

            var matA = a.GlobalMatrix;
            matA.Translation = pos;
            a.GlobalMatrix = matA;
        }
    }
}