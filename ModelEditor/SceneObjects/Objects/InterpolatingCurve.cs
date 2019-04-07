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
            var nodes = GetNodes(5);
            var a = GetN(3, 0, 0, nodes);

            if (Children.Count > 3)
            {
                var verts = CalculateVerts();
                var data = GerSplineCurve(verts);
                data.Add(GerSplinePolygon(verts));

                return data;
            }
            else
                return new ObjRenderData();
        }

        private ObjRenderData GerSplineCurve(List<Vector3> verts)
        {
            var data = new ObjRenderData();
            if (verts.Count > 4)
                data.Vertices = GetSplineRec(verts, 0, 0, 1);

            return data;
        }
        private List<Vector3> GetSplineRec(List<Vector3> verts, int level, float start, float end)
        {
            var pointA = GetSplineValue(verts, start);
            var pointB = GetSplineValue(verts, end);
            var screenPosA = _rayCaster.GetExScreenPositionOf(pointA);
            var screenPosB = _rayCaster.GetExScreenPositionOf(pointB);

            var result = new List<Vector3>();

            if (Dist(screenPosA, screenPosB) <= 1 || level > 8)
            {
                if (screenPosA != Vector2Int.Empty)
                    result.Add(pointA);
                if (screenPosB != Vector2Int.Empty)
                    result.Add(pointB);
            }
            else
            {
                float mid = (start + end) / 2;
                var left = GetSplineRec(verts, level + 1, start, mid);
                var right = GetSplineRec(verts, level + 1, mid, end);
                result.AddRange(left);

                if (left.Count > 0 && right.Count > 0 && left[left.Count - 1] == right[0])
                    result.AddRange(right.Skip(1));
                else
                    result.AddRange(right);
            }

            return result;
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
        public Vector3 GetSplineValue(List<Vector3> points, float t)
        {
            int degree = 3;

            var left = degree;
            var right = points.Count;
            t = t * (right - left) + left;

            int s;
            for (s = left; s < right; s++)
            {
                if (t >= s && t <= s + 1)
                {
                    break;
                }
            }

            var verts = points.ToList();
            for (int l = 1; l <= degree + 1; l++)
            {
                for (int i = s; i > s - degree - 1 + l; i--)
                {
                    float alpha = (t - i) / (degree + 1 - l);
                    verts[i] = (1 - alpha) * verts[i - 1] + alpha * verts[i];
                }
            }

            var result = verts[s];

            return result;
        }

        private double[] mul(float[,] mat, double[] vec)
        {
            int n = vec.Length;
            var result = new double[n];

            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < n; j++)
                {
                    sum += mat[i, j] * vec[j];
                }
                result[i] = sum;
            }

            return result;
        }
        private List<Vector3> CalculateVerts()
        {
            var verts = GetVerts();
            var nodes = GetNodes(verts.Count);
            var mat = GetMatrix(nodes);

            var x = verts.Select(v => (double)v.X).ToArray();
            var y = verts.Select(v => (double)v.Y).ToArray();
            var z = verts.Select(v => (double)v.Z).ToArray();

            var xx = verts.Select(v => (double)v.X).ToArray();
            var yy = verts.Select(v => (double)v.Y).ToArray();
            var zz = verts.Select(v => (double)v.Z).ToArray();

            ComputeCoefficents(mat, x);
            var a = mul(mat, x);
            ComputeCoefficents(mat, y);
            var b = mul(mat, y);
            ComputeCoefficents(mat, z);
            var c = mul(mat, z);

            var result = new List<Vector3>();
            result.Add(new Vector3((float)x[0], (float)y[0], (float)z[0]));
            result.Add(new Vector3((float)x[0], (float)y[0], (float)z[0]));
            for (int i = 0; i < verts.Count; i++)
            {
                result.Add(new Vector3((float)x[i], (float)y[i], (float)z[i]));
            }
            var last = verts.Count - 1;
            result.Add(new Vector3((float)x[last], (float)y[last], (float)z[last]));
            result.Add(new Vector3((float)x[last], (float)y[last], (float)z[last]));

            return result;
        }
        public List<float> GetNodes(int n)
        {
            int degreee = 3;
            var nodes = new List<float>();
            //nodes.AddRange(Enumerable.Repeat(0f, degreee));
            nodes.AddRange(Enumerable.Range(0, n).Select(x => 1f * x / (n - 1)));
            //nodes.AddRange(Enumerable.Repeat(1f, degreee));
            //return new List<float>() { 0, 0, 0, 0, 0.5f, 1, 1, 1, 1 };
            return nodes;
        }
        public float[,] GetMatrix(List<float> nodes)
        {
            int m = nodes.Count;
            float[,] mat = new float[m, m];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    mat[i, j] = GetN(3, j, nodes[i], nodes);
                }
            }

            return mat;
        }
        public double[,] copy(float[,] arr)
        {
            int n = arr.GetLength(0);
            var copy = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    copy[i, j] = arr[i, j];

            return copy;
        }
        public void print(float[,] X)
        {
            Console.WriteLine();
            for (int i = 0; i < X.GetLength(0); i++)
            {
                for (int j = 0; j < X.GetLength(1); j++)
                {
                    Console.Write(X[i, j].ToString() + " ");
                }
                Console.WriteLine();
            }
        }
        public void ComputeCoefficents(float[,] X, double[] Y)
        {
            int n = Y.Length;
            double[,] mat = copy(X);
            for (int k = 0; k < n; k++)
            {
                int k1 = k + 1;
                for (int i = k; i < n; i++)
                {
                    if (mat[i, k] != 0)
                    {
                        for (int j = k1; j < n; j++)
                        {
                            mat[i, j] /= mat[i, k];
                        }
                        Y[i] /= mat[i, k];
                    }
                }
                for (int i = k1; i < n; i++)
                {
                    if (mat[i, k] != 0)
                    {
                        for (int j = k1; j < n; j++)
                        {
                            mat[i, j] -= mat[k, j];
                        }
                        Y[i] -= Y[k];
                    }
                }
            }

            for (int i = n - 2; i >= 0; i--)
            {
                for (int j = n - 1; j >= i + 1; j--)
                {
                    Y[i] -= mat[i, j] * Y[j];
                }
            }
        }

        private float GetN(int n, int i, float t, List<float> nodes)
        {
            if (n == 0)
                return Get(nodes, i - 1) <= t && t <= Get(nodes, i) ? 1 : 0;
            //return Get(nodes, i) <= t && t <= Get(nodes, i + 1) ? 1 : 0;
            else
            {
                //var N = GetN(j - 1, i, t, nodes);
                //var l = (t - Get(nodes, i));
                //var m = Get(nodes, i + j) - Get(nodes, i);

                //var NN = GetN(j - 1, i + 1, t, nodes);
                //var ll = (Get(nodes, i + j + 1) - t);
                //var mm = (Get(nodes, i + j + 1) - Get(nodes, i + 1));

                var N = GetN(n - 1, i, t, nodes);
                var l = (t - Get(nodes, i - 1));
                var m = Get(nodes, i + n - 1) - Get(nodes, i - 1);

                var NN = GetN(n - 1, i + 1, t, nodes);
                var ll = (Get(nodes, i + n) - t);
                var mm = (Get(nodes, i + n) - Get(nodes, i));

                var a = m == 0 ? 0 : N * l / m;
                var b = mm == 0 ? 0 : NN * ll / mm;
                return a + b;
                //return GetN(n - 1, i, t, nodes) * (t - Get(nodes, i - 1)) / (Get(nodes, i + n - 1) - Get(nodes, i - 1))
                //    + GetN(n - 1, i + 1, t, nodes) * (Get(nodes, i + n) - t) / (Get(nodes, i + n) - Get(nodes, i));
            }
        }

        private float Get(List<float> nodes, int i)
        {
            var n = nodes.Count;
            if (i < 0)
            {
                return nodes[0];
                //return nodes[0] + i * (nodes[1] - nodes[0]);
            }
            if (i > n - 1)
            {
                return nodes[n - 1];
                //return nodes[n - 1] + (i - n + 1) * (nodes[n - 1] - nodes[n - 2]);
            }

            return nodes[i];
        }
    }
}