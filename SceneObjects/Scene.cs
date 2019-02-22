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
        public ManipObj Camera { get; private set; }
        public List<RenderableObj> Objects { get; private set; }

        public Scene()
        {
            Camera = new ManipObj();
            Objects = new List<RenderableObj>();
        }
    }
}


