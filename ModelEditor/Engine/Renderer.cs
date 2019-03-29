using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Windows.Media;

namespace ModelEditor
{
    public class RenderFrameData
    {
        public BitmapContext Context { get; set; }
        public Matrix4x4 ProjMatrix { get; set; }
        public Matrix4x4 View { get; set; }
        public Color DefaultColor { get; set; }
        public bool AddColors { get; set; }
    }

    public class RenderAccessor
    {
        private readonly Renderer _renderer;
        public RenderAccessor(Renderer renderer)
        {
            _renderer = renderer;
        }

        public Matrix4x4 GetViewMatrix()
        {
            return _renderer.GetViewMatrix();
        }
        public Matrix4x4 GetProjectionMatrix()
        {
            return _renderer.GetProjectionMatrix();
        }
        public int BitmapWidth => _renderer.BitmapWidth;
        public int BitmapHeight => _renderer.BitmapHeight;
    }

    public class Renderer
    {
        public bool Anaglyphic { get; set; }
        public float EyeDistance { get; set; }
        public float ViewportDistance { get; set; }

        private WriteableBitmap _wb;
        private Scene _scene;

        private Color _drawColor = Colors.White;
        private Color _drawLeftColor = Colors.Red;
        private Color _drawRightColor = Colors.Cyan;
        private Color _selectedColor = Colors.Yellow;
        private Color _heldColor = Colors.Blue;
        private Color _selectedHeldColor = Colors.Green;
        private float _aspect;
        private float _fov = 1.4f;
        private float _near = 0.1f;
        private float _far = 100f;

        public Renderer(WriteableBitmap wb, Scene scene)
        {
            _wb = wb;
            _scene = scene;
            _aspect = 1f * _wb.PixelWidth / _wb.PixelHeight;
        }

        public void RenderFrame()
        {
            _wb.Clear(Colors.Black);
            if (Anaglyphic)
            {
                Render(GetLeftAnaglyphProjectionMatrix(), _drawLeftColor, false);
                Render(GetRightAnaglyphProjectionMatrix(), _drawRightColor, true);
            }
            else
            {
                Render(GetProjectionMatrix(), _drawColor, false);
            }
        }
        private void Render(Matrix4x4 projMatrix, Color color, bool addColors)
        {
            var view = GetViewMatrix();

            using (var context = _wb.GetBitmapContext())
            {
                var frameData = new RenderFrameData()
                {
                    AddColors = addColors,
                    DefaultColor = color,
                    ProjMatrix = projMatrix,
                    View = view,
                    Context = context
                };

                RenderRec(_scene, Matrix4x4.Identity, frameData);
            }
        }
        private void RenderRec(SceneObject obj, Matrix4x4 parentMatrix, RenderFrameData frameData)
        {
            var model = obj.Matrix * parentMatrix;
            var color = GetColor(obj, frameData.DefaultColor);

            if (obj is IRenderableObj renderableObj)
            {
                Render(renderableObj, model, frameData, color);
            }

            if (obj is IScreenRenderable screenRenderable)
            {
                Render(screenRenderable, model, frameData, color);
            }

            foreach (var child in obj.Children)
            {
                RenderRec(child, model, frameData);
            }
            
            foreach (var child in obj.HiddenChildren)
            {
                RenderRec(child, model, frameData);
            }
        }
        private void Render(IRenderableObj obj, Matrix4x4 model, RenderFrameData frameData, Color color)
        {
            var data = obj.GetRenderData();
            var matrix = MyMatrix4x4.Compose(frameData.ProjMatrix, frameData.View, model);

            foreach (var vert in data.Vertices)
            {
                var v = matrix.Multiply(vert.ToVector4());

                if (v.Z > 0)
                    DrawVertex(frameData.Context, v, color, frameData.AddColors);
            }

            foreach (var edge in data.Edges)
            {
                var vertA = matrix.Multiply(data.Vertices[edge.IdxA].ToVector4());
                var vertB = matrix.Multiply(data.Vertices[edge.IdxB].ToVector4());

                if (vertA.Z > 0 && vertB.Z > 0)
                    DrawLine(frameData.Context, vertA, vertB, color, frameData.AddColors);
            }
        }
        private void Render(IScreenRenderable obj, Matrix4x4 model, RenderFrameData frameData, Color color)
        {
            var data = obj.GetScreenRenderData();
            var matrix = MyMatrix4x4.Compose(frameData.ProjMatrix, frameData.View, model);

            var center = matrix.Multiply(new Vector4(0, 0, 0, 1));
            if (center.Z > 0)
            {
                foreach (var pix in data.Pixels)
                {
                    DrawOnScren(frameData.Context, center, pix, color, frameData.AddColors);
                }
            }
        }

