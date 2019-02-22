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
    public static class MathExtension
    {
        public static Vector4 ToVector4(this Vector3 v)
        {
            return new Vector4(v.X, v.Y, v.Z, 1);
        }
        public static Vector3 ToVector3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Matrix4x4 Multiply(this Matrix4x4 m1, Matrix4x4 m2)
        {
            return new Matrix4x4(
                Vector4.Dot(m1.GetRow1(), m2.GetCol1()),
                Vector4.Dot(m1.GetRow2(), m2.GetCol1()),
                Vector4.Dot(m1.GetRow3(), m2.GetCol1()),
                Vector4.Dot(m1.GetRow4(), m2.GetCol1()),

                Vector4.Dot(m1.GetRow1(), m2.GetCol2()),
                Vector4.Dot(m1.GetRow2(), m2.GetCol2()),
                Vector4.Dot(m1.GetRow3(), m2.GetCol2()),
                Vector4.Dot(m1.GetRow4(), m2.GetCol2()),

                Vector4.Dot(m1.GetRow1(), m2.GetCol3()),
                Vector4.Dot(m1.GetRow2(), m2.GetCol3()),
                Vector4.Dot(m1.GetRow3(), m2.GetCol3()),
                Vector4.Dot(m1.GetRow4(), m2.GetCol3()),

                Vector4.Dot(m1.GetRow1(), m2.GetCol4()),
                Vector4.Dot(m1.GetRow2(), m2.GetCol4()),
                Vector4.Dot(m1.GetRow3(), m2.GetCol4()),
                Vector4.Dot(m1.GetRow4(), m2.GetCol4()));
        }
        public static Vector4 Multiply(this Matrix4x4 matrix, Vector4 vector)
        {
            return new Vector4(
                Vector4.Dot(matrix.GetRow1(), vector),
                Vector4.Dot(matrix.GetRow2(), vector),
                Vector4.Dot(matrix.GetRow3(), vector),
                Vector4.Dot(matrix.GetRow4(), vector));
        }
        public static Vector4 Multiply(this Vector4 vector, Matrix4x4 matrix)
        {
            return new Vector4(
                Vector4.Dot(vector, matrix.GetCol1()),
                Vector4.Dot(vector, matrix.GetCol2()),
                Vector4.Dot(vector, matrix.GetCol3()),
                Vector4.Dot(vector, matrix.GetCol4()));
        }

        public static Matrix4x4 Transpose(this Matrix4x4 matrix)
        {
            return Matrix4x4.Transpose(matrix);
        }

        public static Vector4 GetRow1(this Matrix4x4 matrix) => new Vector4(matrix.M11, matrix.M21, matrix.M31, matrix.M41);
        public static Vector4 GetRow2(this Matrix4x4 matrix) => new Vector4(matrix.M12, matrix.M22, matrix.M32, matrix.M42);
        public static Vector4 GetRow3(this Matrix4x4 matrix) => new Vector4(matrix.M13, matrix.M23, matrix.M33, matrix.M43);
        public static Vector4 GetRow4(this Matrix4x4 matrix) => new Vector4(matrix.M14, matrix.M24, matrix.M34, matrix.M44);

        public static Vector4 GetCol1(this Matrix4x4 matrix) => new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14);
        public static Vector4 GetCol2(this Matrix4x4 matrix) => new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24);
        public static Vector4 GetCol3(this Matrix4x4 matrix) => new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34);
        public static Vector4 GetCol4(this Matrix4x4 matrix) => new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    }
}
