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
    public class GregoryEdgeData
    {
        public BezierSurfaceC0 Surface { get; set; }
        public Vertex A { get; set; }
        public Vertex B { get; set; }
    }
    public class GregoryData
    {
        public List<Vector3> Points { get; set; } = new List<Vector3>();
        public List<List<List<Vector3>>> Arrays { get; set; } = new List<List<List<Vector3>>>();
    }



    public class GregoryPatch : SceneObject, IRenderableObj
    {
        private static int _count = 0;
        private List<GregoryEdgeData> _data;

        public GregoryPatch(List<GregoryEdgeData> data)
        {
            Name = nameof(GregoryPatch) + " " + _count++.ToString();
            _data = data;
        }

        public GregoryData GetGregoryData()
        {
            var greg = new GregoryData();

            for (int i = 0; i < _data.Count; i++)
                greg.Points.Add(_data[i].A.GlobalMatrix.Translation);

            for (int i = 0; i < _data.Count; i++)
                greg.Arrays.Add(GetArray(_data[i]));

            return greg;
        }

        public List<List<Vector3>> GetArray(GregoryEdgeData data)
        {
            var result = new List<List<Vector3>>();
            result.Add(new List<Vector3>());
            result.Add(new List<Vector3>());

            var vertA = data.Surface.GetIndices(data.A);
            var vertB = data.Surface.GetIndices(data.B);
            var verts = data.Surface.GetVertsGlobal();

            //X=h, Y=w
            if (vertA.X == vertB.X)
            {
                int change = (vertB.Y - vertA.Y) / 3;
                for (int i = 0; i < 4; i++)
                    result[0].Add(data.Surface.GetVertex(vertA.X, vertA.Y+i*change));

                int intX = vertA.X == 0 ? 1 : data.Surface.HeightVertexCount - 2;

                for (int i = 0; i < 4; i++)
                    result[1].Add(data.Surface.GetVertex(intX, vertA.Y + i * change));
            }
            else
            {
                int change = (vertB.X - vertA.Y) / 3;
                for (int i = 0; i < 4; i++)
                    result[0].Add(data.Surface.GetVertex(vertA.Y + i * change, vertA.Y));

                int intY = vertA.Y == 0 ? 1 : data.Surface.WidthVertexCount - 2;

                for (int i = 0; i < 4; i++)
                    result[1].Add(data.Surface.GetVertex(vertA.Y + i * change, intY));
            }

            return result;
        }

        public ObjRenderData GetRenderData()
        {
            var data = new ObjRenderData();
            var greg = GetGregoryData();

            var qwe = new Qwe();

            data.Vertices = qwe.RebuildGregory(greg.Arrays, greg.Points, 4, 4);

            return data;
        }

        #region manipulation
        public override void Move(Vector3 CreateTranslation) { }
        public override void Move(double x, double y, double z) { }
        public override void MoveLoc(Vector3 CreateTranslation) { }
        public override void MoveLoc(double x, double y, double z) { }
        public override void Rotate(Vector3 CreateRotation) { }
        public override void Rotate(double x, double y, double z) { }
        public override void RotateLoc(Vector3 CreateRotation) { }
        public override void RotateLoc(double x, double y, double z) { }
        public override void Scale(double x, double y, double z) { }
        public override void ScaleLoc(double x, double y, double z) { }
        #endregion
    }
}