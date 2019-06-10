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
        private readonly BitmapBuffer _bb;
        private readonly Panel _bitmapConatiner;
        private readonly RayCaster _rayCaster;

        private Dictionary<Move, bool> _moveActions;
        public Vector3 CameraRotation = Vector3.Zero;
        private Point? _lastMousePosition;
        private bool _ctrlPressed = false;

        public InputManager(Panel bitmapConatiner, BitmapBuffer bb, Scene scene, RayCaster rayCaster)
        {
            _bitmapConatiner = bitmapConatiner;
            _bb = bb;
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
                var speed = 0.3f;
                var vec = _scene.Camera.Matrix.Multiply(new Vector4(speed * moveDir, 0));
                var translateMatrix = Matrix4x4.CreateTranslation(vec.ToVector3());
                _scene.Cursor.Matrix = _scene.Cursor.Matrix.Multiply(translateMatrix);
            }
            else
            {
                var speed = 0.6f;
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

            CameraRotation.X += (float)(rotationSpeed * diff.Y);
            CameraRotation.X = (float)Math.Max(-Math.PI * 0.4, Math.Min(Math.PI * 0.4, CameraRotation.X));
            CameraRotation.Y += (float)(rotationSpeed * diff.X);

            var rotationX = Matrix4x4.CreateRotationX(CameraRotation.X);
            var rotationY = Matrix4x4.CreateRotationY(CameraRotation.Y);
            var move = Matrix4x4.CreateTranslation(translation);

            _scene.Camera.Matrix = rotationX * rotationY * move;
        }

        public SceneObject OnMouseLeftButtonDown(Point pos)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= _bb.Width || pos.Y >= _bb.Height)
                return null;

            var toCheck = new Stack<SceneObject>();
            toCheck.Push(_scene);
            float best = float.MaxValue;
            SceneObject toSelect = null;

            while (toCheck.Count > 0)
            {
                var obj = toCheck.Pop();

                if (obj.Holdable)
                {
                    var objPos = _rayCaster.GetScreenPositionOf(obj);
                    if (objPos != Vector2Int.Empty)
                    {
                        var x = objPos.X - pos.X;
                        var y = objPos.Y - pos.Y;
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

                foreach (var child in obj.HiddenChildren)
                    toCheck.Push(child);
            }

            return toSelect;
        }
        public void OnMouseRightButtonDown(Point position)
        {
            var pos = position;
            if (pos.X >= 0 && pos.Y >= 0 && pos.X < _bb.Width && pos.Y < _bb.Height)
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
