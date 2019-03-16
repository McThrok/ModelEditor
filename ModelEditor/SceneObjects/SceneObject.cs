using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Collections.ObjectModel;

namespace ModelEditor
{
    public class SceneObject
    {
        public string Name { get; set; }

        public Guid Id { get; set; } = Guid.NewGuid();
        public SceneObject Parent { get; set; }
        public ObservableCollection<SceneObject> Children { get; private set; } = new ObservableCollection<SceneObject>();

        public delegate void MatrixDelegate(object sender, ChangeMatrixEventArgs e);

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
            Matrix *= MyMatrix4x4.CreateRotation(CreateRotation);
        }
        public virtual void Rotate(double x, double y, double z)
        {
            Rotate(new Vector3((float)x, (float)y, (float)z));
        }
        public virtual void RotateLoc(Vector3 CreateRotation)
        {
            Matrix = MyMatrix4x4.CreateRotation(CreateRotation) * Matrix;
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

        public bool IsEqualOrDescendantOf(SceneObject obj)
        {
            var node = this;

            while (node != null)
            {
                if (node.Id == obj.Id)
                    break;

                node = node.Parent;
            }

            var result = node != null;

            return result;
        }

        private Matrix4x4 _matrix = Matrix4x4.Identity;
        public event MatrixDelegate MatrixChange;
        public Matrix4x4 Matrix
        {
            get
            {
                return _matrix;
            }
            set
            {
                if (_matrix != value)
                {
                    var old = _matrix;
                    _matrix = value;
                    MatrixChange?.Invoke(this, new ChangeMatrixEventArgs(old, value));
                }
            }
        }

        public event MatrixDelegate GlobalMatrixChange;
        public Matrix4x4 GlobalMatrix
        {
            get
            {
                return Matrix * (Parent != null ? Parent.GlobalMatrix : Matrix4x4.Identity);
            }
            set
            {
                if (GlobalMatrix != value)
                {
                    var old = GlobalMatrix;
                    Matrix = value * Parent.GlobalMatrix.Inversed();
                    GlobalMatrixChange?.Invoke(this, new ChangeMatrixEventArgs(old, value));
                }
            }
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

    public class ChangeMatrixEventArgs : EventArgs
    {
        public Matrix4x4 OldMatrix { get; set; }
        public Matrix4x4 NewMatrix { get; set; }

        public ChangeMatrixEventArgs(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
        {
            OldMatrix = oldMatrix;
            NewMatrix = newMatrix;
        }

    }
}
