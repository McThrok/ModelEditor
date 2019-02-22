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
    public struct Edge
    {
        public Edge(int idxA, int idxB)
        {
            IdxA = idxA;
            IdxB = idxB;
        }
        public int IdxA { get; set; }
        public int IdxB { get; set; }
    }
}


