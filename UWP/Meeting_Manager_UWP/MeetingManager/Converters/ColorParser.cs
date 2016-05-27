//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace MeetingManager.Converters
{
    public class ColorParser
    {
        private const string _xaml = "<StackPanel Background=\"###\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"/>";

        private static IDictionary<string, Brush> _brushCache = new Dictionary<string, Brush>();

        public static Brush Parse(string colorName)
        {
            Brush result;

            if (_brushCache.TryGetValue(colorName, out result))
            {
                return result;
            }

            var xaml = _xaml.Replace("###", colorName);

            try
            {
                var element = XamlReader.Load(xaml);

                result = (element as Panel).Background;
                _brushCache.Add(colorName, result);

                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
