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
    public interface IRenderableObj
    {
        ObjRenderData GetRenderData();
    }

    public class ObjRenderData
    {
        public Matrix4x4 Matrix { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<Edge> Edges { get; set; }
    }

    public interface IScreenRenderable
    {
        ScreenRenderData GetRenderData();
    }

    public class ScreenRenderData
    {
        public List<Vector2> Pixels { get; set; }
    }

}
