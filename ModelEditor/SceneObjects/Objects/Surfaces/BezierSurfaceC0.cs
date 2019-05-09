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
            DrawHeightCount = 5;
            DrawWidthCount = 5;

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

        protected override void InitPositions()
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
    }
}