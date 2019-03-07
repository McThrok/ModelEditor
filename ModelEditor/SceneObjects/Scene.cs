using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Collections.ObjectModel;

namespace ModelEditor
{
    public class Scene : ManipObj
    {
        public ManipObj Camera { get; private set; }
        public Light Light { get; private set; }
        public List<RenderableObj> Objects { get; private set; }
        public ObservableCollection<ManipObj> MainpObjects { get; private set; }

        public Scene()
        {
            Camera = new ManipObj();
            Objects = new List<RenderableObj>();
            MainpObjects = new ObservableCollection<ManipObj>();

            Light = new Light();
            Light.Name = nameof(Light);
        }
    }
}


