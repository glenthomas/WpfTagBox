namespace WpfControls.TagBox
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    [TemplatePart(Name = "Content", Type = typeof(ContentPresenter)),
     TemplatePart(Name = "CloseButton", Type = typeof(ButtonBase)),
     TemplateVisualState(Name = "Deleted", GroupName = "CommonStates"),
     TemplateVisualState(Name = "Added", GroupName = "CommonStates"),
     TemplateVisualState(Name = "Focused", GroupName = "CommonStates"),
     TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
	public class ResolvedTag : TagBase
	{
		private ContentPresenter content;
		private Storyboard deleted;
		private ButtonBase closeButton;

		public bool ShowAddAnimation { get; set; }

		public override object ValidatedValue
		{
			get
			{
				return base.DataContext;
			}
			set
			{
				base.DataContext = value;
			}
		}

		public ResolvedTag()
		{
			base.IsInvalid = false;
			base.Background = new SolidColorBrush(Colors.Blue);
			base.DefaultStyleKey = typeof(ResolvedTag);
			base.KeyDown += ResolvedTagKeyDown;
			base.GotFocus += ResolvedTagGotFocus;
		    base.LostFocus += ResolvedTagLostFocus;
			base.MouseLeftButtonUp += delegate { base.Focus(); };

			base.MouseEnter += delegate
			    {
				if (!HasFocus())
				{
					VisualStateManager.GoToState(this, "MouseOver", true);
				}
			};

			base.MouseLeave += delegate { VisualStateManager.GoToState(this, HasFocus() ? "Focused" : "Normal", true); };
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

            this.content = (ContentPresenter)base.GetTemplateChild("Content");

		    var binding = new Binding { Mode = BindingMode.OneWay, Source = base.DataContext };

		    if (base.DisplayMemberPath != null)
			{
				binding.Path = new PropertyPath(base.DisplayMemberPath);
			}

            this.content.SetBinding(ContentPresenter.ContentProperty, binding);

            this.closeButton = (base.GetTemplateChild("CloseButton") as ButtonBase);

			if (this.closeButton != null)
			{
                this.closeButton.Click += delegate
			        {
			            VisualStateManager.GoToState(this, "Focused", false);
                        OnDeleteRequested(NavigateDirection.Self);
			        };
			}

			var visualState = base.GetTemplateChild("Deleted") as VisualState;
			if (visualState != null)
			{
                this.deleted = visualState.Storyboard;
                this.deleted.Completed += DeleteSbCompleted;
			}

			var visualState2 = base.GetTemplateChild("Added") as VisualState;
			if (visualState2 != null)
			{
				var storyboard = visualState2.Storyboard;
			    storyboard.Completed += delegate { OnAddCompleted(); };

				if (ShowAddAnimation)
				{
					VisualStateManager.GoToState(this, "Added", false);
				}
			}
		}

		private bool HasFocus()
		{
			return FocusManager.GetFocusedElement(Window.GetWindow(this)) == this;
		}

		private void ResolvedTagGotFocus(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Focused", true);
		}

		private void ResolvedTagLostFocus(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Normal", true);
		}

		private void ResolvedTagKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.ImeAccept || e.Key == Key.ImeConvert)
			{
				var direction = (e.Key == Key.ImeConvert) ? NavigateDirection.Left : NavigateDirection.Right;
                OnNavigationRequested(direction);
			}
			else if (e.Key == Key.Cancel || e.Key == Key.PageUp)
			{
				var direction = NavigateDirection.Self;
                OnDeleteRequested(direction);
			}
		}

		public override void DeleteTag()
		{
            //this.deleted.Begin();
            VisualStateManager.GoToState(this, "Deleted", true);
            if (this.deleted == null)
			{
                OnDeleteCompleted(null);
			}
		}

		private void DeleteSbCompleted(object sender, EventArgs e)
		{
			Console.WriteLine("delete sb completed");
            OnDeleteCompleted(null);
		}

		public override void OnDeleteCompleted(RoutedEventArgs e)
		{
            this.deleted?.Stop();
		    base.OnDeleteCompleted(e);
		}

		public override string ToString()
		{
		    return base.DataContext != null ? base.DataContext.ToString() : base.ToString();
		}
	}
}
