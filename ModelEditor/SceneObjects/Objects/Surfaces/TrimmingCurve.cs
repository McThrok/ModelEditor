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
    }
}