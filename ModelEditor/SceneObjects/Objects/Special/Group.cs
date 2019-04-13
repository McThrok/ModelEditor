using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.ComponentModel;

namespace ModelEditor
{
    public class GroupObject : SceneObject
    {
        public List<SceneObject> Objects { get; set; } = new List<SceneObject>();

        public GroupObject()
        {
            Name = nameof(Cursor);
        }
        #region manipulation
        public virtual void Move(Vector3 CreateTranslation)
        {
            Matrix = Matrix4x4.CreateTranslation(CreateTranslation).Multiply(Matrix);
        }
        public virtual void MoveLoc(Vector3 CreateTranslation)
        {
            Matrix = Matrix.Multiply(Matrix4x4.CreateTranslation(CreateTranslation));
        }

        public virtual void Rotate(Vector3 CreateRotation)
        {
            Matrix *= MyMatrix4x4.CreateRotation(CreateRotation);
        }
        public virtual void RotateLoc(Vector3 CreateRotation)
        {
            Matrix = MyMatrix4x4.CreateRotation(CreateRotation) * Matrix;
        }

        public virtual void Scale(double x, double y, double z)
        {
            Matrix = Matrix4x4.CreateScale((float)x, (float)y, (float)z).Multiply(Matrix);
        }
        public virtual void ScaleLoc(double x, double y, double z)
        {
            Matrix = Matrix.Multiply(Matrix4x4.CreateScale((float)x, (float)y, (float)z));
        }
        #endregion

    }
}
