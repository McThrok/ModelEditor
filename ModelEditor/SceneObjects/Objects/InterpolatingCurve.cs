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
            var order = 3;
            if (Children.Count > order)
            {
                var dataPoints = GetVerts();

                var knots = GetKnots(order, dataPoints.Count - 1);
                var parameters = GetParameters(dataPoints);
                var verts = Interpolate(dataPoints, order, parameters, knots);
                var data = GetSplineCurve(verts, knots, order);

                data.Add(GerSplinePolygon(verts));

                return data;
            }
            else
                return new ObjRenderData();
        }
        private ObjRenderData GetSplineCurve(List<Vector3> verts, List<float> knots, int order)
        {
            var data = new ObjRenderData();
            if (verts.Count > order)
            {
                var count = 1000;
                for (float i = 0; i < count; i++)
                {
                    var t = 1f * i / count;
                    var point = DeBoor(t, verts, knots, order);
                    data.Vertices.Add(point);
                }
            }

            return data;
        }
        private ObjRenderData GerSplinePolygon(List<Vector3> verts)
        {
            var data = new ObjRenderData();
            if (ShowPolygon && verts.Count > 3)
            {
                data.Vertices = verts;
                data.Edges = Enumerable.Range(0, verts.Count - 1).Select(x => new Edge(x, x + 1)).ToList();
            }

            return data;
        }

        public List<float> GetParametersRegularized(List<Vector3> dataPoints)
        {
            var parameters = new List<float>();
            parameters.Add(0);

            float totalLength = 0;
            var l = new List<float>();

            for (var k = 0; k < dataPoints.Count - 1; k++)
            {

                var distanceK = Vector3.Distance(dataPoints[k], dataPoints[k + 1]);
                totalLength += distanceK;
                l.Add(totalLength);
            }

            for (var k = 0; k < dataPoints.Count - 1; k++)
            {
                parameters.Add(l[k] / totalLength);
            }
            return parameters;
        }
        public List<float> GetParameters(List<Vector3> dataPoints)
        {
            var n = dataPoints.Count;
            var parameters = Enumerable.Range(0, n).Select(x => 1f * x / (n - 1)).ToList();

            return parameters;

        }
        public List<float> GetKnots(int p, int n)
        {
            var knots = new List<float>();
            knots.AddRange(Enumerable.Repeat(0f, p + 1));
            knots.AddRange(Enumerable.Range(1, n - p).Select(x => 1f * x / (n - p + 1)));
            knots.AddRange(Enumerable.Repeat(1f, p + 1));

            return knots;
        }

        public List<Vector3> Interpolate(List<Vector3> verts, int p, List<float> parameters, List<float> knots)
        {
            var n = verts.Count - 1;
            var N = ComputerMatN(n, p, parameters, knots);

            var dX = verts.Select(v => v.X).ToList();
            var dY = verts.Select(v => v.Y).ToList();
            var dZ = verts.Select(v => v.Z).ToList();

            ComputeCoefficents(N, dX);
            ComputeCoefficents(N, dY);
            ComputeCoefficents(N, dZ);

            var controlPoints = Enumerable.Range(0, n + 1).Select(i => new Vector3(dX[i], dY[i], dZ[i])).ToList();

            return controlPoints;
        }

        public float[][] ComputerMatN(int n, int p, List<float> parameters, List<float> knots)
        {

            var N = new float[n + 1][];

            for (var i = 0; i <= n; i++)
            {

                N[i] = new float[n + 1];

                for (var j = 0; j <= n; j++)
                {

                    N[i][j] = ComputeN(parameters[i], j, p, knots);
                }
            }

            return N;
        }
        public float ComputeN(float t, int i, int p, List<float> knots)
        {
            var m = knots.Count - 1;
            var N = new List<float>();

            if (i == 0 && t == knots[0])
                return 1;

            if (i == m - p - 1 && t == knots[m])
                return 1;

            if (t < knots[i] || t >= knots[i + p + 1])
                return 0;

            for (var j = 0; j <= p; j++)
            {
                if (t >= knots[i + j] && t < knots[i + j + 1])
                    N.Add(1.0f);
                else
                    N.Add(0.0f);
            }

            float saved = 0;

            for (var k = 1; k <= p; k++)
            {

                if (N[0] == 0)
                    saved = 0;
                else
                    saved = ((t - knots[i]) * N[0]) / (knots[i + k] - knots[i]);

                for (var j = 0; j < p - k + 1; j++)
                {

                    var left = knots[i + j + 1];
                    var right = knots[i + j + k + 1];

                    if (N[j + 1] == 0)
                    {
                        N[j] = (float)saved;
                        saved = 0;
                    }
                    else
                    {
                        var temp = N[j + 1] / (right - left);
                        N[j] = (float)(saved + (right - t) * temp);
                        saved = (t - left) * temp;
                    }
                }

            }

            return N[0];
        }

        public void ComputeCoefficents(float[][] X, List<float> Y)
        {
            int n = Y.Count;
            float[][] mat = X.Select(a => a.ToArray()).ToArray();

            for (int k = 0; k < n; k++)
            {
                int k1 = k + 1;
                for (int i = k; i < n; i++)
                {
                    if (mat[i][k] != 0)
                    {
                        for (int j = k1; j < n; j++)
                        {
                            mat[i][j] /= mat[i][k];
                        }
                        Y[i] /= mat[i][k];
                    }
                }
                for (int i = k1; i < n; i++)
                {
                    if (mat[i][k] != 0)
                    {
                        for (int j = k1; j < n; j++)
                        {
                            mat[i][j] -= mat[k][j];
                        }
                        Y[i] -= Y[k];
                    }
                }
            }

            for (int i = n - 2; i >= 0; i--)
            {
                for (int j = n - 1; j >= i + 1; j--)
                {
                    Y[i] -= mat[i][j] * Y[j];
                }
            }
        }

        public Vector3 DeBoor(float t, List<Vector3> controlPoints, List<float> knots, int order)
        {
            var indexOfRange = FindRangeIndex(t, knots);

            var d = new List<Vector3>();

            for (var i = 0; i <= order; i++)
            {
                var downIndex = indexOfRange - order + i;

                if (downIndex > -1 && downIndex < controlPoints.Count)
                    d.Add(controlPoints[downIndex]);
                else
                    d.Add(Vector3.Zero);
            }


            for (var r = 1; r <= order; r++)
            {
                for (var i = indexOfRange - order + r; i <= indexOfRange; i++)
                {
                    var alpha = (t - knots[i]) / (knots[i + order - r + 1] - knots[i]);

                    int p = indexOfRange - order + r;
                    var x = d[i - p].X * (1 - alpha) + d[i - p + 1].X * alpha;
                    var y = d[i - p].Y * (1 - alpha) + d[i - p + 1].Y * alpha;
                    var z = d[i - p].Z * (1 - alpha) + d[i - p + 1].Z * alpha;

                    d[i - (indexOfRange - order + r)] = new Vector3(x, y, z);
                }
            }

            return d[0];
        }
        public int FindRangeIndex(float t, List<float> knots)
        {
            for (var i = 0; i < knots.Count; i++)
            {
                if (knots[i] > t)
                    return i - 1;

                if (t == 1 && knots[i + 1] == 1)
                    return i;
            }

            return 0;//never
        }
    }
}