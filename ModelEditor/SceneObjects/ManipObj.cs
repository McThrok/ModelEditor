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

        public virtual void Move(Vector3 translate)
        {
            Matrix = MyMatrix4x4.Translate(translate).Multiply(Matrix);
        }
        public virtual void Move(double x, double y, double z)
        {
            Move(new Vector3((float)x, (float)y, (float)z));
        }
        public virtual void MoveLoc(Vector3 translate)
        {
            Matrix = Matrix.Multiply(MyMatrix4x4.Translate(translate));
        }
        public virtual void MoveLoc(double x, double y, double z)
        {
            MoveLoc(new Vector3((float)x, (float)y, (float)z));
        }
                
        public virtual void Rotate(Vector3 rotation)
        {
            Matrix = MyMatrix4x4.Compose(MyMatrix4x4.RotationX(rotation.X), MyMatrix4x4.RotationY(rotation.Y), MyMatrix4x4.RotationZ(rotation.Z), Matrix);
        }
        public virtual void Rotate(double x, double y, double z)
        {
            Rotate(new Vector3((float)x, (float)y, (float)z));
        }
        public virtual void RotateLoc(Vector3 rotation)
        {
            Matrix = MyMatrix4x4.Compose(Matrix, MyMatrix4x4.RotationX(rotation.X), MyMatrix4x4.RotationY(rotation.Y), MyMatrix4x4.RotationZ(rotation.Z));
        }
        public virtual void RotateLoc(double x, double y, double z)
        {
            RotateLoc(new Vector3((float)x, (float)y, (float)z));
        }
                
        public virtual void Scale(double scale)
        {
            Matrix = MyMatrix4x4.Scale((float)scale).Multiply(Matrix);
        }
        public virtual void ScaleLoc(double scale)
        {
            Matrix = Matrix.Multiply(MyMatrix4x4.Scale((float)scale));
        }

    }
}
