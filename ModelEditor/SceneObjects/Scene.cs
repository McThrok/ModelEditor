using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ModelEditor
{
    public class Scene : SceneObject
    {
        public Camera Camera { get; private set; }
        public Cursor Cursor { get; private set; }
        public RayCaster RayCaster { get; set; }

        private Random _rd = new Random();

        public Scene()
        {
            Name = "Scene";
            Holdable = false;
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

        internal void Init(RayCaster rayCaster)
        {
            RayCaster = rayCaster;
            Camera = AddObj(new Camera(), this);
            ResetCamera();
            Cursor = AddObj(new Cursor(RayCaster), this);
        }

        public SceneObject AddEmptyObject(SceneObject parent)
        {
            return AddObj(new EmptyObject(), parent);
        }
        public SceneObject AddBezierCurveC0(SceneObject parent)
        {
            return AddObj(new BezierCurveC0(RayCaster), parent);
        }
        public SceneObject AddBezierCurveC2(SceneObject parent)
        {
            return AddObj(new BezierCurveC2(RayCaster), parent);
        }
        public SceneObject AddInterpolatingCurve(SceneObject parent)
        {
            return AddObj(new InterpolatingCurve(RayCaster), parent);
        }
        public SceneObject AddBezierSurfaceC0(SceneObject parent)
        {
            return AddObj(new BezierSurfaceC0(RayCaster), parent);
        }
        public SceneObject AddBezierCylinderC0(SceneObject parent)
        {
            return AddObj(new BezierCylinderC0(RayCaster), parent);
        }
        public SceneObject AddBezierSurfaceC2(SceneObject parent)
        {
            return AddObj(new BezierSurfaceC2(RayCaster), parent);
        }
        public SceneObject AddBezierCylinderC2(SceneObject parent)
        {
            return AddObj(new BezierCylinderC2(RayCaster), parent);
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

        private SceneObject _selectedObject;
        public SceneObject SelectedObject
        {
            get => _selectedObject;
            set
            {
                var invoke = (value != null && _selectedObject == null)
                    || (value == null && _selectedObject != null)
                    || (value != null && _selectedObject != null && value.Id != _selectedObject.Id);

                if (invoke)
                {
                    _selectedObject = value;
                    InvokePropertyChanged(nameof(SelectedObject));
                }
            }
        }

    }
}


