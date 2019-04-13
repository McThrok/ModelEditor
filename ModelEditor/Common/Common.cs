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
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();
        public List<Edge> Edges { get; set; } = new List<Edge>();

        public void Add(ObjRenderData data)
        {
            if (data == null)
                return;

            int count = Vertices.Count;
            Vertices.AddRange(data.Vertices);
            Edges.AddRange(data.Edges.Select(x => new Edge(x.IdxA + count, x.IdxB + count)));
        }

    }

    public interface IScreenRenderable
    {
        ScreenRenderData GetScreenRenderData();
    }

    public class ScreenRenderData
    {
        public List<Vector2Int> Pixels { get; set; } = new List<Vector2Int>();
        public List<Vector2Int> GobalPixels { get; set; } = new List<Vector2Int>();
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
