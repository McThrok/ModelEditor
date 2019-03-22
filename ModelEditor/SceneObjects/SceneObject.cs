using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ModelEditor
{
    public class SceneObject : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        public bool Holdable { get; set; }

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

        public virtual void Scale(double x, double y, double z)
        {
            Matrix = Matrix4x4.CreateScale((float)x, (float)y, (float)z).Multiply(Matrix);
        }
        public virtual void ScaleLoc(double x, double y, double z)
        {
            Matrix = Matrix.Multiply(Matrix4x4.CreateScale((float)x, (float)y, (float)z));
        }

        public bool IsEqualOrDescendantOf(SceneObject obj)
        {
            if (obj == null)
                return false;

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
                    var oldGlobal = GlobalMatrix;

                    _matrix = value;
                    _recalculateTransform = true;

                    MatrixChange?.Invoke(this, new ChangeMatrixEventArgs(old, Matrix));
                    GlobalMatrixChange?.Invoke(this, new ChangeMatrixEventArgs(oldGlobal, GlobalMatrix));
                    NotifyAllChanges();
                }
            }
        }
        public void InvokePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        public void SetParent(SceneObject parent)
        {
            var global = GlobalMatrix;
            if (Parent != null)
            {
                Parent.Children.Remove(this);
            }
            if (parent != null && !parent.IsEqualOrDescendantOf(this))
            {
                Parent = parent;
                parent.Children.Add(this);
            }
            GlobalMatrix = global;
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
                    var old = _matrix;
                    var oldGlobal = GlobalMatrix;

                    var parentGlobalMatrix = Parent != null ? Parent.GlobalMatrix : Matrix4x4.Identity;
                    Matrix = value * parentGlobalMatrix.Inversed();
                    _recalculateTransform = true;

                    MatrixChange?.Invoke(this, new ChangeMatrixEventArgs(old, Matrix));
                    GlobalMatrixChange?.Invoke(this, new ChangeMatrixEventArgs(oldGlobal, GlobalMatrix));
                    NotifyAllChanges();
                }
            }
        }

        private bool _recalculateTransform = true;
        private Transform _transform;
        public Transform Transform
        {
            get
            {
                if (_recalculateTransform)
                {
                    _recalculateTransform = false;
                    if (!Matrix4x4.Decompose(Matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation))
                    {
                        throw new InvalidOperationException("Cannot decompose matrix");
                    }
                    _transform = new Transform(translation, rotation.ToEuler(), scale);
                }

                return _transform;
            }
            set
            {
                Matrix = MyMatrix4x4.Transform(value.Position, value.Rotation, value.Scale);
            }
        }

        #region UIBinding
        public float PositionX
        {
            get => Round(Transform.Position.X);
            set
            {
                var t = Transform;
                t.Position = new Vector3(value, t.Position.Y, t.Position.Z);
                Transform = t;
            }
        }
        public float PositionY
        {
            get => Round(Transform.Position.Y);
            set
            {
                var t = Transform;
                t.Position = new Vector3(t.Position.X, value, t.Position.Z);
                Transform = t;
            }
        }
        public float PositionZ
        {
            get => Round(Transform.Position.Z);
            set
            {
                var t = Transform;
                t.Position = new Vector3(t.Position.X, t.Position.Y, value);
                Transform = t;
            }
        }

        public float RotationX
        {
            get => Round(Transform.Rotation.X.ToAngles());
            set
            {
                var t = Transform;
                t.Rotation = new Vector3(value.ToRadians(), t.Rotation.Y, t.Rotation.Z);
                Transform = t;
            }
        }
        public float RotationY
        {
            get => Round(Transform.Rotation.Y.ToAngles());
            set
            {
                var t = Transform;
                t.Rotation = new Vector3(t.Rotation.X, value.ToRadians(), t.Rotation.Z);
                Transform = t;
            }
        }
        public float RotationZ
        {
            get => Round(Transform.Rotation.Z.ToAngles());
            set
            {
                var t = Transform;
                t.Rotation = new Vector3(t.Rotation.X, t.Rotation.Y, value.ToRadians());
                Transform = t;
            }
        }

        public float ScaleX
        {
            get => Round(Transform.Scale.X);
            set
            {
                var t = Transform;
                t.Scale = new Vector3(value, t.Scale.Y, t.Scale.Z);
                Transform = t;
            }
        }
        public float ScaleY
        {
            get => Round(Transform.Scale.Y);
            set
            {
                var t = Transform;
                t.Scale = new Vector3(t.Scale.X, value, t.Scale.Z);
                Transform = t;
            }
        }
        public float ScaleZ
        {
            get => Round(Transform.Scale.Z);
            set
            {
                var t = Transform;
                t.Scale = new Vector3(t.Scale.X, t.Scale.Y, value);
                Transform = t;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyAllChanges()
        {
            string[] props = { nameof(PositionX), nameof(PositionY), nameof(PositionZ),
                                nameof(RotationX), nameof(RotationY), nameof(RotationZ),
                                nameof(ScaleX), nameof(ScaleY), nameof(ScaleZ),};

            foreach (var prop in props)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private float Round(float v)
        {
            return (float)Math.Round(v, 2);
        }
        #endregion

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
