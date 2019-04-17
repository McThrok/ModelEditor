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
            if (data != null)
                Add(data.Vertices, data.Edges);
        }

        public void Add(List<Vector3> vertices, List<Edge> edges)
        {
            int count = Vertices.Count;

            var eCount = edges.Count;
            var vCount = vertices.Count;

            Vertices.Capacity += vCount;
            for (int i = 0; i < vCount; i++)
                Vertices.Add(vertices[i]);

            Edges.Capacity += eCount;
            for (int i = 0; i < eCount; i++)
                Edges.Add(new Edge(edges[i].IdxA + count, edges[i].IdxB + count));
        }


        public void AddLine(List<Vector3> vertices)
        {
            int count = Vertices.Count;
            var vCount = vertices.Count;

            Vertices.Capacity += vCount;
            for (int i = 0; i < vCount; i++)
                Vertices.Add(vertices[i]);

            for (int i = 0; i < vCount - 1; i++)
                Edges.Add(new Edge(count+i, count+ i + 1));
        }
    }

    public interface IScreenRenderable
    {
        ScreenRenderData GetScreenRenderData();
    }

    public class ScreenRenderData
    {
        public List<Vector2Int> Pixels { get; set; } = new List<Vector2Int>();
        public List<Vector2Int> CameraPixels { get; set; } = new List<Vector2Int>();
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
