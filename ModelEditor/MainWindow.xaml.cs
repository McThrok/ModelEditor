using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace ModelEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Engine Engine { get; set; }

        private float _positionChangeSpeed = 1f;
        private float _rotationChangeSpeed = (float)(Math.PI / 8);
        private float _scaleChangeSpeed = 1.2f;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoad;
        }

        private async void OnLoad(object sender, RoutedEventArgs e)
        {
            Engine = new Engine(BitmapContainer);
            objectList.ItemsSource = new ObservableCollection<SceneObject>() { Engine.Scene };
            Engine.Run();

            ViewportSlider.Value = 1;
            EyeSlider.Value = 0.1;

            var sceneNode = objectList.ItemContainerGenerator.ContainerFromItem(objectList.Items[0]) as TreeViewItem;
            sceneNode.IsExpanded = true;

            BitmapContainer.MouseDown += BitmapContainer_MouseDown;
        }

        private void BitmapContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //bitmapImage.Focus();         // Set Logical Focus
            //Keyboard.Focus(bitmapImage); // Set Keyboard Focus
            //FocusManager.SetIsFocusScope(bitmapImage, true);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Engine?.Input.OnMouseLeftButtonDown(e.GetPosition(BitmapContainer));
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            Engine?.Input.OnMouseLeftButtonUp();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Engine?.Input.OnKeyDown(e.Key);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            Engine?.Input.OnKeyUp(e.Key);
        }

        private void Torus_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.AddTorus(GetSelectedObj());
        }
        private void Empty_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.AddEmptyObject(GetSelectedObj());
        }
        private void Vertex_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.AddVertex(GetSelectedObj());
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.Delete(GetSelectedObj());
        }

        private void FocuCamera(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedObj();
            if (item != null)
                Engine.Scene.Camera.SetTarget(item);
        }

        private SceneObject GetSelectedObj()
        {
            return objectList.SelectedItem as SceneObject;
        }


        private void PositionXUp(object sender, RoutedEventArgs e) { GetSelectedObj().Move(_positionChangeSpeed, 0, 0); }
        private void PositionXDown(object sender, RoutedEventArgs e) { GetSelectedObj().Move(-_positionChangeSpeed, 0, 0); }
        private void PositionYUp(object sender, RoutedEventArgs e) { GetSelectedObj().Move(0, _positionChangeSpeed, 0); }
        private void PositionYDown(object sender, RoutedEventArgs e) { GetSelectedObj().Move(0, -_positionChangeSpeed, 0); }
        private void PositionZUp(object sender, RoutedEventArgs e) { GetSelectedObj().Move(0, 0, _positionChangeSpeed); }
        private void PositionZDown(object sender, RoutedEventArgs e) { GetSelectedObj().Move(0, 0, -_positionChangeSpeed); }

        private void RotationXUp(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(_rotationChangeSpeed, 0, 0); }
        private void RotationXDown(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(-_rotationChangeSpeed, 0, 0); }
        private void RotationYUp(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(0, _rotationChangeSpeed, 0); }
        private void RotationYDown(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(0, -_rotationChangeSpeed, 0); }
        private void RotationZUp(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(0, 0, _rotationChangeSpeed); }
        private void RotationZDown(object sender, RoutedEventArgs e) { GetSelectedObj().RotateLoc(0, 0, -_rotationChangeSpeed); }

        private void ScaleUp(object sender, RoutedEventArgs e) { GetSelectedObj().ScaleLoc(_scaleChangeSpeed); }
        private void ScaleDown(object sender, RoutedEventArgs e) { GetSelectedObj().ScaleLoc(1 / _scaleChangeSpeed); }

        private void CbxAnaglyph_Checked(object sender, RoutedEventArgs e) { Engine.Renderer.Anaglyphic = true; }
        private void CbxAnaglyph_Unchecked(object sender, RoutedEventArgs e) { Engine.Renderer.Anaglyphic = false; }

        private void Anaglyph_Change(object sender, RoutedPropertyChangedEventArgs<double> e) { Engine.Renderer.EyeDistance = (float)(0.3f * e.NewValue); }
        private void Viewport_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) { Engine.Renderer.ViewportDistance = (float)(5 + 35f * e.NewValue); }

        private void SelectedObjectChange(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null)
                objectMenu.Visibility = Visibility.Visible;
            else
                objectMenu.Visibility = Visibility.Collapsed;

            if (e.NewValue is SceneObject item)
            {
                Engine.Scene.Cursor.GlobalMatrix = item.GlobalMatrix;

                TorusMenu.Visibility = item.Name == nameof(Torus) ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        #region dragAndDrop
        private Point _lastMouseDown;
        private SceneObject _draggedItem, _target;

        private void TreeView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                _lastMouseDown = e.GetPosition(objectList);

        }
        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentPosition = e.GetPosition(objectList);

                    if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) || (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                    {
                        _draggedItem = objectList.SelectedItem as SceneObject;
                        if (_draggedItem != null)
                        {
                            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(objectList, objectList.SelectedValue, DragDropEffects.Move);
                            //Checking target is not null and item is dragging(moving)
                            if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
                            {
                                // A Move drop was accepted
                                if (_draggedItem.Id != _target.Id)
                                {
                                    CopyItem(_draggedItem, _target);
                                    _target = null;
                                    _draggedItem = null;
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                Point currentPosition = e.GetPosition(objectList);

                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) || (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    SceneObject item = GetNearestContainer(e.OriginalSource as UIElement);
                    if (CheckDropTarget(_draggedItem, item))
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                e.Handled = true;
            }
            catch (Exception)
            {
            }
        }
        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                // Verify that this is a valid drop and then store the drop target
                SceneObject TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
                if (TargetItem != null && _draggedItem != null)
                {
                    _target = TargetItem;
                    e.Effects = DragDropEffects.Move;

                }
            }
            catch (Exception)
            {
            }

        }
        private bool CheckDropTarget(SceneObject _sourceItem, SceneObject _targetItem)
        {
           return !_targetItem.IsEqualOrDescendantOf(_sourceItem);
          
        }
        private void CopyItem(SceneObject _sourceItem, SceneObject _targetItem)
        {
            var global = _sourceItem.GlobalMatrix;
            _sourceItem.Parent.Children.Remove(_sourceItem);
            _sourceItem.Parent = _targetItem;
            _targetItem.Children.Add(_sourceItem);
            _sourceItem.GlobalMatrix = global;
        }

        private void Hold_click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.Cursor.HoldObject(Engine.Scene.Children);
        }

        private void HoldAll_click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.Cursor.HoldAllObjects(Engine.Scene.Children);

        }

        private void Release_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.Cursor.ReleaseObjects();
        }

        private SceneObject GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }

            var result = container.Header as SceneObject;
            return result;
        }
        #endregion
    }
}
