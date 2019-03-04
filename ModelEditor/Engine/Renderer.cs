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

        public void RenderFrame()
        {
            _wb.Clear(Colors.Black);

            var elip = _scene.Elipsoid;
            var model = elip.Matrix;
            var view = _scene.Camera.Matrix.Inversed();
            var projection = MyMatrix4x4.CreatePerspectiveFieldOfView(1.3f, 1.0f * _wb.PixelWidth / _wb.PixelHeight, 0.1f, 10000.0f);

            var invMat = MyMatrix4x4.Compose(projection, view, model).Inversed();

            using (var context = _wb.GetBitmapContext())
            {
                for (int x = 0; x < _wb.PixelWidth; x++)
                {
                    for (int y = 0; y < _wb.PixelHeight; y++)
                    {
                        var xScaled = (1.0f * x / _wb.PixelWidth) * 2 - 1;
                        var yScaled = (1 - 1.0f * y / _wb.PixelHeight) * 2 - 1;

                        var zScaled = CastRay(elip, xScaled, yScaled, invMat);
                        if (float.IsNaN(zScaled))
                            continue;

                        var pos = invMat.Multiply(new Vector4(xScaled, yScaled, zScaled, 1));
                        pos /= pos.W;
                        var light_pos = _scene.Light.Matrix.Multiply(new Vector4(0, 0, 0, 1));
                        light_pos = new Vector4(0, 5, 0, 1);

                        var color = GetColor(elip, pos, light_pos, model, view);
                        DrawRectangle(context, x, y, x + 1, y + 1, color);

                    }
                }
            }
        }

        private Color GetColor(Elipsoid e, Vector4 position, Vector4 light, Matrix4x4 model, Matrix4x4 view)
        {
            var nx = (float)(position.X / e.RadiusX * e.RadiusX);
            var ny = (float)(position.Y / e.RadiusY * e.RadiusY);
            var nz = (float)(position.Z / e.RadiusZ * e.RadiusZ);
            var normal = view.Multiply(model.Multiply(new Vector4(nx, ny, nz, 0).Normalized()));
            //var normal =model.Multiply(new Vector4(nx, ny, nz, 0).Normalized());

            var pos = view.Multiply(model.Multiply(position));
            //var pos = model.Multiply(position);
            var lightPos = view.Multiply(light);
            //var lightPos =light;

            var toLight = lightPos - pos;

            var angle = Math.Max(0, Vector3.Dot(toLight.ToVector3().Normalized(), normal.ToVector3().Normalized()));

            var xn = Convert.ToByte(Math.Max(0, normal.X) * 255);
            var yn = Convert.ToByte(Math.Max(0, normal.Y) * 255);
            var zn = Convert.ToByte(Math.Max(0, normal.Z) * 255);
            //return Color.FromArgb(255, xn, yn, zn);

            var col = Convert.ToByte(angle * 255);
            return Color.FromArgb(255, col, col, 0);

        }

        unsafe private void DrawRectangle(BitmapContext context, int x1, int y1, int x2, int y2, Color col)
        {
            int color = WriteableBitmapExtensions.ConvertColor(col);
            // Use refs for faster access (really important!) speeds up a lot!
            var w = context.Width;
            var h = context.Height;
            var pixels = context.Pixels;

            // Check boundaries
            if ((x1 < 0 && x2 < 0) || (y1 < 0 && y2 < 0)
             || (x1 >= w && x2 >= w) || (y1 >= h && y2 >= h))
            {
                return;
            }

            // Clamp boundaries
            if (x1 < 0) { x1 = 0; }
            if (y1 < 0) { y1 = 0; }
            if (x2 < 0) { x2 = 0; }
            if (y2 < 0) { y2 = 0; }
            if (x1 >= w) { x1 = w - 1; }
            if (y1 >= h) { y1 = h - 1; }
            if (x2 >= w) { x2 = w - 1; }
            if (y2 >= h) { y2 = h - 1; }

            var startY = y1 * w;
            var endY = y2 * w;

            var offset2 = endY + x1;
            var endOffset = startY + x2;
            var startYPlusX1 = startY + x1;

            for (var x = startYPlusX1; x <= endOffset; x++)
            {
                pixels[x] = color;
                pixels[offset2] = color;
                offset2++;
            }

            endOffset = startYPlusX1 + w;
            offset2 -= w;

            for (var y = startY + x2 + w; y <= offset2; y += w)
            {
                pixels[y] = color;
                pixels[endOffset] = color;
                endOffset += w;
            }
        }
        //_wb.ForEach((int x, int y) =>
        //{
        //    var xScaled = (1.0f * x / _wb.PixelWidth) * 2 - 1;
        //    var yScaled = (1 - 1.0f * y / _wb.PixelHeight) * 2 - 1;
        //    var data = elip.CastRay(xScaled, yScaled, invMat);
        //    if (!data.HasValue)
        //    {
        //        return Colors.Red;
        //    }
        //    else
        //    {
        //        //return Colors.Yellow;
        //        var val = Vector3.Dot(data.Value.normal.ToVector3(), new Vector3(0, 0, -1));
        //        val = Math.Min(1,Math.Max(0, val));
        //        //if (val == 0)
        //        //    return Colors.Blue;
        //        var col = Convert.ToByte(val * 255);
        //        return Color.FromRgb(col, col, 0);
        //    }
        //});

        private float CastRay(Elipsoid e, float x, float y, Matrix4x4 invMat)
        {
            var m = new Matrix4x4();
            m.M11 = (float)(1f / (e.RadiusX * e.RadiusX));
            m.M22 = (float)(1f / (e.RadiusY * e.RadiusY));
            m.M33 = (float)(1f / (e.RadiusZ * e.RadiusZ));
            m.M44 = -1;
            //m.M11 = (float)(RadiusX);
            //m.M22 = (float)(RadiusY);
            //m.M33 = (float)(RadiusZ);
            //m.M44 = -1;

            m = invMat.Transposed().Multiply(m.Multiply(invMat));

            var c = (m.M11 * x + m.M12 * y + m.M14) * x
                    + (m.M21 * x + m.M22 * y + m.M24) * y
                    + (m.M41 * x + m.M42 * y + m.M44);
            var b = m.M13 * x + m.M23 * y + m.M43 + m.M31 * x + m.M32 * y + m.M34;
            var a = m.M33;

            var delta = b * b - 4 * a * c;

            if (delta < 0)
                return float.NaN;

            var z = (float)((-b - Math.Sqrt(delta)) / (2 * a));

            return z;
        }

        //public void RenderFrame()
        //{
        //    _wb.Clear(Colors.Black);

        //    var projection = MyMatrix4x4.CreatePerspectiveFieldOfView(1.3f, 1.0f * _wb.PixelWidth / _wb.PixelHeight, 0.01f, 100.0f);
        //    var view = _scene.Camera.Matrix.Inversed();

        //    foreach (var obj in _scene.Objects)
        //    {
        //        var data = obj.GetRenderData();
        //        var matrix = MyMatrix4x4.Compose(projection, view, _scene.Matrix, obj.Matrix);
        //        foreach (var edge in data.Edges)
        //        {
        //            var vertA = matrix.Multiply(data.Vertices[edge.IdxA].ToVector4());
        //            var vertB = matrix.Multiply(data.Vertices[edge.IdxB].ToVector4());

        //            if (vertA.Z > 0 && vertB.Z > 0)
        //                DrawLine(vertA, vertB);
        //        }
        //    }
        //}

        private void DrawLine(Vector4 vertA, Vector4 vertB)
        {
            var A = new Point(vertA.X / vertA.W, vertA.Y / vertA.W);
            var B = new Point(vertB.X / vertB.W, vertB.Y / vertB.W);

            var width = _wb.PixelWidth;
            var height = _wb.PixelHeight;

            var x1 = Convert.ToInt32((A.X + 1) / 2 * width);
            var y1 = Convert.ToInt32((1 - (A.Y + 1) / 2) * height);
            var x2 = Convert.ToInt32((B.X + 1) / 2 * width);
            var y2 = Convert.ToInt32((1 - (B.Y + 1) / 2) * height);

            _wb.DrawLine(x1, y1, x2, y2, _drawColor);
        }
    }
}
