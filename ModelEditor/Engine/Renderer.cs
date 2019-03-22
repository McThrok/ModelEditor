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
        public Color Color { get; set; }
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
        private Color _drawColor = Colors.Green;
        private Color _drawLeftColor = Colors.Red;
        private Color _drawRightColor = Colors.Cyan;
        private float _aspect;
        private float _fov = 0.8f;
        private float _near = 0.1f;
        private float _far = 100f;

        public Renderer(WriteableBitmap wb, Scene scene)
        {
            _wb = wb;
            _scene = scene;
            _aspect = _wb.PixelWidth / _wb.PixelHeight;

            _scene.Cursor.GlobalMatrixChange += UpdateCursorScreenPosition;
            _scene.Camera.GlobalMatrixChange += UpdateCursorScreenPosition;

            UpdateCursorScreenPosition(this, new ChangeMatrixEventArgs(_scene.Cursor.GlobalMatrix, _scene.Cursor.GlobalMatrix));
        }

        private void UpdateCursorScreenPosition(object sender, ChangeMatrixEventArgs e)
        {
            var cursor = _scene.Cursor;
            var projection = MyMatrix4x4.CreatePerspectiveFieldOfView(0.8f, 1.0f * _aspect, 0.1f, 100);
            var view = _scene.Camera.GlobalMatrix.Inversed();

            var matrix = MyMatrix4x4.Compose(projection, view, cursor.GlobalMatrix);

            var center = matrix.Multiply(new Vector4(0, 0, 0, 1));
            if (center.Z < 0)
            {
                cursor.ScreenPosition = new Vector2Int(-1, -1);
                return;
            }

            var V = new Point(center.X / center.W, center.Y / center.W);

            var width = _wb.PixelWidth;
            var height = _wb.PixelHeight;

            var x = Convert.ToInt32((V.X + 1) / 2 * width);
            var y = Convert.ToInt32((1 - (V.Y + 1) / 2) * height);

            if (x > 0 && x < width && y > 0 && y < height)
                cursor.ScreenPosition = new Vector2Int(x, y);
            else
                cursor.ScreenPosition = new Vector2Int(-1, -1);
        }

        public void RenderFrame()
        {
            _wb.Clear(Colors.Black);
            if (Anaglyphic)
            {
                var projLeft = MyMatrix4x4.CreateAnaglyphicPerspectiveFieldOfView(0.8f, 1.0f * _aspect, 0.1f, 100.0f, EyeDistance / 2, ViewportDistance);
                Render(projLeft, _drawLeftColor, false);

                var projRight = MyMatrix4x4.CreateAnaglyphicPerspectiveFieldOfView(0.8f, 1.0f * _aspect, 0.1f, 100.0f, -EyeDistance / 2, ViewportDistance);
                Render(projRight, _drawRightColor, true);
            }
            else
            {
                var projection = MyMatrix4x4.CreatePerspectiveFieldOfView(0.8f, 1.0f * _aspect, 0.1f, 100);
                Render(projection, _drawColor, false);
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
                    Color = color,
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

            if (obj is IRenderableObj renderableObj)
            {
                Render(renderableObj, model, frameData);
            }

            if (obj is IScreenRenderable screenRenderable)
            {
                Render(screenRenderable, model, frameData);
            }

            foreach (var child in obj.Children)
            {
                RenderRec(child, model, frameData);
            }
        }
        private void Render(IRenderableObj obj, Matrix4x4 model, RenderFrameData frameData)
        {
            var data = obj.GetRenderData();
            var matrix = MyMatrix4x4.Compose(frameData.ProjMatrix, frameData.View, model);
            foreach (var edge in data.Edges)
            {
                var vertA = matrix.Multiply(data.Vertices[edge.IdxA].ToVector4());
                var vertB = matrix.Multiply(data.Vertices[edge.IdxB].ToVector4());

                if (vertA.Z > 0 && vertB.Z > 0)
                    DrawLine(frameData.Context, vertA, vertB, frameData.Color, frameData.AddColors);
            }
        }
        private void Render(IScreenRenderable obj, Matrix4x4 model, RenderFrameData frameData)
        {
            var data = obj.GetScreenRenderData();
            var matrix = MyMatrix4x4.Compose(frameData.ProjMatrix, frameData.View, model);

            if (!data.UsePositionOffsets)
            {
                var center = matrix.Multiply(new Vector4(0, 0, 0, 1));
                if (center.Z > 0)
                {
                    foreach (var pix in data.Pixels)
                    {
                        DrawOnScren(frameData.Context, center, pix, frameData.Color, frameData.AddColors);
                    }
                }
            }
            else
            {
                for (int i = 0; i < data.Pixels.Count; i++)
                {
                    var pix = data.Pixels[i];
                    var offset = data.PositionOffsets[i];
                    var position = matrix.Multiply(offset.ToVector4());
                    if (position.Z > 0)
                    {
                        DrawOnScren(frameData.Context, position, pix, frameData.Color, frameData.AddColors);
                    }
                }

            }
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
