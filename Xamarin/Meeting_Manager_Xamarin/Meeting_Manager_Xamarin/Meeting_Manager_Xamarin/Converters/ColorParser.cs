//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System.Collections.Generic;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Converters
{
    public class ColorParser
    {
        private static IDictionary<string, Color> _colorCache = new Dictionary<string, Color>();

        public static Color Parse(string colorName)
        {
            Color result = Color.Pink;

            if (!_colorCache.TryGetValue(colorName, out result))
            {
                try
                {
                    result = Color.FromHex(colorName);
                }
                catch
                {
                }
            }

            return result;
        }
    }
}
