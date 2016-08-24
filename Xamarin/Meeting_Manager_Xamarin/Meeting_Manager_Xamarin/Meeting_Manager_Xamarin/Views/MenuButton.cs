//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    class MenuButton : Button
    {
        public static readonly BindableProperty MenuProperty =
            BindableProperty.Create("Menu", typeof(IEnumerable<MenuItem>), typeof(MenuButton));

        public MenuButton()
        {
            Clicked += OnClicked;
        }

        public IEnumerable<MenuItem> Menu
        {
            get { return (IEnumerable<MenuItem>)GetValue(MenuProperty); }
            set { SetValue(MenuProperty, value); }
        }

        private async void OnClicked(object sender, EventArgs args)
        {
            var menu = (sender as MenuButton).Menu;

            if (menu == null) return;

            var element = sender as Element;

            while (element != null && !(element is Page))
            {
                element = element.Parent;
            }

            if (element != null)
            {
                var result = await (element as Page).DisplayActionSheet(Text, cancel:null, destruction:null,
                                        buttons: menu
                                        .Select(i => i.Text)
                                        .ToArray());

                if (result != null)
                {
                    var selectedItem = menu.First(x => x.Text == result);

                    if (selectedItem.Command?.CanExecute(selectedItem) == true)
                    {
                        selectedItem.Command.Execute(selectedItem.CommandParameter);
                    }
                }
            }
        }
    }
}