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
    public class BernsteinCurve : SceneObject
    {
        private static int _count = 0;
        public BernsteinCurve()
        {
            Name = nameof(BernsteinCurve) + " " + _count++.ToString();
            Holdable = true;

            GlobalMatrixChange += OnMatrixChange;
            Children.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (SceneObject child in e.NewItems)
                    child.GlobalMatrixChange += OnMatrixChange;

            if (e.OldItems != null)
                foreach (SceneObject child in e.OldItems)
                    child.GlobalMatrixChange -= OnMatrixChange;
        }

        private void OnMatrixChange(object sender, ChangeMatrixEventArgs e)
        {
            Recalculate();
        }

        public void Recalculate()
        {
            var vertices = Children.Select(x => x.GlobalMatrix.Multiply(Vector3.Zero.ToVector4())).ToList();
        }



    }
}
