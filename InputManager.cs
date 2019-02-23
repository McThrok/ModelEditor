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
            InitMouseHandling();
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
        private void InitMouseHandling()
        {
            _bitmapConatiner.MouseLeftButtonDown += OnMouseLeftButtonDown;
            _bitmapConatiner.MouseLeftButtonUp += OnMouseLeftButtonUp;
            _bitmapConatiner.MouseWheel += OnMouseWheel;
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
            _scene.Camera.Move(cameraSpeed * moveDir);

        }
        private void UpdateRotations()
        {
            if (!_lastMousePosition.HasValue)
                return;

            var position = Mouse.GetPosition(_bitmapConatiner);
            var rotationSpeed = 0.001f;
            var diff = position - _lastMousePosition.Value;
            _scene.Camera.Rotate(rotationSpeed * diff.Y, 0, 0);
            _scene.Camera.Rotate(0, rotationSpeed * diff.X, 0);
            

           _lastMousePosition = position;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(_bitmapConatiner);
            if (position.X >= 0 && position.Y >= 0 && position.X < _writeableBitmap.PixelWidth && position.Y < _writeableBitmap.PixelHeight)
                _lastMousePosition = position;
        }
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _lastMousePosition = null;
        }
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scaleSpeed = 0.01;
            _scene.Scale(scaleSpeed * e.Delta);
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
