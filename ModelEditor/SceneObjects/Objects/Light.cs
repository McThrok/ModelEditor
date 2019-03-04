using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Windows.Media;

namespace ModelEditor
{
    public class Light : ManipObj
    {
        public byte R { get; set; } = 255;
        public byte G { get; set; } = 255;
        public byte B { get; set; } = 255;

        public float M { get; set; } = 30;

        public float Ka { get; set; } = 0.1f;
        public float Kd { get; set; } = 0.7f;
        public float Ks { get; set; } = 0.5f;


        public Color GetColor(Vector3 normal, Vector3 toLight, Vector3 toCamera, Color col)
        {
            var ambient = Ka;
            var diffuse = Kd * Math.Max(0, Vector3.Dot(toLight.Normalized(), normal.Normalized()));

            var R = -Vector3.Reflect(toLight, normal);
            var specular = Ks * Math.Pow(Math.Max(0, Vector3.Dot(R.Normalized(), toCamera.Normalized())), M);

            var r = GetColor(ambient, diffuse, specular, col.R);
            var g = GetColor(ambient, diffuse, specular, col.G);
            var b = GetColor(ambient, diffuse, specular, col.B);

            return Color.FromArgb(255, r, g, b);
        }
        private byte GetColor(double ambient, double diffuse, double specular, byte col)
        {
            var result = ambient * col + diffuse * col + specular * 255;
            return Convert.ToByte(Math.Max(0, Math.Min(255, result)));
        }
    }
}
