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
    public class SceneObject
    {
        public Matrix4x4 Matrix { get; set; } = Matrix4x4.Identity;
        public List<SceneObject> Children { get; private set; } = new List<SceneObject>();

        public virtual string Name { get; set; }

        public virtual void Move(Vector3 CreateTranslation)
        {
            Matrix = Matrix4x4.CreateTranslation(CreateTranslation).Multiply(Matrix);
        }
        public virtual void Move(double x, double y, double z)
        {
            Move(new Vector3((float)x, (float)y, (float)z));
        }
        public virtual void MoveLoc(Vector3 CreateTranslation)
        {
            Matrix = Matrix.Multiply(Matrix4x4.CreateTranslation(CreateTranslation));
        }
        public virtual void MoveLoc(double x, double y, double z)
        {
            MoveLoc(new Vector3((float)x, (float)y, (float)z));
        }

        public virtual void Rotate(Vector3 CreateRotation)
        {
            Matrix = MyMatrix4x4.Compose(Matrix4x4.CreateRotationX(CreateRotation.X), Matrix4x4.CreateRotationY(CreateRotation.Y), Matrix4x4.CreateRotationZ(CreateRotation.Z), Matrix);
        }
        public virtual void Rotate(double x, double y, double z)
        {
            Rotate(new Vector3((float)x, (float)y, (float)z));
        }
        public virtual void RotateLoc(Vector3 CreateRotation)
        {
            Matrix = MyMatrix4x4.Compose(Matrix, Matrix4x4.CreateRotationX(CreateRotation.X), Matrix4x4.CreateRotationY(CreateRotation.Y), Matrix4x4.CreateRotationZ(CreateRotation.Z));
        }
        public virtual void RotateLoc(double x, double y, double z)
        {
            RotateLoc(new Vector3((float)x, (float)y, (float)z));
        }

        public virtual void Scale(double scale)
        {
            Matrix = Matrix4x4.CreateScale(Vector3.One * (float)scale).Multiply(Matrix);
        }
        public virtual void ScaleLoc(double scale)
        {
            Matrix = Matrix.Multiply(Matrix4x4.CreateScale(Vector3.One * (float)scale));
        }

        public Transform Transform
        {
            get
            {
                if (!Matrix4x4.Decompose(Matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation))
                {
                    throw new InvalidOperationException("Cannot decompose matrix");
                }
                return new Transform(translation, rotation.ToEuler(), scale);
            }
            set
            {
                Matrix = MyMatrix4x4.Transform(value.Position, value.Rotation, value.Scale);
            }
        }
    }

    public struct Transform
    {
        public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public Transform(Vector3 position, Vector3 rotation) : this(Vector3.Zero, Vector3.Zero, Vector3.One)
        {
        }

        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
    }

}
