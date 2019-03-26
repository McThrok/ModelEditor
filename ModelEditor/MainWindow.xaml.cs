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

            //scene
            objectList.SelectedItemChanged += ObjectList_SelectedItemChanged;
        }

        private void ObjectList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.OldValue != null)
                Engine.Scene.SelectedItem = null;

            if (e.NewValue != null)
                Engine.Scene.SelectedItem = e.NewValue as SceneObject;
        }

        private void SelectItem(SceneObject obj)
        {
            var a = ExpandAndSelectItem(objectList, obj);
            //var tvi = ContainerFromItemRecursive(objectList.ItemContainerGenerator, obj);
            //if (tvi != null)
            //    tvi.IsSelected = true;
        }
        private SceneObject GetSelectedObj()
        {
            return objectList.SelectedItem as SceneObject;
        }


        #region handleInput
        private void BitmapContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //bitmapImage.Focus();         // Set Logical Focus
            //Keyboard.Focus(bitmapImage); // Set Keyboard Focus
            //FocusManager.SetIsFocusScope(bitmapImage, true);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
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
        private void Bezier_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(Engine.Scene.AddBezierCurve(GetSelectedObj()));
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
        private void FocuCamera(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedObj();
            if (item != null)
                Engine.Scene.Camera.SetTarget(item);
        }
        private void DeleteFlat_Click(object sender, RoutedEventArgs e)
        {
            Engine.Scene.FlatDelete(GetSelectedObj());
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
                objectMenu.Visibility = Visibility.Visible;
            else
                objectMenu.Visibility = Visibility.Collapsed;

            if (e.NewValue is SceneObject item)
            {
                Engine.Scene.Cursor.SetTarget(item);

                TorusMenu.Visibility = item is Torus ? Visibility.Visible : Visibility.Collapsed;
                BezierMenu.Visibility = item is BezierCurve ? Visibility.Visible : Visibility.Collapsed;
            }
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

        private void ScaleXUp(object sender, RoutedEventArgs e) { GetSelectedObj().ScaleLoc(_scaleChangeSpeed, 1, 1); }
        private void ScaleXDown(object sender, RoutedEventArgs e) { GetSelectedObj().ScaleLoc(1 / _scaleChangeSpeed, 1, 1); }
        private void ScaleYUp(object sender, RoutedEventArgs e) { GetSelectedObj().ScaleLoc(1, _scaleChangeSpeed, 1); }
        private void ScaleYDown(object sender, RoutedEventArgs e) { GetSelectedObj().ScaleLoc(1, 1 / _scaleChangeSpeed, 1); }
        private void ScaleZUp(object sender, RoutedEventArgs e) { GetSelectedObj().ScaleLoc(1, 1, _scaleChangeSpeed); }
        private void ScaleZDown(object sender, RoutedEventArgs e) { GetSelectedObj().ScaleLoc(1, 1, 1 / _scaleChangeSpeed); }
        #endregion


        #region dragAndDrop
        private Point _lastMouseDown;
        private SceneObject _draggedItem, _target;


        public TreeViewItem ContainerFromItemRecursive(ItemContainerGenerator root, object item)
        {
            var treeViewItem = root.ContainerFromItem(item) as TreeViewItem;
            if (treeViewItem != null)
                return treeViewItem;
            foreach (var subItem in root.Items)
            {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
                if (treeViewItem != null)
                {

                    //TODO: update expanded
                    //var isExp = treeViewItem.IsExpanded;
                    treeViewItem.IsExpanded = true;
                    var search = ContainerFromItemRecursive(treeViewItem.ItemContainerGenerator, item);
                    if (search != null)
                        return search;
                    //else
                    //    if (subItem is Camera)
                    //    treeViewItem.IsExpanded = isExp;

                }
            }
            return null;
        }
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
        #endregion

        private bool ExpandAndSelectItem(ItemsControl parentContainer, object itemToSelect)
        {
            //check all items at the current level
            foreach (object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                //if the data item matches the item we want to select, set the corresponding
                //TreeViewItem IsSelected to true
                if (item == itemToSelect && currentContainer != null)
                {
                    currentContainer.IsSelected = true;
                    currentContainer.BringIntoView();
                    currentContainer.Focus();

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
                                if (ExpandAndSelectItem(currentContainer, itemToSelect) == false)
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
                        if (ExpandAndSelectItem(currentContainer, itemToSelect) == false)
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
    }
}
