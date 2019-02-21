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

        public static Matrix4x4 Perspective()
        {
            return Matrix4x4.CreatePerspective(800, 600, 1, 1000);
        }

        public static Matrix4x4 Identity
        {
            get
            {
                return Matrix4x4.Identity;
            }
        }
    }
}
