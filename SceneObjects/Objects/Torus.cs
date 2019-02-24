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
    public class Torus : RenderableObj
    {
        public double LargeRadius { get; set; } = 5;
        public double SmallRadius { get; set; } = 1;
        public int LargeDensity { get; set; } = 20;
        public int SmallDensity { get; set; } = 20;

        public override List<Vector3> GetVertices()
        {
            var vertices = new List<Vector3>();

            var largeDiff = 2.0 * Math.PI / LargeDensity;
            var smallDiff = 2.0 * Math.PI / SmallDensity;

            for (int i = 0; i < LargeDensity; i++)
            {
                for (int j = 0; j < SmallDensity; j++)
                {
                    var a = LargeRadius + SmallRadius * Math.Cos(j * smallDiff);
                    float x = (float)(a * Math.Cos(i * largeDiff));
                    float y = (float)(a * Math.Sin(i * largeDiff));
                    float z = (float)(SmallRadius * Math.Sin(j * smallDiff));
                    vertices.Add(new Vector3(x, y, z));
                }
            }

            return vertices;
        }
        public override List<Edge> GetEdges()
        {
            var edges = new List<Edge>();

            for (int i = 0; i < LargeDensity; i++)
            {
                for (int j = 0; j < SmallDensity; j++)
                {
                    edges.Add(new Edge(i * SmallDensity + j, i * SmallDensity + ((j + 1) % SmallDensity)));
                    edges.Add(new Edge(i * SmallDensity + j, (i + 1) % LargeDensity * SmallDensity + j));
                }
            }

            return edges;

        }
    }
}
