//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Meeting_Manager_Xamarin.Views
{
    [ContentProperty("Text")]
    public class ResxStringExtension : IMarkupExtension
    {
        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return "";

            return ResMan.GetString(Text);
        }
    }
}
