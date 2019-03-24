using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModelEditor
{
    public class RayCaster
    {
        private readonly RenderAccessor _renderer;
        public RayCaster(RenderAccessor renderer)
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

        public Vector2Int GetScreenPositionOf(Vector3 position)
        {
            var projection = GetProjectionMatrix();
            var view = GetViewMatrix();

            var matrix = view * projection;

            var center = matrix.Multiply(position.ToVector4());
            if (center.Z < 0)
            {
                return Vector2Int.Empty;
            }

            var v = new Point(center.X / center.W, center.Y / center.W);

            var width = BitmapWidth;
            var height = BitmapHeight;

            var x = Convert.ToInt32((v.X + 1) / 2 * width);
            var y = Convert.ToInt32((1 - (v.Y + 1) / 2) * height);

           // if (x > 0 && x < width && y > 0 && y < height)
                return new Vector2Int(x, y);
           // else
              //  return Vector2Int.Empty;
        }
        public Vector2Int GetScreenPositionOf(SceneObject obj)
        {
            return GetScreenPositionOf(obj.GlobalMatrix.Multiply(Vector3.Zero.ToVector4()).ToVector3());
        }
    }
}
