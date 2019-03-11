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

        public Renderer(WriteableBitmap wb, Scene scene)
        {
            _wb = wb;
            _scene = scene;
        }

        public void RenderFrame()
        {
            _wb.Clear(Colors.Black);
            if (Anaglyphic)
            {
                var projLeft = MyMatrix4x4.CreateAnaglyphicPerspectiveFieldOfView(0.8f, 1.0f * _wb.PixelWidth / _wb.PixelHeight, 0.1f, 100.0f, EyeDistance / 2, ViewportDistance);
                Render(projLeft, _drawLeftColor, false);

                var projRight = MyMatrix4x4.CreateAnaglyphicPerspectiveFieldOfView(0.8f, 1.0f * _wb.PixelWidth / _wb.PixelHeight, 0.1f, 100.0f, -EyeDistance / 2, ViewportDistance);
                Render(projRight, _drawRightColor, true);
            }
            else
            {
                var view = _scene.Camera.Matrix.Inversed();

                var projection = MyMatrix4x4.CreatePerspectiveFieldOfView(0.8f, 1.0f * _wb.PixelWidth / _wb.PixelHeight, 0.1f, 100);
                Render(projection, _drawColor, false);
            }
        }

        private void Render(Matrix4x4 projMatrix, Color color, bool addColors)
        {
            var view = _scene.Camera.Matrix.Inversed();

            using (var context = _wb.GetBitmapContext())
            {
                foreach (var obj in _scene.Objects)
                {
                    var data = obj.GetRenderData();
                    var matrix = MyMatrix4x4.Compose(projMatrix, view, _scene.Matrix, obj.Matrix);
                    foreach (var edge in data.Edges)
                    {
                        var vertA = matrix.Multiply(data.Vertices[edge.IdxA].ToVector4());
                        var vertB = matrix.Multiply(data.Vertices[edge.IdxB].ToVector4());

                        if (vertA.Z > 0 && vertB.Z > 0)
                            DrawLine(context, vertA, vertB, color, addColors);
                    }
                }
            }
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
    }
}
