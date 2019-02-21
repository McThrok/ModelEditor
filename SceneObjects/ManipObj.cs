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
    public class ManipObj
    {
        public Matrix4x4 Matrix { get; protected set; } = MyMatrix4x4.Identity;

        public void Move(Vector3 movement)
        {
            Matrix = MyMatrix4x4.Translate(movement) * Matrix;
        }

        public void Rotate(Vector3 rotation)
        {
            Matrix = MyMatrix4x4.RotationX(rotation.X) * MyMatrix4x4.RotationY(rotation.Y) * MyMatrix4x4.RotationZ(rotation.Z) * Matrix;
        }

        public void Scale(float scale)
        {
            Matrix = MyMatrix4x4.Scale(scale) * Matrix;
        }

    }
}
