// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// An infinitely scrolling, UI- and data-virtualizing selection control.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [TemplatePart(Name = ItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = CenteringTransformName, Type = typeof(TranslateTransform))]
    [TemplatePart(Name = PanningTransformName, Type = typeof(TranslateTransform))]
    public class HorizontalLoopingSelector : Control
    {
        // The names of the template parts
        private const string ItemsPanelName = "ItemsPanel";
        private const string CenteringTransformName = "CenteringTransform";
        private const string PanningTransformName = "PanningTransform";

        // Amount of finger movement before the manipulation is considered a dragging manipulation.
        private const double DragSensitivity = 12;

        private static readonly Duration _selectDuration = new Duration(TimeSpan.FromMilliseconds(500));
        private readonly IEasingFunction _selectEase = new ExponentialEase() { EasingMode = EasingMode.EaseInOut };

        private static readonly Duration _panDuration = new Duration(TimeSpan.FromMilliseconds(100));
        private readonly IEasingFunction _panEase = new ExponentialEase();

        private DoubleAnimation _panelAnimation;
        private Storyboard _panelStoryboard;

        private Panel _itemsPanel;
        private TranslateTransform _panningTransform;
        private TranslateTransform _centeringTransform;

        private bool _isSelecting;
        private HorizontalLoopingSelectorItem _selectedItem;

        private Queue<HorizontalLoopingSelectorItem> _temporaryItemsPool;

        private double _minimumPanelScroll = float.MinValue;
        private double _maximumPanelScroll = float.MaxValue;

        private int _additionalItemsCount = 0;

        private bool _isAnimating;

        private double _dragTarget;

        // Once the user starts dragging horizontally, he is not allowed to drag vertically
        // until he completes his touch gesture and starts again.
        private bool _isAllowedToDragHorizontally = true;

        // Specify whether or not the user is dragging with his finger.
        private bool _isDragging;

        private enum State
        {
            Normal,
            Expanded,
            Dragging,
            Snapping,
            Flicking
        }

        private State _state;

        /// <summary>
        /// The data source that the this control is the view for.
        /// </summary>
        public ILoopingSelectorDataSource DataSource
        {
            get { return (ILoopingSelectorDataSource)GetValue(DataSourceProperty); }
            set
            {
                if (DataSource != null)
                {
                    DataSource.SelectionChanged -= value_SelectionChanged;
                }

                SetValue(DataSourceProperty, value);

                if (value != null)
                {
                    value.SelectionChanged += value_SelectionChanged;
                }
            }
        }

        void value_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsReady)
            {
                return;
            }

            if (!_isSelecting && e.AddedItems.Count == 1)
            {
                object selection = e.AddedItems[0];

                foreach (HorizontalLoopingSelectorItem child in _itemsPanel.Children)
                {
                    if (child.DataContext == selection)
                    {
                        SelectAndSnapTo(child);
                        return;
                    }
                }
                UpdateData();
            }
        }

        /// <summary>
        /// The DataSource DependencyProperty
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(ILoopingSelectorDataSource), typeof(HorizontalLoopingSelector), new PropertyMetadata(null, OnDataModelChanged));

        private static void OnDataModelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            HorizontalLoopingSelector picker = (HorizontalLoopingSelector)obj;
            picker.UpdateData();
        }

        void DataModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsReady)
            {
                return;
            }

            if (!_isSelecting && e.AddedItems.Count == 1)
            {
                object selection = e.AddedItems[0];

                foreach (HorizontalLoopingSelectorItem child in _itemsPanel.Children)
                {
                    if (child.DataContext == selection)
                    {
                        SelectAndSnapTo(child);
                        break;
                    }
                }

                UpdateData();
            }
        }

        /// <summary>
        /// The ItemTemplate property
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// The ItemTemplate DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(HorizontalLoopingSelector), new PropertyMetadata(null));

        /// <summary>
        /// The size of the items, excluding the ItemMargin.
        /// </summary>
        public Size ItemSize { get; set; }

        /// <summary>
        /// The margin around the items, to be a part of the touchable area.
        /// </summary>
        public Thickness ItemMargin { get; set; }

        /// <summary>
        /// Creates a new HorizontalLoopingSelector.
        /// </summary>
        public HorizontalLoopingSelector()
        {
            DefaultStyleKey = typeof(HorizontalLoopingSelector);
            CreateEventHandlers();
        }

        /// <summary>
        /// The IsExpanded property controls the expanded state of this control.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        /// <summary>
        /// The IsExpanded DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(HorizontalLoopingSelector), new PropertyMetadata(false, OnIsExpandedChanged));

        /// <summary>
        /// The IsExpandedChanged event will be raised whenever the IsExpanded state changes.
        /// </summary>
        public event DependencyPropertyChangedEventHandler IsExpandedChanged;

        private static void OnIsExpandedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HorizontalLoopingSelector picker = (HorizontalLoopingSelector)sender;

            picker.UpdateItemState();
            if (!picker.IsExpanded)
            {
                picker.SelectAndSnapToClosest();
            }

            if (picker._state == State.Normal || picker._state == State.Expanded)
            {
                picker._state = picker.IsExpanded ? State.Expanded : State.Normal;
            }

            var listeners = picker.IsExpandedChanged;
            if (listeners != null)
            {
                listeners(picker, e);
            }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:FrameworkElement.ApplyTemplate()"/>.
        /// In simplest terms, this means the method is called just before a UI element displays in an application.
        /// For more information, see Remarks.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Find the template parts. Create dummy objects if parts are missing to avoid
            // null checks throughout the code (although we can't escape them completely.)
            _itemsPanel = GetTemplateChild(ItemsPanelName) as Panel ?? new Canvas();
            _centeringTransform = GetTemplateChild(CenteringTransformName) as TranslateTransform ?? new TranslateTransform();
            _panningTransform = GetTemplateChild(PanningTransformName) as TranslateTransform ?? new TranslateTransform();

            CreateVisuals();
        }

        void LoopingSelector_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isAnimating)
            {
                double x = _panningTransform.X;
                StopAnimation();
                _panningTransform.X = x;
                _isAnimating = false;
                _state = State.Dragging;
            }
        }

        void LoopingSelector_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_selectedItem != sender && _state == State.Dragging && !_isAnimating)
            {
                SelectAndSnapToClosest();
                _state = State.Expanded;
            }
        }

        #region Touch Events
        private void OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_panningTransform != null)
            {
                SelectAndSnapToClosest();
                e.Handled = true;
            }
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _isAllowedToDragHorizontally = true;
            _isDragging = false;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_isDragging)
            {
                AnimatePanel(_panDuration, _panEase, _dragTarget += e.DeltaManipulation.Translation.X);
                e.Handled = true;
            }
            else if (Math.Abs(e.CumulativeManipulation.Translation.Y) > DragSensitivity)
            {
                _isAllowedToDragHorizontally = false;
            }
            else if (_isAllowedToDragHorizontally && Math.Abs(e.CumulativeManipulation.Translation.X) > DragSensitivity)
            {
                _isDragging = true;
                _state = State.Dragging;
                e.Handled = true;
                _selectedItem = null;

                if (!IsExpanded)
                {
                    IsExpanded = true;
                }

                _dragTarget = _panningTransform.X;
                UpdateItemState();
            }
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_isDragging)
            {
                // See if it was a flick
                if (e.IsInertial)
                {
                    _state = State.Flicking;
                    _selectedItem = null;

                    if (!IsExpanded)
                    {
                        IsExpanded = true;
                    }

                    Point velocity = new Point(e.FinalVelocities.LinearVelocity.X, 0);
                    double flickDuration = PhysicsConstants.GetStopTime(velocity);
                    Point flickEndPoint = PhysicsConstants.GetStopPoint(velocity);
                    IEasingFunction flickEase = PhysicsConstants.GetEasingFunction(flickDuration);

                    AnimatePanel(new Duration(TimeSpan.FromSeconds(flickDuration)), flickEase, _panningTransform.X + flickEndPoint.X);

                    e.Handled = true;

                    _selectedItem = null;
                    UpdateItemState();
                }

                if (_state == State.Dragging)
                {
                    SelectAndSnapToClosest();
                }

                _state = State.Expanded;
            }
        }
        #endregion

        void LoopingSelector_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _centeringTransform.X = Math.Round(e.NewSize.Width / 2);
            Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
            UpdateData();
        }

        void wrapper_Click(object sender, EventArgs e)
        {
            if (_state == State.Normal)
            {
                _state = State.Expanded;
                IsExpanded = true;
            }
            else if (_state == State.Expanded)
            {
                if (!_isAnimating && sender == _selectedItem)
                {
                    _state = State.Normal;
                    IsExpanded = false;
                }
                else if (sender != _selectedItem && !_isAnimating)
                {
                    SelectAndSnapTo((HorizontalLoopingSelectorItem)sender);
                }
            }
        }

        private void SelectAndSnapTo(HorizontalLoopingSelectorItem item)
        {
            if (item == null)
            {
                return;
            }

            if (_selectedItem != null)
            {
                _selectedItem.SetState(IsExpanded ? HorizontalLoopingSelectorItem.State.Expanded : HorizontalLoopingSelectorItem.State.Normal, true);
            }

            if (_selectedItem != item)
            {
                _selectedItem = item;
                //Needed for Selection Changed Event
                //Selection = _selectedItem;
                // Update DataSource.SelectedItem aynchronously so that animations have a chance to start.
                Dispatcher.BeginInvoke(() =>
                {
                    _isSelecting = true;
                    DataSource.SelectedItem = item.DataContext;
                    _isSelecting = false;
                });
            }

            _selectedItem.SetState(HorizontalLoopingSelectorItem.State.Selected, true);

            TranslateTransform transform = item.Transform;
            if (transform != null)
            {
                double newPosition = -transform.X - Math.Round(item.ActualWidth / 2);
                if (_panningTransform.X != newPosition)
                {
                    AnimatePanel(_selectDuration, _selectEase, newPosition);
                }
            }
        }

        private void UpdateData()
        {
            if (!IsReady)
            {
                return;
            }

            // Save all items
            _temporaryItemsPool = new Queue<HorizontalLoopingSelectorItem>(_itemsPanel.Children.Count);
            foreach (HorizontalLoopingSelectorItem item in _itemsPanel.Children)
            {
                if (item.GetState() == HorizontalLoopingSelectorItem.State.Selected)
                {
                    item.SetState(HorizontalLoopingSelectorItem.State.Expanded, false);
                }
                _temporaryItemsPool.Enqueue(item);
                item.Remove();
            }

            _itemsPanel.Children.Clear();
            StopAnimation();
            _panningTransform.X = 0;

            // Reset the extents
            _minimumPanelScroll = float.MinValue;
            _maximumPanelScroll = float.MaxValue;

            Balance();
        }

        private void AnimatePanel(Duration duration, IEasingFunction ease, double to)
        {
            // Be sure not to run past the first or last items
            double newTo = Math.Max(_minimumPanelScroll, Math.Min(_maximumPanelScroll, to));
            if (to != newTo)
            {
                // Adjust the duration
                double originalDelta = Math.Abs(_panningTransform.X - to);
                double modifiedDelta = Math.Abs(_panningTransform.X - newTo);
                double factor = modifiedDelta / originalDelta;

                duration = new Duration(TimeSpan.FromMilliseconds(duration.TimeSpan.Milliseconds * factor));

                to = newTo;
            }

            double from = _panningTransform.X;
            StopAnimation();
            CompositionTarget.Rendering += AnimationPerFrameCallback;

            _panelAnimation.Duration = duration;
            _panelAnimation.EasingFunction = ease;
            _panelAnimation.From = from;
            _panelAnimation.To = to;
            _panelStoryboard.Begin();
            _panelStoryboard.SeekAlignedToLastTick(TimeSpan.Zero);

            _isAnimating = true;
        }

        private void StopAnimation()
        {
            _panelStoryboard.Stop();
            CompositionTarget.Rendering -= AnimationPerFrameCallback;
        }

        private void Brake(double newStoppingPoint)
        {
            double originalDelta = _panelAnimation.To.Value - _panelAnimation.From.Value;
            double remainingDelta = newStoppingPoint - _panningTransform.X;
            double factor = remainingDelta / originalDelta;

            Duration duration = new Duration(TimeSpan.FromMilliseconds(_panelAnimation.Duration.TimeSpan.Milliseconds * factor));

            AnimatePanel(duration, _panelAnimation.EasingFunction, newStoppingPoint);
        }

        private bool IsReady
        {
            get { return ActualWidth > 0 && DataSource != null && _itemsPanel != null; }
        }

        /// <summary>
        /// Balances the items.
        /// </summary>
        private void Balance()
        {
            if (!IsReady)
            {
                return;
            }

            double actualItemWidth = ActualItemWidth;
            double actualItemHeight = ActualItemHeight;

            _additionalItemsCount = (int)Math.Round((ActualWidth * 1.5) / actualItemWidth);

            HorizontalLoopingSelectorItem closestToMiddle = null;
            int closestToMiddleIndex = -1;

            if (_itemsPanel.Children.Count == 0)
            {
                // We need to get the selection and start from there
                closestToMiddleIndex = 0;
                _selectedItem = closestToMiddle = CreateAndAddItem(_itemsPanel, DataSource.SelectedItem);
                //Needed for Selection Changed Event
                //Selection = _selectedItem;
                closestToMiddle.Transform.X = -actualItemWidth / 2;
                closestToMiddle.Transform.Y = (ActualHeight - actualItemHeight) / 2;
                closestToMiddle.SetState(HorizontalLoopingSelectorItem.State.Selected, false);
            }
            else
            {
                closestToMiddleIndex = GetClosestItem();
                closestToMiddle = (HorizontalLoopingSelectorItem)_itemsPanel.Children[closestToMiddleIndex];
            }

            int itemsBeforeCount;
            HorizontalLoopingSelectorItem firstItem = GetFirstItem(closestToMiddle, out itemsBeforeCount);

            int itemsAfterCount;
            HorizontalLoopingSelectorItem lastItem = GetLastItem(closestToMiddle, out itemsAfterCount);

            // Does the top need items?
            if (itemsBeforeCount < itemsAfterCount || itemsBeforeCount < _additionalItemsCount)
            {
                while (itemsBeforeCount < _additionalItemsCount)
                {
                    object newData = DataSource.GetPrevious(firstItem.DataContext);
                    if (newData == null)
                    {
                        // There may be room to display more items, but there is no more data.
                        _maximumPanelScroll = -firstItem.Transform.X - actualItemWidth / 2;
                        if (_isAnimating && _panelAnimation.To.Value > _maximumPanelScroll)
                        {
                            Brake(_maximumPanelScroll);
                        }
                        break;
                    }

                    HorizontalLoopingSelectorItem newItem = null;

                    // Can an item from the bottom be re-used?
                    if (itemsAfterCount > _additionalItemsCount)
                    {
                        newItem = lastItem;
                        lastItem = lastItem.Previous;
                        newItem.Remove();
                        newItem.Content = newItem.DataContext = newData;
                    }
                    else
                    {
                        // Make a new item
                        newItem = CreateAndAddItem(_itemsPanel, newData);
                        newItem.Transform.Y = (ActualHeight - actualItemHeight) / 2;
                    }

                    // Put the new item on the top
                    newItem.Transform.X = firstItem.Transform.X - actualItemWidth;
                    newItem.InsertBefore(firstItem);
                    firstItem = newItem;

                    ++itemsBeforeCount;
                }
            }

            // Does the bottom need items?
            if (itemsAfterCount < itemsBeforeCount || itemsAfterCount < _additionalItemsCount)
            {
                while (itemsAfterCount < _additionalItemsCount)
                {
                    object newData = DataSource.GetNext(lastItem.DataContext);
                    if (newData == null)
                    {
                        // There may be room to display more items, but there is no more data.
                        _minimumPanelScroll = -lastItem.Transform.X - actualItemWidth / 2;
                        if (_isAnimating && _panelAnimation.To.Value < _minimumPanelScroll)
                        {
                            Brake(_minimumPanelScroll);
                        }
                        break;
                    }

                    HorizontalLoopingSelectorItem newItem = null;

                    // Can an item from the top be re-used?
                    if (itemsBeforeCount > _additionalItemsCount)
                    {
                        newItem = firstItem;
                        firstItem = firstItem.Next;
                        newItem.Remove();
                        newItem.Content = newItem.DataContext = newData;
                    }
                    else
                    {
                        // Make a new item
                        newItem = CreateAndAddItem(_itemsPanel, newData);
                        newItem.Transform.Y = (ActualHeight - actualItemHeight) / 2;
                    }

                    // Put the new item on the bottom
                    newItem.Transform.X = lastItem.Transform.X + actualItemWidth;
                    newItem.InsertAfter(lastItem);
                    lastItem = newItem;

                    ++itemsAfterCount;
                }
            }

            _temporaryItemsPool = null;
        }

        private static HorizontalLoopingSelectorItem GetFirstItem(HorizontalLoopingSelectorItem item, out int count)
        {
            count = 0;
            while (item.Previous != null)
            {
                ++count;
                item = item.Previous;
            }

            return item;
        }

        private static HorizontalLoopingSelectorItem GetLastItem(HorizontalLoopingSelectorItem item, out int count)
        {
            count = 0;
            while (item.Next != null)
            {
                ++count;
                item = item.Next;
            }

            return item;
        }

        void AnimationPerFrameCallback(object sender, EventArgs e)
        {
            Balance();
        }

        private int GetClosestItem()
        {
            if (!IsReady)
            {
                return -1;
            }

            double actualItemWidth = ActualItemWidth;

            int count = _itemsPanel.Children.Count;
            double panelX = _panningTransform.X;
            double halfWidth = actualItemWidth / 2;
            int found = -1;
            double closestDistance = double.MaxValue;

            for (int index = 0; index < count; ++index)
            {
                HorizontalLoopingSelectorItem wrapper = (HorizontalLoopingSelectorItem)_itemsPanel.Children[index];
                double distance = Math.Abs((wrapper.Transform.X + halfWidth) + panelX);
                if (distance <= halfWidth)
                {
                    found = index;
                    break;
                }
                else if (closestDistance > distance)
                {
                    closestDistance = distance;
                    found = index;
                }
            }

            return found;
        }

        void PanelStoryboardCompleted(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= AnimationPerFrameCallback;
            _isAnimating = false;
            if (_state != State.Dragging)
            {
                SelectAndSnapToClosest();
            }
        }

        private void SelectAndSnapToClosest()
        {
            if (!IsReady)
            {
                return;
            }

            int index = GetClosestItem();
            if (index == -1)
            {
                return;
            }

            HorizontalLoopingSelectorItem item = (HorizontalLoopingSelectorItem)_itemsPanel.Children[index];
            SelectAndSnapTo(item);
        }

        private void UpdateItemState()
        {
            if (!IsReady)
            {
                return;
            }

            bool isExpanded = IsExpanded;

            foreach (HorizontalLoopingSelectorItem child in _itemsPanel.Children)
            {
                if (child == _selectedItem)
                {
                    child.SetState(HorizontalLoopingSelectorItem.State.Selected, true);
                }
                else
                {
                    child.SetState(isExpanded ? HorizontalLoopingSelectorItem.State.Expanded : HorizontalLoopingSelectorItem.State.Normal, true);
                }
            }
        }

        private double ActualItemWidth { get { return Padding.Left + Padding.Right + ItemSize.Width; } }
        private double ActualItemHeight { get { return Padding.Top + Padding.Bottom + ItemSize.Height; } }

        private void CreateVisuals()
        {
            _panelAnimation = new DoubleAnimation();
            Storyboard.SetTarget(_panelAnimation, _panningTransform);
            Storyboard.SetTargetProperty(_panelAnimation, new PropertyPath("X"));

            _panelStoryboard = new Storyboard();
            _panelStoryboard.Children.Add(_panelAnimation);
            _panelStoryboard.Completed += PanelStoryboardCompleted;
        }

        private void CreateEventHandlers()
        {

            SizeChanged += new SizeChangedEventHandler(LoopingSelector_SizeChanged);

            this.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnManipulationStarted);
            this.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(OnManipulationCompleted);
            this.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(OnManipulationDelta);

            this.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(OnTap);

            AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(LoopingSelector_MouseLeftButtonDown), true);
            AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(LoopingSelector_MouseLeftButtonUp), true);
        }

        private HorizontalLoopingSelectorItem CreateAndAddItem(Panel parent, object content)
        {
            bool reuse = _temporaryItemsPool != null && _temporaryItemsPool.Count > 0;

            HorizontalLoopingSelectorItem wrapper = reuse ? _temporaryItemsPool.Dequeue() : new HorizontalLoopingSelectorItem();

            if (!reuse)
            {
                wrapper.ContentTemplate = this.ItemTemplate;
                wrapper.Width = ItemSize.Width;
                wrapper.Height = ItemSize.Height;
                wrapper.Padding = ItemMargin;

                wrapper.Click += new EventHandler<EventArgs>(wrapper_Click);
                if (ItemStyle != null) { wrapper.Style = ItemStyle; }
            }

            wrapper.DataContext = wrapper.Content = content;

            parent.Children.Add(wrapper); // Need to do this before calling ApplyTemplate
            if (!reuse)
            {
                wrapper.ApplyTemplate();
            }

            return wrapper;
        }

        /// <summary>
        /// Style property for an item in a Looping Selector
        /// </summary>
        public Style ItemStyle
        {
            get { return (Style)GetValue(ItemStyleProperty); }
            set { SetValue(ItemStyleProperty, value); }
        }
        /// <summary>
        /// DependencyProperty for ItemStyle
        /// </summary>
        public static readonly DependencyProperty ItemStyleProperty = DependencyProperty.Register("ItemSt​yle", typeof(Style), typeof(HorizontalLoopingSelector), new PropertyMetadata(null));

        ///// <summary>
        ///// Custom Selection event
        ///// </summary>
        //public object Selection
        //{
        //    get { return (object)GetValue(SelectionProperty); }
        //    set { SetValue(SelectionProperty, value); }
        //}
        //public static readonly DependencyProperty SelectionProperty = DependencyProperty.Register("Selection", typeof(object), typeof(HorizontalLoopingSelector), new PropertyMetadata(null, OnSelectionChanged));

        //public event SelectionChangedEventHandler SelectionChanged;

        //private static void OnSelectionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        //{
        //    ((HorizontalLoopingSelector)o).OnSelectionChanged(e.OldValue, e.NewValue);
        //}

        //private void OnSelectionChanged(object oldValue, object newValue)
        //{
        //    IList removedItems = (null == oldValue) ? new object[0] : new object[] { oldValue };
        //    IList addedItems = (null == newValue) ? new object[0] : new object[] { newValue };
        //    SelectionChanged(this, new SelectionChangedEventArgs(removedItems, addedItems));
        //}
    }
}
