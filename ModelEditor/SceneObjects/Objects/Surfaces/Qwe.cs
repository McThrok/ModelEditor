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
    public class Qwe
    {
        public Vector3 findP1(Vector3 q, Vector3 center)
        {
            return DividePoint(SumPoints(MultiplyPoint(q, 2), center), 3);
        }
        public Vector3 findCenter(Vector3 q1, Vector3 q2, Vector3 q3)
        {
            return DividePoint(SumPoints(SumPoints(q1, q2), q3), 3);
        }
        public Vector3 findQ(Vector3 p2, Vector3 p3)
        {
            return DividePoint(DiffPoints(MultiplyPoint(p2, 3), p3), 2);
        }
        public Vector3 DividePoint(Vector3 a, Vector3 b)
        {
            return a / b;
        }
        public Vector3 DividePoint(Vector3 a, float b)
        {
            return a / b;
        }
        public Vector3 DividePoint(Vector3 a, int b)
        {
            return a / b;
        }
        public Vector3 MultiplyPoint(Vector3 a, Vector3 b)
        {
            return a * b;
        }
        public Vector3 MultiplyPoint(Vector3 a, float b)
        {
            return a * b;
        }
        public Vector3 SumPoints(Vector3 a, Vector3 b)
        {
            return a + b;
        }
        public Vector3 SumPoints(Vector3 a, float b)
        {
            return a + Vector3.One * b;
        }
        public Vector3 DiffPoints(Vector3 a, Vector3 b)
        {
            return a - b;
        }
        public Vector3 DiffPoints(Vector3 a, float b)
        {
            return a - Vector3.One * b;
        }

        public Vector3 getF0(Vector3 f0, Vector3 f1, float u, float v)
        {
            return DividePoint(SumPoints(MultiplyPoint(f1, u), MultiplyPoint(f0, v)), u + v != 0 ? u + v : 0.0001f);
        }
        public Vector3 getF1(Vector3 f0, Vector3 f1, float u, float v)
        {
            return DividePoint(SumPoints(MultiplyPoint(f0, 1 - u), MultiplyPoint(f1, v)), (1 - u + v) == 0 ? +0.0001f : (1 - u + v));
        }
        public Vector3 getF2(Vector3 f0, Vector3 f1, float u, float v)
        {
            return DividePoint(SumPoints(MultiplyPoint(f1, 1 - u), MultiplyPoint(f0, 1 - v)), 2 - u - v == 0 ? 0.0001f : 2 - u - v);
        }
        public Vector3 getF3(Vector3 f0, Vector3 f1, float u, float v)
        {
            return DividePoint(SumPoints(MultiplyPoint(f0, u), MultiplyPoint(f1, 1 - v)), 1 + u - v == 0 ? 0.0001f : 1 + u - v);
        }

        public List<float> getBezierVector(float number)
        {
            var ret = new List<float>();
            ret.Add((float)(Math.Pow((1 - number), 3)));
            ret.Add((float)(3 * Math.Pow((1 - number), 2) * number));
            ret.Add((float)(3 * (1 - number) * Math.Pow(number, 2)));
            ret.Add((float)(Math.Pow(number, 3)));
            return ret;
        }

        public Pstruct findP3(List<List<Vector3>> importantArray)
        {
            var firstCut = new List<List<Vector3>>();
            var secondCut = new List<List<Vector3>>();
            for (var i = 0; i < 2; i++)
            {
                firstCut.Add(new List<Vector3>());
                secondCut.Add(new List<Vector3>());
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    firstCut[j].Add(DividePoint(SumPoints(importantArray[j][i], importantArray[j][i + 1]), 2));
                }
            }

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    secondCut[j].Add(DividePoint(SumPoints(firstCut[j][i], firstCut[j][i + 1]), 2));
                }
            }

            var P3 = SumPoints(secondCut[0][0], secondCut[0][1]) / 2;
            return new Pstruct()
            {
                P3 = P3,
                P2 = SumPoints(P3, MultiplyPoint(DiffPoints(P3, DividePoint(SumPoints(secondCut[1][0], secondCut[1][1]), 2)), 0.5f)),
            };
        }


        public struct Pstruct
        {
            public Vector3 P3 { get; set; }
            public Vector3 P2 { get; set; }
        }

        public List<float> multiplyVectorAndMatrix(List<List<float>> matrix, List<float> vector)
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

        public float getQuv(List<List<float>> G, float u, float v, List<Vector3> aPrim, List<Vector3> bPrim, List<Vector3> cPrim, List<Vector3> dPrim, string axis)
        {
            // GregoryPoints.Add(bPrim[0]);
            // GregoryPoints.Add(cPrim[0]);
            if (axis == "X")
            {
                G[1][1] = getF0(aPrim[1], aPrim[0], u, v).X;
                G[2][1] = getF1(bPrim[0], cPrim[0], u, v).X;
                G[2][2] = getF2(dPrim[0], dPrim[1], u, v).X;
                G[1][2] = getF3(cPrim[1], bPrim[1], u, v).X;
            }
            else if (axis == "Y")
            {
                G[1][1] = getF0(aPrim[1], aPrim[0], u, v).Y;
                G[2][1] = getF1(bPrim[0], cPrim[0], u, v).Y;
                G[2][2] = getF2(dPrim[0], dPrim[1], u, v).Y;
                G[1][2] = getF3(cPrim[1], bPrim[1], u, v).Y;
            }
            else if (axis == "Z")
            {
                G[1][1] = getF0(aPrim[1], aPrim[0], u, v).Z;
                G[2][1] = getF1(bPrim[0], cPrim[0], u, v).Z;
                G[2][2] = getF2(dPrim[0], dPrim[1], u, v).Z;
                G[1][2] = getF3(cPrim[1], bPrim[1], u, v).Z;
            }
            var vec = multiplyVectorAndMatrix(G, getBezierVector(v));
            var bezierV = getBezierVector(u);
            return vec[0] * bezierV[0] + vec[1] * bezierV[1] + vec[2] * bezierV[2] + vec[3] * bezierV[3];
        }

        public ABstruct findAB(List<List<Vector3>> importantArray1, List<List<Vector3>> importantArray2)
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
                    firstCut1[j].Add(DividePoint(SumPoints(importantArray1[j][i], importantArray1[j][i + 1]), 2));
                    firstCut2[j].Add(DividePoint(SumPoints(importantArray2[j][i], importantArray2[j][i + 1]), 2));
                }
            }

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    secondCut1[j].Add(DividePoint(SumPoints(firstCut1[j][i], firstCut1[j][i + 1]), 2));
                    secondCut2[j].Add(DividePoint(SumPoints(firstCut2[j][i], firstCut2[j][i + 1]), 2));
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
            aPrim.Add(SumPoints(MultiplyPoint(DiffPoints(a[0], firstCut1[1][2]), 0.5f), a[0]));
            aPrim.Add(SumPoints(MultiplyPoint(DiffPoints(a[1], firstCut2[1][0]), 0.5f), a[1]));
            bPrim.Add(SumPoints(MultiplyPoint(DiffPoints(b[0], secondCut1[1][1]), 0.5f), b[0]));
            bPrim.Add(SumPoints(MultiplyPoint(DiffPoints(b[1], secondCut2[1][0]), 0.5f), b[1]));
            return new ABstruct() { a = a, b = b, aPrim = aPrim, bPrim = bPrim, helps = helps };
        }

        public struct ABstruct
        {
            public List<Vector3> a { get; set; }
            public List<Vector3> b { get; set; }
            public List<Vector3> aPrim { get; set; }
            public List<Vector3> bPrim { get; set; }
            public List<Vector3> helps { get; set; }
        }


        public List<Vector3> getPartialGregoryPoints(List<List<Vector3>> preG, List<Vector3> aPrim, List<Vector3> bPrim, List<Vector3> cPrim, List<Vector3> dPrim, float _u, float _v)
        {
            var ret = new List<Vector3>();
            var Gx = new List<List<float>>();
            var Gy = new List<List<float>>();
            var Gz = new List<List<float>>();

            int i;
            for (i = 0; i < 4; i++)
            {
                Gx.Add(new List<float>());
                Gy.Add(new List<float>());
                Gz.Add(new List<float>());
            }
            for (i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    //if (preG[i][j] == undefined)
                    //{
                    //    Gx[i].Add(undefined);
                    //    Gy[i].Add(undefined);
                    //    Gz[i].Add(undefined);
                    //    continue;
                    //}
                    Gx[i].Add(preG[i][j].X);
                    Gy[i].Add(preG[i][j].Y);
                    Gz[i].Add(preG[i][j].Z);
                }
            }
            //var { ctx } = getContexts();
            i = 0;
            for (float u = 0; u <= 1.0; u += (1.0f / (_u - 1)))
            {
                //ctx.beginPath()
                for (float v = 0; v <= 1.0; v += 0.02f)
                {
                    var p = Vector3.Zero;
                    p.X = getQuv(Gx, u, v, aPrim, bPrim, cPrim, dPrim, "X");
                    p.Y = getQuv(Gy, u, v, aPrim, bPrim, cPrim, dPrim, "Y");
                    p.Z = getQuv(Gz, u, v, aPrim, bPrim, cPrim, dPrim, "Z");
                    //setTranslationPoints([p]);
                    //ret.Add(UpdatePointsForCanvas(Translate({}))[0]);
                    //if(v != 0) {
                    //    drawLine(ret[i].X, ret[i].Y, ret[i - 1].X, ret[i - 1].Y, ctx);
                    //}
                    if (v + 0.02 > 1.0 && v + 0.02 != 1.0 && v != 1)
                    {
                        v = 1.0f;
                        i++;
                        var q = Vector3.Zero;
                        q.X = getQuv(Gx, u, v, aPrim, bPrim, cPrim, dPrim, "X");
                        q.Y = getQuv(Gy, u, v, aPrim, bPrim, cPrim, dPrim, "Y");
                        q.Z = getQuv(Gz, u, v, aPrim, bPrim, cPrim, dPrim, "Z");
                        //setTranslationPoints([p]);
                        //ret.Add(UpdatePointsForCanvas(Translate({}))[0]);
                        //if(u != 0) {
                        //    drawLine(ret[i].X, ret[i].Y, ret[i - 1].X, ret[i - 1].Y, ctx);
                        //}
                    }
                    i++;
                }
                //ctx.stroke();
            }
            i = 0;
            //ret = [];
            for (float v = 0; v <= 1.00; v += (1.0f / (_v - 1)))
            {
                //ctx.beginPath()
                for (float u = 0; u <= 1.00; u += 0.02f)
                {
                    var p = Vector3.Zero;
                    p.X = getQuv(Gx, u, v, aPrim, bPrim, cPrim, dPrim, "X");
                    p.Y = getQuv(Gy, u, v, aPrim, bPrim, cPrim, dPrim, "Y");
                    p.Z = getQuv(Gz, u, v, aPrim, bPrim, cPrim, dPrim, "Z");
                    //setTranslationPoints([p]);
                    //ret.Add(UpdatePointsForCanvas(Translate({}))[0]);
                    //            if(u != 0) {
                    //                drawLine(ret[i].X, ret[i].Y, ret[i - 1].X, ret[i - 1].Y, ctx);
                    //            }
                    if (u + 0.02 > 1.0 && u + 0.02 != 1.0 && u != 1.0)
                    {
                        u = 1.0f;
                        i++;
                        var q = Vector3.Zero;
                        q.X = getQuv(Gx, u, v, aPrim, bPrim, cPrim, dPrim, "X");
                        q.Y = getQuv(Gy, u, v, aPrim, bPrim, cPrim, dPrim, "Y");
                        q.Z = getQuv(Gz, u, v, aPrim, bPrim, cPrim, dPrim, "Z");
                        //setTranslationPoints([p]);
                        //ret.Add(UpdatePointsForCanvas(Translate({}))[0]);
                        //                if(u != 0) {
                        //                    drawLine(ret[i].X, ret[i].Y, ret[i - 1].X, ret[i - 1].Y, ctx);
                        //                }
                    }
                    i++;
                }
                //ctx.stroke();
            }
            return ret;
        }


        public List<float> getBezier2(float v)
        {
            var ret = new List<float>();
            ret.Add((float)(Math.Pow(1 - v, 2)));
            ret.Add((float)(2 * (1 - v) * v));
            ret.Add((float)(Math.Pow(v, 2)));
            return ret;
        }

        public List<Vector3> getGs(Vector3 a0, Vector3 b0, Vector3 a3, Vector3 b3)
        {
            var ret = new List<Vector3>();
            var g0 = DividePoint(SumPoints(a0, b0), 2);
            var g2 = DividePoint(SumPoints(a3, b3), 2);
            var g1 = DividePoint(SumPoints(g0, g2), 2);
            ret.Add(g0);
            ret.Add(g1);
            ret.Add(g2);
            return ret;
        }
        public Vector3 getDValue(float v, List<Vector3> gs, List<Vector3> cs, KHstruct kh0, KHstruct kh1)
        {
            var kv = kh0.k * (1 - v) + kh1.k * v;
            var hv = kh0.h * (1 - v) + kh1.h * v;
            var gv = getGValue(v, gs);
            var cv = getCValue(v, cs);
            return SumPoints(MultiplyPoint(gv, kv), MultiplyPoint(cv, hv));
        }
        public Vector3 getGValue(float v, List<Vector3> gs)
        {
            var beziers2 = getBezier2(v);
            var sums = new List<Vector3>();
            for (int i = 0; i < 3; i++)
            {
                sums.Add(MultiplyPoint(gs[i], beziers2[i]));
            }
            return SumPoints(SumPoints(sums[0], sums[2]), sums[1]);
        }
        public Vector3 getCValue(float v, List<Vector3> cs)
        {
            var beziers2 = getBezier2(v);
            var sums = new List<Vector3>();
            for (int i = 0; i < 3; i++)
            {
                sums.Add(MultiplyPoint(cs[i], beziers2[i]));
            }
            return SumPoints(SumPoints(sums[0], sums[2]), sums[1]);
        }
        public List<Vector3> getCis(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3)
        {
            return new List<Vector3>() { DiffPoints(P2, P3), DiffPoints(P1, P2), DiffPoints(P0, P1) };
        }

        public struct KHstruct
        {
            public float k { get; set; }
            public float h { get; set; }
        }

        public KHstruct countK0AndH0(Vector3 g, Vector3 c, Vector3 b)
        {
            var number = 0;
            var W = countW(g.X, g.Y, c.X, c.Y);
            float Wx, Wy;
            if (W == 0)
            {
                number = 1;
                W = countW(g.Y, g.Z, c.Y, c.Z);
            }
            if (W == 0)
            {
                number = 2;
                W = countW(g.X, g.Z, c.X, c.Z);
            }
            if (number == 0)
            {
                Wx = countW(b.X, b.Y, c.X, c.Y);
                Wy = countW(g.X, g.Y, b.X, b.Y);
            }
            else if (number == 1)
            {
                Wx = countW(b.Y, b.Z, c.Y, c.Z);
                Wy = countW(g.Y, g.Z, b.Y, b.Z);
            }
            else
            {
                Wx = countW(b.X, b.Z, c.X, c.Z);
                Wy = countW(g.X, g.Z, b.X, b.Z);
            }
            return new KHstruct() { k = Wx / W, h = Wy / W };
        }
        public float countW(float g1, float g2, float c1, float c2)
        {
            return g1 * c2 - g2 * c1;
        }

        public void RebuildGregory(List<List<List<Vector3>>> importantArrays, List<Vector3> points, int u, int v)
        {
            var GregoryPoints = new List<Vector3>();
            var GregoryVectors = new List<List<Vector3>>();
            var p3 = new List<Vector3>();
            var p2 = new List<Vector3>();
            for (var i = 0; i < 3; i++)
            {
                var help = findP3(importantArrays[i]);
                p3.Add(help.P3);
                p2.Add(help.P2);
                //GregoryVectors.Add([p2[i], p3[i]]);
            }
            List<Vector3> q = new List<Vector3>();
            for (var i = 0; i < 3; i++)
            {
                q.Add(findQ(p2[i], p3[i]));
            }
            var p = findCenter(q[0], q[1], q[2]);
            List<Vector3> p1 = new List<Vector3>();
            for (var i = 0; i < 3; i++)
            {
                p1.Add(findP1(q[i], p));
                // GregoryVectors.Add([p2[i], p1[i]]);
                // GregoryVectors.Add([p1[i], p]);
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

            createSmallPatch(PArrs[0], vvArrs[0], importantArrays[0], importantArrays[1], u, v, GregoryPoints, GregoryVectors);
            createSmallPatch(PArrs[1], vvArrs[1], importantArrays[1], importantArrays[2], u, v, GregoryPoints, GregoryVectors);
            createSmallPatch(PArrs[2], vvArrs[2], importantArrays[2], importantArrays[0], u, v, GregoryPoints, GregoryVectors);
            // addPoint(vvArrs[2][0].x, vvArrs[2][0].y, vvArrs[2][0].z, 'dfsds');
            // addPoint(vvArrs[2][1].x, vvArrs[2][1].y, vvArrs[2][1].z, 'dfsds');
            // addPoint(vvArrs[2][2].x, vvArrs[2][2].y, vvArrs[2][2].z, 'dfsds');
            // addPoint(vvArrs[2][3].x, vvArrs[2][3].y, vvArrs[2][3].z, 'dfsds');
        }
        public void createSmallPatch(List<Vector3> P, List<Vector3> vv, List<List<Vector3>> ia1, List<List<Vector3>> ia2, int u, int v, List<Vector3> GregoryPoints, List<List<Vector3>> GregoryVectors)
        {
            var cPrim = new List<Vector3>();
            var dPrim = new List<Vector3>();
            var abstruct = findAB(ia1, ia2);
            var a = abstruct.a;
            var b = abstruct.b;
            var aPrim = abstruct.aPrim;
            var bPrim = abstruct.bPrim;
            var helps = abstruct.helps;

            var a0 = DiffPoints(P[1], helps[0]);
            var b0 = DiffPoints(b[0], P[1]);
            var a3 = DiffPoints(P[2], vv[4]);
            var b3 = DiffPoints(vv[2], P[2]);
            var gs = getGs(a0, b0, a3, b3);
            var cs = getCis(P[2], vv[1], vv[0], P[1]);
            var kh0 = countK0AndH0(gs[0], cs[0], b0);
            var kh1 = countK0AndH0(gs[2], cs[2], b3);
            var dv0 = getDValue(1 / 3, gs, cs, kh0, kh1);
            var dv1 = getDValue(2 / 3, gs, cs, kh0, kh1);
            // GregoryVectors.Add([vv[0], SumPoints(vv[0], dv0)]);
            cPrim.Add(SumPoints(vv[0], dv0));
            dPrim.Add(SumPoints(vv[1], dv1));
            // GregoryVectors.Add([vv[0], SumPoints(vv[0], dv0)]);
            // GregoryVectors.Add([vv[1], SumPoints(vv[1], dv1)]);

            a0 = DiffPoints(P[3], helps[1]);
            b0 = DiffPoints(b[1], P[3]);
            b3 = DiffPoints(vv[1], P[2]);
            a3 = DiffPoints(P[2], vv[4]);
            gs = getGs(a0, b0, a3, b3);
            cs = getCis(P[2], vv[2], vv[3], P[3]);
            kh0 = countK0AndH0(gs[0], cs[0], b0);
            kh1 = countK0AndH0(gs[2], cs[2], b3);
            dv0 = getDValue(1 / 3, gs, cs, kh0, kh1);
            dv1 = getDValue(2 / 3, gs, cs, kh0, kh1);

            cPrim.Add(SumPoints(vv[3], dv0));
            dPrim.Add(SumPoints(vv[2], dv1));
            //  GregoryVectors.Add([vv[3], SumPoints(vv[3], dv0)]);
            GregoryVectors.Add(new List<Vector3>() { b[0], bPrim[0] });
            GregoryVectors.Add(new List<Vector3>() { b[1], bPrim[1] });
            GregoryVectors.Add(new List<Vector3>() { a[0], aPrim[0] });
            GregoryVectors.Add(new List<Vector3>() { a[1], aPrim[1] });
            // GregoryVectors.Add([vv[2], SumPoints(vv[2], dv1)]);

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
            preG[1].Add(Vector3.Zero);
            preG[1].Add(Vector3.Zero);
            preG[2].Add(vv[2]);
            preG[3].Add(P[1]);
            preG[3].Add(vv[0]);
            preG[3].Add(vv[1]);
            preG[3].Add(P[2]);
            GregoryPoints.AddRange(getPartialGregoryPoints(preG, aPrim, bPrim, cPrim, dPrim, u, v));
        }
    }
}