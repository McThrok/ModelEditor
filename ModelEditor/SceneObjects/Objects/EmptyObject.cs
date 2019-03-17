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
    public class EmptyObject : SceneObject
    {
        private static int _count = 0;
        public EmptyObject()
        {
            Name = nameof(EmptyObject) + _count++.ToString();
            Holdable = true;
        }

    }
}
