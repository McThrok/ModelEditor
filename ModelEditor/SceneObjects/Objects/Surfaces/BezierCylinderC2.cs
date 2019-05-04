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

            _height = 10;
            _range = 10;
            HeightCount = 5;
            WidthCount = 5;
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
                if (result.Count > degree)
                    for (int i = 0; i < degree + 1; i++)
                        result.Add(result[i]);
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
            var stepH = Height / (HeightCount - 1);

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

            int degree = 3;
            _tmpW = new Vector3[WidthCount + degree + 1];
            _tmpH = new Vector3[HeightCount];
        }

    }
}