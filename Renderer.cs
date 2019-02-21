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
            var projection = MyMatrix4x4.Perspective();
            var view = _scene.Camera;

            foreach (var obj in _scene.Objects)
            {
                var vertices = obj.GetVertices();
                var matrix = projection * view * _scene.Matrix * obj.Matrix;

                foreach (var edge in obj.GetEdges())
                {
                    var vertA = matrix.Multiply(vertices[edge.idxA].ToVector4());
                    var vertB = matrix.Multiply(vertices[edge.idxB].ToVector4());
                    DrawLine(vertA, vertB);
                }
            }
        }

        private void DrawLine(Vector4 vert, Vector4 vertB)
        {
            var width = _wb.PixelWidth;
            var height = _wb.PixelHeight;

            var x1 = Convert.ToInt32((vert.X + 1) / 2 * width);
            var y1 = Convert.ToInt32((vert.Y + 1) / 2 * height);
            var x2 = Convert.ToInt32((vert.X + 1) / 2 * height);
            var y2 = Convert.ToInt32((vert.Y + 1) / 2 * width);

            _wb.DrawLine(x1, y1, x2, y2, _drawColor);
        }
    }
}
