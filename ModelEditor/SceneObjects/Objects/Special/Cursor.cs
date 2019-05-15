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
    public class Cursor : SceneObject, IRenderableObj, IScreenRenderable
    {
        public Int32Rect? SelectionRect { get; set; } = null;
        public float Tolerance { get; set; } = 3;
        public HashSet<SceneObject> HeldObjects { get; set; } = new HashSet<SceneObject>();
        protected readonly RayCaster _rayCaster;

        public Cursor(RayCaster rayCaster)
        {
            Name = nameof(Cursor);
            _rayCaster = rayCaster;
            GlobalMatrixChange += MoveHeldObjects;
            Holdable = false;
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

            var linked = new List<Vertex>();
            foreach (var obj in HeldObjects)
            {
                if(obj is Vertex vert)
                {
                    if (linked.Contains(vert))
                        continue;

                    if (vert.Parent is BezierSurfaceC0 surf && surf.LinkedVertices.ContainsKey(vert))
                        linked.Add(surf.LinkedVertices[vert]);
                }

                obj.GlobalMatrix *= change;
            }
        }
        public void HoldClosestObject(SceneObject sceneObject)
        {
            Stack<SceneObject> toCheck = new Stack<SceneObject>();
            toCheck.Push(sceneObject);
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

                foreach (var child in obj.HiddenChildren)
                    toCheck.Push(child);
            }

            if (toHeld != null)
                HeldObjects.Add(toHeld);
        }
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

        public ScreenRenderData GetScreenRenderData()
        {
            var data = new ScreenRenderData();

            if (SelectionRect == null)
                return data;

            var position = GlobalMatrix.Translation;
            var rect = FixedRect.Value;
            rect.X -= _rayCaster.BitmapWidth / 2 ;
            rect.Y -= _rayCaster.BitmapHeight / 2;

            for (int x = 0; x < rect.Width; x++)
            {
                data.CameraPixels.Add(new Vector2Int(rect.X + x, rect.Y));
                data.CameraPixels.Add(new Vector2Int(rect.X + x, rect.Y + rect.Height - 1));
            }

            for (int y = 0; y < rect.Height; y++)
            {
                data.CameraPixels.Add(new Vector2Int(rect.X, rect.Y + y));
                data.CameraPixels.Add(new Vector2Int(rect.X + rect.Width - 1, rect.Y + y));
            }

            return data;
        }

        public void HoldObject(SceneObject sceneObj)
        {
            if (sceneObj == null || !sceneObj.Holdable)
                return;

            HeldObjects.Add(sceneObj);
        }

        public void HoldObjectsInRect(SceneObject sceneObj)
        {
            var toCheck = new Stack<SceneObject>();
            toCheck.Push(sceneObj);

            while (toCheck.Count > 0)
            {
                var obj = toCheck.Pop();
                if (CanBeSelected(obj))
                {
                    HeldObjects.Add(obj);
                }
                else
                {
                    foreach (var item in obj.Children)
                        toCheck.Push(item);

                    foreach (var item in obj.HiddenChildren)
                        toCheck.Push(item);
                }
            }

        }
        private bool CanBeSelected(SceneObject obj)
        {
            if (!obj.Holdable)
                return false;

            var pos = _rayCaster.GetScreenPositionOf(obj);

            var rect = FixedRect.Value;
            var result = rect.X < pos.X && pos.X < rect.X + rect.Width && rect.Y < pos.Y && pos.Y < rect.Y + rect.Height;

            return result;
        }
        public Int32Rect? FixedRect
        {
            get
            {
                if (SelectionRect == null)
                    return null;

                var rect = SelectionRect.Value;

                if (rect.Height < 0)
                {
                    rect.Y += rect.Height;
                    rect.Height *= -1;
                }

                if (rect.Width < 0)
                {
                    rect.X += rect.Width;
                    rect.Width *= -1;
                }

                return rect;
            }
        }
    }
}
