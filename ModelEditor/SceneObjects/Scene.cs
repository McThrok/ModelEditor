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
    public class Scene : SceneObject
    {
        public SceneObject Camera { get; private set; }
        public List<RenderableObj> Objects { get; private set; }
        public ObservableCollection<SceneObject> MainpObjects { get; private set; }

        public Scene()
        {
            Camera = new SceneObject();
            Objects = new List<RenderableObj>();
            MainpObjects = new ObservableCollection<SceneObject>();

        }
    }
}


