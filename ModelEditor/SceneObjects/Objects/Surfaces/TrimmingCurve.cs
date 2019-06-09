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

        Vector3 Evaluate(Vector2 hw);
        Vector3 EvaluateDU(Vector2 hw);
        Vector3 EvaluateDV(Vector2 hw);
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
        public Vector2 UV { get; set; }
        public Vector2 UVNew { get; set; }
        public bool Backed { get; set; }
    }

    public struct UpdUvStruct
    {
        public Vector2 uv;
        public bool end;
        public bool backThisTime;
        public int crossed;
        public Vector2 uvLast;
    }

    public class TrimmingCurve : SceneObject, IRenderableObj
    {
        private static int _count = 0;

        private static float gradientEpsilon = 0.001f;
        private static float gradientStep = 0.1f;

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

        public static TrimmingCurve FindTrimmingCurve(List<TrimmingSurface> objs, Vector3 cursorPos, float precision)
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

                            var ev1 = objs[0].Evaluate(new Vector2(ii, jj));
                            var ev2 = objs[1].Evaluate(new Vector2(kk, mm));
                            var len = Vector3.Distance(ev1, cursorPos) + Vector3.Distance(ev2, cursorPos);
                            if (len < bestLen && (!sameObjects || (eps < Math.Abs(ii - kk) && eps < Math.Abs(jj - mm))))
                            {
                                p0 = new Vector2(ii, jj);
                                p1 = new Vector2(kk, mm);
                                bestLen = len;
                            }
                        }

            return CountGradientMethod(objs[0], objs[1], p0, p1,precision);
        }

        private static TrimmingCurve CountGradientMethod(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 value0, Vector2 value1, float precision)
        {
            var p0 = obj0.Evaluate(value0);
            var p1 = obj1.Evaluate(value1);

            var i = 0;
            var alpha = 1f;
            var dist = Vector3.Distance(p1, p0);
            while (dist > gradientEpsilon)
            {
                i++;
                if (i > 1000)
                    return null;

                try
                {
                    var grads = GetGradient(obj0, obj1, value0, value1);
                    value0 -= alpha * grads[0];
                    value1 -= alpha * grads[1];

                    value0.X = Math.Min(1, Math.Max(0, value0.X));
                    value0.Y = Math.Min(1, Math.Max(0, value0.Y));
                    value1.X = Math.Min(1, Math.Max(0, value1.X));
                    value1.Y = Math.Min(1, Math.Max(0, value1.Y));

                    var pNew0 = obj0.Evaluate(value0);
                    var pNew1 = obj1.Evaluate(value1);

                    var newDist = Vector3.Distance(pNew0, pNew1);
                    if (newDist > dist)
                    {
                        alpha /= 2;
                    }
                    else
                    {
                        dist = newDist;
                        p0 = pNew0;
                        p1 = pNew1;
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            return goGoNewton(obj0, obj1, value0, value1, precision);
        }
        private static List<Vector2> GetGradient(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 point0, Vector2 point1)
        {
            var eval0 = obj0.Evaluate(point0);
            var eval1 = obj1.Evaluate(point1);

            var diff = eval0 - eval1;

            var eval0u = obj0.EvaluateDU(point0);
            var eval0v = obj0.EvaluateDV(point0);

            var eval1u = obj1.EvaluateDU(point1);
            var eval1v = obj1.EvaluateDV(point1);

            var grad0 = new Vector2(
             Vector3.Dot(diff, eval0u),
             Vector3.Dot(diff, eval0v));

            var grad1 = new Vector2(
             Vector3.Dot(-diff, eval1u),
             Vector3.Dot(-diff, eval1v));

            return new List<Vector2>() { gradientStep * grad0.Normalized(), gradientStep * grad1.Normalized() };
        }
        private static TrimmingCurve goGoNewton(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 uv0, Vector2 uv1, float precision)
        {
            //var { obj0, obj1, u, v} = best;
            //CurveData cuttingCurve = null;
            var _alpha = alpha;
            cuttingCurve = addCuttingCurve();

            var uvStart0 = uv0;
            var uvStart1 = uv1;
            var uvPrev0 = uvStart0;
            var uvPrev1 = uvStart1;

            var backed = false;
            var finished = false;

            var pStart = obj0.Evaluate(uvStart0);
            var notFinishYet = 0;
            var loops = 0;
            int crossed1 = 0;
            int crossed2 = 0;
            var pointsList = new List<Vector3>();
            var uvList0 = new List<Vector2>();
            var uvList1 = new List<Vector2>();

            while (!finished)
            {
                var tempAlpha = _alpha;
                while (true)
                {
                    var betterPoint = findNewNewtonPoint(obj0, obj1, uvPrev0, uvPrev1, uv0, uv1, tempAlpha);
                    //if (!(obj0 is Torus) && obj1 is Torus)
                    //{
                    //    //?
                    //    betterPoint.Y *= 0.15f;
                    //    betterPoint.Z *= 0.15f;
                    //}

                    var uvNew0 = new Vector2(betterPoint.X, betterPoint.Y);
                    var uvNew1 = new Vector2(betterPoint.Z, betterPoint.W);

                    var upd0 = updateUVAfterNewton(obj0, uv0, uvNew0, backed);
                    var upd1 = updateUVAfterNewton(obj1, uv1, uvNew1, backed);
                    uv0 = upd0.uv;
                    uv1 = upd1.uv;

                    if (upd0.end || upd1.end)
                    {
                        finished = true;
                        if (upd0.end)
                        {
                            addBorder(upd0.crossed, 1, cuttingCurve, uvPrev0, uvPrev1, obj0);
                        }
                        else if (upd1.end)
                        {
                            addBorder(upd1.crossed, 2, cuttingCurve, uvPrev0, uvPrev1, obj1);
                        }
                        break;
                    }
                    //if (upd0.crossed != 0 && !upd0.backThisTime)
                    //{
                    //    crossed1 += upd0.crossed;
                    //    addBorder(upd0.crossed, 1, cuttingCurve, uvPrev0, uvPrev1, obj0);
                    //    updateIn1Visualisation(cuttingCurve.Id, upd0.uvLast.X, upd0.uvLast.Y);
                    //    updateIn1Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    //    updateIn1Visualisation(cuttingCurve.Id, uv0.X, uv0.Y);
                    //}
                    //if (upd1.crossed != 0 && !upd1.backThisTime)
                    //{
                    //    crossed2 += upd1.crossed;
                    //    addBorder(upd1.crossed, 2, cuttingCurve, uvPrev0, uvPrev1, obj1);
                    //    updateIn2Visualisation(cuttingCurve.Id, upd1.uvLast.X, upd1.uvLast.Y);
                    //    updateIn2Visualisation(cuttingCurve.Id, float.NaN, float.NaN);
                    //    updateIn2Visualisation(cuttingCurve.Id, uv1.X, uv1.Y);
                    //}
                    //if (upd0.backThisTime || upd1.backThisTime)
                    //{
                    //    if (upd0.backThisTime)
                    //    {
                    //        addBorder(upd0.crossed, 1, cuttingCurve, uvPrev0, uvPrev1, obj0);
                    //    }
                    //    else if (upd1.backThisTime)
                    //    {
                    //        addBorder(upd1.crossed, 2, cuttingCurve, uvPrev0, uvPrev1, obj1);
                    //    }
                    //    updateIn1Visualisation(cuttingCurve.Id, float.Epsilon, float.Epsilon);
                    //    updateIn2Visualisation(cuttingCurve.Id, float.Epsilon, float.Epsilon);
                    //}
                    if (upd0.backThisTime || upd1.backThisTime)
                    {
                        pointsList.Add(obj0.Evaluate(uv0));
                        backNewton(pointsList, ref uvStart0, ref uvStart1, ref uv0, ref uv1, ref uvPrev0, ref uvPrev1, ref _alpha);
                        notFinishYet = 5;
                        backed = true;
                        tempAlpha = _alpha;
                        break;
                    }

                    if (precision > Vector3.Distance(obj0.Evaluate(uv0), obj1.Evaluate(uv1)))
                    {
                        break;
                    }

                }

                uvPrev0 = uv0;
                uvPrev1 = uv1;

                var p1 = obj0.Evaluate(uv0);
                var p2 = obj1.Evaluate(uv1);
                var dist = Vector3.Distance(p2, p1);
                if (alphaEpsilon < Vector3.Distance(p2, p1))
                {
                    tempAlpha /= 2;
                }

                pointsList.Add(obj0.Evaluate(uv0));
                uvList0.Add(uv0);
                uvList1.Add(uv1);

                //  updateIn1Visualisation(cuttingCurve.Id, u.X / obj0.HeightQwe, v.X / obj0.WidthQwe);
                //  updateIn2Visualisation(cuttingCurve.Id, u.Y / obj1.HeightQwe, v.Y / obj1.WidthQwe);
                if (loops > 1000 || finalEpsilon > Vector3.Distance(pStart, p1) && notFinishYet > 10)
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
                uvList0.Add(uvStart0);
                uvList1.Add(uvStart1);
            }
            for (var i = 0; i < pointsList.Count; i++)
            {
                cuttingCurve.Points.Add(pointsList[i]);
            }
            if (!backed)
            {
                //cuttingCurve.Points.Add(pStart);
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

        public static CurveData addCuttingCurve()
        {
            var curveData = new CurveData()
            {
                Id = numberOfIntersections,
                Name = "Intersection curve " + numberOfIntersections.ToString(),
                intersectionVisualization1 = new List<Vector2>(),
                intersectionVisualization2 = new List<Vector2>(),
                Points = new List<Vector3>(),
            };
            numberOfIntersections++;
            curves.Add(curveData);
            return curveData;
        }

        private static Vector4 findNewNewtonPoint(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 uv0, Vector2 uv1, Vector2 uvNew0, Vector2 uvNew1, float alpha)
        {
            var mat = generateJacobi(obj0, obj1, uv0, uv1, uvNew0, uvNew1);
            var vec = getFforJacobi(obj0, obj1, uv0, uv1, uvNew0, uvNew1, alpha);
            return vec.Multiply(mat);
        }
        public static Matrix4x4 generateJacobi(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 uv0, Vector2 uv1, Vector2 uvNew0, Vector2 uvNew1)
        {
            var dU1 = obj0.EvaluateDU(uv0);
            var dV1 = obj0.EvaluateDV(uv0);
            var dU2 = obj1.EvaluateDU(uv1);
            var dV2 = obj1.EvaluateDV(uv1);

            var t = getT(dU1, dU2, dV1, dV2);
            dU1 = obj0.EvaluateDU(uvNew0);
            dV1 = obj0.EvaluateDV(uvNew0);
            dU2 = -obj1.EvaluateDU(uvNew1);
            dV2 = -obj1.EvaluateDV(uvNew1);

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
        public static Vector4 getFforJacobi(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 uv0, Vector2 uv1, Vector2 uvNew0, Vector2 uvNew1, float alpha)
        {
            var P0 = obj0.Evaluate(uv0);
            var Q = obj1.Evaluate(uvNew1);
            var P1 = obj0.Evaluate(uvNew0);

            var dU1 = obj0.EvaluateDU(uv0);
            var dV1 = obj0.EvaluateDV(uv0);
            var dU2 = obj1.EvaluateDU(uv1);
            var dV2 = obj1.EvaluateDV(uv1);
            var t = getT(dU1, dU2, dV1, dV2);

            var d = alpha * 10;

            return new Vector4(P1 - Q, Vector3.Dot(P1 - P0, t) - d);
        }
        public static Vector3 getT(Vector3 du1, Vector3 du2, Vector3 dv1, Vector3 dv2)
        {
            var np = Vector3.Normalize(Vector3.Cross(du1, dv1));
            var nq = Vector3.Normalize(Vector3.Cross(du2, dv2));
            var t = Vector3.Cross(np, nq);
            return Vector3.Normalize(t);
        }

        private static UpdUvStruct updateUVAfterNewton(TrimmingSurface obj, Vector2 uv, Vector2 uvNew, bool backed)
        {
            // crossed:
            // left -1
            // right 1
            // top -2
            // bottom 2

            var backThisTime = false;
            var crossed = 0;
            var end = false;

            var epsWrap = 0.00001f;

            //var eps = 0.0009f;
            //eps = 1f;

            //float _uNew = uv.X - (uvNew.X * eps);
            //float _vNew = uv.Y - (uvNew.Y * eps);
            //float _uLast = uv.X - (uvNew.X * eps);
            //float _vLast = uv.Y - (uvNew.Y * eps);
            float _uNew = uv.X - uvNew.X;
            float _vNew = uv.Y - uvNew.Y;
            float _uLast = uv.X - uvNew.X;
            float _vLast = uv.Y - uvNew.Y;

            if (_uNew < 0)
            {
                if (obj.WrappedU)
                {
                    _uNew = 1 - epsWrap;
                    _uLast = 0;
                }
                else
                {
                    _uNew = 0;
                    if (backed)
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
                if (obj.WrappedU)
                {
                    _uNew = 0;
                    _uLast = 1 - epsWrap;
                }
                else
                {
                    _uNew = 1 - epsWrap;
                    if (backed)
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
                if (obj.WrappedV)
                {
                    _vNew = 0;
                    _vLast = 1 - epsWrap;
                }
                else
                {
                    _vNew = 1 - epsWrap;
                    if (backed)
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
                if (obj.WrappedV)
                {
                    _vNew = 1 - epsWrap;
                    _vLast = 0;
                }
                else
                {
                    _vNew = 0;
                    if (backed)
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
                uv = new Vector2(_uNew, _vNew),
                end = end,
                backThisTime = backThisTime,
                crossed = crossed,
                uvLast = new Vector2(_uLast, _vLast),
            };
        }
        public static void backNewton(List<Vector3> pointsList, ref Vector2 uvStart0, ref Vector2 uvStart1, ref Vector2 uv0, ref Vector2 uv1, ref Vector2 uvPrev0, ref Vector2 uvPrev1, ref float alpha)
        {
            uv0 = uvStart0;
            uv1 = uvStart1;

            uvPrev0 = uvStart0;
            uvPrev1 = uvStart1;

            pointsList.Reverse();
            alpha = -alpha;
        }
        public static void addBorder(int crossed, int num, CurveData cuttingCurve, Vector2 uv0, Vector2 uv1, TrimmingSurface obj)
        {
            if (crossed == -2)
            {
                if (num == 1) updateIn1Visualisation(cuttingCurve.Id, uv0.X, 0);
                else updateIn2Visualisation(cuttingCurve.Id, uv1.X, 0);
            }
            else if (crossed == 2)
            {
                if (num == 1) updateIn1Visualisation(cuttingCurve.Id, uv0.X, 0.99999f);
                else updateIn2Visualisation(cuttingCurve.Id, uv1.X, 0.99999f);
            }
            else if (crossed == -1)
            {
                if (num == 1) updateIn1Visualisation(cuttingCurve.Id, 0, uv0.Y);
                else updateIn2Visualisation(cuttingCurve.Id, 0, uv1.Y);
            }
            else if (crossed == 1)
            {
                if (num == 1) updateIn1Visualisation(cuttingCurve.Id, 0.99999f, uv0.Y);
                else updateIn2Visualisation(cuttingCurve.Id, 0.99999f, uv1.Y);
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