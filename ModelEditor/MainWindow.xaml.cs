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
using System.Windows.Controls.Primitives;

namespace ModelEditor
{

    public partial class MainWindow : Window, INotifyPropertyChanged
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

            //init sliders
            ViewportSlider.Value = 1;
            EyeSlider.Value = 0.1;

            //expand scene list
            var sceneNode = objectList.ItemContainerGenerator.ContainerFromItem(objectList.Items[0]) as TreeViewItem;
            sceneNode.IsExpanded = true;

            //TODO: clear focus
            // BitmapContainer.MouseDown += BitmapContainer_MouseDown;

            //cursor
            Engine.Scene.Cursor.PropertyChanged += Cursor_PropertyChanged;
            Cursor_PropertyChanged(this, new PropertyChangedEventArgs(nameof(Engine.Scene.Cursor.ScreenPosition)));

            //selectedObject
            Engine.Scene.PropertyChanged += Scene_PropertyChanged;

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Scene_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedObject)));
            UpdateMenu();
        }
        public SceneObject SelectedObject
        {
            get
            {
                return Engine?.Scene?.SelectedObject;
            }
            set
            {
                Engine.Scene.SelectedObject = value;
            }
        }

        private void SelectItem(SceneObject obj)
        {
            var found = ExpandAndSelectItem(objectList, obj, true);
            if (!found)
            {
                ExpandAndSelectItem(objectList, Engine.Scene.SelectedObject, false);
                Engine.Scene.SelectedObject = obj;
            }
            //var tvi = ContainerFromItemRecursive(objectList.ItemContainerGenerator, obj);
            //if (tvi != null)
            //    tvi.IsSelected = true;
        }
        private SceneObject GetVisibleSelectedObj()
        {
            return objectList.SelectedItem as SceneObject;
        }


        #region handleInput
        private bool rectSelect = false;
        private void BitmapContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //bitmapImage.Focus();         // Set Logical Focus
            //Keyboard.Focus(bitmapImage); // Set Keyboard Focus
            //FocusManager.SetIsFocusScope(bitmapImage, true);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            var obj = Engine?.Input.OnMouseLeftButtonDown(e.GetPosition(BitmapContainer));
            SelectItem(obj);
        }
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            Engine?.Input.OnMouseRightButtonDown(e.GetPosition(BitmapContainer));
        }
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            Engine?.Input.OnMouseRightButtonUp();
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
        #endregion

        #region topMenu
        private void Surface_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(Engine.Scene.AddBezierSurface(GetVisibleSelectedObj()));
        }
        private void Cylinder_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(Engine.Scene.AddBezierCylinder(GetVisibleSelectedObj()));
        }
        private void Bezier0_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(Engine.Scene.AddBezierCurveC0(GetVisibleSelectedObj()));
        }
        private void Bezier2_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(Engine.Scene.AddBezierCurveC2(GetVisibleSelectedObj()));
        }
        private void InterpolatingCurve(object sender, RoutedEventArgs e)
        {
            SelectItem(Engine.Scene.AddInterpolatingCurve(GetVisibleSelectedObj()));
        }
        private void Torus_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.AddTorus(GetVisibleSelectedObj());
        }
        private void Empty_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.AddEmptyObject(GetVisibleSelectedObj());
        }
        private void Vertex_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.AddVertex(GetVisibleSelectedObj());
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.Delete(GetVisibleSelectedObj());
        }
        private void HoldRelease_click(object sender, RoutedEventArgs e)
        {
            if (Engine.Scene.Cursor.HeldObjects.Count > 0)
            {
                Engine.Scene.Cursor.ReleaseObjects();
                holdReleaseBtn.Content = "Hold";
            }
            else
            {
                Engine.Scene.Cursor.HoldObject(Engine.Scene);
                if (Engine.Scene.Cursor.HeldObjects.Count > 0)
                    holdReleaseBtn.Content = "Release";
            }
        }

        private void FocuCamera(object sender, RoutedEventArgs e)
        {
            var item = SelectedObject;
            if (item != null)
                Engine.Scene.Camera.SetTarget(item);
        }
        private void DeleteFlat_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.FlatDelete(SelectedObject);
        }
        private void ResetCamera(object sender, RoutedEventArgs e)
        {
            Engine.Scene.ResetCamera();
        }

        private void CbxAnaglyph_Checked(object sender, RoutedEventArgs e)
        {
            Engine.Renderer.Anaglyphic = true;
        }
        private void CbxAnaglyph_Unchecked(object sender, RoutedEventArgs e)
        {
            Engine.Renderer.Anaglyphic = false;
        }
        private void Anaglyph_Change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Engine.Renderer.EyeDistance = (float)(0.3f * e.NewValue);
        }
        private void Viewport_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Engine.Renderer.ViewportDistance = (float)(5 + 35f * e.NewValue);
        }

        private void Cursor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Engine.Scene.Cursor.ScreenPosition))
                cursorPosition.Text = Engine.Scene.Cursor.ScreenPosition.ToString();
        }
        #endregion

        #region objMenu
        private void SelectedObjectChange(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            if (e.NewValue != null)
            {
                SelectedObject = e.NewValue as SceneObject;
            }
            else if (e.OldValue != null && e.OldValue is SceneObject obj && obj.Id == SelectedObject.Id)
            {
                SelectedObject = null;
            }
        }
        private void UpdateMenu()
        {
            var item = SelectedObject;
            if (item != null)
            {
                Engine.Scene.Cursor.SetTarget(item);

                objectMenu.Visibility = Visibility.Visible;

                TorusMenu.Visibility = item is Torus ? Visibility.Visible : Visibility.Collapsed;

                BezierMenu.Visibility = item is BezierCurveBase ? Visibility.Visible : Visibility.Collapsed;
                BezierC2Menu.Visibility = item is BezierCurveC2 ? Visibility.Visible : Visibility.Collapsed;
                InterpolatingMenu.Visibility = item is InterpolatingCurve ? Visibility.Visible : Visibility.Collapsed;

                BaseSurfaceMenu.Visibility = item is BezierSurfaceBase ? Visibility.Visible : Visibility.Collapsed;
                SurfaceMenu.Visibility = item is BezierSurface ? Visibility.Visible : Visibility.Collapsed;
                CylinderMenu.Visibility = item is BezierCylinder ? Visibility.Visible : Visibility.Collapsed;

            }
            else
            {
                objectMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void PositionXUp(object sender, RoutedEventArgs e) { SelectedObject.Move(_positionChangeSpeed, 0, 0); }
        private void PositionXDown(object sender, RoutedEventArgs e) { SelectedObject.Move(-_positionChangeSpeed, 0, 0); }
        private void PositionYUp(object sender, RoutedEventArgs e) { SelectedObject.Move(0, _positionChangeSpeed, 0); }
        private void PositionYDown(object sender, RoutedEventArgs e) { SelectedObject.Move(0, -_positionChangeSpeed, 0); }
        private void PositionZUp(object sender, RoutedEventArgs e) { SelectedObject.Move(0, 0, _positionChangeSpeed); }
        private void PositionZDown(object sender, RoutedEventArgs e) { SelectedObject.Move(0, 0, -_positionChangeSpeed); }

        private void RotationXUp(object sender, RoutedEventArgs e) { SelectedObject.RotateLoc(_rotationChangeSpeed, 0, 0); }
        private void RotationXDown(object sender, RoutedEventArgs e) { SelectedObject.RotateLoc(-_rotationChangeSpeed, 0, 0); }
        private void RotationYUp(object sender, RoutedEventArgs e) { SelectedObject.RotateLoc(0, _rotationChangeSpeed, 0); }
        private void RotationYDown(object sender, RoutedEventArgs e) { SelectedObject.RotateLoc(0, -_rotationChangeSpeed, 0); }
        private void RotationZUp(object sender, RoutedEventArgs e) { SelectedObject.RotateLoc(0, 0, _rotationChangeSpeed); }
        private void RotationZDown(object sender, RoutedEventArgs e) { SelectedObject.RotateLoc(0, 0, -_rotationChangeSpeed); }

        private void ScaleXUp(object sender, RoutedEventArgs e) { SelectedObject.ScaleLoc(_scaleChangeSpeed, 1, 1); }
        private void ScaleXDown(object sender, RoutedEventArgs e) { SelectedObject.ScaleLoc(1 / _scaleChangeSpeed, 1, 1); }
        private void ScaleYUp(object sender, RoutedEventArgs e) { SelectedObject.ScaleLoc(1, _scaleChangeSpeed, 1); }
        private void ScaleYDown(object sender, RoutedEventArgs e) { SelectedObject.ScaleLoc(1, 1 / _scaleChangeSpeed, 1); }
        private void ScaleZUp(object sender, RoutedEventArgs e) { SelectedObject.ScaleLoc(1, 1, _scaleChangeSpeed); }
        private void ScaleZDown(object sender, RoutedEventArgs e) { SelectedObject.ScaleLoc(1, 1, 1 / _scaleChangeSpeed); }
        #endregion

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
                                    _draggedItem.SetParent(_target);
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
        private bool ExpandAndSelectItem(ItemsControl parentContainer, object itemToSelect, bool select)
        {
            //check all items at the current level
            foreach (object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                //if the data item matches the item we want to select, set the corresponding
                //TreeViewItem IsSelected to true
                if (item == itemToSelect && currentContainer != null)
                {
                    if (select)
                    {
                        currentContainer.IsSelected = true;
                        currentContainer.BringIntoView();
                        currentContainer.Focus();
                    }
                    else
                    {

                        currentContainer.IsSelected = false;
                    }

                    //the item was found
                    return true;
                }
            }

            //if we get to this point, the selected item was not found at the current level, so we must check the children
            foreach (object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                //if children exist
                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    //keep track of if the TreeViewItem was expanded or not
                    bool wasExpanded = currentContainer.IsExpanded;

                    //expand the current TreeViewItem so we can check its child TreeViewItems
                    currentContainer.IsExpanded = true;

                    //if the TreeViewItem child containers have not been generated, we must listen to
                    //the StatusChanged event until they are
                    if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        //store the event handler in a variable so we can remove it (in the handler itself)
                        EventHandler eh = null;
                        eh = new EventHandler(delegate
                        {
                            if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                if (ExpandAndSelectItem(currentContainer, itemToSelect, select) == false)
                                {
                                    //The assumption is that code executing in this EventHandler is the result of the parent not
                                    //being expanded since the containers were not generated.
                                    //since the itemToSelect was not found in the children, collapse the parent since it was previously collapsed
                                    currentContainer.IsExpanded = false;
                                }

                                //remove the StatusChanged event handler since we just handled it (we only needed it once)
                                currentContainer.ItemContainerGenerator.StatusChanged -= eh;
                            }
                        });
                        currentContainer.ItemContainerGenerator.StatusChanged += eh;
                    }
                    else //otherwise the containers have been generated, so look for item to select in the children
                    {
                        if (ExpandAndSelectItem(currentContainer, itemToSelect, select) == false)
                        {
                            //restore the current TreeViewItem's expanded state
                            currentContainer.IsExpanded = wasExpanded;
                        }
                        else //otherwise the node was found and selected, so return true
                        {
                            return true;
                        }
                    }
                }
            }

            //no item was found
            return false;
        }
        #endregion

    }
}
