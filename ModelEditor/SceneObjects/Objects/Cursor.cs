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
    public class Cursor : SceneObject, IRenderableObj
    {

        public float Tolerance { get; set; } = 10;

        public Cursor()
        {
            Name = nameof(Cursor);
        }

        public ObjRenderData GetRenderData()
        {
            var renderData = new ObjRenderData();
            renderData.Vertices = GetVertices();
            renderData.Edges = GetEdges();

            return renderData;
        }
        private List<Vector3> GetVertices()
        {
            var vertices = new List<Vector3>();
            vertices.Add(new Vector3(0, 0, 0));
            vertices.Add(new Vector3(3, 0, 0));
            vertices.Add(new Vector3(0, 2, 0));
            vertices.Add(new Vector3(0, 0, 1));

            return vertices;
        }
        private List<Edge> GetEdges()
        {
            var edges = new List<Edge>();
            edges.Add(new Edge(0, 1));
            edges.Add(new Edge(0, 2));
            edges.Add(new Edge(0, 3));

            return edges;
        }
    }
}
