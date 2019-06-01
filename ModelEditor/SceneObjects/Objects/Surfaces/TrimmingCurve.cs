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
        float Width { get; }
        float Height { get; }
        Guid Id { get; }
        Vector3 Evaluate(float h, float w);
        Vector3 evaluateDU(float h, float w);
        Vector3 evaluateDV(float h, float w);
    }
    public class TrimmingCurve : SceneObject, IRenderableObj
    {
        private static int _count = 0;

        public TrimmingCurve()
        {
            Name = nameof(TrimmingCurve) + " " + _count++.ToString();
        }

        public ObjRenderData GetRenderData()
        {
            return new ObjRenderData();
        }

        public static TrimmingCurve FindTrimmingCurve(List<TrimmingSurface> objs, float trimPrecision, Vector3 cursorPos)
        {
            float bestLen = 1000;
            Vector2 p0 = Vector2.Zero;
            Vector2 p1 = Vector2.Zero;

            int divCount = 10;
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

        private static float intersectionEpsilon = 0.001f;
        private static int intersectionStep = 3;

        private static TrimmingCurve CountGradientMethod(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 value0, Vector2 value1)
        {
            var u = new Vector2(value0.X, value1.X);
            var v = new Vector2(value0.Y, value1.Y);
            var p0 = obj0.Evaluate(value0.X, value0.Y);
            var p1 = obj1.Evaluate(value1.X, value1.Y);

            Vector2 point0 = Vector2.Zero;
            Vector2 point1 = Vector2.Zero;
            var i = 0;
            while (Vector3.Distance(p1, p0) > intersectionEpsilon)
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
                    //alert("There is no intersection. Try to put cursor in other place." + e);
                    return null;
                }

                betterPoint *= intersectionStep;
                p0 = obj0.Evaluate(u.X, v.X);
                p1 = obj1.Evaluate(u.Y, v.Y);
                Vector2 uPrev = u;
                Vector2 vPrev = v;

                u = new Vector2(uPrev.X - betterPoint.X, uPrev.Y - betterPoint.Y);
                v = new Vector2(vPrev.X - betterPoint.Y, vPrev.Y - betterPoint.Z);

            }

            return goGoNewton(obj0, obj1, u, v);
        }

        private static Vector4 GetGradient(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 point0, Vector2 point1)
        {
            var eval1 = obj0.Evaluate(point0.X, point0.Y);
            var eval2 = obj1.Evaluate(point1.X, point1.Y);

            var diff = eval1 - eval2;

            var eval0u = obj0.evaluateDU(point0.X, point0.Y);
            var eval0v = obj0.evaluateDV(point0.X, point0.Y);

            var eval1u = obj1.evaluateDU(point1.X, point1.Y);
            var eval1v = obj1.evaluateDV(point1.X, point1.Y);

           return new Vector4(
            Vector3.Dot(diff, eval0u) * 2,
            Vector3.Dot(diff, eval0v) * 2,
            Vector3.Dot(diff, eval1u) * -2,
            Vector3.Dot(diff, eval1v) * -2
            );
        }

        private static TrimmingCurve goGoNewton(TrimmingSurface obj0, TrimmingSurface obj1, Vector2 u, Vector2 v)
        {
            throw new NotImplementedException();
        }
    }
}