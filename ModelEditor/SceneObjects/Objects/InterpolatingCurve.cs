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

                data.Add(GerSplinePolygon(verts, order));

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
        private ObjRenderData GerSplinePolygon(List<Vector3> verts, int order)
        {
            var data = new ObjRenderData();
            if (ShowPolygon && verts.Count > order)
            {
                data.Vertices = verts;
                data.Edges = Enumerable.Range(0, verts.Count - 1).Select(x => new Edge(x, x + 1)).ToList();
            }

            return data;
        }

        private List<float> GetParametersRegularized(List<Vector3> dataPoints)
        {
            var parameters = new List<float>();
            parameters.Add(0);

            float totalLength = 0;
            var distances = new List<float>();

            for (var k = 0; k < dataPoints.Count - 1; k++)
            {
                var dist = Vector3.Distance(dataPoints[k], dataPoints[k + 1]);
                totalLength += dist;
                distances.Add(totalLength);
            }

            for (var k = 0; k < dataPoints.Count - 1; k++)
            {
                parameters.Add(distances[k] / totalLength);
            }
            return parameters;
        }
        private List<float> GetParameters(List<Vector3> dataPoints)
        {
            var n = dataPoints.Count;
            var parameters = Enumerable.Range(0, n).Select(x => 1f * x / (n - 1)).ToList();

            return parameters;

        }
        private List<float> GetKnots(int order, int n)
        {
            var knots = new List<float>();

            knots.AddRange(Enumerable.Repeat(0f, order + 1));
            knots.AddRange(Enumerable.Range(1, n - order).Select(x => 1f * x / (n - order + 1)));
            knots.AddRange(Enumerable.Repeat(1f, order + 1));

            return knots;
        }

        private List<Vector3> Interpolate(List<Vector3> verts, int order, List<float> parameters, List<float> knots)
        {
            var n = verts.Count - 1;
            var MatN = ComputerMatN(n, order, parameters, knots);

            var dX = verts.Select(v => v.X).ToList();
            var dY = verts.Select(v => v.Y).ToList();
            var dZ = verts.Select(v => v.Z).ToList();

            ComputeCoefficents(MatN, dX);
            ComputeCoefficents(MatN, dY);
            ComputeCoefficents(MatN, dZ);

            var controlPoints = Enumerable.Range(0, n + 1).Select(i => new Vector3(dX[i], dY[i], dZ[i])).ToList();

            return controlPoints;
        }
        private float[][] ComputerMatN(int n, int p, List<float> parameters, List<float> knots)
        {
            var MatN = new float[n + 1][];

            for (var i = 0; i < n + 1; i++)
            {
                MatN[i] = new float[n + 1];

                for (var j = 0; j < n + 1; j++)
                {
                    MatN[i][j] = ComputeN(parameters[i], j, p, knots);
                }
            }

            return MatN;
        }
        private float ComputeN1(float t, int i, int n, List<float> knots)
        {
            var m = knots.Count - 1;

            if (i == 0 && t == knots[0])
                return 1;

            if (i == m - n - 1 && t == knots[m])
                return 1;

            if (t < knots[i] || t >= knots[i + n + 1])
                return 0;

            if (n == 0)
                return 1;
            else
            {
                var N = ComputeN(t, i, n - 1, knots);
                var NN = ComputeN(t, i + 1, n - 1, knots);

                i += 1;
                var l = t - knots[i - 1];
                var b = knots[i + n - 1] - knots[i - 1];

                var ll = knots[i + n] - t;
                var bb = knots[i + n] - knots[i];

                return l * N / b + ll * NN / bb;
            }

        }
        private float ComputeN(float t, int i, int n, List<float> knots)
        {
            var m = knots.Count - 1;
            var N = new List<float>();

            if (i == 0 && t == knots[0])
                return 1;

            if (i == m - n - 1 && t == knots[m])
                return 1;

            if (t < knots[i] || t >= knots[i + n + 1])
                return 0;

            for (var j = 0; j <= n; j++)
            {
                N.Add(t >= knots[i + j] && t < knots[i + j + 1] ? 1f : 0f);
            }

            float saved = 0;

            for (var k = 1; k <= n; k++)
            {
                if (N[0] == 0)
                    saved = 0;
                else
                    saved = ((t - knots[i]) * N[0]) / (knots[i + k] - knots[i]);

                for (var j = 0; j < n - k + 1; j++)
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
        private void ComputeCoefficents(float[][] X, List<float> Y)
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

        private Vector3 DeBoor(float t, List<Vector3> controlPoints, List<float> knots, int order)
        {
            var rangeIdx = GetRangeIdx(t, knots);

            var d = new List<Vector3>();

            for (var i = 0; i < order + 1; i++)
            {
                var downIndex = rangeIdx - order + i;

                if (downIndex > -1 && downIndex < controlPoints.Count)
                    d.Add(controlPoints[downIndex]);
                else
                    d.Add(Vector3.Zero);
            }

            for (var r = 1; r < order + 1; r++)
            {
                for (var i = rangeIdx - order + r; i < rangeIdx + 1; i++)
                {
                    var alpha = (t - knots[i]) / (knots[i + order - r + 1] - knots[i]);

                    int p = i - (rangeIdx - order + r);

                    d[p] = d[p] * (1 - alpha) + d[p + 1] * alpha;
                }
            }

            return d[0];
        }
        private int GetRangeIdx(float t, List<float> knots)
        {
            var result = 0;
            for (int i = 0; i < knots.Count; i++)
            {
                if (knots[i] > t)
                {
                    result = i - 1;
                    break;
                }

                if (t == 1 && knots[i + 1] == 1)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }
    }
}