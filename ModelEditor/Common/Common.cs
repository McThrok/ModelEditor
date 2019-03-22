using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;

namespace ModelEditor
{
    public interface IRenderableObj
    {
        ObjRenderData GetRenderData();
    }

    public class ObjRenderData
    {
        public List<Vector3> Vertices { get; set; }
        public List<Edge> Edges { get; set; }
    }

    public interface IScreenRenderable
    {
        ScreenRenderData GetScreenRenderData();
    }

    public class ScreenRenderData
    {
        public List<Vector2Int> Pixels { get; set; } = new List<Vector2Int>();
        public List<PixelPosition> PixelPositions { get; set; } = new List<PixelPosition>();
    }
    public struct PixelPosition
    {
        public PixelPosition(Vector2Int pixel, Vector3 position)
        {
            Pixel = pixel;
            Position = position;
        }
        public PixelPosition(Vector3 position) : this(Vector2Int.Zero, position)
        {
        }

        public Vector2Int Pixel { get; set; }
        public Vector3 Position { get; set; }
    }



}
