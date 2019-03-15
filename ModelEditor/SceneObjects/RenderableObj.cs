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
        RenderData GetRenderData();
    }

    public class RenderData
    {
        public Matrix4x4 Matrix { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<Edge> Edges { get; set; }
    }

}
