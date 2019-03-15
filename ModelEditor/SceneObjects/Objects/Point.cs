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
    public class ScenePoint : SceneObject, IScreenRenderable
    {
        public ScenePoint()
        {
            Name = nameof(Point);
        }

        public ScreenRenderData GetRenderData()
        {
            var data = new ScreenRenderData();
            data.Pixels = new List<Vector2>();
            data.Pixels.Add(new Vector2(0, 0));

            return data;
        }
    }
}
