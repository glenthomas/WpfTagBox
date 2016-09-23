
namespace TagBox.Demo
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using Model;

    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private IEnumerable<Tag> searchTags;

        public ObservableCollection<Tag> Tags { get; set; }

        public Action<string> SearchAction { get; set; }

        public IEnumerable<Tag> SearchTags
        {
            get
            {
                return this.searchTags;
            }
            set
            {
                this.searchTags = value;
                this.OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            this.Tags = new ObservableCollection<Tag>(new[] { new Tag { Value = "Glen"}, new Tag { Value = "Thomas"}, new Tag { Value = ".NET" }, new Tag { Value = "Developer" } });
            SearchAction = s => this.SearchTags = this.Tags.Where(tag => tag.Value.StartsWith(s));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
