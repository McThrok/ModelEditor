﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;

namespace ModelEditor
{
    public struct ElipsoidRenderPointData
    {
        public float z;
        public Vector4 normal;
    }

    public class Elipsoid : ManipObj
    {
        public double RadiusX { get; set; } = 1;
        public double RadiusY { get; set; } = 1;
        public double RadiusZ { get; set; } = 1;

        public byte R { get; set; } = 255;
        public byte G { get; set; } = 255;
        public byte B { get; set; } = 0;


        public Elipsoid()
        {
            Name = nameof(Elipsoid);
        }

        //public ElipsoidRenderPointData? CastRay(float x, float y, Matrix4x4 invMat)
        //{
        //    var m = new Matrix4x4();
        //    m.M11 = (float)(1f / (RadiusX * RadiusX));
        //    m.M22 = (float)(1f / (RadiusY * RadiusY));
        //    m.M33 = (float)(1f / (RadiusZ * RadiusZ));
        //    m.M44 = -1;
        //    //m.M11 = (float)(RadiusX);
        //    //m.M22 = (float)(RadiusY);
        //    //m.M33 = (float)(RadiusZ);
        //    //m.M44 = -1;

        //    m = invMat.Transposed().Multiply(m.Multiply(invMat));

        //    var c = (m.M11 * x + m.M12 * y + m.M14) * x
        //            + (m.M21 * x + m.M22 * y + m.M24) * y
        //            + (m.M41 * x + m.M42 * y + m.M44);
        //    var b = m.M13 * x + m.M23 * y + m.M43 + m.M31 * x + m.M32 * y + m.M34;
        //    var a = m.M33;
                
        //    var delta = b * b - 4 * a * c;

        //    if (delta < 0)
        //        return null;


        //    var z = (float)((-b - Math.Sqrt(delta)) / (2 * a));

        //    var result = new ElipsoidRenderPointData();
        //    result.z = z;
        //    result.normal = invMat.Multiply(new Vector4(x, y, result.z, 1));
        //    result.normal = (new Vector4(x, y, result.z, 1)).Multiply(invMat);
        //    result.normal.W = 0;
        //    result.normal = result.normal / result.normal.Length();

        //    return result;
        //}
       

        //public override void Move(Vector3 translate)
        //{
        //    var inv = MyMatrix4x4.Translate(translate).Inversed();
        //    Matrix = MyMatrix4x4.Compose(inv.Transposed(), Matrix, inv);
        //}
        //public override void Move(double x, double y, double z)
        //{
        //    Move(new Vector3((float)x, (float)y, (float)z));
        //}
        //public override void MoveLoc(Vector3 translate)
        //{
        //    Move(translate);
        //}
        //public override void MoveLoc(double x, double y, double z)
        //{
        //    MoveLoc(new Vector3((float)x, (float)y, (float)z));
        //}

        //public override void Rotate(Vector3 rotation)
        //{
        //    var inv = MyMatrix4x4.Compose(MyMatrix4x4.RotationX(rotation.X), MyMatrix4x4.RotationY(rotation.Y), MyMatrix4x4.RotationZ(rotation.Z)).Inversed();
        //    Matrix = MyMatrix4x4.Compose(inv.Transposed(), Matrix, inv);
        //}
        //public override void Rotate(double x, double y, double z)
        //{
        //    Rotate(new Vector3((float)x, (float)y, (float)z));
        //}
        //public override void RotateLoc(Vector3 rotation)
        //{
        //    Rotate(rotation);
        //}
        //public override void RotateLoc(double x, double y, double z)
        //{
        //    RotateLoc(new Vector3((float)x, (float)y, (float)z));
        //}

        //public override void Scale(double scale)
        //{
        //    var inv = MyMatrix4x4.Scale((float)scale).Inversed();
        //    Matrix = MyMatrix4x4.Compose(inv.Transposed(), Matrix, inv);
        //}
        //public override void ScaleLoc(double scale)
        //{
        //    Scale(scale);
        //}
    }
}
