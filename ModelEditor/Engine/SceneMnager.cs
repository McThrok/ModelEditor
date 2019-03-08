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
    public class SceneMnager
    {
        public Scene Scene { get; set; } = new Scene();

        private Random _rd = new Random();

        public SceneMnager()
        {
            InitScene();
        }
        private void InitScene()
        {
            Scene.Camera.Move(0, 0, 4);
            AddCube();
            //Scene.Light.Move(5, 0, 5);
            //Scene.MainpObjects.Add(Scene.Light);
        }
        public void AddCube()
        {
            var obj = new Cube();
            Scene.MainpObjects.Add(obj);
            Scene.Objects.Add(obj);
        }
        public void AddTorus()
        {
            var obj = new Torus();
            obj.Rotate(Math.PI / 2, 0,0);
            //obj.Move(GetRandomPosition());
            Scene.MainpObjects.Add(obj);
            Scene.Objects.Add(obj);
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


