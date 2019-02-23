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

            var projection = MyMatrix4x4.CreatePerspectiveFieldOfView(1.3f, 1.0f * _wb.PixelWidth / _wb.PixelHeight, 0.001f, 100.0f);
            var view = _scene.Camera.Matrix.Inversed();

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
            //if (A != Vector3.Clamp(A, -1 * Vector3.One, Vector3.One)
            //    || B != Vector3.Clamp(B, -1 * Vector3.One, Vector3.One))
            //    return;
            if (vertA.Z < 0 || vertB.Z < 0)
                return;

            //var extends = new Extents()
            //{
            //    Bottom = -0.5f,
            //    Left = -0.5f,
            //    Right = 0.5f,
            //    Top = 0.5f
            //};
            //var result = CohenSutherland.CohenSutherlandLineClip(extends,new Point(A.X, A.Y), new Point(B.X, B.Y));
            //if (result == null)
            //    return;

            var  A = (vertA / vertA.W).ToVector3();
             var B = (vertB / vertB.W).ToVector3();

            var width = _wb.PixelWidth;
            var height = _wb.PixelHeight;


            //var x1 = Convert.ToInt32((result[0].X + 1) / 2 * width);
            //var y1 = Convert.ToInt32((1 - (result[0].Y + 1) / 2) * height);
            //var x2 = Convert.ToInt32((result[1].X + 1) / 2 * width);
            //var y2 = Convert.ToInt32((1 - (result[1].Y + 1) / 2) * height);

            var x1 = Convert.ToInt32((A.X + 1) / 2 * width);
            var y1 = Convert.ToInt32((1 - (A.Y + 1) / 2) * height);
            var x2 = Convert.ToInt32((B.X + 1) / 2 * width);
            var y2 = Convert.ToInt32((1 - (B.Y + 1) / 2) * height);

            _wb.DrawLine(x1, y1, x2, y2, _drawColor);
        }
    }
    public struct Extents
    {
        public float Top;
        public float Left;
        public float Right;
        public float Bottom;
    }

    public class CohenSutherland
    {
        /// <summary>
        /// Bitfields used to partition the space into 9 regiond
        /// </summary>
        private const byte INSIDE = 0; // 0000
        private const byte LEFT = 1;   // 0001
        private const byte RIGHT = 2;  // 0010
        private const byte BOTTOM = 4; // 0100
        private const byte TOP = 8;    // 1000

        /// <summary>
        /// Compute the bit code for a point (x, y) using the clip rectangle
        /// bounded diagonally by (xmin, ymin), and (xmax, ymax)
        /// ASSUME THAT xmax , xmin , ymax and ymin are global constants.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static byte ComputeOutCode(Extents extents, double x, double y)
        {
            // initialised as being inside of clip window
            byte code = INSIDE;

            if (x < extents.Left)           // to the left of clip window
                code |= LEFT;
            else if (x > extents.Right)     // to the right of clip window
                code |= RIGHT;
            if (y < extents.Bottom)         // below the clip window
                code |= BOTTOM;
            else if (y > extents.Top)       // above the clip window
                code |= TOP;

            return code;
        }

        /// <summary>
        /// Cohen–Sutherland clipping algorithm clips a line from
        /// P0 = (x0, y0) to P1 = (x1, y1) against a rectangle with
        /// diagonal from (xmin, ymin) to (xmax, ymax).
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0""</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns>a list of two points in the resulting clipped line, or zero</returns>
        public static List<Point> CohenSutherlandLineClip(Extents extents,
                               Point p0, Point p1)
        {
            double x0 = p0.X;
            double y0 = p0.Y;
            double x1 = p1.X;
            double y1 = p1.Y;

            // compute outcodes for P0, P1, and whatever point lies outside the clip rectangle
            byte outcode0 = CohenSutherland.ComputeOutCode(extents, x0, y0);
            byte outcode1 = CohenSutherland.ComputeOutCode(extents, x1, y1);
            bool accept = false;

            while (true)
            {
                // Bitwise OR is 0. Trivially accept and get out of loop
                if ((outcode0 | outcode1) == 0)
                {
                    accept = true;
                    break;
                }
                // Bitwise AND is not 0. Trivially reject and get out of loop
                else if ((outcode0 & outcode1) != 0)
                {
                    break;
                }
                else
                {
                    // failed both tests, so calculate the line segment to clip
                    // from an outside point to an intersection with clip edge
                    double x, y;

                    // At least one endpoint is outside the clip rectangle; pick it.
                    byte outcodeOut = (outcode0 != 0) ? outcode0 : outcode1;

                    // Now find the intersection point;
                    // use formulas y = y0 + slope * (x - x0), x = x0 + (1 / slope) * (y - y0)
                    if ((outcodeOut & TOP) != 0)
                    {   // point is above the clip rectangle
                        x = x0 + (x1 - x0) * (extents.Top - y0) / (y1 - y0);
                        y = extents.Top;
                    }
                    else if ((outcodeOut & BOTTOM) != 0)
                    { // point is below the clip rectangle
                        x = x0 + (x1 - x0) * (extents.Bottom - y0) / (y1 - y0);
                        y = extents.Bottom;
                    }
                    else if ((outcodeOut & RIGHT) != 0)
                    {  // point is to the right of clip rectangle
                        y = y0 + (y1 - y0) * (extents.Right - x0) / (x1 - x0);
                        x = extents.Right;
                    }
                    else if ((outcodeOut & LEFT) != 0)
                    {   // point is to the left of clip rectangle
                        y = y0 + (y1 - y0) * (extents.Left - x0) / (x1 - x0);
                        x = extents.Left;
                    }
                    else
                    {
                        x = double.NaN;
                        y = double.NaN;
                    }

                    // Now we move outside point to intersection point to clip
                    // and get ready for next pass.
                    if (outcodeOut == outcode0)
                    {
                        x0 = x;
                        y0 = y;
                        outcode0 = CohenSutherland.ComputeOutCode(extents, x0, y0);
                    }
                    else
                    {
                        x1 = x;
                        y1 = y;
                        outcode1 = CohenSutherland.ComputeOutCode(extents, x1, y1);
                    }
                }
            }

            // return the clipped line
            return (accept) ?
                new List<Point>()
            {
            new Point(x0,y0),
            new Point(x1, y1),
            } : null;

        }
    }
}
