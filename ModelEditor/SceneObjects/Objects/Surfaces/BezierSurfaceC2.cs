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
    public class BezierSurfaceC2 : BezierSurfaceBaseC2, IRenderableObj
    {
        private static int _count = 0;

        public BezierSurfaceC2(RayCaster rayCaster) : base(rayCaster)
        {
            Name = nameof(BezierSurfaceC2) + " " + _count++.ToString();

            _height = 10;
            _width = 10;
            HeightPatchCount = 2;
            WidthPatchCount = 2;
            DrawHeightCount = 5;
            DrawWidthCount = 5;
            InitVertices();
        }
        public BezierSurfaceC2(RayCaster rayCaster, string data) : base(rayCaster, data)
        {
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

        protected override void InitPositions()
        {
            var startW = -Width / 2;
            var startH = -Height / 2;
            var stepW = Width / (WidthCount - 1);
            var stepH = Height / (HeightCount - 1);

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
        protected override void InitKnots()
        {
            int degree = 3;
            _tmpW = new Vector3[WidthCount];
            _tmpH = new Vector3[HeightCount];


            int h = HeightCount;
            _knotsH = new int[h + degree + 1];

            for (int i = 0; i < degree; i++)
                _knotsH[i] = 0;

            for (int i = 0; i < h - degree + 1; i++)
                _knotsH[degree + i] = i;

            for (int i = 0; i < degree; i++)
                _knotsH[h + 1 + i] = h - degree;


            int w = WidthCount;
            _knotsW = new int[w + degree + 1];

            for (int i = 0; i < degree; i++)
                _knotsW[i] = 0;

            for (int i = 0; i < w - degree + 1; i++)
                _knotsW[degree + i] = i;

            for (int i = 0; i < degree; i++)
                _knotsW[w + 1 + i] = w - degree;
        }

        public override string[] GetData()
        {
            var data = new string[2];
            data[0] = "surfaceC2";
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
    }
}