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

                            var ev1 = evaluate(objs[0], jj, ii);
                            var ev2 = evaluate(objs[1], mm, kk);
                            var _lenght = Vector3.Distance(ev1, cursorPos) + Vector3.Distance(ev2, cursorPos);
                            if (_lenght < bestLen && (!sameObjects || (eps < Math.Abs(ii - kk) && eps < Math.Abs(jj - mm))))
                            {
                                p0 = new Vector2(jj, ii);
                                p0 = new Vector2(mm, kk);
                                bestLen = _lenght;
                            }
                        }
            return countGradientMethod(objs[0], objs[1], p1,p2);
        }

        private static TrimmingCurve countGradientMethod(TrimmingSurface trimmingSurface1, TrimmingSurface trimmingSurface2, Vector2 p1, object p2)
        {
            throw new NotImplementedException();
        }

        private static Vector3 evaluate(TrimmingSurface trimmingSurface, float jj, float ii)
        {
            throw new NotImplementedException();
        }
    }
}