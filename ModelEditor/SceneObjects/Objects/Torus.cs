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
    public class Torus : SceneObject, IRenderableObj, IIntersect
    {
        private static int _count = 0;
        private double _largeRadius = 5;
        private double _smallRadius = 1;
        private int _largeDensity = 10;
        private int _smallDensity = 10;

        public double LargeRadius { get { return _largeRadius; } set { _dataChanged = true; _largeRadius = value; } }
        public double SmallRadius { get { return _smallRadius; } set { _dataChanged = true; _smallRadius = value; } }
        public int LargeDensity { get { return _largeDensity; } set { _dataChanged = true; _largeDensity = value; } }
        public int SmallDensity { get { return _smallDensity; } set { _dataChanged = true; _smallDensity = value; } }

        public bool WrappedU => true;
        public bool WrappedV => true;

        private bool _dataChanged = true;
        private ObjRenderData _renderData;

        private List<Vector3> GetVertices()
        {
            var vertices = new List<Vector3>();

            var largeDiff = 2.0 * Math.PI / LargeDensity;
            var smallDiff = 2.0 * Math.PI / SmallDensity;

            for (int i = 0; i < LargeDensity; i++)
            {
                for (int j = 0; j < SmallDensity; j++)
                {
                    var a = LargeRadius + SmallRadius * Math.Cos(j * smallDiff);
                    float x = (float)(a * Math.Cos(i * largeDiff));
                    float y = (float)(a * Math.Sin(i * largeDiff));
                    float z = (float)(SmallRadius * Math.Sin(j * smallDiff));
                    vertices.Add(new Vector3(x, y, z));
                }
            }
            _dataChanged = false;

            return vertices;
        }
        private List<Edge> GetEdges()
        {
            var edges = new List<Edge>();

            for (int i = 0; i < LargeDensity; i++)
            {
                for (int j = 0; j < SmallDensity; j++)
                {
                    edges.Add(new Edge(i * SmallDensity + j, i * SmallDensity + ((j + 1) % SmallDensity)));
                    edges.Add(new Edge(i * SmallDensity + j, (i + 1) % LargeDensity * SmallDensity + j));
                }
            }

            return edges;

        }

        public Torus()
        {
            Name = nameof(Torus) + _count++.ToString();
            Holdable = true;
        }

        public ObjRenderData GetRenderData()
        {
            if (_dataChanged)
            {
                _renderData = new ObjRenderData();
                _renderData.Vertices = GetVertices();
                _renderData.Edges = GetEdges();
            }

            return _renderData;
        }

        public Vector3 Evaluate(Vector2 hw)
        {
            var angleH = 2.0 * Math.PI * hw.X;
            var angleW = 2.0 * Math.PI * hw.Y;

            var a = LargeRadius + SmallRadius * Math.Cos(angleW);
            float x = (float)(a * Math.Cos(angleH));
            float y = (float)(a * Math.Sin(angleH));
            float z = (float)(SmallRadius * Math.Sin(angleW));

            var localPos = new Vector4(x, y, z, 1);
            var globalPos = GlobalMatrix.Multiply(localPos);

            return globalPos.ToVector3();
        }

        public Vector3 EvaluateDU(Vector2 hw)
        {
            var angleH = 2.0 * Math.PI * hw.X;
            var angleW = 2.0 * Math.PI * hw.Y;

            var a = LargeRadius + SmallRadius * Math.Cos(angleW);
            float x = (float)(a * -Math.Sin(angleH) * 2.0 * Math.PI);
            float y = (float)(a * Math.Cos(angleH) * 2.0 * Math.PI);
            float z = (float)(0);

            var localDrv = new Vector4(x, y, z, 0);
            var globalDrv = GlobalMatrix.Multiply(localDrv);

            return globalDrv.ToVector3();
        }

        public Vector3 EvaluateDV(Vector2 hw)
        {
            var angleH = 2.0 * Math.PI * hw.X;
            var angleW = 2.0 * Math.PI * hw.Y;

            var aDrv = SmallRadius * -Math.Sin(angleW) * 2.0 * Math.PI;
            float x = (float)(aDrv * Math.Cos(angleH));
            float y = (float)(aDrv * Math.Sin(angleH));
            float z = (float)(SmallRadius * Math.Cos(angleW) * 2.0 * Math.PI);

            var localDrv = new Vector4(x, y, z, 0);
            var globalDrv = GlobalMatrix.Multiply(localDrv);

            return globalDrv.ToVector3();
        }
    }
}
