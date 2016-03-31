using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MeetingManager.Converters
{
    class IndexToBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Color result = Colors.Magenta;

            if (value is int)
            {
                result = ((int)value)%2 == 0 ? Colors.Beige : Colors.LightGray;
            }

            return new SolidColorBrush(result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
