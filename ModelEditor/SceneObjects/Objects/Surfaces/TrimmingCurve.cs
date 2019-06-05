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
    public interface TrimmingSurface
    {
        Guid Id { get; }
        bool WrappedU { get; }
        bool WrappedV { get; }

        Vector3 Evaluate(float h, float w);
        Vector3 EvaluateDU(float h, float w);
        Vector3 EvaluateDV(float h, float w);
    }

    public class CurveData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Vector3> Points { get; set; }
        public object InterpolationCurve { get; set; }
        public List<Vector2> intersectionVisualization1 { get; set; }
        public List<Vector2> intersectionVisualization2 { get; set; }
    }

    public struct UpdStruct
    {
        public TrimmingSurface Obj { get; set; }
        public float U { get; set; }
        public float V { get; set; }
        public float UNew { get; set; }
        public float VNew { get; set; }
        public bool Backed { get; set; }
    }

    public struct UpdUvStruct
    {
        public float u;
        public float v;
        public bool end;
        public bool backThisTime;
        public int crossed;
        public float uLast;
        public float vLast;
    }

    public class TrimmingCurve : SceneObject, IRenderableObj
    {
        private static int _count = 0;

        private static float gradientEpsilon = 0.001f;
        private static float gradientStep = 0.5f;

        static int numberOfIntersections = 0;
        static float size = 501.0f;

        public static float alpha = 0.002f;
        public static float finalEpsilon = 0.01f;
        public static float alphaEpsilon = 0.001f;
        static List<CurveData> curves = new List<CurveData>();

        public TrimmingCurve()
        {
            Name = nameof(TrimmingCurve) + " " + _count++.ToString();
        }

        List<Vector3> _verts;
        public TrimmingCurve(List<Vector3> verts) : this()
        {
            _verts = verts;
        }

        public ObjRenderData GetRenderData()
        {
            var obj = new ObjRenderData();
            obj.AddLine(_verts);
            return obj;
        }

        public static TrimmingCurve FindTrimmingCurve(List<TrimmingSurface> objs, Vector3 cursorPos)
        {
            float bestLen = 1000;
            Vector2 p0 = Vector2.Zero;
            Vector2 p1 = Vector2.Zero;

            int divCount = 11;
            bool sameObjects = objs[0].Id == objs[1].Id;
            float eps = 0.5f;

            for (int i = 0; i < divCount; i++)
                for (int j = 0; j < divCount; j++)
                    for (int k = 0; k < divCount; k++)
                        for (int m = 0; m < divCount; m++)
                        {
                            float ii = 1f * i / (divCount - 1);
                            float jj = 1f * j / (divCount - 1);
                            float kk = 1f * k / (divCount - 1);
                            float mm = 1f * m / (divCount - 1);

                            var ev1 = objs[0].Evaluate(jj, ii);
                            var ev2 = objs[1].Evaluate(mm, kk);
                            var len = Vector3.Distance(ev1, cursorPos) + Vector3.Distance(ev2, cursorPos);
                            if (len < bestLen && (!sameObjects || (eps < Math.Abs(ii - kk) && eps < Math.Abs(jj - mm))))
                            {
                                p0 = new Vector2(jj, ii);
                                p1 = new Vector2(mm, kk);
                                bestLen = len;
                            }
                        }

            return CountGradientMethod(objs[0], objs[1], p0, p1);
        }

        private static TrimmingCurve CountGradientMethod(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 value0, Vector2 value1)
        {
            var u = new Vector2(value0.X, value1.X);
            var v = new Vector2(value0.Y, value1.Y);
            var p0 = obj0.Evaluate(value0.X, value0.Y);
            var p1 = obj1.Evaluate(value1.X, value1.Y);

            Vector2 point0 = Vector2.Zero;
            Vector2 point1 = Vector2.Zero;
            var i = 0;
            while (Vector3.Distance(p1, p0) > gradientEpsilon)
            {
                i++;
                if (i > 1000)
                    return null;

                point0 = new Vector2(u.X, v.X);
                point1 = new Vector2(u.Y, v.Y);

                Vector4 betterPoint;
                try
                {
                    betterPoint = GetGradient(obj0, obj1, point0, point1);
                }
                catch (Exception e)
                {
                    return null;
                }

                betterPoint *= gradientStep;
                p0 = obj0.Evaluate(u.X, v.X);
                p1 = obj1.Evaluate(u.Y, v.Y);

                Vector2 uPrev = u;
                Vector2 vPrev = v;
                u = new Vector2(uPrev.X - betterPoint.X, uPrev.Y - betterPoint.Z);
                v = new Vector2(vPrev.X - betterPoint.Y, vPrev.Y - betterPoint.W);
            }

            return goGoNewton(obj0, obj1, u, v);
        }
        private static Vector4 GetGradient(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 point0, Vector2 point1)
        {
            var eval1 = obj0.Evaluate(point0.X, point0.Y);
            var eval2 = obj1.Evaluate(point1.X, point1.Y);

            var diff = eval1 - eval2;

            var eval0u = obj0.EvaluateDU(point0.X, point0.Y);
            var eval0v = obj0.EvaluateDV(point0.X, point0.Y);

            var eval1u = obj1.EvaluateDU(point1.X, point1.Y);
            var eval1v = obj1.EvaluateDV(point1.X, point1.Y);

            return new Vector4(
             Vector3.Dot(diff, eval0u),
             Vector3.Dot(diff, eval0v),
             Vector3.Dot(-diff, eval1u),
             Vector3.Dot(-diff, eval1v)
             );
        }
        private static TrimmingCurve goGoNewton(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 u, Vector2 v)
        {
            //var { obj0, obj1, u, v} = best;
            object interpolation = null;
            CurveData cuttingCurve = null;
            var _alpha = alpha;
            cuttingCurve = addCuttingCurve(interpolation);

            var uStart = u;
            var vStart = v;
            var uPrev = uStart;
            var vPrev = vStart;
            Vector4 betterPoint = Vector4.Zero;
            var backed = false;
            var pStart = obj0.Evaluate(uStart.X, vStart.X);
            var notFinishYet = 0;
            var loops = 0;
            int crossed1 = 0;
            int crossed2 = 0;
            var pointsList = new List<Vector3>();
            var stop = 1000;
            var finished = false;

            while (!finished)
            {
                var tempAlpha = _alpha;
                for (var i = 0; i < 10; i++)
                {
                    betterPoint = findNewNewtonPoint(obj0, obj1, uPrev, vPrev, u, v, tempAlpha);
                    if (!(obj0 is Torus) && obj1 is Torus)
                    {
                        betterPoint.Y *= 0.15f;
                        betterPoint.Z *= 0.15f;
                    }
                    //var { ob, u, v, uNew, vNew, uStart, vStart, alpha, backed };
                    var u0 = new UpdStruct()
                    {
                        Obj = obj0,
                        U = u.X,
                        V = v.X,
                        UNew = betterPoint.X,
                        VNew = betterPoint.Y,
                        Backed = backed
                    };
                    var u1 = new UpdStruct()
                    {
                        Obj = obj1,
                        U = u.Y,
                        V = v.Y,
                        UNew = betterPoint.Z,
                        VNew = betterPoint.W,
                        Backed = backed
                    };
                    var upd0 = updateUVAfterNewton(u0);
                    var upd1 = updateUVAfterNewton(u1);
                    u.X = upd0.u;
                    u.Y = upd1.u;

                    v.X = upd0.v;
                    v.Y = upd1.v;
                    if (upd0.end || upd1.end)
                    {
                        finished = true;
                        //if (upd0.end)
                        //{
                        //    addBorder(upd0.crossed, 1, cuttingCurve, uPrev, vPrev, obj0);
                        //}
                        //else if (upd1.end)
                        //{
                        //    addBorder(upd1.crossed, 2, cuttingCurve, uPrev, vPrev, obj1);
                        //}
                        break;
                    }
                    //if ( upd0.crossed != 0 && !upd0.backThisTime)
                    //{
                    //    crossed1 += upd0.crossed;
                    //    addBorder(upd0.crossed, 1, cuttingCurve, uPrev, vPrev, obj0);
                    //    updateIn1Visualisation(cuttingCurve.Id, upd0.uLast / obj0.HeightQwe, upd0.vLast / obj0.WidthQwe);
                    //    updateIn1Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    //    updateIn1Visualisation(cuttingCurve.Id, u.X / obj0.HeightQwe, v.X / obj0.WidthQwe);
                    //}
                    //if ( upd1.crossed != 0 && !upd1.backThisTime)
                    //{
                    //    crossed2 += upd1.crossed;
                    //    addBorder(upd1.crossed, 2, cuttingCurve, uPrev, vPrev, obj1);
                    //    updateIn2Visualisation(cuttingCurve.Id, upd1.uLast / obj1.HeightQwe, upd1.vLast / obj1.WidthQwe);
                    //    updateIn2Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    //    updateIn2Visualisation(cuttingCurve.Id, u.Y / obj1.HeightQwe, v.Y / obj1.WidthQwe);
                    //}
                    //if  (upd0.backThisTime || upd1.backThisTime)
                    //{
                    //    if (upd0.backThisTime)
                    //    {
                    //        addBorder(upd0.crossed, 1, cuttingCurve, uPrev, vPrev, obj0);
                    //    }
                    //    else if (upd1.backThisTime)
                    //    {
                    //        addBorder(upd1.crossed, 2, cuttingCurve, uPrev, vPrev, obj1);
                    //    }
                    //    updateIn1Visualisation(cuttingCurve.Id, float.Epsilon, float.Epsilon);
                    //    updateIn2Visualisation(cuttingCurve.Id, float.Epsilon, float.Epsilon);
                    //}
                    if (upd0.backThisTime || upd1.backThisTime)
                    {
                        pointsList.Add(obj0.Evaluate(u.X, v.X));
                        backNewton(pointsList, ref uStart, ref vStart, ref u, ref v, ref uPrev, ref vPrev, ref _alpha);
                        notFinishYet = 5;
                        backed = true;
                        tempAlpha = _alpha;
                        break;
                    }

                }

                uPrev = u;
                vPrev = v;

                var p1 = obj0.Evaluate(u.X, v.X);
                var p2 = obj1.Evaluate(u.Y, v.Y);
                if (alphaEpsilon < Vector3.Distance(p2, p1))
                {
                    tempAlpha /= 2;
                }

                pointsList.Add(obj0.Evaluate(u.X, v.X));

                //  updateIn1Visualisation(cuttingCurve.Id, u.X / obj0.HeightQwe, v.X / obj0.WidthQwe);
                //  updateIn2Visualisation(cuttingCurve.Id, u.Y / obj1.HeightQwe, v.Y / obj1.WidthQwe);
                if (finalEpsilon > Vector3.Distance(pStart, p1) && notFinishYet > 10)
                {
                    break;
                }
                if (loops > stop)
                {
                    break;
                }
                notFinishYet++;
                loops++;
            }

            //setVisualisationObjects(obj0, obj1);
            if (!backed)
            {
                cuttingCurve.Points.Add(pStart);
            }
            for (var i = 0; i < pointsList.Count; i++)
            {
                cuttingCurve.Points.Add(pointsList[i]);
            }
            if (!backed)
            {
                cuttingCurve.Points.Add(pStart);
                //if (isLenghtNotToLong(cuttingCurve.intersectionVisualization1))
                //{
                //    cuttingCurve.intersectionVisualization1.Add(new Vector2(cuttingCurve.intersectionVisualization1[0].X, cuttingCurve.intersectionVisualization1[0].Y));
                //}
                //if (isLenghtNotToLong(cuttingCurve.intersectionVisualization2))
                //{
                //    cuttingCurve.intersectionVisualization2.Add(new Vector2(cuttingCurve.intersectionVisualization2[0].X, cuttingCurve.intersectionVisualization2[0].Y));
                //}
            }

            return new TrimmingCurve(cuttingCurve.Points);
        }

        public static CurveData addCuttingCurve(object iCurve)
        {
            var curveData = new CurveData()
            {
                Id = numberOfIntersections,
                Name = "Intersection curve " + numberOfIntersections.ToString(),
                intersectionVisualization1 = new List<Vector2>(),
                intersectionVisualization2 = new List<Vector2>(),
                Points = new List<Vector3>(),
                InterpolationCurve = iCurve
            };
            numberOfIntersections++;
            curves.Add(curveData);
            return curveData;
        }

        private static Vector4 findNewNewtonPoint(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 u, Vector2 v, Vector2 uNew, Vector2 vNew, float alpha)
        {
            var mat = generateJacobi(obj0, obj1, u, v, uNew, vNew, alpha);
            var vec = getFforJacobi(obj0, obj1, u, v, uNew, vNew, alpha);
            return vec.Multiply(mat);
        }
        public static Matrix4x4 generateJacobi(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 u, Vector2 v, Vector2 uNew, Vector2 vNew, float alpha)
        {
            var dU1 = obj0.EvaluateDU(u.X, v.X);
            var dV1 = obj0.EvaluateDV(u.X, v.X);
            var dU2 = obj1.EvaluateDU(u.Y, v.Y);
            var dV2 = obj1.EvaluateDV(u.Y, v.Y);
            if (dU1 == Vector3.Zero)
            {
                dU1 = obj0.EvaluateDU(u.X, v.X);
                dV1 = obj0.EvaluateDV(u.X, v.X);
                dU2 = obj1.EvaluateDU(u.Y, v.Y);
                dV2 = obj1.EvaluateDV(u.Y, v.Y);
            }
            var t = getT(dU1, dU2, dV1, dV2);
            dU1 = obj0.EvaluateDU(uNew.X, vNew.X);
            dV1 = obj0.EvaluateDV(uNew.X, vNew.X);
            dU2 = -obj1.EvaluateDU(uNew.Y, vNew.Y);
            dV2 = -obj1.EvaluateDV(uNew.Y, vNew.Y);

            var dot1 = Vector3.Dot(dU1, t);
            var dot2 = Vector3.Dot(dV1, t);
            var jacobiMatrix = new Matrix4x4(
                dU1.X, dV1.X, dU2.X, dV2.X,
                dU1.Y, dV1.Y, dU2.Y, dV2.Y,
                dU1.Z, dV1.Z, dU2.Z, dV2.Z,
                dot1, dot2, 0, 0);

            Matrix4x4.Invert(jacobiMatrix, out Matrix4x4 inv);
            return inv;
        }
        public static Vector4 getFforJacobi(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 u, Vector2 v, Vector2 uNew, Vector2 vNew, float alpha)
        {
            var P0 = obj0.Evaluate(u.X, v.X);
            var Q = obj1.Evaluate(uNew.Y, vNew.Y);
            var P1 = obj0.Evaluate(uNew.X, vNew.X);
            var dU1 = obj0.EvaluateDU(u.X, v.X);
            var dV1 = obj0.EvaluateDV(u.X, v.X);
            var dU2 = obj1.EvaluateDU(u.Y, v.Y);
            var dV2 = obj1.EvaluateDV(u.Y, v.Y);
            var t = getT(dU1, dU2, dV1, dV2, alpha);
            return new Vector4(
                P1.X - Q.X,
                P1.Y - Q.Y,
                P1.Z - Q.Z,
                Vector3.Dot(P1 - P0, t) + (alpha * 100.0f)
            );
        }
        public static Vector3 getT(Vector3 du1, Vector3 du2, Vector3 dv1, Vector3 dv2, float alpha = 0)
        {
            var np = Vector3.Normalize(Vector3.Cross(du1, dv1));
            var nq = Vector3.Normalize(Vector3.Cross(du2, dv2));
            var t = Vector3.Cross(np, nq);
            return Vector3.Normalize(t);
        }


        private static UpdUvStruct updateUVAfterNewton(UpdStruct u)
        {
            // crossed:
            // left -1
            // right 1
            // top -2
            // bottom 2

            var backThisTime = false;
            var crossed = 0;
            var end = false;

            var eps = 0.0009f;
            var epsWrap = 0.00001f;

            float _uNew = u.U - (u.UNew * eps);
            float _vNew = u.V - (u.VNew * eps);
            float _uLast = u.U - (u.UNew * eps);
            float _vLast = u.V - (u.VNew * eps);

            if (_uNew < 0)
            {
                if (u.Obj.WrappedU)
                {
                    _uNew = 1 - epsWrap;
                    _uLast = 0;
                }
                else
                {
                    _uNew = 0;
                    if (u.Backed)
                    {
                        end = true;
                    }
                    else
                    {
                        backThisTime = true;
                    }
                }
                crossed = -1;
            }
            if (_uNew >= 1)
            {
                if (u.Obj.WrappedU)
                {
                    _uNew = 0;
                    _uLast = 1 - epsWrap;
                }
                else
                {
                    _uNew = 1 - epsWrap;
                    if (u.Backed)
                    {
                        end = true;
                    }
                    else
                    {
                        backThisTime = true;
                    }
                }
                crossed = 1;
            }
            if (_vNew >= 1)
            {
                if (u.Obj.WrappedV)
                {
                    _vNew = 0;
                    _vLast = 1 - epsWrap;
                }
                else
                {
                    _vNew = 1 - epsWrap;
                    if (u.Backed)
                    {
                        end = true;
                    }
                    else
                    {
                        backThisTime = true;
                    }
                }
                crossed = 2;
            }
            if (_vNew < 0)
            {
                if (u.Obj.WrappedV)
                {
                    _vNew = 1 - epsWrap;
                    _vLast = 0;
                }
                else
                {
                    _vNew = 0;
                    if (u.Backed)
                    {
                        end = true;
                    }
                    else
                    {
                        backThisTime = true;
                    }
                }
                crossed = -2;
            }

            return new UpdUvStruct()
            {
                u = _uNew,
                v = _vNew,
                end = end,
                backThisTime = backThisTime,
                crossed = crossed,
                uLast = _uLast,
                vLast = _vLast
            };
        }
        public static void backNewton(List<Vector3> pointsList, ref Vector2 uStarts, ref Vector2 vStarts, ref Vector2 us, ref Vector2 vs, ref Vector2 uPrevs, ref Vector2 vPrevs, ref float alpha)
        {
            us = uStarts;
            vs = vStarts;

            uPrevs = uStarts;
            vPrevs = vStarts;

            pointsList.Reverse();
            alpha = -alpha;
        }
        public static void addBorder(int crossed, int num, CurveData cuttingCurve, Vector2 u, Vector2 v, TrimmingSurface obj)
        {
            if (crossed == -2)
            {
                if (num == 1)
                {
                    updateIn1Visualisation(cuttingCurve.Id, u.X, 0);
                }
                else
                {
                    updateIn2Visualisation(cuttingCurve.Id, u.Y, 0);
                }
            }
            else if (crossed == 2)
            {
                if (num == 1)
                {
                    updateIn1Visualisation(cuttingCurve.Id, u.X, 0.99999f);
                }
                else
                {
                    updateIn2Visualisation(cuttingCurve.Id, u.Y, 0.99999f);
                }
            }
            else if (crossed == -1)
            {
                if (num == 1)
                {
                    updateIn1Visualisation(cuttingCurve.Id, 0, v.X);
                }
                else
                {
                    updateIn2Visualisation(cuttingCurve.Id, 0, v.Y);
                }
            }
            else if (crossed == 1)
            {
                if (num == 1)
                {
                    updateIn1Visualisation(cuttingCurve.Id, 0.99999f, v.X);
                }
                else
                {
                    updateIn2Visualisation(cuttingCurve.Id, 0.99999f, v.Y);
                }
            }
        }
        public static void updateIn1Visualisation(int id, float u, float v)
        {
            //break = float.NaN, back = float.Epsilon
            var curve = curves.FirstOrDefault(x => x.Id == id);
            if (float.IsNaN(u))
            {
                curve.intersectionVisualization1.Add(new Vector2(float.NaN, float.NaN));
                return;
            }
            if (u == float.Epsilon)
            {
                curve.intersectionVisualization1.Add(new Vector2(float.Epsilon, float.Epsilon));
                return;
            }
            var _u = Convert.ToInt32(u * size) % size;
            var _v = Convert.ToInt32(v * size) % size;
            if (_u < 0 || _v < 0)
            {
                return;
            }
            curve.intersectionVisualization1.Add(new Vector2(_u, _v));
        }
        public static void updateIn2Visualisation(int id, float u, float v)
        {
            //break = float.NaN, back = float.Epsilon
            var curve = curves.FirstOrDefault(x => x.Id == id);
            if (float.IsNaN(u))
            {
                curve.intersectionVisualization2.Add(new Vector2(float.NaN, float.NaN));
                return;
            }
            if (u == float.Epsilon)
            {
                curve.intersectionVisualization2.Add(new Vector2(float.Epsilon, float.Epsilon));
                return;
            }
            var _u = Convert.ToInt32(u * size) % size;
            var _v = Convert.ToInt32(v * size) % size;
            if (_u < 0 || _v < 0)
            {
                return;
            }
            curve.intersectionVisualization2.Add(new Vector2(_u, _v));
        }
        public static void updateWrappingBeforeFinish(int crossed, int num, CurveData cuttingCurve, Vector2 u, Vector2 v, TrimmingSurface obj)
        {
            if (crossed == -2)
            {
                if (num == 1)
                {
                    updateIn1Visualisation(cuttingCurve.Id, u.X, 0);
                    updateIn1Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    updateIn1Visualisation(cuttingCurve.Id, u.X, 0.99999f);
                }
                else
                {
                    updateIn2Visualisation(cuttingCurve.Id, u.Y, 0);
                    updateIn2Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    updateIn2Visualisation(cuttingCurve.Id, u.Y, 0.99999f);
                }
            }
            else if (crossed == 2)
            {
                if (num == 1)
                {
                    updateIn1Visualisation(cuttingCurve.Id, u.X, 0.99999f);
                    updateIn1Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    updateIn1Visualisation(cuttingCurve.Id, u.X, 0);
                }
                else
                {
                    updateIn2Visualisation(cuttingCurve.Id, u.Y, 0.99999f);
                    updateIn2Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    updateIn2Visualisation(cuttingCurve.Id, u.Y, 0);
                }
            }
            if (crossed == -1)
            {
                if (num == 1)
                {
                    updateIn1Visualisation(cuttingCurve.Id, 0, v.X);
                    updateIn1Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    updateIn1Visualisation(cuttingCurve.Id, 0.99999f, v.X);
                }
                else
                {
                    updateIn2Visualisation(cuttingCurve.Id, 0, v.Y);
                    updateIn2Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    updateIn2Visualisation(cuttingCurve.Id, 0.99999f, v.Y);
                }
            }
            else if (crossed == 1)
            {
                if (num == 1)
                {
                    updateIn1Visualisation(cuttingCurve.Id, 0.99999f, v.X);
                    updateIn1Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    updateIn1Visualisation(cuttingCurve.Id, 0, v.X);
                }
                else
                {
                    updateIn2Visualisation(cuttingCurve.Id, 0.99999f, v.Y);
                    updateIn2Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    updateIn2Visualisation(cuttingCurve.Id, 0, v.Y);
                }
            }
        }
        public static bool isLenghtNotToLong(List<Vector2> intersectionVisualization)
        {
            var len0 = intersectionVisualization[0].X - intersectionVisualization[intersectionVisualization.Count - 1].X;
            var len1 = intersectionVisualization[0].Y - intersectionVisualization[intersectionVisualization.Count - 1].Y;
            return Math.Abs(len0) < 50 && Math.Abs(len1) < 50;
        }

    }
}