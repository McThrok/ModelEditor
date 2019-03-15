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
    public class Camera : SceneObject
    {
        public float TargetOffset { get; set; } = 10;

        public Camera()
        {
            Name = nameof(Camera);
            SetTarget(Vector3.Zero);

        }

        public void SetTarget(Vector3 position)
        {
            var pos = GlobalMatrix.Inversed().Multiply(position.ToVector4());
            pos.Z += TargetOffset;

            MoveLoc(pos.ToVector3());
        }
        public void SetTarget(SceneObject obj)
        {
            var pos = obj.GlobalMatrix.Multiply(Vector3.Zero.ToVector4());
            SetTarget(pos.ToVector3());
        }

    }
}
