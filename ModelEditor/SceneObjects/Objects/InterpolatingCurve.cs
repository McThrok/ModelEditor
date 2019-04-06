﻿using System;
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
            List<float> nodes = GetNodes(4);
            var a = GetN(3, 0, 0, nodes);
            if (Children.Count > 1)
            {

                var verts = CalculateVerts();
                var data = GerSplineCurve(verts);
                data.Add(GerSplinePolygon(verts));

                return data;
            }
            else return new ObjRenderData();
        }

        private ObjRenderData GerSplineCurve(List<Vector3> verts)
        {
            var data = new ObjRenderData();
            if (verts.Count > 3)
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

        private List<Vector3> CalculateVerts()
        {
            var verts = GetVerts();
            var nodes = GetNodes(verts.Count);
            var mat = GetMatrix(nodes);

            var x = verts.Select(v => v.X).ToArray();
            var y = verts.Select(v => v.Y).ToArray();
            var z = verts.Select(v => v.Z).ToArray();

            ComputeCoefficents(mat, x);
            ComputeCoefficents(mat, y);
            ComputeCoefficents(mat, z);

            var result = new List<Vector3>();
            result.Add(new Vector3(x[0], y[0], z[0]));
            result.Add(new Vector3(x[0], y[0], z[0]));
            for (int i = 0; i < verts.Count; i++)
            {
                result.Add(new Vector3(x[i], y[i], z[i]));
            }
            var last = verts.Count - 1;
            result.Add(new Vector3(x[last], y[last], z[last]));
            result.Add(new Vector3(x[last], y[last], z[last]));

            return result;
        }
        public List<float> GetNodes(int n)
        {
            var nodes = Enumerable.Range(0, n).Select(x => 1f * x / (n - 1)).ToList();
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
        public void ComputeCoefficents(float[,] X, float[] Y)
        {
            int n = Y.Length;
            for (int k = 0; k < n; k++)
            {
                int k1 = k + 1;
                for (int i = k; i < n; i++)
                {
                    if (X[i, k] != 0)
                    {
                        for (int j = k1; j < n; j++)
                        {
                            X[i, j] /= X[i, k];
                        }
                        Y[i] /= X[i, k];
                    }
                }
                for (int i = k1; i < n; i++)
                {
                    if (X[i, k] != 0)
                    {
                        for (int j = k1; j < n; j++)
                        {
                            X[i, j] -= X[k, j];
                        }
                        Y[i] -= Y[k];
                    }
                }
            }

            for (int i = n - 2; i >= 0; i--)
            {
                for (int j = n - 1; j >= i + 1; j--)
                {
                    Y[i] -= X[i, j] * Y[j];
                }
            }
        }

        private float GetN(int n, int i, float t, List<float> nodes)
        {
            if (n == 0)
                return Get(nodes, i - 1) <= t && t < Get(nodes, i) ? 1 : 0;
            else
            {
                var l = GetN(n - 1, i, t, nodes) * (t - Get(nodes, i - 1));
                var m = Get(nodes, i + n - 1) - Get(nodes, i - 1);

                var ll = GetN(n - 1, i + 1, t, nodes) * (Get(nodes, i + n) - t);
                var mm = (Get(nodes, i + n) - Get(nodes, i));

                var a = m == 0 ? 0 : l / m;
                var b = mm == 0 ? 0 : ll / mm;
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
                return nodes[0] + i * (nodes[1] - nodes[0]);
            }
            if (i > n - 1)
            {
                return nodes[n - 1];
                return nodes[n - 1] + (i - n + 1) * (nodes[n - 1] - nodes[n - 2]);
            }

            return nodes[i];
        }
    }
}