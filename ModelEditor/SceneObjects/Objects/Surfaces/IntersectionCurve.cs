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
    public interface IIntersect
    {
        Guid Id { get; }
        bool WrappedU { get; }
        bool WrappedV { get; }

        Vector3 Evaluate(Vector2 hw);
        Vector3 EvaluateDU(Vector2 hw);
        Vector3 EvaluateDV(Vector2 hw);
    }

    public struct UpdStruct
    {
        public IIntersect Obj { get; set; }
        public Vector2 UV { get; set; }
        public Vector2 UVNew { get; set; }
        public bool Backed { get; set; }
    }

    public struct UpdateUVStruct
    {
        public Vector2 UV;
        public bool End;
        public bool Back;
    }

    public class IntersectionCurve : SceneObject, IRenderableObj, IIntersectionRenderableObj
    {
        private static int _count = 0;

        private static float _gradientEps = 0.0001f;
        private static float _startGradientAlpha = 0.01f;

        public static float _newtonStartAlpha = 0.002f;
        public static float _finalEpsilon = 0.01f;
        public static float _alphaEpsilon = 0.001f;

        public IntersectionCurve()
        {
            Name = nameof(IntersectionCurve) + " " + _count++.ToString();
        }

        public List<Vector3> Verts { get; }
        private readonly List<Vector3> _uv0;
        private readonly List<Vector3> _uv1;

        public IntersectionCurve(List<Vector3> verts, List<Vector2> uv0, List<Vector2> uv1) : this()
        {
            Verts = verts;
            _uv0 = uv0.Select(v => new Vector3(v, 0)).ToList();
            _uv1 = uv1.Select(v => new Vector3(v, 0)).ToList();
        }

        public ObjRenderData GetRenderData()
        {
            var obj = new ObjRenderData();
            obj.AddLine(Verts);
            return obj;
        }
        public ObjRenderData IntersectionGetRenderData0()
        {
            var obj = new ObjRenderData();
            obj.AddLine(_uv0);
            return obj;
        }
        public ObjRenderData IntersectionGetRenderData1()
        {
            var obj = new ObjRenderData();
            obj.AddLine(_uv1);
            return obj;
        }

        public static IntersectionCurve FindIntersectionCurve(List<IIntersect> objs, Vector3 cursorPos, float precision)
        {
            float maxDist = float.MaxValue;
            Vector2 p0 = Vector2.Zero;
            Vector2 p1 = Vector2.Zero;

            int divCount = 11;
            bool oneObject = objs[0].Id == objs[1].Id;
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
                            var dist = Vector3.Distance(ev1, cursorPos) + Vector3.Distance(ev2, cursorPos);
                            if (dist < maxDist && (!oneObject || (eps < Math.Abs(ii - kk) && eps < Math.Abs(jj - mm))))
                            {
                                p0 = new Vector2(ii, jj);
                                p1 = new Vector2(kk, mm);
                                maxDist = dist;
                            }
                        }

            return Gradient(objs[0], objs[1], p0, p1, precision);
        }

        private static IntersectionCurve Gradient(IIntersect obj0, IIntersect obj1, Vector2 value0, Vector2 value1, float precision)
        {
            var p0 = obj0.Evaluate(value0);
            var p1 = obj1.Evaluate(value1);

            var i = 0;
            var currAlpha = _startGradientAlpha;
            var dist = Vector3.Distance(p1, p0);
            while (dist > _gradientEps)
            {
                if (++i > 5000)
                    return null;

                try
                {
                    var grads = GetGradient(obj0, obj1, value0, value1);
                    value0 -= currAlpha * grads[0];
                    value1 -= currAlpha * grads[1];

                    value0.X = Math.Min(1, Math.Max(0, value0.X));
                    value0.Y = Math.Min(1, Math.Max(0, value0.Y));
                    value1.X = Math.Min(1, Math.Max(0, value1.X));
                    value1.Y = Math.Min(1, Math.Max(0, value1.Y));

                    var pNew0 = obj0.Evaluate(value0);
                    var pNew1 = obj1.Evaluate(value1);

                    var newDist = Vector3.Distance(pNew0, pNew1);
                    if (newDist > dist)
                    {
                        currAlpha /= 2;
                        currAlpha = Math.Max(currAlpha, 0.0001f);
                    }
                    else
                    {
                        currAlpha *= 2;
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

            return MyFriendNewton(obj0, obj1, value0, value1, precision);
        }
        private static List<Vector2> GetGradient(IIntersect obj0, IIntersect obj1, Vector2 point0, Vector2 point1)
        {
            var eval0 = obj0.Evaluate(point0);
            var eval1 = obj1.Evaluate(point1);

            var diff = eval0 - eval1;

            var eval0u = obj0.EvaluateDU(point0).Normalized();
            var eval0v = obj0.EvaluateDV(point0).Normalized();

            var eval1u = obj1.EvaluateDU(point1).Normalized();
            var eval1v = obj1.EvaluateDV(point1).Normalized();

            var grad0 = new Vector2(Vector3.Dot(diff, eval0u), Vector3.Dot(diff, eval0v));
            var grad1 = new Vector2(Vector3.Dot(-diff, eval1u), Vector3.Dot(-diff, eval1v));

            return new List<Vector2>() { grad0, grad1 };
        }
        private static IntersectionCurve MyFriendNewton(IIntersect obj0, IIntersect obj1, Vector2 uv0, Vector2 uv1, float precision)
        {
            var newtonAlpha = _newtonStartAlpha;

            var uvStart0 = uv0;
            var uvStart1 = uv1;
            var uvPrev0 = uvStart0;
            var uvPrev1 = uvStart1;

            var backed = false;
            var finished = false;

            var pStart = obj0.Evaluate(uvStart0);
            var countForCylinder = 0;
            var loops = 0;

            var pointsList = new List<Vector3>();
            var uvList0 = new List<Vector2>();
            var uvList1 = new List<Vector2>();

            pointsList.Add(obj0.Evaluate(uv0));
            uvList0.Add(uv0);
            uvList1.Add(uv1);

            while (!finished)
            {
                var currAlpha = newtonAlpha;
                int innerLoops = 0;
                while (true)
                {
                    var betterPoint = GetNewtonIterationPoint(obj0, obj1, uvPrev0, uvPrev1, uv0, uv1, currAlpha);

                    var uvDiff0 = new Vector2(betterPoint.X, betterPoint.Y);
                    var ufDivv1 = new Vector2(betterPoint.Z, betterPoint.W);

                    var upd0 = UpdateUV(obj0, uv0, uvDiff0, backed);
                    var upd1 = UpdateUV(obj1, uv1, ufDivv1, backed);
                    uv0 = upd0.UV;
                    uv1 = upd1.UV;

                    if (upd0.End || upd1.End)
                    {
                        finished = true;
                        break;
                    }

                    if (upd0.Back || upd1.Back)
                    {
                        pointsList.Reverse();
                        uvList0.Reverse();
                        uvList1.Reverse();

                        uv0 = uvStart0;
                        uv1 = uvStart1;

                        uvPrev0 = uvStart0;
                        uvPrev1 = uvStart1;

                        newtonAlpha = -_newtonStartAlpha;

                        countForCylinder = 5;
                        backed = true;
                        currAlpha = newtonAlpha;
                        break;
                    }

                    var ev0 = obj0.Evaluate(uv0);
                    var ev1 = obj1.Evaluate(uv1);
                    var dst = Vector3.Distance(ev0, ev1);
                    if (precision > dst)
                        break;

                    if (++innerLoops > 30)
                        return null;
                }

                uvPrev0 = uv0;
                uvPrev1 = uv1;

                var p1 = obj0.Evaluate(uv0);
                var p2 = obj1.Evaluate(uv1);
                var dist = Vector3.Distance(p2, p1);
                if (_alphaEpsilon < Vector3.Distance(p2, p1))
                {
                    currAlpha /= 2;
                }

                pointsList.Add(obj0.Evaluate(uv0));
                uvList0.Add(uv0);
                uvList1.Add(uv1);

                if (loops > 1000 || _finalEpsilon > Vector3.Distance(pStart, p1) && countForCylinder > 10)
                {
                    break;
                }

                countForCylinder++;
                loops++;
            }

            return new IntersectionCurve(pointsList, uvList0, uvList1);
        }

        private static Vector4 GetNewtonIterationPoint(IIntersect obj0, IIntersect obj1, Vector2 uv0, Vector2 uv1, Vector2 uvNew0, Vector2 uvNew1, float alpha)
        {
            var mat = GetJacobi(obj0, obj1, uv0, uv1, uvNew0, uvNew1);
            var vec = GetF(obj0, obj1, uv0, uv1, uvNew0, uvNew1, alpha);
            return vec.Multiply(mat);
        }
        public static Matrix4x4 GetJacobi(IIntersect obj0, IIntersect obj1, Vector2 uv0, Vector2 uv1, Vector2 uvNew0, Vector2 uvNew1)
        {
            var dU0 = obj0.EvaluateDU(uv0);
            var dV0 = obj0.EvaluateDV(uv0);
            var dU1 = obj1.EvaluateDU(uv1);
            var dV1 = obj1.EvaluateDV(uv1);

            var normalT = GetTNormal(dU0, dU1, dV0, dV1);
            dU0 = obj0.EvaluateDU(uvNew0);
            dV0 = obj0.EvaluateDV(uvNew0);
            dU1 = -obj1.EvaluateDU(uvNew1);
            dV1 = -obj1.EvaluateDV(uvNew1);

            var dot1 = Vector3.Dot(dU0, normalT);
            var dot2 = Vector3.Dot(dV0, normalT);

            var jacobiMatrix = new Matrix4x4(
                dU0.X, dV0.X, dU1.X, dV1.X,
                dU0.Y, dV0.Y, dU1.Y, dV1.Y,
                dU0.Z, dV0.Z, dU1.Z, dV1.Z,
                dot1, dot2, 0, 0);

            Matrix4x4.Invert(jacobiMatrix, out Matrix4x4 inv);
            return inv;
        }
        public static Vector4 GetF(IIntersect obj0, IIntersect obj1, Vector2 uv0, Vector2 uv1, Vector2 uvNew0, Vector2 uvNew1, float alpha)
        {
            var P0 = obj0.Evaluate(uv0);
            var Q = obj1.Evaluate(uvNew1);
            var P1 = obj0.Evaluate(uvNew0);

            var dU0 = obj0.EvaluateDU(uv0);
            var dV0 = obj0.EvaluateDV(uv0);
            var dU1 = obj1.EvaluateDU(uv1);
            var dV1 = obj1.EvaluateDV(uv1);

            var normalT = GetTNormal(dU0, dU1, dV0, dV1);
            var d = alpha * 10;

            return new Vector4(P1 - Q, Vector3.Dot(P1 - P0, normalT) - d);
        }
        public static Vector3 GetTNormal(Vector3 du0, Vector3 du1, Vector3 dv0, Vector3 dv1)
        {
            var np = Vector3.Normalize(Vector3.Cross(du0, dv0));
            var nq = Vector3.Normalize(Vector3.Cross(du1, dv1));
            var normalT = Vector3.Cross(np, nq);
            return Vector3.Normalize(normalT);
        }

        private static UpdateUVStruct UpdateUV(IIntersect obj, Vector2 uv, Vector2 uvDiff, bool backed)
        {
            var backThisTime = false;
            var end = false;

            float _uNew = uv.X - uvDiff.X;
            float _vNew = uv.Y - uvDiff.Y;

            if (_uNew < 0)
            {
                if (obj.WrappedU)
                {
                    _uNew = 1;
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
            }
            else if (_uNew > 1)
            {
                if (obj.WrappedU)
                {
                    _uNew = 0;
                }
                else
                {
                    _uNew = 1;
                    if (backed)
                    {
                        end = true;
                    }
                    else
                    {
                        backThisTime = true;
                    }
                }
            }

            if (_vNew > 1)
            {
                if (obj.WrappedV)
                {
                    _vNew = 0;
                }
                else
                {
                    _vNew = 1;
                    if (backed)
                    {
                        end = true;
                    }
                    else
                    {
                        backThisTime = true;
                    }
                }
            }
            else if (_vNew < 0)
            {
                if (obj.WrappedV)
                {
                    _vNew = 1;
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
            }

            return new UpdateUVStruct()
            {
                UV = new Vector2(_uNew, _vNew),
                End = end,
                Back = backThisTime,
            };
        }
    }
}