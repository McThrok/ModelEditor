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
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int X;
        public int Y;

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2Int && Equals((Vector2Int)obj);
        }

        public bool Equals(Vector2Int other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return  "(" + X.ToString() + ", " + Y.ToString() + ")";
        }

        public static bool operator ==(Vector2Int left, Vector2Int right)
        {
            return left.X == right.X && left.Y == right.Y;
        }
        public static bool operator !=(Vector2Int left, Vector2Int right)
        {
            return !(left == right);
        }

    }

}
