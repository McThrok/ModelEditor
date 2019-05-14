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

    public class GregoryPatch : SceneObject, IRenderableObj
    {
        private static int _count = 0;
        private List<GregoryEdgeData> _data;

        public GregoryPatch(List<GregoryEdgeData> data)
        {
            Name = nameof(GregoryPatch) + " " + _count++.ToString();
            _data = data;
        }

        public ObjRenderData GetRenderData()
        {
            var data = new ObjRenderData();
            var middleData = new List<List<Vector3>>();
            for (int i = 0; i < _data.Count; i++)
                middleData.Add(GetMiddleData(_data[i]));
            var pPoins = GetP(middleData);

            for (int i = 0; i < pPoins.Count; i++)
                data.AddLine(pPoins[i]);

            return data;
        }

        private List<List<Vector3>> GetP(List<List<Vector3>> middleData)
        {
            var result = new List<List<Vector3>>();

            for (int i = 0; i < middleData.Count; i++)
            {
                result.Add(new List<Vector3>());
                result[i].Add(middleData[i][0]);
                result[i].Add(result[i][0] + middleData[i][1]);
                result[i].Add(0.5f * (3 * result[i][1] - result[i][0]));//Q
            }

            var p = (result[0][2] + result[1][2] + result[2][2]) / 3;

            for (int i = 0; i < middleData.Count; i++)
            {
                result[i].Add(p);
            }

            for (int i = 0; i < middleData.Count; i++)
            {
                result[i][2] = (2 * result[i][2] + p) / 3;
            }

            return result;
        }

        private List<Vector3> GetMiddleData(GregoryEdgeData data)
        {
            var result = new List<Vector3>();

            var vertA = data.Surface.GetIndices(data.A);
            var vertB = data.Surface.GetIndices(data.B);

            int idxW;
            int idxH;

            if (vertA.X == vertB.X)
            {
                idxW = data.Surface.WidthVertexCount / 2;
                idxH = vertA.X == 0 ? 1 : data.Surface.HeightVertexCount - 2;

                result.Add(data.Surface.GetVertex(vertA.X, idxW));
            }
            else
            {
                idxH = data.Surface.HeightVertexCount / 2;
                idxW = vertA.Y == 0 ? 1 : data.Surface.WidthVertexCount - 2;

                result.Add(data.Surface.GetVertex(idxH, vertA.Y));
            }

            result.Add(result[0] - data.Surface.GetVertex(idxH, idxW));

            return result;
        }
    }
}