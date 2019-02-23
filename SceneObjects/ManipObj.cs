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
        public Matrix4x4 Matrix { get; set; } = MyMatrix4x4.Identity;

        public void Move(Vector3 translate)
        {
            Matrix = MyMatrix4x4.Translate(translate).Multiply(Matrix);
        }

        public void Move(double x, double y, double z)
        {
            Move(new Vector3((float)x, (float)y, (float)z));
        }

        public void Rotate(Vector3 rotation)
        {
            Matrix = MyMatrix4x4.Compose(MyMatrix4x4.RotationX(rotation.X), MyMatrix4x4.RotationY(rotation.Y), MyMatrix4x4.RotationZ(rotation.Z), Matrix);
        }

        public void Rotate(double x, double y, double z)
        {
            Rotate(new Vector3((float)x, (float)y, (float)z));
        }

        public void Scale(double scale)
        {
            Matrix = MyMatrix4x4.Scale((float)scale).Multiply(Matrix);
        }

    }
}
