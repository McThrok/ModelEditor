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
            if (Children.Count > 3)
            {
                var verts = CalculateVerts();
                var data = GerSplineCurve(verts, GetKnots(3, GetVerts().Count - 1));
                data.Add(GerSplinePolygon(verts));

                return data;
            }
            else
                return new ObjRenderData();
        }

        private ObjRenderData GerSplineCurve(List<Vector3> verts, List<float> knots)
        {
            var data = new ObjRenderData();
            if (verts.Count > 4)
            {
                for (float i = 0; i < 1; i += 0.001f)
                {

                    var point = deboorAlgorithm(i, verts, knots, 3);
                    data.Vertices.Add(point);

                }
                //data.Vertices = GetSplineRec(verts, 0, 0, 1);
            }

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
            var result = interpolate(verts, 3);

            return result;

        }

        public float[][] copy(float[][] arr)
        {
            int n = arr.GetLength(0);
            var copy = new float[n][];
            for (int i = 0; i < n; i++)
            {
                copy[i] = new float[n];
                for (int j = 0; j < n; j++)
                    copy[i][j] = arr[i][j];
            }

            return copy;
        }
        public void ComputeCoefficents(float[][] X, List<float> Y)
        {
            int n = Y.Count;
            float[][] mat = copy(X);
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

        public List<Vector3> interpolate(List<Vector3> dataPoints, int order)
        {
            var parameters = GetParameters(dataPoints);
            var knots = GetKnots(order, dataPoints.Count - 1);
            return interpolate(dataPoints, order, parameters, knots);
        }
        public List<Vector3> interpolate(List<Vector3> dataPoints, int p, List<float> parameters, List<float> knots)
        {

            var n = dataPoints.Count - 1;
            var N = computeN(n, p, parameters, knots);
            var dX = new List<float>();
            var dY = new List<float>();
            var dZ = new List<float>();

            for (var i = 0; i <= n; i++)
            {
                dX.Add(dataPoints[i].X);
                dY.Add(dataPoints[i].Y);
                dZ.Add(dataPoints[i].Z);
            }

            ComputeCoefficents(N, dX);
            ComputeCoefficents(N, dY);
            ComputeCoefficents(N, dZ);

            var controlPoints = new List<Vector3>();

            for (var j = 0; j <= n; j++)
            {
                controlPoints.Add(new Vector3(dX[j], dY[j], dZ[j]));
            }

            //var bspline = BSplineBuilder.build(p, controlPoints, knots);

            return controlPoints;
        }

        public float[][] computeN(int n, int p, List<float> parameters, List<float> knots)
        {

            var N = new float[n + 1][];

            for (var i = 0; i <= n; i++)
            {

                N[i] = new float[n + 1];

                for (var j = 0; j <= n; j++)
                {

                    N[i][j] = compute(parameters[i], j, p, knots);
                }
            }

            return N;
        }

        public float compute(float t, int i, int p, List<float> knots)
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

            var saved = 0.0;

            for (var k = 1; k <= p; k++)
            {

                if (N[0] == 0.0)
                    saved = 0.0;
                else
                {
                    saved = ((t - knots[i]) * N[0]) / (knots[i + k] - knots[i]);
                }

                for (var j = 0; j < p - k + 1; j++)
                {

                    var left = knots[i + j + 1];
                    var right = knots[i + j + k + 1];

                    if (N[j + 1] == 0.0)
                    {
                        N[j] = (float)saved;
                        saved = 0.0;
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
        public int findRangeIndex(float t, List<float> knots)
        {
            for (var i = 0; i < knots.Count; i++)
            {

                if (knots[i] > t)
                    return i - 1;

                if (t == 1 && knots[i + 1] == 1)
                    return i;
            }

            throw new Exception("qwe");
        }
        public Vector3 deboorAlgorithm(float t, List<Vector3> controlPoints, List<float> knots, int order)
        {

            var indexOfRange = findRangeIndex(t, knots);

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


                    var x = d[i - (indexOfRange - order + r)].X * (1 - alpha)
                        + d[i - (indexOfRange - order + r) + 1].X * alpha;

                    var y = d[i - (indexOfRange - order + r)].Y * (1 - alpha)
                        + d[i - (indexOfRange - order + r) + 1].Y * alpha;

                    var z = d[i - (indexOfRange - order + r)].Z * (1 - alpha)
                        + d[i - (indexOfRange - order + r) + 1].Z * alpha;

                    d[i - (indexOfRange - order + r)] = new Vector3(x, y, z);
                }
            }

            return d[0];
        }
    }
}