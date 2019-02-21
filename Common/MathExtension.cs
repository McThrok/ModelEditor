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

        public static Vector4 Multiply(this Matrix4x4 matrix, Vector4 vector)
        {
            return new Vector4(
                Vector4.Dot(new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14),vector),
                Vector4.Dot(new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24),vector),
                Vector4.Dot(new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34),vector),
                Vector4.Dot(new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44),vector)); 
        }
    }
}
