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
        public float TrimPrecision { get; set; }
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

            var a = (BezierSurfaceC0)AddBezierSurfaceC0(this);
            a.Height = 6;
            a.Width = 6;
            a.Rotate( Math.PI / 2, 0, 0);
            //a.Scale(0.1, 0.1, 0.1);

            var b = (BezierSurfaceC0)AddBezierSurfaceC0(this);
            //b.HeightPatchCount = 2;
            b.WidthPatchCount = 2;
            b.Height = 6;
            b.Width = 6;
            //b.Scale(0.1, 0.1, 0.1);
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
        public SceneObject AddGregoryPatfch(BezierSurfaceC0 surfA, BezierSurfaceC0 surfB, BezierSurfaceC0 surfC)
        {
            var verts = BezierSurfaceC0.CheckGregory(surfA, surfB, surfC);
            if (verts == null)
                return null;

            var data = new List<GregoryEdgeData>();
            data.Add(new GregoryEdgeData() { Surface = surfA, A = verts[0], B = verts[1] });
            data.Add(new GregoryEdgeData() { Surface = surfB, A = verts[2], B = verts[3] });
            data.Add(new GregoryEdgeData() { Surface = surfC, A = verts[4], B = verts[5] });

            var greg = new GregoryPatch(data);
            greg.SetParent(this);

            return greg;
        }

        public void LoadModel(string[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                string[] header = data[i].Split(' ');
                string elementName = header[0];
                int n = int.Parse(header[1]);
                for (int j = 0; j < n; j++)
                {
                    i += 1;
                    SceneObject obj = null;
                    var d = data[i + j];
                    //var d = data[i + j].Replace('.', ',');
                    switch (elementName)
                    {
                        case "curveC0": obj = new BezierCurveC0(RayCaster, d); break;
                        case "curveC2": obj = new BezierCurveC2(RayCaster, d); break;
                        case "curveInt": obj = new InterpolatingCurve(RayCaster, d); break;
                        case "surfaceC0": obj = new BezierSurfaceC0(RayCaster, d); break;
                        case "surfaceC2": obj = new BezierSurfaceC2(RayCaster, d); break;
                        case "tubeC0": obj = new BezierCylinderC0(RayCaster, d); break;
                        case "tubeC2": obj = new BezierCylinderC2(RayCaster, d); break;
                        case "point": obj = new Vertex(d); break;
                        default: break;
                    }

                    if (obj != null)
                        obj.SetParent(this);
                }
            }
        }

        public string[] GetSaveData()
        {
            List<string> data = new List<string>();

            foreach (var child in Children)
            {
                var d = child.GetData();
                for (int i = 0; i < d.Length; i++)
                {
                    //d[i].Replace(',', '.');
                }
                data.AddRange(d);
            }

            return data.ToArray();
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


