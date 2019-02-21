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
    public class Scene : ManipObj
    {
        public Matrix4x4 Camera { get; private set; }
        public List<RenderableObj> Objects { get; private set; }

        public Scene()
        {
            Camera = MyMatrix4x4.Translate(new Vector3(0, 0, 10));
            Objects = new List<RenderableObj>();
        }
    }
}


