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
    public class InterpolatingCurve : BezierCurveBase, IRenderableObj
    {
        private static int _count = 0;
        public InterpolatingCurve(RayCaster rayCaster) : base(rayCaster)
        {
            Name = nameof(InterpolatingCurve) + " " + _count++.ToString();
        }

        protected List<Vector3> GetVerts()
        {
            return Children.Select(x => x.Matrix.Translation).ToList();
        }
        public ObjRenderData GetRenderData()
        {
            var verts = CalculateVerts();
            var data = new ObjRenderData();

            //polygon
            if (ShowPolygon && verts.Count > 1)
            {
                data.Vertices.AddRange(verts);
                data.Edges.AddRange(Enumerable.Range(0, verts.Count - 1).Select(x => new Edge(x, x + 1)).ToList());
            }

            //curve
            int i;
            for (i = 0; i + 3 < verts.Count; i += 3)
                data.Vertices.AddRange(GetSegment(verts, i, 4));

            var left = verts.Count - i;
            data.Vertices.AddRange(GetSegment(verts, i, left));

            return data;
        }
        private List<Vector3> CalculateVerts()
        {
            var children = GetVerts();

            var verts = new List<Vector3>(children.Take(4));

            foreach (var child in children.Skip(4))
            {
                var last = verts.Count-1;
                var g = Vector3.Distance(verts[last], child) / Vector3.Distance(verts[last-3], verts[last]);

                var a = verts[last] + g * (verts[last] - verts[last - 1]);
                var deBoor = verts[last-1] + g * (verts[last - 1] - verts[last - 2]);
                var b = a + g * (a - deBoor);

                verts.Add(a);
                verts.Add(b);
                verts.Add(child);
            }

            return verts;
        }
    }
}