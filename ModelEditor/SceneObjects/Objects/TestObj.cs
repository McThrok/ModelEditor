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
    public class TestObj : RenderableObj
    {
        public override RenderData GetRenderData()
        {
            return new RenderData()
            {
                Vertices = GetVertices(),
                Edges = GetEdges(),
            };
        }
        private  List<Vector3> GetVertices()
        {
            return new List<Vector3>() {
                new Vector3(-0.5f, -0.5f,-2),
                new Vector3(0.5f, -0.5f,-2),
                new Vector3(0.5f, -0.5f, -3),
                new Vector3(-0.5f, -0.5f, -3),
                new Vector3(-0.5f, 0.5f,-2),
                new Vector3(0.5f, 0.5f,-2),
                new Vector3(0.5f, 0.5f, -3),
                new Vector3(-0.5f, 0.5f, -3),
            };
        }
        private List<Edge> GetEdges()
        {
            return new List<Edge>() {
                new Edge(0, 1),
                new Edge(1, 2),
                new Edge(2, 3),
                new Edge(0, 3),

                new Edge(0, 4),
                new Edge(1, 5),
                new Edge(2, 6),
                new Edge(3, 7),

                new Edge(4, 5),
                new Edge(5, 6),
                new Edge(6, 7),
                new Edge(4, 7),
            };
        }
    }
}
