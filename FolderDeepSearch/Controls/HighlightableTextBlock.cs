using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using TheRThemes;

namespace FolderDeepSearch.Controls
{
    public class HighlightableTextBlock : TextBlock
    {
        public static readonly DependencyProperty SelectionProperty =  
            DependencyProperty.RegisterAttached(
                "Selection", 
                typeof(string), 
                typeof(HighlightableTextBlock), 
                new PropertyMetadata(new PropertyChangedCallback(SelectText)));

        public string Selection
        {
            get => (string)GetValue(SelectionProperty);
            set => SetValue(SelectionProperty, value);
        }

        private static void SelectText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null) return;
            if (!(d is TextBlock)) throw new InvalidOperationException("Only valid for TextBlock");

            TextBlock txtBlock = d as TextBlock;
            string text = txtBlock.Text;
            if (string.IsNullOrEmpty(text)) return;

            string highlightText = (string)d.GetValue(SelectionProperty);
            if (string.IsNullOrEmpty(highlightText)) return;

            int index = text.IndexOf(highlightText, StringComparison.CurrentCultureIgnoreCase);
            if (index < 0) return;

            SolidColorBrush selectionColor = ThemesController.GetSolidBrush("ControlPrimaryColourBackground");
            SolidColorBrush forecolor = ThemesController.GetSolidBrush("ControlDefaultForeground");

            txtBlock.Inlines.Clear();
            // idk stops it from freezing the whole app in the event of a bug
            for (int i = 0; i < 500; i++)
            {
                txtBlock.Inlines.AddRange(new Inline[] 
                {
                    new Run(text.Substring(0, index)),
                    new Run(text.Substring(index, highlightText.Length)) { Background = selectionColor, Foreground = forecolor }
                });

                text = text.Substring(index + highlightText.Length);
                index = text.IndexOf(highlightText, StringComparison.CurrentCultureIgnoreCase);

                if (index < 0)
                {
                    txtBlock.Inlines.Add(new Run(text));
                    break;
                }
            }
        }
    }
}
