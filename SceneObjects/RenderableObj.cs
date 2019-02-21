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
    public abstract class RenderableObj : ManipObj
    {
        public abstract List<Edge> GetEdges();
        public abstract List<Vector3> GetVertices();
    }
}
