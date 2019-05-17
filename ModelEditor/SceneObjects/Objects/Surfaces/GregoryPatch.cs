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

    public struct Pstruct
    {
        public Vector3 P3 { get; set; }
        public Vector3 P2 { get; set; }
    }

    public struct ABstruct
    {
        public List<Vector3> a { get; set; }
        public List<Vector3> b { get; set; }
        public List<Vector3> aPrim { get; set; }
        public List<Vector3> bPrim { get; set; }
        public List<Vector3> helps { get; set; }
    }

    public struct KHstruct
    {
        public float k { get; set; }
        public float h { get; set; }
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
        public ObjRenderData GetRenderData()
        {
            var data = new ObjRenderData();
            var greg = GetGregoryData();


            data.Vertices = RebuildGregory(greg.Arrays, greg.Points, 4, 4);

            return data;
        }

        public Vector3 FindP1(Vector3 q, Vector3 center)
        {
            return (2 * q + center) / 3;
        }
        public Vector3 FindQ(Vector3 p2, Vector3 p3)
        {
            return (3 * p2 - p3) / 2;
        }
        public Pstruct FindP3(List<List<Vector3>> array)
        {
            var firstCut = new List<List<Vector3>>();
            var secondCut = new List<List<Vector3>>();
            for (var i = 0; i < 2; i++)
            {
                firstCut.Add(new List<Vector3>());
                secondCut.Add(new List<Vector3>());
            }

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 2; j++)
                    firstCut[j].Add((array[j][i] + array[j][i + 1]) / 2);

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    secondCut[j].Add((firstCut[j][i] + firstCut[j][i + 1]) / 2);

            var P3 = (secondCut[0][0] + secondCut[0][1]) / 2;
            return new Pstruct()
            {
                P3 = P3,
                P2 = P3 + (P3 - (secondCut[1][0] + secondCut[1][1]) / 2) * 0.5f,
            };
        }

        public Vector3 GetF0(Vector3 f0, Vector3 f1, float u, float v)
        {
            return (f1 * u + f0 * v) / (u + v != 0 ? u + v : 0.0001f);
        }
        public Vector3 GetF1(Vector3 f0, Vector3 f1, float u, float v)
        {
            return (f1 * (1 - u) + f0 * v) / ((1 - u + v) == 0 ? +0.0001f : (1 - u + v));
        }
        public Vector3 GetF2(Vector3 f0, Vector3 f1, float u, float v)
        {
            return (f1 * (1 - u) + f0 * (1 - v)) / (2 - u - v == 0 ? 0.0001f : 2 - u - v);
        }
        public Vector3 GetF3(Vector3 f0, Vector3 f1, float u, float v)
        {
            return (f1 * u + f0 * (1 - v)) / (1 + u - v == 0 ? 0.0001f : 1 + u - v);
        }

        public Vector4 GetBezierCubic(float t)
        {
            var c = 1 - t;
            return new Vector4(c * c * c, 3 * c * c * t, 3 * c * t * t, t * t * t);
        }
        public Vector3 GetBezierSquare(float t)
        {
            float c = 1 - t;
            return new Vector3(c * c, 2 * c * t, t * t);
        }

        public List<Vector3> GetGs(Vector3 a0, Vector3 b0, Vector3 a3, Vector3 b3)
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
        public Vector3 GetDValue(float v, List<Vector3> gs, List<Vector3> cs, KHstruct kh0, KHstruct kh1)
        {
            var kv = kh0.k * (1 - v) + kh1.k * v;
            var hv = kh0.h * (1 - v) + kh1.h * v;
            var gv = GetValueSquare(v, gs);
            var cv = GetValueSquare(v, cs);
            return gv * kv + cv * hv;
        }
        public Vector3 GetValueSquare(float v, List<Vector3> values)
        {
            var b = GetBezierSquare(v);
            return b.X * values[0] + b.Y * values[1] + b.Z * values[2];
        }
       
        public List<Vector3> GetCis(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3)
        {
            return new List<Vector3>() { P2 - P3, P1 - P2, P0 - P1 };
        }

        public List<float> MultiplyVectorAndMatrix(List<List<float>> matrix, List<float> vector)
        {
            var result = new List<float>();
            for (var i = 0; i < vector.Count; i++)
            {
                result.Add(0);
                for (var j = 0; j < matrix[i].Count; j++)
                {
                    result[i] += vector[j] * matrix[i][j];
                }
            }
            return result;
        }
        public float GetQuv(List<List<float>> G, float u, float v, List<Vector3> aPrim, List<Vector3> bPrim, List<Vector3> cPrim, List<Vector3> dPrim, string axis)
        {
            if (axis == "X")
            {
                G[1][1] = GetF0(aPrim[1], aPrim[0], u, v).X;
                G[2][1] = GetF1(bPrim[0], cPrim[0], u, v).X;
                G[2][2] = GetF2(dPrim[0], dPrim[1], u, v).X;
                G[1][2] = GetF3(cPrim[1], bPrim[1], u, v).X;
            }
            else if (axis == "Y")
            {
                G[1][1] = GetF0(aPrim[1], aPrim[0], u, v).Y;
                G[2][1] = GetF1(bPrim[0], cPrim[0], u, v).Y;
                G[2][2] = GetF2(dPrim[0], dPrim[1], u, v).Y;
                G[1][2] = GetF3(cPrim[1], bPrim[1], u, v).Y;
            }
            else if (axis == "Z")
            {
                G[1][1] = GetF0(aPrim[1], aPrim[0], u, v).Z;
                G[2][1] = GetF1(bPrim[0], cPrim[0], u, v).Z;
                G[2][2] = GetF2(dPrim[0], dPrim[1], u, v).Z;
                G[1][2] = GetF3(cPrim[1], bPrim[1], u, v).Z;
            }

            var m = new Matrix4x4(
                G[0][0], G[0][1], G[0][2], G[0][3],
                G[1][0], G[1][1], G[1][2], G[1][3],
                G[2][0], G[2][1], G[2][2], G[2][3],
                G[3][0], G[3][1], G[3][2], G[3][3]
                );

            var vector = GetBezierCubic(v);
            var vec = m.Multiply(vector);
            var bezierV = GetBezierCubic(u);
            var result = Vector4.Dot(vec, bezierV);

            return result;
        }
        public ABstruct FindAB(List<List<Vector3>> importantArray1, List<List<Vector3>> importantArray2)
        {
            var firstCut1 = new List<List<Vector3>>();
            var secondCut1 = new List<List<Vector3>>();
            var firstCut2 = new List<List<Vector3>>();
            var secondCut2 = new List<List<Vector3>>();
            for (var i = 0; i < 2; i++)
            {
                firstCut1.Add(new List<Vector3>());
                secondCut1.Add(new List<Vector3>());
                firstCut2.Add(new List<Vector3>());
                secondCut2.Add(new List<Vector3>());
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    firstCut1[j].Add((importantArray1[j][i] + importantArray1[j][i + 1]) / 2);
                    firstCut2[j].Add((importantArray2[j][i] + importantArray2[j][i + 1]) / 2);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    secondCut1[j].Add((firstCut1[j][i] + firstCut1[j][i + 1]) / 2);
                    secondCut2[j].Add((firstCut2[j][i] + firstCut2[j][i + 1]) / 2);
                }
            }
            var a = new List<Vector3>();
            var b = new List<Vector3>();
            var aPrim = new List<Vector3>();
            var bPrim = new List<Vector3>();
            var helps = new List<Vector3>();
            helps.Add(secondCut1[0][0]);
            helps.Add(secondCut2[0][1]);
            a.Add(firstCut1[0][2]);
            a.Add(firstCut2[0][0]);
            b.Add(secondCut1[0][1]);
            b.Add(secondCut2[0][0]);
            aPrim.Add(((a[0] - firstCut1[1][2]) * 0.5f) + a[0]);
            aPrim.Add(((a[1] - firstCut2[1][0]) * 0.5f) + a[1]);
            bPrim.Add(((b[0] - secondCut1[1][1]) * 0.5f) + b[0]);
            bPrim.Add(((b[1] - secondCut2[1][0]) * 0.5f) + b[1]);
            return new ABstruct() { a = a, b = b, aPrim = aPrim, bPrim = bPrim, helps = helps };
        }
        public List<Vector3> GetPartialGregoryPoints(List<List<Vector3>> preG, List<Vector3> aPrim, List<Vector3> bPrim, List<Vector3> cPrim, List<Vector3> dPrim, float _u, float _v)
        {
            var ret = new List<Vector3>();
            var Gx = new List<List<float>>();
            var Gy = new List<List<float>>();
            var Gz = new List<List<float>>();

            for (int i = 0; i < 4; i++)
            {
                Gx.Add(new List<float>());
                Gy.Add(new List<float>());
                Gz.Add(new List<float>());
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Gx[i].Add(preG[i][j].X);
                    Gy[i].Add(preG[i][j].Y);
                    Gz[i].Add(preG[i][j].Z);
                }
            }
            for (float u = 0; u <= 1.0; u += (1.0f / (_u - 1)))
            {
                for (float v = 0; v <= 1.0; v += 0.02f)
                {
                    var p = Vector3.Zero;
                    p.X = GetQuv(Gx, u, v, aPrim, bPrim, cPrim, dPrim, "X");
                    p.Y = GetQuv(Gy, u, v, aPrim, bPrim, cPrim, dPrim, "Y");
                    p.Z = GetQuv(Gz, u, v, aPrim, bPrim, cPrim, dPrim, "Z");
                    ret.Add(p);
                }
            }
            for (float v = 0; v <= 1.00; v += (1.0f / (_v - 1)))
            {
                for (float u = 0; u <= 1.00; u += 0.02f)
                {
                    var p = Vector3.Zero;
                    p.X = GetQuv(Gx, u, v, aPrim, bPrim, cPrim, dPrim, "X");
                    p.Y = GetQuv(Gy, u, v, aPrim, bPrim, cPrim, dPrim, "Y");
                    p.Z = GetQuv(Gz, u, v, aPrim, bPrim, cPrim, dPrim, "Z");
                    ret.Add(p);
                }
            }
            return ret;
        }

        public KHstruct CountK0AndH0(Vector3 g, Vector3 c, Vector3 b)
        {
            var number = 0;
            var W = CountW(g.X, g.Y, c.X, c.Y);
            float Wx, Wy;
            if (W == 0)
            {
                number = 1;
                W = CountW(g.Y, g.Z, c.Y, c.Z);
            }
            if (W == 0)
            {
                number = 2;
                W = CountW(g.X, g.Z, c.X, c.Z);
            }
            if (number == 0)
            {
                Wx = CountW(b.X, b.Y, c.X, c.Y);
                Wy = CountW(g.X, g.Y, b.X, b.Y);
            }
            else if (number == 1)
            {
                Wx = CountW(b.Y, b.Z, c.Y, c.Z);
                Wy = CountW(g.Y, g.Z, b.Y, b.Z);
            }
            else
            {
                Wx = CountW(b.X, b.Z, c.X, c.Z);
                Wy = CountW(g.X, g.Z, b.X, b.Z);
            }
            return new KHstruct() { k = Wx / W, h = Wy / W };
        }
        public float CountW(float g1, float g2, float c1, float c2)
        {
            return g1 * c2 - g2 * c1;
        }

        public List<Vector3> RebuildGregory(List<List<List<Vector3>>> importantArrays, List<Vector3> points, int u, int v)
        {
            var GregoryPoints = new List<Vector3>();
            var GregoryVectors = new List<List<Vector3>>();
            var p3 = new List<Vector3>();
            var p2 = new List<Vector3>();
            for (var i = 0; i < 3; i++)
            {
                var help = FindP3(importantArrays[i]);
                p3.Add(help.P3);
                p2.Add(help.P2);
            }
            List<Vector3> q = new List<Vector3>();
            for (var i = 0; i < 3; i++)
            {
                q.Add(FindQ(p2[i], p3[i]));
            }
            var p = (q[0] + q[1] + q[2]) / 3;
            List<Vector3> p1 = new List<Vector3>();
            for (var i = 0; i < 3; i++)
            {
                p1.Add(FindP1(q[i], p));
            }

            var PArrs = new List<List<Vector3>>();
            PArrs.Add(new List<Vector3>() { points[1], p3[0], p, p3[1] });
            PArrs.Add(new List<Vector3>() { points[2], p3[1], p, p3[2] });
            PArrs.Add(new List<Vector3>() { points[0], p3[2], p, p3[0] });

            var vvArrs = new List<List<Vector3>>();
            vvArrs.Add(new List<Vector3>() { p2[0], p1[0], p1[1], p2[1], p1[2] });
            vvArrs.Add(new List<Vector3>() { p2[1], p1[1], p1[2], p2[2], p1[0] });
            vvArrs.Add(new List<Vector3>() { p2[2], p1[2], p1[0], p2[0], p1[1] });

            var importantArrays2 = new List<List<List<Vector3>>>();
            importantArrays2.Add(importantArrays[1]);
            importantArrays2.Add(importantArrays[2]);
            importantArrays2.Add(importantArrays[0]);

            CreateSmallPatch(PArrs[0], vvArrs[0], importantArrays[0], importantArrays[1], u, v, GregoryPoints, GregoryVectors);
            CreateSmallPatch(PArrs[1], vvArrs[1], importantArrays[1], importantArrays[2], u, v, GregoryPoints, GregoryVectors);
            CreateSmallPatch(PArrs[2], vvArrs[2], importantArrays[2], importantArrays[0], u, v, GregoryPoints, GregoryVectors);

            return GregoryPoints;
        }
        public void CreateSmallPatch(List<Vector3> P, List<Vector3> vv, List<List<Vector3>> ia1, List<List<Vector3>> ia2, int u, int v, List<Vector3> GregoryPoints, List<List<Vector3>> GregoryVectors)
        {
            var cPrim = new List<Vector3>();
            var dPrim = new List<Vector3>();
            var abstruct = FindAB(ia1, ia2);
            var a = abstruct.a;
            var b = abstruct.b;
            var aPrim = abstruct.aPrim;
            var bPrim = abstruct.bPrim;
            var helps = abstruct.helps;

            var a0 = P[1] - helps[0];
            var b0 = b[0] - P[1];
            var a3 = P[2] - vv[4];
            var b3 = vv[2] - P[2];
            var gs = GetGs(a0, b0, a3, b3);
            var cs = GetCis(P[2], vv[1], vv[0], P[1]);
            var kh0 = CountK0AndH0(gs[0], cs[0], b0);
            var kh1 = CountK0AndH0(gs[2], cs[2], b3);
            var dv0 = GetDValue(1 / 3, gs, cs, kh0, kh1);
            var dv1 = GetDValue(2 / 3, gs, cs, kh0, kh1);
            cPrim.Add(vv[0] + dv0);
            dPrim.Add(vv[1] + dv1);

            a0 = P[3] - helps[1];
            b0 = b[1] - P[3];
            b3 = vv[1] - P[2];
            a3 = P[2] - vv[4];
            gs = GetGs(a0, b0, a3, b3);
            cs = GetCis(P[2], vv[2], vv[3], P[3]);
            kh0 = CountK0AndH0(gs[0], cs[0], b0);
            kh1 = CountK0AndH0(gs[2], cs[2], b3);
            dv0 = GetDValue(1 / 3, gs, cs, kh0, kh1);
            dv1 = GetDValue(2 / 3, gs, cs, kh0, kh1);

            cPrim.Add(vv[3] + dv0);
            dPrim.Add(vv[2] + dv1);
            GregoryVectors.Add(new List<Vector3>() { b[0], bPrim[0] });
            GregoryVectors.Add(new List<Vector3>() { b[1], bPrim[1] });
            GregoryVectors.Add(new List<Vector3>() { a[0], aPrim[0] });
            GregoryVectors.Add(new List<Vector3>() { a[1], aPrim[1] });

            var preG = new List<List<Vector3>>();
            for (int i = 0; i < 4; i++)
            {
                preG.Add(new List<Vector3>());
            }

            preG[0].Add(P[0]);
            preG[0].Add(a[1]);
            preG[0].Add(b[1]);
            preG[0].Add(P[3]);
            preG[1].Add(a[0]);
            preG[1].Add(Vector3.Zero);
            preG[1].Add(Vector3.Zero);
            preG[1].Add(vv[3]);
            preG[2].Add(b[0]);
            preG[2].Add(Vector3.Zero);
            preG[2].Add(Vector3.Zero);
            preG[2].Add(vv[2]);
            preG[3].Add(P[1]);
            preG[3].Add(vv[0]);
            preG[3].Add(vv[1]);
            preG[3].Add(P[2]);
            GregoryPoints.AddRange(GetPartialGregoryPoints(preG, aPrim, bPrim, cPrim, dPrim, u, v));
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