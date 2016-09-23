namespace WpfControls.TagBox
{
    using System.Windows;
    using System.Windows.Controls;

    using WpfControls.TagBox.Handlers;

    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates"), TemplateVisualState(Name = "Invalid", GroupName = "CommonStates")]
	public class TagBase : Control
	{
		public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(TagBase), new PropertyMetadata(null));
		public static readonly DependencyProperty IsInvalidProperty = DependencyProperty.Register("IsInvalid", typeof(bool), typeof(TagBase), new PropertyMetadata(true, OnIsInvalidPropertyChanged));
		public event DeleteCompletedHandler DeletedCompleted;
		public event NavigationRequestHandler NavigationRequested;
		public event DeleteRequestHandler DeleteRequested;
		public event SearchRequestedHandler SearchRequested;
		public event SelectionRequestedHandler SelectionRequested;
		public event AddCompletedHandler AddCompleted;
		public event MergeRequestedHandler MergeRequested;
		public event AdjacentInsertRequestedHandler AdjacentInsertRequested;

		public string DisplayMemberPath
		{
			get
			{
				return (string)this.GetValue(DisplayMemberPathProperty);
			}
			set
			{
				this.SetValue(DisplayMemberPathProperty, value);
			}
		}

		public bool IsInvalid
		{
			get
			{
				return (bool)this.GetValue(IsInvalidProperty);
			}
			set
			{
				this.SetValue(IsInvalidProperty, value);
			}
		}

		public virtual object ValidatedValue
		{
			get;
			set;
		}

		public TagBase()
		{
			this.IsTabStop = true;
		}

		public void MarkInvalid()
		{
			this.IsInvalid = true;
			VisualStateManager.GoToState(this, "Invalid", true);
		}

		private static void OnIsInvalidPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tagBase = d as TagBase;
			var flag = (bool)e.NewValue;
			if (flag)
			{
				VisualStateManager.GoToState(tagBase, "Invalid", true);
			}
			else
			{
				VisualStateManager.GoToState(tagBase, "Normal", true);
			}
		}

		public virtual bool Focus(NavigateDirection direction)
		{
			return base.Focus();
		}

		public virtual bool Focus(int selectionOffset)
		{
			return base.Focus();
		}

		public virtual void DeleteTag()
		{
			this.OnDeleteCompleted(null);
		}

		public virtual void OnDeleteCompleted(RoutedEventArgs e)
		{
			var deletedCompleted = this.DeletedCompleted;
			if (deletedCompleted != null)
			{
				deletedCompleted(this);
			}
		}

		public virtual void OnNavigationRequested(NavigateDirection direction)
		{
			var navigationRequested = this.NavigationRequested;
			if (navigationRequested != null)
			{
				navigationRequested(this, direction);
			}
		}

		public virtual void OnDeleteRequested(NavigateDirection direction)
		{
			var deleteRequested = this.DeleteRequested;
			if (deleteRequested != null)
			{
				deleteRequested(this, direction);
			}
		}

		public virtual void OnSearchRequested(string searchText)
		{
			var searchRequested = this.SearchRequested;
			if (searchRequested != null)
			{
				searchRequested(this, searchText);
			}
		}

		public virtual void OnSelectionRequested(SelectionType selectionType)
		{
			var selectionRequested = this.SelectionRequested;
			if (selectionRequested != null)
			{
				selectionRequested(this, selectionType);
			}
		}

		public virtual void OnAddCompleted()
		{
			var addCompleted = this.AddCompleted;
			if (addCompleted != null)
			{
				addCompleted(this);
			}
		}

		public virtual void OnMergeRequested()
		{
			var mergeRequested = this.MergeRequested;
			if (mergeRequested != null)
			{
				mergeRequested(this);
			}
		}

		public virtual void OnAdjacentInsertRequested(string adjacentText, int selectionOffset)
		{
			var adjacentInsertRequested = this.AdjacentInsertRequested;
			if (adjacentInsertRequested != null)
			{
				adjacentInsertRequested(this, adjacentText, selectionOffset);
			}
		}
	}
}
