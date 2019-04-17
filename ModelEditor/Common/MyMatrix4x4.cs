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
        public static Matrix4x4 Transform(Vector3 translation, Vector3 rotation,Vector3 scale)
        {
            return Matrix4x4.CreateScale(scale) * CreateRotation(rotation) * Matrix4x4.CreateTranslation(translation);
        }
        public static Matrix4x4 CreateRotation(Vector3 rotation)
        {
            return Matrix4x4.CreateRotationX(rotation.X) * Matrix4x4.CreateRotationY(rotation.Y) * Matrix4x4.CreateRotationZ(rotation.Z);
        }

        public static Matrix4x4 CreateAnaglyphicPerspectiveFieldOfView(float fov, float aspect, float zNear, float zFar, float eDiff, float r)
        {
            var top = zNear * (float)Math.Tan(fov / 2);
            var bottom = -top;

            float a = aspect * (float)Math.Tan(fov / 2) * r;
            float b = a - eDiff;
            float c = a + eDiff;

            var left = -b * zNear / r;
            var right = c * zNear / r;

            var result = CreatePerspectiveOffCenter(left, right, bottom, top, zNear, zFar).Multiply(Matrix4x4.CreateTranslation(new Vector3(eDiff, 0, 0)));
            return result;

        }
        public static Matrix4x4 CreatePerspectiveFieldOfView(float fovy, float aspect, float zNear, float zFar)
        {
            if (fovy <= 0 || fovy > Math.PI)
                throw new ArgumentOutOfRangeException("fovy");
            if (aspect <= 0)
                throw new ArgumentOutOfRangeException("aspect");
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");

            float yMax = zNear * (float)System.Math.Tan(0.5f * fovy);
            float yMin = -yMax;
            float xMin = yMin * aspect;
            float xMax = yMax * aspect;

            return CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
        }
        public static Matrix4x4 CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");
            if (zNear >= zFar)
                throw new ArgumentOutOfRangeException("zNear");

            float x = (2.0f * zNear) / (right - left);
            float y = (2.0f * zNear) / (top - bottom);
            float a = (right + left) / (right - left);
            float b = (top + bottom) / (top - bottom);
            float c = -(zFar + zNear) / (zFar - zNear);
            float d = -(2.0f * zFar * zNear) / (zFar - zNear);

            var result = new Matrix4x4(x, 0, 0, 0,
                                       0, y, 0, 0,
                                       a, b, c, -1,
                                       0, 0, d, 0);

            return result;
        }



    }
}
