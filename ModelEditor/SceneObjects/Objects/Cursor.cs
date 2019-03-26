using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.ComponentModel;

namespace ModelEditor
{
    public class Cursor : SceneObject, IRenderableObj
    {
        public float Tolerance { get; set; } = 3;
        public List<SceneObject> HeldObjects { get; set; } = new List<SceneObject>();

        public Cursor()
        {
            Name = nameof(Cursor);
            GlobalMatrixChange += MoveHeldObjects;
        }

        public void SetTarget(Vector3 position)
        {
            var pos = GlobalMatrix.Inversed().Multiply(position.ToVector4());
            MoveLoc(pos.ToVector3());
        }
        public void SetTarget(SceneObject obj)
        {
            var pos = obj.GlobalMatrix.Multiply(Vector3.Zero.ToVector4());
            SetTarget(pos.ToVector3());
        }

        private void MoveHeldObjects(object sender, ChangeMatrixEventArgs e)
        {
            if (HeldObjects.Count == 0)
                return;

            var change = e.NewMatrix * e.OldMatrix.Inversed();

            foreach (var obj in HeldObjects)
                obj.GlobalMatrix *= change;
        }
        public void HoldObject(IEnumerable<SceneObject> objs)
        {
            ReleaseObjects();

            Stack<SceneObject> toCheck = new Stack<SceneObject>(objs);
            float best = float.MaxValue;
            SceneObject toHeld = null;

            while (toCheck.Count > 0)
            {
                var obj = toCheck.Pop();

                if (CanBeHeld(obj, out float dist) && dist < best)
                {
                    best = dist;
                    toHeld = obj;
                }

                foreach (var child in obj.Children)
                    toCheck.Push(child);
            }

            if (toHeld != null)
                HeldObjects.Add(toHeld);
        }
        //public void HoldAllObjects(IEnumerable<SceneObject> objs)
        //{
        //    ReleaseObjects();

        //    foreach (var obj in objs)
        //    {
        //        if (CanBeHeld(obj, out float dist))
        //            HeldObjects.Add(obj);
        //        else
        //            HoldAllObjects(obj.Children);
        //    }
        //}
        private bool CanBeHeld(SceneObject obj, out float distance)
        {
            distance = 0;
            if (!obj.Holdable)
                return false;

            var cursorPos = GlobalMatrix.Multiply(Vector3.Zero.ToVector4()).ToVector3();
            var objPos = obj.GlobalMatrix.Multiply(Vector3.Zero.ToVector4()).ToVector3();

            distance = Vector3.Distance(cursorPos, objPos);
            return distance <= Tolerance;
        }
        public void ReleaseObjects()
        {
            HeldObjects.Clear();
        }


        private Vector2Int _screenPosition;
        public Vector2Int ScreenPosition
        {
            get => _screenPosition;
            set
            {
                if (_screenPosition != value)
                {
                    _screenPosition = value;
                    this.InvokePropertyChanged(nameof(ScreenPosition));
                }
            }
        }

        public ObjRenderData GetRenderData()
        {
            var renderData = new ObjRenderData();
            renderData.Vertices = GetVertices();
            renderData.Edges = GetEdges();

            return renderData;
        }
        private List<Vector3> GetVertices()
        {
            var vertices = new List<Vector3>();
            vertices.Add(new Vector3(0, 0, 0));
            vertices.Add(new Vector3(0.3f, 0, 0));
            vertices.Add(new Vector3(0, 0.2f, 0));
            vertices.Add(new Vector3(0, 0, 0.1f));

            return vertices;
        }
        private List<Edge> GetEdges()
        {
            var edges = new List<Edge>();
            edges.Add(new Edge(0, 1));
            edges.Add(new Edge(0, 2));
            edges.Add(new Edge(0, 3));

            return edges;
        }
    }
}
