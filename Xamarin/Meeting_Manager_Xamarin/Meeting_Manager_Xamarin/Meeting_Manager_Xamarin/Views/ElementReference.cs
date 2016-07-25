//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Meeting_Manager_Xamarin.Views
{
    [ContentProperty("ElementName")]
    class ElementReference : IMarkupExtension
    {
        public string ElementName { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var rootProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;

            if (rootProvider == null)
                return null;

            var root = rootProvider.RootObject as Element;

            if (root == null)
                return null;

            var child = root.FindByName<Element>(ElementName);

            return child;
        }
    }
}
