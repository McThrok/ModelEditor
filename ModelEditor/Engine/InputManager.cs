using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Controls;

namespace ModelEditor
{
    public enum Move
    {
        Forward,
        Back,
        Left,
        Right,
        Up,
        Down
    }

    public class InputManager
    {
        private readonly Scene _scene;
        private readonly WriteableBitmap _wb;
        private readonly Panel _bitmapConatiner;
        private readonly RayCaster _rayCaster;

        private Dictionary<Move, bool> _moveActions;
        private Vector3 _cameraRotation = Vector3.Zero;
        private Point? _lastMousePosition;
        private bool _ctrlPressed = false;

        public InputManager(Panel bitmapConatiner, WriteableBitmap writeableBitmap, Scene scene, RayCaster rayCaster)
        {
            _bitmapConatiner = bitmapConatiner;
            _wb = writeableBitmap;
            _scene = scene;
            _rayCaster = rayCaster;

            InitMoveActions();

            _scene.Cursor.GlobalMatrixChange += UpdateCursorScreenPosition;
            _scene.Camera.GlobalMatrixChange += UpdateCursorScreenPosition;

            UpdateCursorScreenPosition(this, new ChangeMatrixEventArgs(_scene.Cursor.GlobalMatrix, _scene.Cursor.GlobalMatrix));
        }

        private void UpdateCursorScreenPosition(object sender, ChangeMatrixEventArgs e)
        {
            _scene.Cursor.ScreenPosition = _rayCaster.GetScreenPositionOf(_scene.Cursor);
        }

        private void InitMoveActions()
        {
            _moveActions = new Dictionary<Move, bool>();
            _moveActions[Move.Forward] = false;
            _moveActions[Move.Back] = false;
            _moveActions[Move.Left] = false;
            _moveActions[Move.Right] = false;
            _moveActions[Move.Up] = false;
            _moveActions[Move.Down] = false;
        }

        public void Update(double deltaTime)
        {
            UpdateMovement(deltaTime);
            UpdateRotations();
        }
        private void UpdateMovement(double deltaTime)
        {
            var moveDir = Vector3.Zero;
            if (_moveActions[Move.Forward]) moveDir.Z--;
            if (_moveActions[Move.Back]) moveDir.Z++;
            if (_moveActions[Move.Left]) moveDir.X--;
            if (_moveActions[Move.Right]) moveDir.X++;
            if (_moveActions[Move.Up]) moveDir.Y++;
            if (_moveActions[Move.Down]) moveDir.Y--;


            if (_ctrlPressed)
            {
                var speed = 0.1f;
                var translateMatrix = Matrix4x4.CreateTranslation(speed * moveDir);
                _scene.Cursor.Matrix = _scene.Cursor.Matrix.Multiply(translateMatrix);
            }
            else
            {
                var speed = 0.3f;
                var translateMatrix = Matrix4x4.CreateTranslation(speed * moveDir);
                _scene.Camera.Matrix = _scene.Camera.Matrix.Multiply(translateMatrix);
            }
        }

        private void UpdateRotations()
        {
            if (!_lastMousePosition.HasValue)
                return;

            var position = Mouse.GetPosition(_bitmapConatiner);
            var diff = position - _lastMousePosition.Value;
            _lastMousePosition = position;

            if (!Matrix4x4.Decompose(_scene.Camera.Matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation))
            {
                throw new InvalidOperationException("Cannot decompose matrix");
            }

            var rotationSpeed = 0.002f;

            _cameraRotation.X += (float)(rotationSpeed * diff.Y);
            _cameraRotation.X = (float)Math.Max(-Math.PI * 0.4, Math.Min(Math.PI * 0.4, _cameraRotation.X));
            _cameraRotation.Y += (float)(rotationSpeed * diff.X);

            var rotationX = Matrix4x4.CreateRotationX(_cameraRotation.X);
            var rotationY = Matrix4x4.CreateRotationY(_cameraRotation.Y);
            var move = Matrix4x4.CreateTranslation(translation);

            _scene.Camera.Matrix = MyMatrix4x4.Compose(move, rotationY, rotationX);
        }

        public SceneObject OnMouseLeftButtonDown(Point pos)
        {
            //TODO: refactor

            if (pos.X >= 0 && pos.Y >= 0 && pos.X < _wb.PixelWidth && pos.Y < _wb.PixelHeight)
            {
                var position = new Vector2((float)(1.0f * pos.X / _wb.PixelWidth * 2 - 1), (float)((1 - 1.0f * pos.Y / _wb.PixelHeight) * 2 - 1));
                var projection = MyMatrix4x4.CreatePerspectiveFieldOfView(0.8f, 1.0f * _wb.PixelWidth / _wb.PixelHeight, 0.1f, 100);

                Stack<SceneObject> toCheck = new Stack<SceneObject>(_scene.Children);
                float best = float.MaxValue;
                SceneObject toSelect = null;

                var matrix = _scene.Camera.GlobalMatrix.Inversed() * projection;

                while (toCheck.Count > 0)
                {
                    var obj = toCheck.Pop();

                    if (obj.Holdable)
                    {
                        var objPos = (obj.GlobalMatrix * matrix).Multiply(Vector3.Zero.ToVector4());

                        if (objPos.Z > 0)
                        {
                            objPos /= objPos.W;
                            var x = objPos.X - position.X;
                            var y = objPos.Y - position.Y;
                            var dist = (float)Math.Sqrt(x * x + y * y);
                            if (dist < best)
                            {
                                best = dist;
                                toSelect = obj;
                            }
                        }

                    }

                    foreach (var child in obj.Children)
                        toCheck.Push(child);
                }

                return toSelect;
            }

            return null;
        }
        public void OnMouseRightButtonDown(Point position)
        {
            var pos = position;
            if (pos.X >= 0 && pos.Y >= 0 && pos.X < _wb.PixelWidth && pos.Y < _wb.PixelHeight)
                _lastMousePosition = pos;
        }
        public void OnMouseRightButtonUp()
        {
            _lastMousePosition = null;
        }
        public void OnKeyDown(Key key)
        {
            HandleMoveAction(key, true);
        }
        public void OnKeyUp(Key key)
        {
            HandleMoveAction(key, false);
        }
        private void HandleMoveAction(Key key, bool down)
        {
            switch (key)
            {
                case Key.W: _moveActions[Move.Forward] = down; break;
                case Key.S: _moveActions[Move.Back] = down; break;
                case Key.A: _moveActions[Move.Left] = down; break;
                case Key.D: _moveActions[Move.Right] = down; break;
                case Key.Q: _moveActions[Move.Up] = down; break;
                case Key.E: _moveActions[Move.Down] = down; break;
                case Key.LeftCtrl: _ctrlPressed = down; break;
            }

        }

    }
}
