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
        private Random _rd = new Random();

        public Scene()
        {
            AddCube();
            InitCamera();
        }

        public void InitCamera()
        {
            Camera = new SceneObject();
            Camera.Name = nameof(Camera);
            Camera.Move(0, 0, 4);
            Camera.Parent = this;
            Children.Add(Camera);
        }

        public void AddCube()
        {
            var obj = new Cube();
            obj.Parent = this;
            Children.Add(obj);
        }
        public void AddTorus()
        {
            var obj = new Torus();
            obj.Parent = this;
            Children.Add(obj);
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


