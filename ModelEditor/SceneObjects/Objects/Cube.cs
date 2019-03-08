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
    public class Cube : RenderableObj
    {
        private List<Vector3> GetVertices()
        {
            var vertices = new List<Vector3>();
            vertices.Add(new Vector3(-1, -1, 1));
            vertices.Add(new Vector3(1, -1, 1));
            vertices.Add(new Vector3(1, -1, -1));
            vertices.Add(new Vector3(-1, -1, -1));
            vertices.Add(new Vector3(-1, 1, 1));
            vertices.Add(new Vector3(1, 1, 1));
            vertices.Add(new Vector3(1, 1, -1));
            vertices.Add(new Vector3(-1, 1, -1));

            return vertices;
        }
        private List<Edge> GetEdges()
        {
            var edges = new List<Edge>();
            edges.Add(new Edge(0, 1));
            edges.Add(new Edge(1, 2));
            edges.Add(new Edge(2, 3));
            edges.Add(new Edge(3, 0));
            edges.Add(new Edge(0, 4));
            edges.Add(new Edge(1, 5));
            edges.Add(new Edge(2, 6));
            edges.Add(new Edge(3, 7));
            edges.Add(new Edge(4, 5));
            edges.Add(new Edge(5, 6));
            edges.Add(new Edge(6, 7));
            edges.Add(new Edge(7, 4));

            return edges;
        }

        public Cube()
        {
            Name = nameof(Cube);
        }

        public override RenderData GetRenderData()
        {
            var renderData = new RenderData();
            renderData.Vertices = GetVertices();
            renderData.Edges = GetEdges();

            return renderData;
        }
    }
}
