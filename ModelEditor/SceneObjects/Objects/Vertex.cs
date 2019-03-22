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
    public class Vertex : SceneObject, IScreenRenderable
    {
        private static int _count = 0;
        private ScreenRenderData _screenRenderData;
        private int _range = 5;

        public Vertex()
        {
            Name = nameof(Vertex) + _count++.ToString();
            Holdable = true;
        }

        public ScreenRenderData GetScreenRenderData()
        {
            if (_screenRenderData == null)
            {
                _screenRenderData = new ScreenRenderData();
                _screenRenderData.Pixels = new List<Vector2Int>();

                for (int i = -_range; i < _range + 1; i++)
                {
                    for (int j = -_range; j < _range + 1; j++)
                    {
                        if (i * i + j * j < _range * _range)
                            _screenRenderData.Pixels.Add(new Vector2Int(i, j));
                    }
                }
            }

            return _screenRenderData;
        }
    }
}
