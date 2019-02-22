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

        //public static Matrix4x4 Perspective(float width, float height)
        //{
        //    return Matrix4x4.CreatePerspective(width, height, 1, 8);
        //}
        
        public static Matrix4x4 CreatePerspectiveFieldOfView(float fovy, float aspect, float zNear, float zFar)
        {
            float yMax = zNear * (float)System.Math.Tan(0.5f * fovy);
            float yMin = -yMax;
            float xMin = yMin * aspect;
            float xMax = yMax * aspect;

            float left = xMin;
            float right = xMax;
            float bottom = yMin;
            float top = yMax;

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





            //float yMax = zNear * (float)Math.Tan(0.5f * fovy);
            //float yMin = -yMax;
            //float xMin = yMin * aspect;
            //float xMax = yMax * aspect;

            //float x = (2.0f * zNear) / (xMax - xMin);
            //float y = (2.0f * zNear) / (yMax - yMin);
            //float a = (xMax + xMin) / (xMax - xMin);
            //float b = (yMax + yMin) / (yMax - yMin);
            //float c = -(zFar + zNear) / (zFar - zNear);
            //float d = -(2.0f * zFar * zNear) / (zFar - zNear);

            //var matrix = new Matrix4x4(x, 0, 0, 0,
            //                     0, y, 0, 0,
            //                     a, b, c, -1,
            //                     0, 0, d, 0).Transpose();
            //return matrix;
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
            var composition = Identity;
            foreach (var matrix in matrices.Reverse())
            {
                composition = matrix.Multiply(composition);
            }
            return composition;
        }
    }
}
