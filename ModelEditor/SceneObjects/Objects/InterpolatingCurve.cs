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
    public class InterpolatingCurve : SceneObject, IRenderableObj
    {
        private static int _count = 0;
        public InterpolatingCurve()
        {
            Name = nameof(InterpolatingCurve) + " " + _count++.ToString();
        }

        public ObjRenderData GetRenderData()
        {
            return new ObjRenderData();
        }
    }
}