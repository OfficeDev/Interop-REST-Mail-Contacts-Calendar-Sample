using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace MeetingManager.Views
{
    public static class AttachedProperties
    {
        public static string GetHtml(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlProperty);
        }

        public static void SetHtml(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlProperty, value);
        }

        // Using a DependencyProperty as the backing store for HTML.  This enables animation, styling, binding, etc...  
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.RegisterAttached("Html", typeof(string), typeof(AttachedProperties), new PropertyMetadata(0, new PropertyChangedCallback(OnHTMLChanged)));

        private static void OnHTMLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wv = d as Windows.UI.Xaml.Controls.WebView;

            if (wv != null && e.NewValue is string)
            {
                wv.NavigateToString((string)e.NewValue);
            }
        }
    }
}
