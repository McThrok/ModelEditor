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
    public class BezierCylinderC2 : BezierSurfaceBaseC2, IRenderableObj
    {
        private static int _count = 0;

        public BezierCylinderC2(RayCaster rayCaster) : base(rayCaster)
        {
            Name = nameof(BezierCylinderC2) + " " + _count++.ToString();

            _height = 5;
            _range = 5;
            HeightPatchCount = 2;
            WidthPatchCount = 2;
            DrawHeightCount = 5;
            DrawWidthCount = 5;
            InitVertices();
        }
        public BezierCylinderC2(RayCaster rayCaster, string data) : base(rayCaster)
        {
            DrawHeightCount = 5;
            DrawWidthCount = 5;

            var parts = data.Split(' ');
            Name = parts[0];
            HeightPatchCount = int.Parse(parts[1]);
            WidthPatchCount = int.Parse(parts[2]);
            int h = HeightVertexCount;
            int w = RangeVertexCount;

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
            int degree = 3;
            return _controlVertices.Select(row =>
            {
                var result = row.Select(v => v.Matrix.Translation).ToList();
                if (result.Count > 0)
                {
                    for (int i = 0; i < degree + 1; i++)
                        result.Add(result[i]);
                }
                return result;
            }).ToList();

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

        public int RangeVertexCount
        {
            get =>  WidthVertexCount - 1;
        }

        protected override void InitVertices()
        {
            HiddenChildren.Clear();
            _controlVertices.Clear();

            _controlVertices.AddRange(
                Enumerable.Range(0, HeightVertexCount).Select(
                    h => Enumerable.Range(0, RangeVertexCount).Select(
                        w => CreateControlVertex()).ToList()).ToList());

            InitPositions();
            InitKnots();

        }
        protected void InitPositions()
        {
            var startH = -Height / 2;
            var stepH = Height / (HeightVertexCount - 1);

            for (int h = 0; h < _controlVertices.Count; h++)
            {
                var row = _controlVertices[h];
                for (int w = 0; w < row.Count; w++)
                {
                    var rad = Math.PI * 2 * w / (row.Count);
                    var x = (float)(Range * Math.Cos(rad));
                    var z = (float)(Range * Math.Sin(rad));

                    var position = new Vector3(x, startH + h * stepH, z);
                    row[w].Matrix = Matrix4x4.Identity;
                    row[w].MoveLoc(position);
                }
            }

        }
        protected void InitKnots()
        {
            int degree = 3;
            _tmpW = new Vector3[WidthVertexCount + degree];
            _tmpH = new Vector3[HeightVertexCount];


            int h = HeightVertexCount;
            _knotsH = new int[h + degree + 1];

            for (int i = 0; i < h + degree + 1; i++)
                _knotsH[i] = i;

            int w = WidthVertexCount;
            _knotsW = new int[w + 2 * degree + 1];

            for (int i = 0; i < w + 2 * degree + 1; i++)
                _knotsW[i] = i;
        }

        public override string[] GetData()
        {
            var data = new string[2];
            data[0] = "tubeC2 1";
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


        public override bool WrappedV => true;
        public override List<List<Vector3>> GetGlobalVerts()
        {
            int degree = 3;
            return _controlVertices.Select(row =>
            {
                var result = row.Select(v => v.GlobalMatrix.Translation).ToList();
                if (result.Count > 0)
                {
                    for (int i = 0; i < degree + 1; i++)
                        result.Add(result[i]);
                }
                return result;
            }).ToList();
        }
    }
}