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
    public static class MyMatrix4x4
    {
        public static Matrix4x4 RotationX(float radians)
        {
            return Matrix4x4.CreateRotationX(radians);
        }

        public static Matrix4x4 RotationY(float radians)
        {
            return Matrix4x4.CreateRotationY(radians);
        }

        public static Matrix4x4 RotationZ(float radians)
        {
            return Matrix4x4.CreateRotationZ(radians);
        }

        public static Matrix4x4 Translate(Vector3 translation)
        {
            return Matrix4x4.CreateTranslation(translation);
        }

        public static Matrix4x4 Scale(Vector3 scale)
        {
            return Matrix4x4.CreateScale(scale);
        }

        public static Matrix4x4 Scale(float scale)
        {
            return Matrix4x4.CreateScale(Vector3.One * scale);
        }

        public static Matrix4x4 CreateAnaglyphicPerspectiveFieldOfView(float fovy, float aspect, float zNear, float zFar, float e)
        {
            var mat = CreatePerspectiveFieldOfView(fovy, aspect, zNear, zFar);
            mat.M31 = e*mat.M34;

            return mat;
        }

        public static Matrix4x4 CreatePerspectiveFieldOfView(float fovy, float aspect, float zNear, float zFar)
        {
            float yMax = zNear * (float)Math.Tan(0.5f * fovy);
            float yMin = -yMax;
            float xMin = yMin * aspect;
            float xMax = yMax * aspect;

            float x = (2.0f * zNear) / (xMax - xMin);
            float y = (2.0f * zNear) / (yMax - yMin);
            float c = -(zFar + zNear) / (zFar - zNear);
            float d = -(2.0f * zFar * zNear) / (zFar - zNear);

            var matrix = new Matrix4x4(x, 0, 0, 0,
                                       0, y, 0, 0,
                                       0, 0, c, -1,
                                       0, 0, d, 0);

            return matrix;
        }

        public static Matrix4x4 Identity
        {
            get
            {
                return Matrix4x4.Identity;
            }
        }

        public static Matrix4x4 Compose(params Matrix4x4[] matrices)
        {
            return matrices.Aggregate(Identity, (composition, matrix) => composition.Multiply(matrix));
        }
    }
}
