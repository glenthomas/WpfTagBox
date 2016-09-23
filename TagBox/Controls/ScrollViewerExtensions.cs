namespace WpfControls.TagBox.Controls
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Animation;

    using WpfControls.TagBox.Controls.Primitives;

    public static class ScrollViewerExtensions
    {
        public static readonly DependencyProperty IsMouseWheelScrollingEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsMouseWheelScrollingEnabled",
                typeof(bool),
                typeof(ScrollViewerExtensions),
                new PropertyMetadata(false, OnIsMouseWheelScrollingEnabledPropertyChanged));

        private static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "VerticalOffset",
                typeof(double),
                typeof(ScrollViewerExtensions),
                new PropertyMetadata(OnVerticalOffsetPropertyChanged));

        private static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "HorizontalOffset",
                typeof(double),
                typeof(ScrollViewerExtensions),
                new PropertyMetadata(OnHorizontalOffsetPropertyChanged));

        public static bool GetIsMouseWheelScrollingEnabled(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            return (bool)viewer.GetValue(IsMouseWheelScrollingEnabledProperty);
        }

        public static void SetIsMouseWheelScrollingEnabled(this ScrollViewer viewer, bool value)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            viewer.SetValue(IsMouseWheelScrollingEnabledProperty, value);
        }

        private static void OnIsMouseWheelScrollingEnabledPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            var flag = (bool)e.NewValue;
            if (flag)
            {
                scrollViewer.MouseWheel += OnMouseWheel;
            }
            else
            {
                scrollViewer.MouseWheel -= OnMouseWheel;
            }
        }

        private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            Debug.Assert(scrollViewer != null, "sender should be a non-null ScrollViewer!");
            Debug.Assert(e != null, "e should not be null!");
            if (!e.Handled)
            {
                var num = CoerceVerticalOffset(scrollViewer, scrollViewer.VerticalOffset - (double)e.Delta);
                scrollViewer.ScrollToVerticalOffset(num);
                e.Handled = true;
            }
        }

        private static double GetVerticalOffset(ScrollViewer element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            return (double)element.GetValue(VerticalOffsetProperty);
        }

        private static void SetVerticalOffset(ScrollViewer element, double value)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            element.SetValue(VerticalOffsetProperty, value);
        }

        private static void OnVerticalOffsetPropertyChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs eventArgs)
        {
            var scrollViewer = dependencyObject as ScrollViewer;
            if (scrollViewer == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            scrollViewer.ScrollToVerticalOffset((double)eventArgs.NewValue);
        }

        private static double GetHorizontalOffset(ScrollViewer element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            return (double)element.GetValue(HorizontalOffsetProperty);
        }

        private static void SetHorizontalOffset(ScrollViewer element, double value)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            element.SetValue(HorizontalOffsetProperty, value);
        }

        private static void OnHorizontalOffsetPropertyChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs eventArgs)
        {
            var scrollViewer = dependencyObject as ScrollViewer;
            if (scrollViewer == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            scrollViewer.ScrollToHorizontalOffset((double)eventArgs.NewValue);
        }

        private static double CoerceVerticalOffset(ScrollViewer viewer, double offset)
        {
            Debug.Assert(viewer != null, "viewer should not be null!");
            return Math.Max(Math.Min(offset, viewer.ExtentHeight), 0.0);
        }

        private static double CoerceHorizontalOffset(ScrollViewer viewer, double offset)
        {
            Debug.Assert(viewer != null, "viewer should not be null!");
            return Math.Max(Math.Min(offset, viewer.ExtentWidth), 0.0);
        }

        private static void ScrollByVerticalOffset(ScrollViewer viewer, double offset)
        {
            Debug.Assert(viewer != null, "viewer should not be null!");
            offset += viewer.VerticalOffset;
            offset = CoerceVerticalOffset(viewer, offset);
            viewer.ScrollToVerticalOffset(offset);
        }

        private static void ScrollByHorizontalOffset(ScrollViewer viewer, double offset)
        {
            Debug.Assert(viewer != null, "viewer should not be null!");
            offset += viewer.HorizontalOffset;
            offset = CoerceHorizontalOffset(viewer, offset);
            viewer.ScrollToHorizontalOffset(offset);
        }

        public static void LineUp(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            ScrollByVerticalOffset(viewer, -16.0);
        }

        public static void LineDown(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            ScrollByVerticalOffset(viewer, 16.0);
        }

        public static void LineLeft(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            ScrollByHorizontalOffset(viewer, -16.0);
        }

        public static void LineRight(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            ScrollByHorizontalOffset(viewer, 16.0);
        }

        public static void PageUp(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            ScrollByVerticalOffset(viewer, -viewer.ViewportHeight);
        }

        public static void PageDown(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            ScrollByVerticalOffset(viewer, viewer.ViewportHeight);
        }

        public static void PageLeft(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            ScrollByHorizontalOffset(viewer, -viewer.ViewportWidth);
        }

        public static void PageRight(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            ScrollByHorizontalOffset(viewer, viewer.ViewportWidth);
        }

        public static void ScrollToTop(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            viewer.ScrollToVerticalOffset(0.0);
        }

        public static void ScrollToBottom(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            viewer.ScrollToVerticalOffset(viewer.ExtentHeight);
        }

        public static void ScrollToLeft(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            viewer.ScrollToHorizontalOffset(0.0);
        }

        public static void ScrollToRight(this ScrollViewer viewer)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            viewer.ScrollToHorizontalOffset(viewer.ExtentWidth);
        }

        public static void ScrollIntoView(this ScrollViewer viewer, FrameworkElement element)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            viewer.ScrollIntoView(element, 0.0, 0.0, TimeSpan.Zero);
        }

        public static void ScrollIntoView(
            this ScrollViewer viewer,
            FrameworkElement element,
            double horizontalMargin,
            double verticalMargin,
            Duration duration)
        {
            if (viewer == null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            var boundsRelativeTo = element.GetBoundsRelativeTo(viewer);
            if (boundsRelativeTo.HasValue)
            {
                var num = viewer.VerticalOffset;
                var num2 = 0.0;
                var viewportHeight = viewer.ViewportHeight;
                var num3 = boundsRelativeTo.Value.Bottom + verticalMargin;
                if (viewportHeight < num3)
                {
                    num2 = num3 - viewportHeight;
                    num += num2;
                }
                var num4 = boundsRelativeTo.Value.Top - verticalMargin;
                if (num4 - num2 < 0.0)
                {
                    num -= num2 - num4;
                }
                var num5 = viewer.HorizontalOffset;
                var num6 = 0.0;
                var viewportWidth = viewer.ViewportWidth;
                var num7 = boundsRelativeTo.Value.Right + horizontalMargin;
                if (viewportWidth < num7)
                {
                    num6 = num7 - viewportWidth;
                    num5 += num6;
                }
                var num8 = boundsRelativeTo.Value.Left - horizontalMargin;
                if (num8 - num6 < 0.0)
                {
                    num5 -= num6 - num8;
                }
                if (duration == TimeSpan.Zero)
                {
                    viewer.ScrollToVerticalOffset(num);
                    viewer.ScrollToHorizontalOffset(num5);
                }
                else
                {
                    var storyboard = new Storyboard();
                    SetVerticalOffset(viewer, viewer.VerticalOffset);
                    SetHorizontalOffset(viewer, viewer.HorizontalOffset);
                    var doubleAnimation = new DoubleAnimation { To = num, Duration = duration };
                    var doubleAnimation2 = doubleAnimation;
                    var doubleAnimation3 = new DoubleAnimation { To = num, Duration = duration };
                    var doubleAnimation4 = doubleAnimation3;
                    Storyboard.SetTarget(doubleAnimation2, viewer);
                    Storyboard.SetTarget(doubleAnimation4, viewer);
                    Storyboard.SetTargetProperty(doubleAnimation4, new PropertyPath(HorizontalOffsetProperty));
                    Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath(VerticalOffsetProperty));
                    storyboard.Children.Add(doubleAnimation2);
                    storyboard.Children.Add(doubleAnimation4);
                    storyboard.Begin();
                }
            }
        }
    }
}