        private Color GetColor(SceneObject obj, Color defColor)
        {
            var col = defColor;

            if (!Anaglyphic)
            {
                var selected = _scene.SelectedObject != null && obj.Id == _scene.SelectedObject.Id;
                var held = _scene.Cursor.HeldObjects.Contains(obj);

                if (selected && !held)
                    col = _selectedColor;

                if (!selected && held)
                    col = _heldColor;

                if (selected && held)
                    col = _selectedHeldColor;
            }

            return col;
        }

        private void DrawOnScren(BitmapContext ctx, Vector4 center, Vector2Int pix, Color col, bool addColors)
        {
            var V = new Point(center.X / center.W, center.Y / center.W);

            var width = _wb.PixelWidth;
            var height = _wb.PixelHeight;

            var x = Convert.ToInt32((V.X + 1) / 2 * width) + pix.X;
            var y = Convert.ToInt32((1 - (V.Y + 1) / 2) * height) + pix.Y;

            if (x > 0 && x < width && y > 0 && y < height)
                ctx.MySetPixel(x, y, col, addColors);
        }
        private void DrawLine(BitmapContext ctx, Vector4 vertA, Vector4 vertB, Color col, bool addColors)
        {
            var A = new Point(vertA.X / vertA.W, vertA.Y / vertA.W);
            var B = new Point(vertB.X / vertB.W, vertB.Y / vertB.W);


            var width = _wb.PixelWidth;
            var height = _wb.PixelHeight;

            var x1 = Convert.ToInt32((A.X + 1) / 2 * width);
            var y1 = Convert.ToInt32((1 - (A.Y + 1) / 2) * height);
            var x2 = Convert.ToInt32((B.X + 1) / 2 * width);
            var y2 = Convert.ToInt32((1 - (B.Y + 1) / 2) * height);

            ctx.MyDrawLine(x1, y1, x2, y2, col, addColors);
        }
        private void DrawVertex(BitmapContext ctx, Vector4 vert, Color col, bool addColors)
        {
            var v = new Point(vert.X / vert.W, vert.Y / vert.W);

            var width = _wb.PixelWidth;
            var height = _wb.PixelHeight;

            var x = Convert.ToInt32((v.X + 1) / 2 * width);
            var y = Convert.ToInt32((1 - (v.Y + 1) / 2) * height);

            if (x > 0 && x < width && y > 0 && y < height)
                ctx.MySetPixel(x, y, col, addColors);
        }

        public Matrix4x4 GetViewMatrix()
        {
            return _scene.Camera.GlobalMatrix.Inversed();
        }
        public Matrix4x4 GetProjectionMatrix()
        {
            return MyMatrix4x4.CreatePerspectiveFieldOfView(_fov, _aspect, _near, _far);
        }
        public Matrix4x4 GetLeftAnaglyphProjectionMatrix()
        {
            return MyMatrix4x4.CreateAnaglyphicPerspectiveFieldOfView(_fov, _aspect, _near, _far, EyeDistance / 2, ViewportDistance);
        }
        public Matrix4x4 GetRightAnaglyphProjectionMatrix()
        {
            return MyMatrix4x4.CreateAnaglyphicPerspectiveFieldOfView(_fov, _aspect, _near, _far, -EyeDistance / 2, ViewportDistance);
        }
        public int BitmapWidth => _wb.PixelWidth;
        public int BitmapHeight => _wb.PixelHeight;

        public RenderAccessor GetRenderAccessor()
        {
            return new RenderAccessor(this);
        }
    }
}
