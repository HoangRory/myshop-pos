using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyShop.Client.Helpers
{
    public static class InputMaskBehavior
    {
        public static readonly DependencyProperty MaskProperty = DependencyProperty.RegisterAttached(
            "Mask",
            typeof(string),
            typeof(InputMaskBehavior),
            new PropertyMetadata(null, OnMaskChanged));

        public static void SetMask(DependencyObject element, string? value) => element.SetValue(MaskProperty, value);
        public static string? GetMask(DependencyObject element) => (string?)element.GetValue(MaskProperty);

        private static void OnMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DatePickerTextBox dpTextBox)
            {
                if (e.NewValue is string mask && !string.IsNullOrEmpty(mask))
                {
                    dpTextBox.PreviewTextInput += OnPreviewTextInput;
                    dpTextBox.PreviewKeyDown += OnPreviewKeyDown;
                    DataObject.AddPastingHandler(dpTextBox, OnPasting);
                    dpTextBox.Loaded += (s, a) => EnsureInitialMask(dpTextBox, mask);
                }
                else
                {
                    dpTextBox.PreviewTextInput -= OnPreviewTextInput;
                    dpTextBox.PreviewKeyDown -= OnPreviewKeyDown;
                    DataObject.RemovePastingHandler(dpTextBox, OnPasting);
                }
            }
        }

        

        private static void EnsureInitialMask(DatePickerTextBox textBox, string mask)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = mask;
                // place caret at first placeholder
                Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() => MoveCaretToNextPlaceholder(textBox, 0)), DispatcherPriority.Input);
            }
        }

        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not DatePickerTextBox textBox) return;
            var mask = GetMask(textBox);
            if (string.IsNullOrEmpty(mask)) return;

            e.Handled = true; // we'll handle input
            var input = e.Text;
            if (string.IsNullOrEmpty(input) || !char.IsDigit(input[0])) return;

            var pos = textBox.CaretIndex;
            var next = FindNextPlaceholderIndex(mask, textBox.Text, pos);
            if (next < 0)
            {
                // try from start
                next = FindNextPlaceholderIndex(mask, textBox.Text, 0);
            }

            if (next < 0) return;

            var chars = textBox.Text.ToCharArray();
            chars[next] = input[0];
            textBox.Text = new string(chars);
            MoveCaretToNextPlaceholder(textBox, next + 1);
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not DatePickerTextBox textBox) return;
            var mask = GetMask(textBox);
            if (string.IsNullOrEmpty(mask)) return;

            if (e.Key == Key.Back)
            {
                e.Handled = true;
                var pos = Math.Max(0, textBox.CaretIndex - 1);
                var prev = FindPreviousFilledIndex(mask, textBox.Text, pos);
                if (prev >= 0)
                {
                    var chars = textBox.Text.ToCharArray();
                    chars[prev] = mask[prev];
                    textBox.Text = new string(chars);
                    MoveCaretToNextPlaceholder(textBox, prev);
                }
            }
            else if (e.Key == Key.Delete)
            {
                e.Handled = true;
                var pos = textBox.CaretIndex;
                var next = FindNextFilledIndex(mask, textBox.Text, pos);
                if (next >= 0)
                {
                    var chars = textBox.Text.ToCharArray();
                    chars[next] = mask[next];
                    textBox.Text = new string(chars);
                    MoveCaretToNextPlaceholder(textBox, next);
                }
            }
        }

        private static void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not DatePickerTextBox textBox) return;
            var mask = GetMask(textBox);
            if (string.IsNullOrEmpty(mask)) return;

            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            var paste = (string)e.DataObject.GetData(typeof(string))!;
            var digits = new string(paste.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digits))
            {
                e.CancelCommand();
                return;
            }

            e.CancelCommand();
            // fill from start
            var chars = mask.ToCharArray();
            int di = 0;
            for (int i = 0; i < chars.Length && di < digits.Length; i++)
            {
                if (IsPlaceholder(mask[i]))
                {
                    chars[i] = digits[di++];
                }
            }
            textBox.Text = new string(chars);
            MoveCaretToNextPlaceholder(textBox, 0);
        }

        private static int FindNextPlaceholderIndex(string mask, string text, int start)
        {
            for (int i = start; i < mask.Length && i < text.Length; i++)
            {
                if (IsPlaceholder(mask[i]) && text[i] == mask[i])
                    return i;
            }
            return -1;
        }

        private static int FindNextFilledIndex(string mask, string text, int start)
        {
            for (int i = start; i < mask.Length && i < text.Length; i++)
            {
                if (IsPlaceholder(mask[i]) && text[i] != mask[i])
                    return i;
            }
            return -1;
        }

        private static int FindPreviousFilledIndex(string mask, string text, int start)
        {
            for (int i = Math.Min(start, mask.Length - 1); i >= 0; i--)
            {
                if (IsPlaceholder(mask[i]) && text[i] != mask[i])
                    return i;
            }
            return -1;
        }

        private static void MoveCaretToNextPlaceholder(DatePickerTextBox textBox, int start)
        {
            var mask = GetMask(textBox) ?? string.Empty;
            for (int i = start; i < mask.Length; i++)
            {
                if (IsPlaceholder(mask[i]) && textBox.Text[i] == mask[i])
                {
                    textBox.CaretIndex = i;
                    return;
                }
            }
            // if none, place at end
            textBox.CaretIndex = mask.Length;
        }

        private static bool IsPlaceholder(char c) => c == 'd' || c == 'M' || c == 'y' || c == 'D' || c == 'm' || c == 'Y';

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && parent is not T)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }
}
