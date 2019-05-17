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
        private int _range = 1;
        public Guid LinkId { get; set; } = Guid.Empty;

        public Vertex()
        {
            Name = nameof(Vertex) + _count++.ToString();
            Holdable = true;
        }
        public Vertex(string data)
        {
            var parts = data.Split(' ');
            Name = parts[0];
            StringToPosition(parts[1]);
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
                        _screenRenderData.Pixels.Add(new Vector2Int(i, j));
                    }
                }
            }

            return _screenRenderData;
        }

        public override string[] GetData()
        {
            var data = new string[2];
            data[0] = "point 1";
            data[1] = Name.Replace(' ', '_');
            data[1] += " " + PositionToString();

            return data;
        }
    }
}
