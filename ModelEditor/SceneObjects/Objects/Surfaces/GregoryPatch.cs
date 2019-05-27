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
            DrawHeightCount = 4;
            DrawWidthCount = 4;
            ShowGrid = true;
        }

        public int DrawPoints => 500 / (int)Math.Sqrt(DrawHeightCount * DrawWidthCount);

        private bool _showVectors;
        public bool ShowVectors
        {
            get => _showVectors;
            set
            {
                if (_showVectors != value)
                {
                    _showVectors = value;
                    InvokePropertyChanged(nameof(ShowVectors));
                }
            }

        }

        private bool _showGrid;
        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                if (_showGrid != value)
                {
                    _showGrid = value;
                    InvokePropertyChanged(nameof(ShowGrid));
                }
            }
        }

        private int _drawHeightCount;
        public int DrawHeightCount
        {
            get => _drawHeightCount;
            set
            {
                if (_drawHeightCount != value)
                {
                    _drawHeightCount = value;
                    InvokePropertyChanged(nameof(DrawHeightCount));
                }
            }
        }

        private int _drawWidthCount;
        public int DrawWidthCount
        {
            get => _drawWidthCount;
            set
            {
                if (_drawWidthCount != value)
                {
                    _drawWidthCount = value;
                    InvokePropertyChanged(nameof(DrawWidthCount));
                }
            }
        }

        public ObjRenderData GetRenderData()
        {
            var greg = GetGregoryData();
            var arrays = greg.Arrays;
            var points = greg.Points;

            var p3 = new List<Vector3>();
            var p2 = new List<Vector3>();
            for (var i = 0; i < 3; i++)
            {
                var p33 = GetP3(arrays[i]);
                var p22 = GetP2(arrays[i], p33);
                p3.Add(p33);
                p2.Add(p22);
            }

            List<Vector3> q = new List<Vector3>();
            for (var i = 0; i < 3; i++)
                q.Add(GetQ(p2[i], p3[i]));

            var p = (q[0] + q[1] + q[2]) / 3;

            List<Vector3> p1 = new List<Vector3>();
            for (var i = 0; i < 3; i++)
                p1.Add(GetP1(q[i], p));

            var patchCorners = new List<List<Vector3>>();
            patchCorners.Add(new List<Vector3>() { points[1], p3[0], p, p3[1] });
            patchCorners.Add(new List<Vector3>() { points[2], p3[1], p, p3[2] });
            patchCorners.Add(new List<Vector3>() { points[0], p3[2], p, p3[0] });

            var patchSides = new List<List<Vector3>>();
            patchSides.Add(new List<Vector3>() { p2[0], p1[0], p1[1], p2[1], p1[2] });
            patchSides.Add(new List<Vector3>() { p2[1], p1[1], p1[2], p2[2], p1[0] });
            patchSides.Add(new List<Vector3>() { p2[2], p1[2], p1[0], p2[0], p1[1] });

            var data = new ObjRenderData();
            data.Add(CreateSubPatch(patchCorners[0], patchSides[0], new List<List<List<Vector3>>>() { arrays[0], arrays[1] }));
            data.Add(CreateSubPatch(patchCorners[1], patchSides[1], new List<List<List<Vector3>>>() { arrays[1], arrays[2] }));
            data.Add(CreateSubPatch(patchCorners[2], patchSides[2], new List<List<List<Vector3>>>() { arrays[2], arrays[0] }));

            return data;
        }

        private GregoryData GetGregoryData()
        {
            var greg = new GregoryData();

            for (int i = 0; i < _data.Count; i++)
                greg.Points.Add(_data[i].A.GlobalMatrix.Translation);

            for (int i = 0; i < _data.Count; i++)
                greg.Arrays.Add(GetArray(_data[i]));

            return greg;
        }
        private List<List<Vector3>> GetArray(GregoryEdgeData data)
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
                    result[0].Add(data.Surface.GetVertex(vertA.X, vertA.Y + i * change));

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
        private ObjRenderData CreateSubPatch(List<Vector3> P, List<Vector3> sides, List<List<List<Vector3>>> arrays)
        {
            var data = new ObjRenderData();
            var ab = GetABArray(arrays);

            var a0 = P[1] - ab[0][4];
            var b0 = ab[0][2] - P[1];
            var a3 = P[2] - sides[4];
            var b3 = sides[2] - P[2];
            var gs = GetGs(a0, b0, a3, b3);
            var cs = GetCs(P[2], sides[1], sides[0], P[1]);
            var kh0 = GetKH(gs[0], cs[0], b0);
            var kh1 = GetKH(gs[2], cs[2], b3);
            var dv0 = GetDValue(1f / 3, gs, cs, kh0, kh1);
            var dv1 = GetDValue(2f / 3, gs, cs, kh0, kh1);
            var cPrim1 = sides[0] + dv0;
            var dPrim1 = sides[1] + dv1;

            a0 = P[3] - ab[1][4];
            b0 = ab[1][2] - P[3];
            b3 = sides[1] - P[2];
            a3 = P[2] - sides[4];
            gs = GetGs(a0, b0, a3, b3);
            cs = GetCs(P[2], sides[2], sides[3], P[3]);
            kh0 = GetKH(gs[0], cs[0], b0);
            kh1 = GetKH(gs[2], cs[2], b3);
            dv0 = GetDValue(1f / 3, gs, cs, kh0, kh1);
            dv1 = GetDValue(2f / 3, gs, cs, kh0, kh1);
            var cPrim2 = sides[3] + dv0;
            var dPrim2 = sides[2] + dv1;

            if (ShowVectors)
            {
                var left0 = GetCubicValue(1 - 1f / 6, arrays[0][0]);
                var left1 = GetCubicValue(1 - 1f / 3, arrays[0][0]);
                var right0 = GetCubicValue(1f / 6, arrays[1][0]);
                var right1 = GetCubicValue(1f / 3, arrays[1][0]);
                data.AddLine(new List<Vector3>() { left0, left0 + ab[0][1] - ab[0][0] });
                data.AddLine(new List<Vector3>() { left1, left1 + ab[0][3] - ab[0][2] });
                data.AddLine(new List<Vector3>() { right0, right0 + ab[1][1] - ab[1][0] });
                data.AddLine(new List<Vector3>() { right1, right1 + ab[1][3] - ab[1][2] });
            }

            if (!ShowGrid)
                return data;

            var preG = new List<List<Vector3>>();
            for (int i = 0; i < 4; i++)
            {
                preG.Add(new List<Vector3>());
            }

            preG[0].Add(P[0]);
            preG[0].Add(ab[1][0]);
            preG[0].Add(ab[1][2]);
            preG[0].Add(P[3]);

            preG[1].Add(ab[0][0]);
            preG[1].Add(Vector3.Zero);
            preG[1].Add(Vector3.Zero);
            preG[1].Add(sides[3]);

            preG[2].Add(ab[0][2]);
            preG[2].Add(Vector3.Zero);
            preG[2].Add(Vector3.Zero);
            preG[2].Add(sides[2]);

            preG[3].Add(P[1]);
            preG[3].Add(sides[0]);
            preG[3].Add(sides[1]);
            preG[3].Add(P[2]);


            var prims = new List<List<Vector3>>();
            for (int i = 0; i < 4; i++)
            {
                prims.Add(new List<Vector3>());
            }

            prims[0].Add(ab[0][1]);
            prims[0].Add(ab[1][1]);
            prims[1].Add(ab[0][3]);
            prims[1].Add(ab[1][3]);
            prims[2].Add(cPrim1);
            prims[2].Add(cPrim2);
            prims[3].Add(dPrim1);
            prims[3].Add(dPrim2);

            data.Add(GetPoints(preG, prims));

            return data;
        }

        private List<Vector3> MultiplyVectorAndMatrix(List<List<Vector3>> matrix, List<float> vector)
        {
            var result = new List<Vector3>();
            for (int i = 0; i < vector.Count; i++)
            {
                result.Add(Vector3.Zero);
                for (int j = 0; j < matrix[i].Count; j++)
                {
                    result[i] += vector[j] * matrix[i][j];
                }
            }
            return result;
        }
        private Vector3 GetPoint(List<List<Vector3>> G, float u, float v, List<List<Vector3>> prims)
        {
            G[1][1] = GetF0(prims[0][0], prims[0][1], u, v);
            G[2][1] = GetF1(prims[1][0], prims[2][0], u, v);
            G[2][2] = GetF2(prims[3][0], prims[3][1], u, v);
            G[1][2] = GetF3(prims[2][1], prims[1][1], u, v);

            var bezierV = MultiplyVectorAndMatrix(G, GetBezierCubic(v).ToList());
            var bezierU = GetBezierCubic(u);

            return bezierV[0] * bezierU.X + bezierV[1] * bezierU.Y + bezierV[2] * bezierU.Z + bezierV[3] * bezierU.W;
        }
        private List<List<Vector3>> GetABArray(List<List<List<Vector3>>> arrays)
        {
            var levels = new List<List<List<List<Vector3>>>>();

            for (var i = 0; i < 2; i++)
            {
                levels.Add(new List<List<List<Vector3>>>());
                for (int j = 0; j < 2; j++)
                {
                    levels[i].Add(new List<List<Vector3>>());
                    for (int k = 0; k < 2; k++)
                    {
                        levels[i][j].Add(new List<Vector3>());
                    }
                }
            }

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 3; k++)
                        levels[i][j][0].Add((arrays[i][j][k] + arrays[i][j][k + 1]) / 2);

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                        levels[i][j][1].Add((levels[i][j][0][k] + levels[i][j][0][k + 1]) / 2);

            var result = new List<List<Vector3>>() { new List<Vector3>(), new List<Vector3>() };

            result[0].Add(levels[0][0][0][2]);
            result[0].Add((result[0][0] - levels[0][1][0][2])/* *0.5f */+ result[0][0]);
            result[0].Add(levels[0][0][1][1]);
            result[0].Add((result[0][2] - levels[0][1][1][1])/* * 0.5f*/ + result[0][2]);
            result[0].Add(levels[0][0][1][0]);

            result[1].Add(levels[1][0][0][0]);
            result[1].Add((result[1][0] - levels[1][1][0][0])/* * 0.5f*/ + result[1][0]);
            result[1].Add(levels[1][0][1][0]);
            result[1].Add((result[1][2] - levels[1][1][1][0])/* * 0.5f*/ + result[1][2]);
            result[1].Add(levels[1][0][1][1]);

            return result;
        }
        private ObjRenderData GetPoints(List<List<Vector3>> preG, List<List<Vector3>> prims)
        {
            int hCount = DrawHeightCount;
            int wCount = DrawWidthCount;
            float change = 1.0f / (DrawPoints - 1);

            var data = new ObjRenderData();
            var ret = new List<Vector3>();

            for (float u = 0; u <= 1.0; u += (1.0f / (hCount - 1)))
            {
                for (float v = 0; v <= 1.0; v += change)
                {
                    ret.Add(GetPoint(preG, u, v, prims));
                }
                data.AddLine(ret);
                ret.Clear();
            }
            for (float v = 0; v <= 1.0; v += (1.0f / (wCount - 1)))
            {
                for (float u = 0; u <= 1.00; u += change)
                {
                    ret.Add(GetPoint(preG, u, v, prims));
                }
                data.AddLine(ret);
                ret.Clear();
            }

            return data;
        }

        private Vector2 GetKH(Vector3 g, Vector3 c, Vector3 b)
        {
            float Wx, Wy;

            var W = GetDet(g.X, g.Y, c.X, c.Y);
            if (W != 0)
            {
                Wx = GetDet(b.X, b.Y, c.X, c.Y);
                Wy = GetDet(g.X, g.Y, b.X, b.Y);
            }
            else
            {
                W = GetDet(g.Y, g.Z, c.Y, c.Z);
                if (W != 0)
                {
                    Wx = GetDet(b.Y, b.Z, c.Y, c.Z);
                    Wy = GetDet(g.Y, g.Z, b.Y, b.Z);
                }
                else
                {
                    W = GetDet(g.X, g.Z, c.X, c.Z);
                    Wx = GetDet(b.X, b.Z, c.X, c.Z);
                    Wy = GetDet(g.X, g.Z, b.X, b.Z);

                }
            }

            return new Vector2() { X = Wx / W, Y = Wy / W };
        }
        private float GetDet(float g1, float g2, float c1, float c2)
        {
            return g1 * c2 - g2 * c1;
        }

        private List<Vector3> GetCs(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3)
        {
            return new List<Vector3>() { P2 - P3, P1 - P2, P0 - P1 };
        }
        private List<Vector3> GetGs(Vector3 a0, Vector3 b0, Vector3 a3, Vector3 b3)
        {
            var ret = new List<Vector3>();
            var g0 = (a0 + b0) / 2;
            var g2 = (a3 + b3) / 2;
            var g1 = (g0 + g2) / 2;
            ret.Add(g0);
            ret.Add(g1);
            ret.Add(g2);
            return ret;
        }
        private Vector3 GetDValue(float v, List<Vector3> gs, List<Vector3> cs, Vector2 kh0, Vector2 kh1)
        {
            //k = x, h = y
            var kv = kh0.X * (1 - v) + kh1.X * v;
            var hv = kh0.Y * (1 - v) + kh1.Y * v;
            var gv = GetValueSquare(v, gs);
            var cv = GetValueSquare(v, cs);
            return gv * kv + cv * hv;
        }

        private Vector4 GetBezierCubic(float t)
        {
            var c = 1 - t;
            return new Vector4(c * c * c, 3 * c * c * t, 3 * c * t * t, t * t * t);
        }
        private Vector3 GetBezierSquare(float t)
        {
            float c = 1 - t;
            return new Vector3(c * c, 2 * c * t, t * t);
        }
        private Vector3 GetCubicValue(float t, List<Vector3> verts)
        {
            var bezierV = GetBezierCubic(t);
            return verts[0] * bezierV.X + verts[1] * bezierV.Y + verts[2] * bezierV.Z + verts[3] * bezierV.W;
        }
        private Vector3 GetValueSquare(float v, List<Vector3> verts)
        {
            var b = GetBezierSquare(v);
            return b.X * verts[0] + b.Y * verts[1] + b.Z * verts[2];
        }

        private Vector3 GetP1(Vector3 q, Vector3 center)
        {
            return (2 * q + center) / 3;
        }
        private Vector3 GetQ(Vector3 p2, Vector3 p3)
        {
            return (3 * p2 - p3) / 2;
        }
        private Vector3 GetP2(List<List<Vector3>> array, Vector3 p3)
        {
            return p3 +/* 0.5f **/ (p3 - GetCubicValue(0.5f, array[1]));
        }
        private Vector3 GetP3(List<List<Vector3>> array)
        {
            return GetCubicValue(0.5f, array[0]);
        }

        private Vector3 GetF0(Vector3 f0, Vector3 f1, float u, float v)
        {
            return (f1 * u + f0 * v) / (u + v != 0 ? u + v : 0.0001f);
        }
        private Vector3 GetF1(Vector3 f0, Vector3 f1, float u, float v)
        {
            return (f1 * (1 - u) + f0 * v) / ((1 - u + v) == 0 ? +0.0001f : (1 - u + v));
        }
        private Vector3 GetF2(Vector3 f0, Vector3 f1, float u, float v)
        {
            return (f1 * (1 - u) + f0 * (1 - v)) / (2 - u - v == 0 ? 0.0001f : 2 - u - v);
        }
        private Vector3 GetF3(Vector3 f0, Vector3 f1, float u, float v)
        {
            return (f1 * u + f0 * (1 - v)) / (1 + u - v == 0 ? 0.0001f : 1 + u - v);
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