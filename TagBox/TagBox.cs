namespace WpfControls.TagBox
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;

    using Controls;

    [TemplatePart(Name = "SearchResultsListBox", Type = typeof(ListBox)), 
     TemplatePart(Name = "TagScrollViewer", Type = typeof(ScrollViewer)), 
     TemplatePart(Name = "SearchResultsPopup", Type = typeof(Popup)), 
     TemplatePart(Name = "TagPanelHolder", Type = typeof(Panel)), 
     TemplatePart(Name = "TagPanel", Type = typeof(Panel))]
	public class TagBox : Control
	{
		private Panel tagPanel;
		private Panel tagPanelHolder;
		private Popup searchResultsPopup;
		private ListBox searchResultsListBox;
		private ScrollViewer tagScrollViewer;
		private readonly object removalInProcess = new object();
		private NavigateDirection removalInProcessDirection = NavigateDirection.Self;
		private TagBase removalInProcessTag;

		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(TagBox), new PropertyMetadata(ItemsSourceChanged));
		public static readonly DependencyProperty AsyncSearchMethodProperty = DependencyProperty.Register("SearchMethod", typeof(Action<string>), typeof(TagBox), null);
		public static readonly DependencyProperty SearchResultsProperty = DependencyProperty.Register("SearchResults", typeof(IEnumerable), typeof(TagBox), new PropertyMetadata(SearchResultsChanged));
		public static readonly DependencyProperty SearchResultsHeightProperty = DependencyProperty.Register("SearchResultsHeight", typeof(double), typeof(TagBox), new PropertyMetadata(150.0));
		public static readonly DependencyProperty SearchResultsWidthProperty = DependencyProperty.Register("SearchResultsWidth", typeof(double), typeof(TagBox), new PropertyMetadata(150.0));
		public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(TagBox), new PropertyMetadata(null));
		public static readonly DependencyProperty SeperatorCharsProperty = DependencyProperty.Register("SeperatorChars", typeof(char[]), typeof(TagBox), new PropertyMetadata(new[] { ',', ';' }));
		public static readonly DependencyProperty UnresolvedValidationMethodProperty = DependencyProperty.Register("UnresolvedValidationMethod", typeof(Func<string, object>), typeof(TagBox), null);
		public static readonly DependencyProperty ValidateUnresolvedProperty = DependencyProperty.Register("ValidateUnresolved", typeof(bool), typeof(TagBox), new PropertyMetadata(false));
		public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(TagBox), new PropertyMetadata(null, OnSelectedItemPropertyChanged));
		public static readonly DependencyProperty TagStyleProperty = DependencyProperty.Register("TagStyle", typeof(Style), typeof(TagBox), new PropertyMetadata(null));

		public int NextIndex { get; set; }

		public bool EnableChangePropagation { get; set; }

		public bool Propagating { get; set; }

		public bool InternalFocusEvent { get; set; }

		public TagBase SearchSource { get; set; }

		public IList ItemsSource
		{
			get
			{
				return (IList)GetValue(ItemsSourceProperty);
			}
			set
			{
                SetValue(ItemsSourceProperty, value);
			}
		}

		public Action<string> SearchMethod
		{
			get
			{
				return (Action<string>)GetValue(AsyncSearchMethodProperty);
			}
			set
			{
                SetValue(AsyncSearchMethodProperty, value);
			}
		}

		public IEnumerable SearchResults
		{
			get
			{
				return (IEnumerable)GetValue(SearchResultsProperty);
			}
			set
			{
                SetValue(SearchResultsProperty, value);
			}
		}

		public double SearchResultsHeight
		{
			get
			{
				return (double)GetValue(SearchResultsHeightProperty);
			}
			set
			{
                SetValue(SearchResultsHeightProperty, value);
			}
		}

		public double SearchResultsWidth
		{
			get
			{
				return (double)GetValue(SearchResultsWidthProperty);
			}
			set
			{
                SetValue(SearchResultsWidthProperty, value);
			}
		}

		public string DisplayMemberPath
		{
			get
			{
				return (string)GetValue(DisplayMemberPathProperty);
			}
			set
			{
                SetValue(DisplayMemberPathProperty, value);
			}
		}

		public char[] SeperatorChars
		{
			get
			{
				return (char[])GetValue(SeperatorCharsProperty);
			}
			set
			{
                SetValue(SeperatorCharsProperty, value);
			}
		}

		public Func<string, object> UnresolvedValidationMethod
		{
			get
			{
				return (Func<string, object>)GetValue(UnresolvedValidationMethodProperty);
			}
			set
			{
                SetValue(UnresolvedValidationMethodProperty, value);
			}
		}

		public bool ValidateUnresolved
		{
			get
			{
				return (bool)GetValue(ValidateUnresolvedProperty);
			}
			set
			{
                SetValue(ValidateUnresolvedProperty, value);
			}
		}

		public object SelectedItem
		{
			get
			{
				return GetValue(SelectedItemProperty);
			}
			set
			{
                SetValue(SelectedItemProperty, value);
			}
		}

		public TagBase NewTag
		{
			get;
			set;
		}

		public Style TagStyle
		{
			get
			{
				return (Style)GetValue(TagStyleProperty);
			}
			set
			{
                SetValue(TagStyleProperty, value);
			}
		}

		public TagBox()
		{
            DefaultStyleKey = typeof(TagBox);
            BorderBrush = new SolidColorBrush(Colors.DarkGray);
            BorderThickness = new Thickness(1.0);
            Background = new SolidColorBrush(Colors.White);
            EnableChangePropagation = true;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

            this.searchResultsPopup = (Popup)GetTemplateChild("SearchResultsPopup");
            this.searchResultsListBox = (ListBox)GetTemplateChild("SearchResultsListBox");

            this.searchResultsListBox.GotFocus += delegate {
				Console.WriteLine("ListBox got focus");
			};

            this.searchResultsPopup.GotFocus += delegate {
				Console.WriteLine("Popup got focus");
			};

            this.tagPanel = (Panel)GetTemplateChild("TagPanel");
			if (this.tagPanel.Children.Count == 0)
			{
                AddTag(NewUnresolved());
			}

            this.tagPanelHolder = (Panel)GetTemplateChild("TagPanelHolder");
            this.tagPanelHolder.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
			{
				if (e.OriginalSource == tagPanelHolder)
				{
					(tagPanel.Children.Cast<UIElement>().Last() as TagBase).Focus();
				}
			};

            this.searchResultsListBox.IsTabStop = false;
            this.tagScrollViewer = (ScrollViewer)GetTemplateChild("TagScrollViewer");
			if (ItemsSource != null)
			{
                IntitiateDatabind(null, ItemsSource);
			}
		}

		private bool LostContainingFocus()
		{
			var frameworkElement = FocusManager.GetFocusedElement(this) as FrameworkElement;
		    while (frameworkElement != null)
			{
				frameworkElement = (VisualTreeHelper.GetParent(frameworkElement) as FrameworkElement);
				if (frameworkElement == this)
				{
				    return false;
				}
			}
			Console.WriteLine("TagBox Lost Focus - ");

			return true;
		}

		private void Validate()
		{
			foreach (var tag in this.tagPanel.Children.OfType<TagBase>())
			{
				if (tag is UnresolvedTag)
				{
                    ValidateOne(tag as UnresolvedTag);
				}
			}
		}

		private void ValidateOne(UnresolvedTag tag)
		{
			if (ValidateUnresolved)
			{
				if (UnresolvedValidationMethod == null)
				{
					throw new Exception("Tagbox.UnresolvedValidationMethod (Func<string, bool>) is not defined.  Must be provided to perform validation");
				}

				var text = tag.FilteredText.Trim();

				Console.WriteLine("Validating " + text);

				tag.ValidatedValue = UnresolvedValidationMethod.Invoke(text);
				var flag = tag.ValidatedValue == null;
				var relativeIndex = GetRelativeIndex(tag);
                EnableChangePropagation = false;

				if (!tag.IsInvalid && !flag)
				{
                    ItemsSource[relativeIndex] = tag.ValidatedValue;
				}
				else if (!tag.IsInvalid && flag)
				{
                    ItemsSource.RemoveAt(relativeIndex);
				}
				else if (tag.IsInvalid && !flag)
				{
                    ItemsSource.Insert(relativeIndex + 1, tag.ValidatedValue);
				}

                EnableChangePropagation = true;
				tag.IsInvalid = flag;

				if (tag.IsInvalid && text.Length > 0)
				{
					tag.MarkInvalid();
				}
			}
		}

		private void AddTag(TagBase tag)
		{
            this.tagPanel.Children.Add(tag);
            AttachTag(tag);
		}

		private void AttachTag(TagBase tag)
		{
			Console.WriteLine("Attaching tag " + tag);
			tag.NavigationRequested += OnTagNavigationRequested;
			tag.DeleteRequested += OnTagDeleteRequested;
			tag.SearchRequested += OnTagSearchRequested;
			tag.SelectionRequested += OnTagSelectionRequested;
			tag.LostFocus += OnTagLostFocus;
			tag.GotFocus += OnTagGotFocus;
			tag.AdjacentInsertRequested += OnTagAdjacentInsertRequested;
			tag.MergeRequested += OnTagMergeRequested;
			tag.DisplayMemberPath = DisplayMemberPath;
		}

		private void InsertTag(int index, TagBase tag)
		{
            this.tagPanel.Children.Insert(index, tag);
            AttachTag(tag);
		}

		private void RemoveTag(TagBase tag)
		{
			lock (this.removalInProcess)
			{
				Debug.WriteLine("Beginning delete of tag " + tag);
				if (this.removalInProcessTag != null)
				{
					Debug.WriteLine("RemovalInProcessTag is " + this.removalInProcessTag);
					if (this.removalInProcessTag == tag)
					{
						Debug.WriteLine("Cannot delete same tag twice");
						return;
					}
                    FinishRemoveTag(this.removalInProcessTag);
				}
				if (Propagating)
				{
                    FinishRemoveTag(tag);
				}
				else
				{
                    this.removalInProcessTag = tag;
					tag.DeletedCompleted += FinishRemoveTag;
					tag.DeleteTag();
				}
			}
		}

		private void FinishRemoveTag(TagBase tag)
		{
			Console.WriteLine("Finish removal of tag");

			var relativeIndex = GetRelativeIndex(tag);
			var num = this.tagPanel.Children.IndexOf(tag);

            this.tagPanel.Children.Remove(tag);
            DetachTagEvents(tag);

            var num2 = num;

			if (this.removalInProcessDirection == NavigateDirection.Left)
			{
				num2--;
			}

			if (num2 >= this.tagPanel.Children.Count)
			{
				num2 = this.tagPanel.Children.Count - 1;
			}

			if (!Propagating && num2 >= 0)
			{
				Console.WriteLine("focus called as part of removal " + num2);
				((TagBase)this.tagPanel.Children[num2]).Focus();
			}

            EnableChangePropagation = false;

			if (!Propagating && ItemsSource != null && !tag.IsInvalid)
			{
                ItemsSource.RemoveAt(relativeIndex);
			}

            EnableChangePropagation = true;
            this.removalInProcessTag = null;
		}

		private void DetachTagEvents(TagBase tag)
		{
			tag.NavigationRequested -= OnTagNavigationRequested;
			tag.DeleteRequested -= OnTagDeleteRequested;
			tag.DeletedCompleted -= FinishRemoveTag;
			tag.SearchRequested -= OnTagSearchRequested;
			tag.SelectionRequested -= OnTagSelectionRequested;
			tag.LostFocus += OnTagLostFocus;
			tag.GotFocus += OnTagGotFocus;
			tag.AdjacentInsertRequested -= OnTagAdjacentInsertRequested;
			tag.MergeRequested -= OnTagMergeRequested;
		}

		private void OnTagDeleteRequested(TagBase source, NavigateDirection direction)
		{
			var num = this.tagPanel.Children.IndexOf(source);
			Console.WriteLine(string.Concat("DeleteRequested ", num, " ", direction));

			var num2 = (int)(num + direction);

			if (num2 >= 0 && num2 < this.tagPanel.Children.Count)
			{
				var tagBase = this.tagPanel.Children[num2] as TagBase;

                this.removalInProcessDirection = direction;

				if (tagBase is UnresolvedTag && (tagBase as UnresolvedTag).Text.Length > 0)
				{
					tagBase.Focus(direction);
				}
				else
				{
                    RemoveTag(tagBase);
				}
			}
		}

        private void OnTagNavigationRequested(TagBase source, NavigateDirection direction)
        {
            switch (direction)
            {
                case NavigateDirection.Up:
                    MoveSearchResultsIndex(-1);
                    break;
                case NavigateDirection.Down:
                    MoveSearchResultsIndex(1);
                    break;
                default:
                    var num = this.tagPanel.Children.IndexOf(source);
                    var num2 = (int)(num + direction);
                    if (num2 >= 0 && num2 < this.tagPanel.Children.Count)
                    {
                        Console.WriteLine("focus called after navigation requested: " + direction);
                        ((TagBase)this.tagPanel.Children[num2]).Focus(direction);
                    }
                    break;
            }
        }

        private void OnTagSelectionRequested(TagBase source, SelectionType selectionType)
		{
			if (selectionType == SelectionType.Close)
			{
                HideSearchResults();
			}
			else
			{
				if (this.searchResultsListBox.SelectedIndex == -1 && this.searchResultsListBox.Items.Count > 0)
				{
                    this.searchResultsListBox.SelectedIndex = 0;
				}

				if (this.searchResultsListBox.SelectedItem != null && this.searchResultsPopup.IsOpen)
				{
                    AddSelectedItem(this.searchResultsListBox.SelectedItem);
                    HideSearchResults();
				}
			}
		}

		private void OnTagMergeRequested(TagBase left)
		{
			var num = this.tagPanel.Children.IndexOf(left);
			if (num + 1 < this.tagPanel.Children.Count && left is UnresolvedTag)
			{
				if (this.tagPanel.Children[num + 1] is UnresolvedTag)
				{
					var unresolvedTag = this.tagPanel.Children[num + 1] as UnresolvedTag;
					var unresolvedTag2 = (UnresolvedTag)left;
                    this.removalInProcessDirection = NavigateDirection.Left;
                    RemoveTag(unresolvedTag);
					unresolvedTag2.MergeText(unresolvedTag.Text);
				}
			}
		}

		private void OnTagAdjacentInsertRequested(TagBase left, string adjacentText, int selectionOffset)
		{
			var num = this.tagPanel.Children.IndexOf(left);
			if (num <= this.tagPanel.Children.Count - 1)
			{
				var unresolvedTag = NewUnresolved();
				unresolvedTag.Text = adjacentText;
                InsertTag(num + 1, unresolvedTag);
				unresolvedTag.Focus(selectionOffset);
			}
		}

		private void MoveSearchResultsIndex(int adjustment)
		{
		    if (!this.searchResultsPopup.IsOpen || this.searchResultsListBox.Items.Count <= 0)
		    {
		        return;
		    }

		    var selectedIndex = this.searchResultsListBox.SelectedIndex;

		    var num = selectedIndex + adjustment;
		    if (num < 0)
		    {
		        num = this.searchResultsListBox.Items.Count - 1;
		    }
		    else if (num >= this.searchResultsListBox.Items.Count)
		    {
		        num = 0;
		    }

            this.searchResultsListBox.SelectedIndex = num;
		}

		private void OnTagSearchRequested(TagBase source, string searchText)
		{
			Console.WriteLine("Search requested from " + this.tagPanel.Children.IndexOf(source));

            SearchSource = source;
			if (SearchMethod == null)
			{
				throw new InvalidOperationException("The tagbox is missing and Action<string> delegate for the SearchMethod");
			}

            SearchMethod.Invoke(searchText.Trim());
		}

		private void OnTagGotFocus(object sender, RoutedEventArgs e)
		{
            this.tagScrollViewer.ScrollIntoView(sender as TagBase, 0.0, 5.0, new Duration(new TimeSpan(0, 0, 0)));
		    var tagBase = sender as TagBase;

		    if (tagBase != null && tagBase.ValidatedValue != SelectedItem)
			{
                InternalFocusEvent = true;
                SelectedItem = ((TagBase)sender).ValidatedValue;
                InternalFocusEvent = false;
			}
		}

		private void OnTagLostFocus(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("Search tag lost focus " + this.tagPanel.Children.IndexOf(sender as TagBase));
			if (SearchSource == sender && this.searchResultsPopup.IsOpen)
			{
				object focusedElement = FocusManager.GetFocusedElement(this);
				if (focusedElement is ListBoxItem)
				{
					var listBoxItem = focusedElement as ListBoxItem;
					if (this.searchResultsListBox.Items.Contains(listBoxItem.DataContext))
					{
                        AddSelectedItem((focusedElement as ListBoxItem).DataContext);
					}
				}
                HideSearchResults();
			}
			if (sender is UnresolvedTag)
			{
                ValidateOne(sender as UnresolvedTag);
			}
		}

		private void HideSearchResults()
		{
			Console.WriteLine("Hiding Search results");
            this.searchResultsPopup.IsOpen = false;
		}

		private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tagBox = d as TagBox;
		    tagBox.IntitiateDatabind(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
		}

		private static void SearchResultsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tagBox = (TagBox)d;
			if (tagBox.SearchSource != null)
			{
				var enumerable = e.NewValue as IEnumerable;
				if (enumerable == null)
				{
					Console.WriteLine("hiding serach results bc of null");
					tagBox.HideSearchResults();
				}
				else
				{
					var flag = false;
					var enumerator = enumerable.GetEnumerator();
					try
					{
						if (enumerator.MoveNext())
						{
						    flag = true;
						}
					}
					finally
					{
						var disposable = enumerator as IDisposable;
					    disposable?.Dispose();
					}
					if (!flag)
					{
						tagBox.HideSearchResults();
					}
					else
					{
                        var containingWindow = Window.GetWindow(d);

                        var generalTransform = tagBox.SearchSource.TransformToVisual(containingWindow);
						var point = generalTransform.Transform(new Point(0.0, 0.0));
						double num;
						if (point.Y + tagBox.SearchResultsHeight + tagBox.SearchSource.ActualHeight > containingWindow.ActualHeight)
						{
							num = -1.0 - tagBox.SearchResultsHeight;
						}
						else
						{
							num = tagBox.SearchSource.ActualHeight + 1.0;
						}

						double num2;
                        
						if (point.X + tagBox.SearchResultsWidth > containingWindow.ActualWidth)
						{
							num2 = containingWindow.ActualWidth - (point.X + tagBox.SearchResultsWidth + 5.0);
						}
						else
						{
							num2 = 0.0;
						}

						var generalTransform2 = tagBox.SearchSource.TransformToVisual(tagBox.tagPanel);
						var point2 = generalTransform2.Transform(new Point(0.0, 0.0));
						tagBox.searchResultsPopup.HorizontalOffset = point2.X + num2;
						tagBox.searchResultsPopup.VerticalOffset = point2.Y + num;
						tagBox.searchResultsPopup.IsOpen = true;
					    tagBox.searchResultsPopup.PlacementTarget = tagBox;
                        tagBox.searchResultsPopup.Placement = PlacementMode.Bottom;

                        Console.WriteLine("Showing serach results");
					}
				}
			}
		}

		private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tagBox = d as TagBox;
			var newValue = e.NewValue;

			if (!tagBox.InternalFocusEvent && newValue != null)
			{
				using (var enumerator = (IEnumerator<UIElement>)tagBox.tagPanel.Children.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						var tagBase = (TagBase)enumerator.Current;
						if (tagBase.ValidatedValue == newValue)
						{
							tagBase.Focus(NavigateDirection.Right);
						}
					}
				}
			}
		}

		private void IntitiateDatabind(IEnumerable oldItems, IEnumerable items)
		{
			if (this.tagPanel != null)
			{
				if (oldItems is INotifyCollectionChanged)
				{
					((INotifyCollectionChanged)items).CollectionChanged -= ItemsSourceCollectionChanged;
				}

                this.tagPanel.Children.Clear();

				if (items is INotifyCollectionChanged)
				{
					((INotifyCollectionChanged)items).CollectionChanged += ItemsSourceCollectionChanged;
				}

			    foreach (var current in items)
			    {
                    AddTag(NewUnresolved());
			        var resolvedTag = new ResolvedTag
                    {
                        DataContext = current
                    };

			        if (TagStyle != null)
			        {
			            resolvedTag.Style = TagStyle;
			        }

                    AddTag(resolvedTag);
                }

                AddTag(NewUnresolved());
			}
		}

		private UnresolvedTag NewUnresolved()
		{
			return new UnresolvedTag
			{
				SeperatorChars = SeperatorChars
            };
		}

		private void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (EnableChangePropagation)
			{
                Propagating = true;
				if (e.Action == NotifyCollectionChangedAction.Remove)
				{
					var num = -1;
				    foreach (var tagBase in this.tagPanel.Children.OfType<TagBase>())
				    {
				        if (!tagBase.IsInvalid)
				        {
				            num++;
				        }
				        if (num == e.OldStartingIndex)
				        {
                            RemoveTag(tagBase);
				            break;
				        }
				    }
				}
				else if (e.Action == NotifyCollectionChangedAction.Add)
				{
					var num2 = e.NewStartingIndex;
				    foreach (var newItem in e.NewItems)
				    {
                        InsertItem(GetAbsoluteIndex(num2), newItem);
                        num2++;
                    }
				}
				else if (e.Action == NotifyCollectionChangedAction.Replace)
				{
					var num2 = e.NewStartingIndex;
				    foreach (var newItem in e.NewItems)
				    {
                        ReplaceItem(GetAbsoluteIndex(num2), newItem);
				        num2++;
				    }
				}
				else if (e.Action == NotifyCollectionChangedAction.Reset)
				{
                    IntitiateDatabind(ItemsSource, ItemsSource);
				}
                Propagating = false;
			}
		}

		private void InsertItem(int index, object item)
		{
			var children = this.tagPanel.Children;

            var resolvedTag = new ResolvedTag { DataContext = item };
            if (TagStyle != null)
            {
                resolvedTag.Style = TagStyle;
            }

            var resolvedTag2 = resolvedTag;
			resolvedTag2.ShowAddAnimation = true;

            InsertTag(index, resolvedTag2);

			if (index == 0)
			{
                InsertTag(index, NewUnresolved());
			}

			if (children.IndexOf(resolvedTag2) == children.Count - 1)
			{
                InsertTag(children.IndexOf(resolvedTag2) + 1, NewUnresolved());
			}

			if (!(children[children.IndexOf(resolvedTag2) - 1] is UnresolvedTag))
			{
                InsertTag(children.IndexOf(resolvedTag2), NewUnresolved());
			}

			if (!(children[children.IndexOf(resolvedTag2) + 1] is UnresolvedTag))
			{
                InsertTag(children.IndexOf(resolvedTag2) + 1, NewUnresolved());
			}

			var relativeIndex = GetRelativeIndex(resolvedTag2);
            EnableChangePropagation = false;

			if (!Propagating)
			{
                ItemsSource?.Insert(relativeIndex, item);
			}

            EnableChangePropagation = true;

			resolvedTag2.AddCompleted += delegate {
                tagScrollViewer.ScrollIntoView(resolvedTag);
			};
		}

		private void ReplaceItem(int index, object item)
		{
            RemoveTag(this.tagPanel.Children[index] as TagBase);
            InsertItem(index, item);
		}

		private void AddSelectedItem(object item)
		{
			var num = this.tagPanel.Children.IndexOf(SearchSource);
			if (num != -1)
			{
				((UnresolvedTag)this.tagPanel.Children[num]).Clear();
                InsertItem(num, item);
			}
		}

		private int GetRelativeIndex(TagBase tag)
		{
			var num = -1;
			foreach(var tagBase in this.tagPanel.Children.OfType<TagBase>())
			{
				if (!tagBase.IsInvalid)
				{
					num++;
				}
				if (tagBase == tag)
				{
					break;
				}
			}
			return num;
		}

		private int GetAbsoluteIndex(int relIndex)
		{
			var num = -1;
			var num2 = -1;
			using (var enumerator = (IEnumerator<UIElement>)this.tagPanel.Children.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					var tagBase = (TagBase)enumerator.Current;
					num++;
					if (!tagBase.IsInvalid)
					{
						num2++;
					}
					if (num2 == relIndex)
					{
						break;
					}
				}
			}
			return num;
		}
	}
}
