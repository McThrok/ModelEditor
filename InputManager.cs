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
        private readonly WriteableBitmap _writeableBitmap;
        private readonly Panel _bitmapConatiner;
        private Dictionary<Move, bool> _moveActions;

        private Point? _lastMousePosition;

        public InputManager(Panel bitmapConatiner, WriteableBitmap writeableBitmap, Scene scene)
        {
            _bitmapConatiner = bitmapConatiner;
            _writeableBitmap = writeableBitmap;
            _scene = scene;

            InitMoveActions();
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

            var cameraSpeed = 0.1f;
            var translateMatrix = MyMatrix4x4.Translate(cameraSpeed * moveDir);
            _scene.Camera.Matrix = _scene.Camera.Matrix.Multiply(translateMatrix);
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
            var rotationX = MyMatrix4x4.RotationX((float)(rotationSpeed * diff.Y));
            var rotationY = MyMatrix4x4.RotationY((float)(rotationSpeed * diff.X));
            var currRotation = Matrix4x4.CreateFromQuaternion(rotation);
            var trainslation = MyMatrix4x4.Translate(translation);

            _scene.Camera.Matrix = MyMatrix4x4.Compose(trainslation, rotationX, currRotation, rotationY);
        }

        public void OnMouseLeftButtonDown(Point position)
        {
            position.X += _writeableBitmap.PixelWidth / 2;
            position.Y += _writeableBitmap.Height / 2;
            if (position.X >= 0 && position.Y >= 0 && position.X < _writeableBitmap.PixelWidth && position.Y < _writeableBitmap.PixelHeight)
                _lastMousePosition = position;
        }
        public void OnMouseLeftButtonUp(Point position)
        {
            _lastMousePosition = null;
        }
        public void OnMouseWheel(int delta)
        {
            var scaleSpeed = 0.001;
            _scene.Scale(Math.Exp(scaleSpeed * delta));
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
            }

        }

    }
}
