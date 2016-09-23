
namespace WpfControls.TagBox
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    [TemplatePart(Name = "TextControl", Type = typeof(TextBox))]
    public class UnresolvedTag : TagBase
    {
        private TextBox _textControl;
        private string _prevText = string.Empty;
        private string _pendingText;
        private bool _pendingFocus;
        private int _pendingSelectionOffset = -1;
        private bool _clearing;

        public string Text
        {
            get
            {
                return (_textControl == null) ? string.Empty : _textControl.Text;
            }
            set
            {
                if (_textControl == null)
                {
                    _pendingText = value;
                }
                else
                {
                    _textControl.Text = value;
                }
            }
        }

        public string FilteredText => FilterSeperators(Text);

        public char[] SeperatorChars { get; set; }

        public UnresolvedTag()
        {
            Background = new SolidColorBrush(Colors.Gray);
            DefaultStyleKey = typeof(UnresolvedTag);
            Cursor = Cursors.IBeam;
            IsTabStop = false;
        }

        public override bool Focus(NavigateDirection direction)
        {
            if (_textControl == null)
            {
                Debug.WriteLine("PENDING FOCUS CALLED ON UNRESOLVED TAG:" + direction);
                _pendingFocus = true;
                return true;
            }

            if (_textControl.Text.Length > 0)
            {
                _textControl.SelectionStart = (direction == NavigateDirection.Left) ? _textControl.Text.Length : 0;
            }

            return base.Focus(direction);
        }

        public override bool Focus(int selectionOffset)
        {
            if (_textControl == null)
            {
                Debug.WriteLine("PENDING FOCUS CALLED ON UNRESOLVED TAG:" + selectionOffset);
                _pendingSelectionOffset = selectionOffset;

                return true;
            }

            _textControl.SelectionLength = 0;
            if (_textControl.Text.Length >= selectionOffset)
            {
                _textControl.SelectionStart = selectionOffset;
            }

            return base.Focus(selectionOffset);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (_textControl != FocusManager.GetFocusedElement(this))
            {
                _textControl.Focus();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textControl = (TextBox)GetTemplateChild("TextControl");

            if (_pendingText != null)
            {
                _textControl.Text = _pendingText;
                _pendingText = null;
            }

            _textControl.TextChanged += TextControlTextChanged;
            _textControl.Width = 50.0;
            _textControl.PreviewKeyDown += TextControlKeyDown;
            _textControl.IsTabStop = true;

            _textControl.Width = string.IsNullOrEmpty(_textControl.Text) ? 5.0 : double.NaN;

            if (_pendingFocus)
            {
                base.Focus();
                _pendingFocus = false;
            }

            if (_pendingSelectionOffset > -1)
            {
                base.Focus();

                if (_pendingSelectionOffset < _textControl.Text.Length)
                {
                    _textControl.SelectionStart = _pendingSelectionOffset;
                }

                _pendingSelectionOffset = -1;
            }
        }

        private void TextControlKeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine(e.Key);
            if (e.Key == Key.Left && _textControl.SelectionLength == 0 && _textControl.SelectionStart == 0)
            {
                Console.WriteLine("NavigateLeft");
                OnNavigationRequested(NavigateDirection.Left);
            }
            else if (e.Key == Key.Right && _textControl.SelectionLength == 0 && _textControl.SelectionStart == _textControl.Text.Length)
            {
                Console.WriteLine("NavigateRight");
                OnNavigationRequested(NavigateDirection.Right);
            }
            else if (e.Key == Key.Back && _textControl.SelectionLength == 0 && _textControl.SelectionLength == 0)
            {
                Console.WriteLine("Delete previous tag");
                OnDeleteRequested(NavigateDirection.Left);
            }
            else if (e.Key == Key.Delete && _textControl.SelectionLength == 0 && _textControl.SelectionStart == _textControl.Text.Length)
            {
                Console.WriteLine("Delete next tag");
                OnDeleteRequested(NavigateDirection.Right);
            }
            else if (e.Key == Key.Up)
            {
                Console.WriteLine("NavigateUp");
                OnNavigationRequested(NavigateDirection.Up);
            }
            else if (e.Key == Key.Down)
            {
                Console.WriteLine("NavigateDown");
                OnNavigationRequested(NavigateDirection.Down);
            }
            else if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                Console.WriteLine("Select tag");
                OnSelectionRequested(SelectionType.Select);
            }
            else if (e.Key == Key.Escape)
            {
                OnSelectionRequested(SelectionType.Close);
            }
        }

        private void TextControlTextChanged(object sender, TextChangedEventArgs e)
        {
            _textControl.Width = string.IsNullOrEmpty(_textControl.Text) ? 5.0 : double.NaN;

            if (!_clearing)
            {
                if (ContainsTrailingSeperatorChar(_prevText) && !ContainsTrailingSeperatorChar(_textControl.Text))
                {
                    Console.WriteLine("Merge Requested");
                    OnMergeRequested();
                }
                else if (ContainsNonTrailingSeperatorChar(_textControl.Text) || (!ContainsSeperatorChar(_prevText) & ContainsSeperatorChar(_textControl.Text)))
                {
                    var num = IndexOfSeperator(_textControl.Text);
                    var num2 = _textControl.SelectionStart - num - 1;
                    Console.WriteLine("Adjacent Insert Requested:  selection offset:" + num2);
                    OnAdjacentInsertRequested(_textControl.Text.Substring(num + 1), num2);
                    _clearing = true;
                    _textControl.Text = _textControl.Text.Substring(0, num + 1);
                }
                else
                {
                    var text = FilterSeperators(_textControl.Text);
                    Console.WriteLine("Searching for " + text);
                    OnSearchRequested(text);
                }
            }

            if (_textControl.Text.Trim().Length == 0)
            {
                VisualStateManager.GoToState(this, "Normal", true);
            }

            _clearing = false;
            _prevText = _textControl.Text;
        }

        private string FilterSeperators(string text)
        {
            var text2 = text;
            var seperatorChars = SeperatorChars;

            for (var i = 0; i < seperatorChars.Length; i++)
            {
                var c = seperatorChars[i];
                text2 = text2.Replace(c.ToString(), "");
            }

            return text2;
        }

        private int IndexOfSeperator(string input)
        {
            var num = -1;
            var seperatorChars = SeperatorChars;

            for (var i = 0; i < seperatorChars.Length; i++)
            {
                var c = seperatorChars[i];
                num = input.IndexOf(c);
                if (num != -1)
                {
                    break;
                }
            }

            return num;
        }

        private bool ContainsTrailingSeperatorChar(string input)
        {
            return input.Length > 0 && SeperatorChars.Contains(input.ToCharArray().Last());
        }

        private bool ContainsNonTrailingSeperatorChar(string input)
        {
            var seperatorChars = SeperatorChars;

            for (var i = 0; i < seperatorChars.Length; i++)
            {
                var c = seperatorChars[i];
                var num = input.IndexOf(c);

                if (num != -1 && num < input.Length - 1)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsSeperatorChar(string testStr)
        {
            var seperatorChars = SeperatorChars;

            for (var i = 0; i < seperatorChars.Length; i++)
            {
                var c = seperatorChars[i];
                if (testStr.ToCharArray().Contains(c))
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _clearing = true;
            _textControl.Text = "";
        }

        public void MergeText(string rightText)
        {
            var selectionStart = _textControl.SelectionStart;
            _clearing = true;
            _textControl.Text = _textControl.Text + rightText;
            _textControl.SelectionStart = selectionStart;
        }
    }
}