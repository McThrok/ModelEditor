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
        public Camera Camera { get; private set; }
        public Cursor Cursor { get; private set; }
        public RayCaster RayCaster { get; set; }
        public SceneObject SelectedObject { get; set; }

        private Random _rd = new Random();

        public Scene()
        {
            Name = "Scene";
            Holdable = false;

            Camera = AddObj(new Camera(), this);
            ResetCamera();
            Cursor = AddObj(new Cursor(), this);
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
            return AddObj(new EmptyObject(), parent);
        }
        public SceneObject AddBezierCurve(SceneObject parent)
        {
            var bezier = new BezierCurveC2(RayCaster);
            //var bezier = new BezierCurveC0(RayCaster);
            return AddObj(bezier, parent);
        }
        private T AddObj<T>(T obj, SceneObject parent) where T : SceneObject
        {
            if (parent == null)
                parent = this;

            if (Cursor != null)
                obj.GlobalMatrix = Cursor.GlobalMatrix;

            obj.SetParent(parent);

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

        public void FlatDelete(SceneObject obj)
        {
            if (obj == null || obj.Parent == null)
                return;

            if (obj.Id == Camera.Id || obj.Id == Cursor.Id)
                return;

            foreach (var child in obj.Children.ToList())
                child.SetParent(obj.Parent);

            obj.Parent.Children.Remove(obj);
        }
        public void Delete(SceneObject obj)
        {
            if (obj == null || obj.Parent == null)
                return;

            if (obj.Id == Camera.Id || obj.Id == Cursor.Id)
                return;

            obj.Parent.Children.Remove(obj);
        }
        public void ResetCamera()
        {
            Camera.Matrix = Matrix4x4.Identity;
            Camera.SetTarget(Vector3.Zero);

        }
    }
}


