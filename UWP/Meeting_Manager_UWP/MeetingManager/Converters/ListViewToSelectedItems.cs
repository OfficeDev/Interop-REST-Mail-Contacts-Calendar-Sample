using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MeetingManager.Converters
{
    class ListViewToSelectedItems : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var listView = value as ListView;   // or IListViewBase

            return listView == null ? null : listView.SelectedItems;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
