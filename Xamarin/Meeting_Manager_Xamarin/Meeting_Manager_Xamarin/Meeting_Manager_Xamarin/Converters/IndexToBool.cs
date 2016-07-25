using System;
using System.Globalization;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Converters
{
    public class IndexToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if ((int) value != 0 && (int) value != 1)
            //{
            //    throw new IndexOutOfRangeException();
            //}

            return (int)value == 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
