namespace WpfControls.TagBox.Controls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public class WrapPanel : Panel
    {
        private bool _ignorePropertyChange;

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight",
            typeof(double),
            typeof(WrapPanel),
            new PropertyMetadata(double.NaN, OnItemHeightOrWidthPropertyChanged));

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            "ItemWidth",
            typeof(double),
            typeof(WrapPanel),
            new PropertyMetadata(double.NaN, OnItemHeightOrWidthPropertyChanged));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(WrapPanel),
            new PropertyMetadata(Orientation.Vertical, OnOrientationPropertyChanged));

        [TypeConverter(typeof(LengthConverter))]
        public double ItemHeight
        {
            get
            {
                return (double)this.GetValue(ItemHeightProperty);
            }
            set
            {
                this.SetValue(ItemHeightProperty, value);
            }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double ItemWidth
        {
            get
            {
                return (double)this.GetValue(ItemWidthProperty);
            }
            set
            {
                this.SetValue(ItemWidthProperty, value);
            }
        }

        public Orientation Orientation
        {
            get
            {
                return (Orientation)this.GetValue(OrientationProperty);
            }
            set
            {
                this.SetValue(OrientationProperty, value);
            }
        }

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wrapPanel = (WrapPanel)d;
            var orientation = (Orientation)e.NewValue;
            if (wrapPanel._ignorePropertyChange)
            {
                wrapPanel._ignorePropertyChange = false;
            }
            else
            {
                if (orientation != Orientation.Vertical && orientation != 0)
                {
                    wrapPanel._ignorePropertyChange = true;
                    wrapPanel.SetValue(OrientationProperty, (Orientation)e.OldValue);
                    var text = string.Format(
                        CultureInfo.InvariantCulture,
                        "Exception thrown when the Orientation property is provided an invalid value.");
                    throw new ArgumentException(text, "value");
                }
                wrapPanel.InvalidateMeasure();
            }
        }

        private static void OnItemHeightOrWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wrapPanel = (WrapPanel)d;
            var num = (double)e.NewValue;
            if (wrapPanel._ignorePropertyChange)
            {
                wrapPanel._ignorePropertyChange = false;
            }
            else
            {
                if (!num.IsNaN() && (num <= 0.0 || double.IsPositiveInfinity(num)))
                {
                    wrapPanel._ignorePropertyChange = true;
                    wrapPanel.SetValue(e.Property, (double)e.OldValue);
                    var text = string.Format(
                        CultureInfo.InvariantCulture,
                        "Exception thrown when the ItemWith or ItemHeight properties are provided an invalid value.");
                    throw new ArgumentException(text, "value");
                }
                wrapPanel.InvalidateMeasure();
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var orientation = this.Orientation;
            var orientedSize = new OrientedSize(orientation);
            var orientedSize2 = new OrientedSize(orientation);
            var orientedSize3 = new OrientedSize(orientation, constraint.Width, constraint.Height);
            var itemWidth = this.ItemWidth;
            var itemHeight = this.ItemHeight;
            var flag = !itemWidth.IsNaN();
            var flag2 = !itemHeight.IsNaN();
            var size = new Size(flag ? itemWidth : constraint.Width, flag2 ? itemHeight : constraint.Height);

            foreach (var child in Children.OfType<UIElement>())
            {
                child.Measure(size);
                var orientedSize4 = new OrientedSize(
                    orientation,
                    flag ? itemWidth : child.DesiredSize.Width,
                    flag2 ? itemHeight : child.DesiredSize.Height);
                if (NumericExtensions.IsGreaterThan(orientedSize.Direct + orientedSize4.Direct, orientedSize3.Direct))
                {
                    orientedSize2.Direct = Math.Max(orientedSize.Direct, orientedSize2.Direct);
                    orientedSize2.Indirect += orientedSize.Indirect;
                    orientedSize = orientedSize4;

                    if (NumericExtensions.IsGreaterThan(orientedSize4.Direct, orientedSize3.Direct))
                    {
                        orientedSize2.Direct = Math.Max(orientedSize4.Direct, orientedSize2.Direct);
                        orientedSize2.Indirect += orientedSize4.Indirect;
                        orientedSize = new OrientedSize(orientation);
                    }
                }
                else
                {
                    orientedSize.Direct += orientedSize4.Direct;
                    orientedSize.Indirect = Math.Max(orientedSize.Indirect, orientedSize4.Indirect);
                }
            }

            orientedSize2.Direct = Math.Max(orientedSize.Direct, orientedSize2.Direct);
            orientedSize2.Indirect += orientedSize.Indirect;

            return new Size(orientedSize2.Width, orientedSize2.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var orientation = this.Orientation;
            var orientedSize = new OrientedSize(orientation);
            var orientedSize2 = new OrientedSize(orientation, finalSize.Width, finalSize.Height);
            var itemWidth = this.ItemWidth;
            var itemHeight = this.ItemHeight;
            var flag = !itemWidth.IsNaN();
            var flag2 = !itemHeight.IsNaN();
            var num = 0.0;
            var directDelta = (orientation == Orientation.Vertical)
                                  ? (flag ? itemWidth : default(double?))
                                  : (flag2 ? itemHeight : default(double?));
            var children = this.Children;
            var count = children.Count;
            var num2 = 0;
            for (var i = 0; i < count; i++)
            {
                var uIElement = children[i];
                var orientedSize3 = new OrientedSize(
                    orientation,
                    flag ? itemWidth : uIElement.DesiredSize.Width,
                    flag2 ? itemHeight : uIElement.DesiredSize.Height);
                if (NumericExtensions.IsGreaterThan(orientedSize.Direct + orientedSize3.Direct, orientedSize2.Direct))
                {
                    this.ArrangeLine(num2, i, directDelta, num, orientedSize.Indirect);
                    num += orientedSize.Indirect;
                    orientedSize = orientedSize3;
                    if (NumericExtensions.IsGreaterThan(orientedSize3.Direct, orientedSize2.Direct))
                    {
                        this.ArrangeLine(i, ++i, directDelta, num, orientedSize3.Indirect);
                        num += orientedSize.Indirect;
                        orientedSize = new OrientedSize(orientation);
                    }
                    num2 = i;
                }
                else
                {
                    orientedSize.Direct += orientedSize3.Direct;
                    orientedSize.Indirect = Math.Max(orientedSize.Indirect, orientedSize3.Indirect);
                }
            }
            if (num2 < count)
            {
                this.ArrangeLine(num2, count, directDelta, num, orientedSize.Indirect);
            }
            return finalSize;
        }

        private void ArrangeLine(
            int lineStart,
            int lineEnd,
            double? directDelta,
            double indirectOffset,
            double indirectGrowth)
        {
            var num = 0.0;
            var orientation = this.Orientation;
            var flag = orientation == Orientation.Vertical;
            var children = this.Children;
            for (var i = lineStart; i < lineEnd; i++)
            {
                var uIElement = children[i];
                var orientedSize = new OrientedSize(
                    orientation,
                    uIElement.DesiredSize.Width,
                    uIElement.DesiredSize.Height);
                var num2 = directDelta ?? orientedSize.Direct;
                var rect = flag
                               ? new Rect(num, indirectOffset, num2, indirectGrowth)
                               : new Rect(indirectOffset, num, indirectGrowth, num2);
                uIElement.Arrange(rect);
                num += num2;
            }
        }
    }
}
