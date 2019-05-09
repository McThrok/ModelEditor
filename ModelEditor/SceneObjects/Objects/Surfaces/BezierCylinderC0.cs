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
    public class BezierCylinderC0 : BezierSurfaceBaseC0, IRenderableObj
    {
        private static int _count = 0;

        public BezierCylinderC0(RayCaster rayCaster) : base(rayCaster)
        {
            Name = nameof(BezierCylinderC0) + " " + _count++.ToString();

            _height = 5;
            _range = 5;
            HeightPatchCount = 2;
            WidthPatchCount = 2;
            DrawHeightCount = 5;
            DrawWidthCount = 5;
            InitVertices();
        }
        public BezierCylinderC0(RayCaster rayCaster, string data) : base(rayCaster)
        {
            DrawHeightCount = 5;
            DrawWidthCount = 5;

            var parts = data.Split(' ');
            Name = parts[0];
            HeightPatchCount = int.Parse(parts[1]);
            WidthPatchCount = int.Parse(parts[2]);
            int h = HeightVertexCount;
            int w = WidthVertexCount - 1;

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
            return _controlVertices.Select(row =>
            {
                var result = row.Select(v => v.Matrix.Translation).ToList();
                if (result.Count > 0)
                    result.Add(result[0]);
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

        protected override void InitPositions()
        {
            var startH = -Height / 2;
            var stepH = Height / (HeightVertexCount - 1);

            for (int h = 0; h < _controlVertices.Count; h++)
            {
                var row = _controlVertices[h];
                for (int w = 0; w < row.Count; w++)
                {
                    var rad = Math.PI * 2 * w / (row.Count - 1);
                    var x = (float)(Range * Math.Cos(rad));
                    var z = (float)(Range * Math.Sin(rad));

                    var position = new Vector3(x, startH + h * stepH, z);
                    row[w].Matrix = Matrix4x4.Identity;
                    row[w].MoveLoc(position);
                }
            }
        }

        public override string[] GetData()
        {
            var data = new string[2];
            data[0] = "tubeC0 1";
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