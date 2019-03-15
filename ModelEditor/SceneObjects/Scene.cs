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
        public SceneObject Cursor { get; private set; }
        private Random _rd = new Random();

        public Scene()
        {
            Name = "Scene";

            InitCamera();
            InitCursor();

            AddVertex(this);
        }

        public void InitCamera()
        {
            Camera = new SceneObject();
            Camera.Name = nameof(Camera);
            Camera.Move(0, 0, 10);
            Camera.Parent = this;
            Children.Add(Camera);
        }

        public void InitCursor()
        {
            Cursor = new SceneObject();
            Cursor.Name = nameof(Cursor);
            Cursor.Parent = this;
            Children.Add(Cursor);
        }

        public SceneObject AddCube(SceneObject parent)
        {
            return AddObj(new Cube(), parent);
        }
        public SceneObject AddTorus(SceneObject parent)
        {
            return AddObj(new Torus(), parent);
        }
        public SceneObject AddVertex(SceneObject parent)
        {
            return AddObj(new Vertex(), parent);
        }
        public SceneObject AddEmptyObject(SceneObject parent)
        {
            return AddObj(new SceneObject() { Name = "Empty Object" }, parent);
        }
        private SceneObject AddObj(SceneObject obj, SceneObject parent)
        {
            if (parent == null)
                parent = this;

            obj.Parent = parent;
            parent.Children.Add(obj);

            return obj;
        }

        private Vector3 GetRandomPosition()
        {
            return new Vector3(GetRandomCoordinate(), GetRandomCoordinate(), GetRandomCoordinate());
        }
        private float GetRandomCoordinate()
        {
            double min = -10;
            double max = 10;
            var result = (float)(_rd.NextDouble() * (max - min) + min);

            return result;
        }
    }
}


