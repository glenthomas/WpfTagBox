namespace WpfControls.TagBox.Controls
{
    using System.Windows.Controls;

    internal struct OrientedSize
	{
        public Orientation Orientation { get; }

        public double Direct { get; set; }

        public double Indirect { get; set; }

        public double Width
		{
			get
			{
				return (this.Orientation == Orientation.Vertical) ? this.Direct : this.Indirect;
			}
			set
			{
				if (this.Orientation == Orientation.Vertical)
				{
					this.Direct = value;
				}
				else
				{
					this.Indirect = value;
				}
			}
		}

		public double Height
		{
			get
			{
				return (this.Orientation != Orientation.Vertical) ? this.Direct : this.Indirect;
			}
			set
			{
				if (this.Orientation != Orientation.Vertical)
				{
					this.Direct = value;
				}
				else
				{
					this.Indirect = value;
				}
			}
		}

		public OrientedSize(Orientation orientation)
		{
			this = new OrientedSize(orientation, 0.0, 0.0);
		}

		public OrientedSize(Orientation orientation, double width, double height)
		{
			this.Orientation = orientation;
			this.Direct = 0.0;
			this.Indirect = 0.0;
			this.Width = width;
			this.Height = height;
		}
	}
}
