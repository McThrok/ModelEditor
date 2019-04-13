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
    public class BezierSurface : BezierSurfaceBase, IRenderableObj
    {
        private static int _count = 0;

        public BezierSurface(RayCaster rayCaster) : base(rayCaster)
        {
            Name = nameof(BezierSurface) + " " + _count++.ToString();

            _height = 10;
            _width = 10;
            HeightPatchCount = 2;
            WidthPatchCount = 2;
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

        private int _widthPatchCount;

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
    }
}