namespace WpfControls.TagBox.Controls.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    using WpfControls.TagBox.Linq;

    public static class VisualTreeExtensions
	{
		public static IEnumerable<DependencyObject> GetVisualAncestors(this DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return GetVisualAncestorsAndSelfIterator(element).Skip(1);
		}

		public static IEnumerable<DependencyObject> GetVisualAncestorsAndSelf(this DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return GetVisualAncestorsAndSelfIterator(element);
		}

		private static IEnumerable<DependencyObject> GetVisualAncestorsAndSelfIterator(DependencyObject element)
		{
			Debug.Assert(element != null, "element should not be null!");
			for (var dependencyObject = element; dependencyObject != null; dependencyObject = VisualTreeHelper.GetParent(dependencyObject))
			{
				yield return dependencyObject;
			}
		}

		public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return Enumerable.Skip(element.GetVisualChildrenAndSelfIterator(), 1);
		}

		public static IEnumerable<DependencyObject> GetVisualChildrenAndSelf(this DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return element.GetVisualChildrenAndSelfIterator();
		}

		private static IEnumerable<DependencyObject> GetVisualChildrenAndSelfIterator(this DependencyObject element)
		{
			Debug.Assert(element != null, "element should not be null!");
			yield return element;
			var childrenCount = VisualTreeHelper.GetChildrenCount(element);
			for (var i = 0; i < childrenCount; i++)
			{
				yield return VisualTreeHelper.GetChild(element, i);
			}
		}

		public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return GetVisualDescendantsAndSelfIterator(element).Skip(1);
		}

		public static IEnumerable<DependencyObject> GetVisualDescendantsAndSelf(this DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return GetVisualDescendantsAndSelfIterator(element);
		}

		private static IEnumerable<DependencyObject> GetVisualDescendantsAndSelfIterator(DependencyObject element)
		{
			Debug.Assert(element != null, "element should not be null!");
			var queue = new Queue<DependencyObject>();
			queue.Enqueue(element);
			while (queue.Count > 0)
			{
				var dependencyObject = queue.Dequeue();
				yield return dependencyObject;
				using (IEnumerator<DependencyObject> enumerator = dependencyObject.GetVisualChildren().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DependencyObject current = enumerator.Current;
						queue.Enqueue(current);
					}
				}
			}
		}

		public static IEnumerable<DependencyObject> GetVisualSiblings(this DependencyObject element)
		{
			return element.GetVisualSiblingsAndSelf().Where((DependencyObject p) => p != element);
		}

		public static IEnumerable<DependencyObject> GetVisualSiblingsAndSelf(this DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			var parent = VisualTreeHelper.GetParent(element);
			return (parent == null) ? Enumerable.Empty<DependencyObject>() : parent.GetVisualChildren();
		}

		public static Rect? GetBoundsRelativeTo(this FrameworkElement element, UIElement otherElement)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (otherElement == null)
			{
				throw new ArgumentNullException("otherElement");
			}
		    try
			{
				var generalTransform = element.TransformToVisual(otherElement);
				Point point;
				Point point2;
				if (generalTransform != null && generalTransform.TryTransform(default(Point), out point) && generalTransform.TryTransform(new Point(element.ActualWidth, element.ActualHeight), out point2))
				{
				    var result = new Rect?(new Rect(point, point2));
				    return result;
				}
			}
			catch (ArgumentException)
			{
			}

            return default(Rect?);
        }

		public static void InvokeOnLayoutUpdated(this FrameworkElement element, Action action)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}

			EventHandler handler = null;
		    handler = delegate
		        {
		            element.LayoutUpdated -= handler;
		            action.Invoke();
		        };

			element.LayoutUpdated += handler;
		}

		internal static IEnumerable<FrameworkElement> GetLogicalChildren(this FrameworkElement parent)
		{
			Debug.Assert(parent != null, "The parent cannot be null.");
			var popup = parent as Popup;
			if (popup != null)
			{
				var frameworkElement = popup.Child as FrameworkElement;
				if (frameworkElement != null)
				{
					yield return frameworkElement;
				}
			}

			var itemsControl = parent as ItemsControl;
			if (itemsControl != null)
			{
				using (var enumerator = Enumerable.Range(0, itemsControl.Items.Count).Select((int index) => itemsControl.ItemContainerGenerator.ContainerFromIndex(index)).OfType<FrameworkElement>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						var current = enumerator.Current;
						yield return current;
					}
				}
			}
		    var queue = new Queue<FrameworkElement>(parent.GetVisualChildren().OfType<FrameworkElement>());
			while (queue.Count > 0)
			{
				var frameworkElement2 = queue.Dequeue();
				if (frameworkElement2.Parent == parent || frameworkElement2 is UserControl)
				{
					yield return frameworkElement2;
				}
				else
				{
					using (IEnumerator<FrameworkElement> enumerator2 = frameworkElement2.GetVisualChildren().OfType<FrameworkElement>().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							var current2 = enumerator2.Current;
							queue.Enqueue(current2);
						}
					}
				}
			}
		}

		internal static IEnumerable<FrameworkElement> GetLogicalDescendents(this FrameworkElement parent)
		{
			Debug.Assert(parent != null, "The parent cannot be null.");
			return FunctionalProgramming.TraverseBreadthFirst(parent, node => node.GetLogicalChildren(), node => true);
		}
	}
}
