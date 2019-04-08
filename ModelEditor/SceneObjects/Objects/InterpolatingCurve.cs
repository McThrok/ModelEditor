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
                var parameters = GetParametersRegularized(dataPoints);
                var verts = Interpolate(dataPoints, order, parameters, knots);
                //var data = GetSplineCurve(verts, knots, order);

                //data.Add(GerSplinePolygon(verts, order));

                //return data;
                return new ObjRenderData();
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
                    var point = GetSplineValue(t, verts, knots, order);
                    data.Vertices.Add(point);
                }
            }

            return data;
        }
        public Vector3 GetSplineValue(float t, List<Vector3> points, List<float> knots, int order)
        {
            var left = knots[order];
            var right = knots[knots.Count - 1 - order];
            t = t * (right - left) + left;

            int s;
            for (s = order; s < knots.Count - 1 - order; s++)
            {
                if (t >= knots[s] && t <= knots[s + 1])
                {
                    break;
                }
            }

            var verts = points.ToList();
            for (int l = 1; l <= order + 1; l++)
            {
                for (int i = s; i > s - order - 1 + l; i--)
                {
                    float alpha = (t - knots[i]) / (knots[i + order + 1 - l] - knots[i]);
                    verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
                }
            }

            return verts[s];
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

        private void print(float[][] mat)
        {
            for (int i = 0; i < mat.Length; i++)
            {
                for (int j = 0; j < mat[i].Length; j++)
                {
                    Console.Write(mat[i][j].ToString() + " ");
                }
                Console.WriteLine();
            }
        }

        private void printB(float[][] mat)
        {
            for (int i = 0; i < mat.Length; i++)
            {
                for (int j = 0; j < mat[i].Length; j++)
                {
                    var val = mat[i][j];
                    val = val == 0 ? 0 : 1;
                    Console.Write(val.ToString() + " ");
                }
                Console.WriteLine();
            }
        }

        private float[][] ComputerMatN(int n, int order, List<float> parameters, List<float> knots)
        {
            var k = 2;
            var MatN = new float[n + 1][];

            for (var i = 0; i < n + 1; i++)
            {
                MatN[i] = new float[2 * k + 1];

                var low = Math.Max(0, k - i);
                var high = Math.Min(n + k - i + 1, 2 * k + 2);

                for (int j = low; j < high; j++)
                {
                    MatN[i][j] = ComputeN(parameters[i], j + i - k, order, knots);
                }
            }

            MatN[0][k] = MatN[n][k] = 1;
            var mat2 = ComputerMatN1(n, order, parameters, knots);

            return MatN;
        }

        private float[][] ComputerMatN1(int n, int order, List<float> parameters, List<float> knots)
        {
            var MatN = new float[n + 1][];

            for (var i = 0; i < n + 1; i++)
            {
                MatN[i] = new float[n + 1];

                for (var j = 0; j < n + 1; j++)
                {
                    MatN[i][j] = ComputeN(parameters[i], j, order, knots);
                }
            }

            MatN[0][0] = MatN[n][n] = 1;

            return MatN;
        }
        private float ComputeN(float t, int i, int n, List<float> knots)
        {
            if (n == 0)
            {
                if (t < knots[i] || t >= knots[i + 1])
                    return 0;
                else
                    return 1;
            }
            else
            {
                var N = ComputeN(t, i, n - 1, knots);
                var NN = ComputeN(t, i + 1, n - 1, knots);

                i += 1;
                var l = t - knots[i - 1];
                var m = knots[i + n - 1] - knots[i - 1];
                var a = 0f;
                if (m != 0)
                    a = l * N / m;

                var ll = knots[i + n] - t;
                var mm = knots[i + n] - knots[i];
                var b = 0f;
                if (mm != 0)
                    b = ll * NN / mm;

                return a + b;
            }
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
    }
}