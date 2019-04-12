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
            throw new NotImplementedException();
        }

        private List<Vector3> GetSegment(List<Vector3> verts, int idx, int length)
        {

            return GetSegmentPrimitive(verts, idx, length);
        }
        private List<Vector3> GetSegmentPrimitive(List<Vector3> verts, int idx, int length)
        {
            if (length == 0) return new List<Vector3>() { };
            if (length == 1) return new List<Vector3>() { verts[idx] };

            int n = DrawPoints;
            var result = new List<Vector3>();
            for (int i = 0; i < n; i++)
            {

                var pointA = GetSegmentValue(verts, idx, length, 1f * i / n);
                var screenPosA = _rayCaster.GetExScreenPositionOf(pointA);
                result.Add(pointA);

            }

            return result;
        }
        private Vector3 GetSegmentValue(List<Vector3> verts, int idx, int length, float t)
        {
            if (length == 2)
                return GetLinear(verts, idx, t);

            if (length == 3)
                return GetQuadratic(verts, idx, t);

            return GetCubic(verts, idx, t);
        }
        private Vector3 GetCubic(List<Vector3> verts, int idx, float t)
        {
            float c = 1.0f - t;

            float b0 = c * c * c;
            float b1 = 3 * t * c * c;
            float b2 = 3 * t * t * c;
            float b3 = t * t * t;

            var point = verts[idx] * b0 + verts[idx + 1] * b1 + verts[idx + 2] * b2 + verts[idx + 3] * b3;
            return point;
        }
        private int Dist(Vector2Int a, Vector2Int b)
        {

            var diff = a - b;
            return Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));

        }

        public int DrawPoints { get; set; } = 1000;
        public int DrawHorizontalCount { get; set; } = 5;
        public int DrawVerticalCount { get; set; } = 5;

        private float _height;
        public float Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    UpdateSurfaceSize();
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
                    UpdateSurfaceSize();
                    InvokePropertyChanged(nameof(Width));
                }
            }

        }

        private int _horizontalPatchCount;
        public int HorizontalPatchCount
        {
            get => _horizontalPatchCount;
            set
            {
                var newValue = value;
                newValue = Math.Max(1, newValue);
                if (_horizontalPatchCount != newValue)
                {
                    _horizontalPatchCount = newValue;
                    InitPositions();
                    InvokePropertyChanged(nameof(HorizontalPatchCount));
                }
            }

        }

        private int _verticalPatchCount;
        public int VerticalPatchCount
        {
            get => _horizontalPatchCount;
            set
            {
                var newValue = value;
                newValue = Math.Max(1, newValue);

                if (_verticalPatchCount != newValue)
                {
                    _horizontalPatchCount = newValue;
                    InitPositions();
                    InvokePropertyChanged(nameof(VerticalPatchCount));
                }
            }

        }


        public int HorizontalVertexCount
        {
            get => 3 * HorizontalPatchCount + 1;
        }
        public int VerticalVertexCount
        {
            get => 3 * VerticalPatchCount + 1;
        }

        private void InitPositions()
        {
            var startX = -Width / 2;
            var startY = -Height / 2;
            var stepX = Width / HorizontalVertexCount;
            var stepY = Height / VerticalVertexCount;

            for (int i = 0; i < _controlVertices.Count; i++)
            {
                var row = _controlVertices[i];
                for (int j = 0; j < row.Count; j++)
                {
                    var position = new Vector3(startX + i * stepX, startY + j * stepY, 0);
                    row[j].Matrix = Matrix4x4.Identity;
                    row[j].MoveLoc(position);
                }
            }
        }

        private void InitVertices()
        {
            _controlVertices.Clear();

            _controlVertices.AddRange(
                Enumerable.Range(0, HorizontalVertexCount).Select(
                    x => Enumerable.Range(0, VerticalVertexCount).Select(
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