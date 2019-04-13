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
    public class BezierCurveC0 : BezierCurveBase, IRenderableObj
    {
        private static int _count = 0;
        public BezierCurveC0(RayCaster rayCaster) : base(rayCaster)
        {
            Name = nameof(BezierCurveC0) + " " + _count++.ToString();
        }

        protected List<Vector3> GetVerts()
        {
            return Children.Select(x => x.Matrix.Multiply(Vector3.Zero.ToVector4()).ToVector3()).ToList();
        }
        public ObjRenderData GetRenderData()
        {
            var verts = GetVerts();
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

    }
}

