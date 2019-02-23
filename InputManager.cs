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
        private Dictionary<Move, bool> _moveActions;

        public InputManager(WriteableBitmap writeableBitmap, Scene scene)
        {
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

        public void OnMouseLeftButtonDown(Point posiiton)
        {
        }

        public void OnMouseLeftButtonUp(Point posiiton)
        {
        }

        public void OnMouseWheel(int delta)
        {
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
