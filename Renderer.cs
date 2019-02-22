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
        private WriteableBitmap _wb;
        private Scene _scene;
        private Color _drawColor = Colors.Green;

        public Renderer(WriteableBitmap wb, Scene scene)
        {
            _wb = wb;
            _scene = scene;
        }

        public void Render()
        {
            var projection = MyMatrix4x4.CreatePerspectiveFieldOfView(1.3f, 1.0f * _wb.PixelWidth / _wb.PixelHeight, 1f, 8.0f);
            var view = _scene.Camera.Matrix;

            foreach (var obj in _scene.Objects)
            {
                var vertices = obj.GetVertices();
                var matrix = MyMatrix4x4.Compose(projection, view, _scene.Matrix, obj.Matrix);
                foreach (var edge in obj.GetEdges())
                {
                    var vertA = matrix.Multiply(vertices[edge.IdxA].ToVector4());
                    var vertB = matrix.Multiply(vertices[edge.IdxB].ToVector4());
                    DrawLine(vertA, vertB);
                }
            }
        }

        private void DrawLine(Vector4 vertA, Vector4 vertB)
        {
            var A = (vertA / vertA.W).ToVector3();
            var B = (vertB / vertB.W).ToVector3();

            if (A != Vector3.Clamp(A, -1 * Vector3.One, Vector3.One)
                || B != Vector3.Clamp(B, -1 * Vector3.One, Vector3.One))
                return;

            var width = _wb.PixelWidth;
            var height = _wb.PixelHeight;

            var x1 = Convert.ToInt32((A.X + 1) / 2 * width);
            var y1 = Convert.ToInt32((A.Y + 1) / 2 * height);
            var x2 = Convert.ToInt32((B.X + 1) / 2 * width);
            var y2 = Convert.ToInt32((B.Y + 1) / 2 * height);

            _wb.DrawLine(x1, y1, x2, y2, _drawColor);
        }
    }
}
