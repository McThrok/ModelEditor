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
    public class GregoryPatchData
    {
        public List<Vector3> Pdown { get; set; }
        public List<Vector3> Pup { get; set; }
        public GregoryEdgeData EdgeDown { get; set; }
        public GregoryEdgeData EdgeUp { get; set; }
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
                data.AddLine(pPoins[1]);

            //var patchDatas = GetPatchDatas(pPoins, _data);
            //data.AddLine(patchDatas[0].Pdown);
            //data.AddLine(patchDatas[0].Pup.Take(2).ToList());

            return data;
        }

        private List<GregoryPatchData> GetPatchDatas(List<List<Vector3>> pPoins, List<GregoryEdgeData> edgeData)
        {
            var data = new List<GregoryPatchData>();

            int n = edgeData.Count;
            for (int i = 0; i < n; i++)
            {
                var patch = new GregoryPatchData();
                patch.EdgeDown = edgeData[i];
                patch.EdgeUp = edgeData[(i + 1) % n];
                patch.Pdown = pPoins[i];
                patch.Pup = pPoins[(i + 1) % n];

                data.Add(patch);
            }

            return data;
        }
        private List<List<Vector3>> GetP(List<List<Vector3>> middleData)
        {
            var result = new List<List<Vector3>>();

            var pEst = (middleData[0][0] + middleData[1][0] + middleData[2][0]) / 3;
            for (int i = 0; i < middleData.Count; i++)
            {
                result.Add(new List<Vector3>());
                result[i].Add(middleData[i][0]);
                var tanget = Vector3.Distance(pEst, result[i][0]) / 3 * middleData[i][1].Normalized();
                result[i].Add(result[i][0] + tanget);
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

            if (vertA.X > vertB.X || vertA.Y > vertB.Y)
            {
                var tmp = vertB;
                vertB = vertA;
                vertA = tmp;
            }

            var verts = data.Surface.GetVertsGlobal();
            Vector3 vertex;
            Vector3 vector;

            if (vertA.X == vertB.X)
            {
                int pIdxW = vertA.Y / 3;

                if (vertA.X == 0)
                {
                    vertex = data.Surface.GetValue(verts, 0, pIdxW, 0, 0.5f);
                    vector = -data.Surface.GetValueDivH(verts, 0, pIdxW, 0, 0.5f);
                }
                else
                {
                    var hpc = data.Surface.HeightPatchCount;
                    vertex = data.Surface.GetValue(verts, hpc - 1, pIdxW, 1, 0.5f);
                    vector = data.Surface.GetValueDivH(verts, hpc - 1, pIdxW, 1, 0.5f);
                }
            }
            else
            {
                int pIdxH = vertA.X / 3;

                if (vertA.Y == 0)
                {
                    vertex = data.Surface.GetValue(verts, pIdxH, 0, 0.5f, 0);
                    vector = -data.Surface.GetValueDivW(verts, pIdxH, 0, 0.5f, 0);
                }
                else
                {
                    var hpw = data.Surface.WidthPatchCount;
                    vertex = data.Surface.GetValue(verts, pIdxH, hpw - 1, 0.5f, 1);
                    vector = data.Surface.GetValueDivW(verts, pIdxH, hpw - 1, 0.5f, 1);
                }
            }

            result.Add(vertex);
            result.Add(vector);

            return result;
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