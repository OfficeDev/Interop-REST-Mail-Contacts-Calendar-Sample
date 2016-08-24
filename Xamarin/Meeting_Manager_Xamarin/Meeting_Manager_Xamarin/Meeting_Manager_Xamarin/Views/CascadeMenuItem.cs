//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    class CascadeMenuItem : Xamarin.Forms.MenuItem
    {
        public static readonly BindableProperty MenuProperty =
            BindableProperty.Create("SubMenu", typeof(IEnumerable<Xamarin.Forms.MenuItem>), typeof(CascadeMenuItem), null, BindingMode.OneWay,
                null, propertyChanged: (bindable, oldValue, newValue) =>
                {
                    var item = bindable as Xamarin.Forms.MenuItem;

                    var origCommand = item.Command;

                    item.Command = new Command<object>((parameter) =>
                    {
                        if (parameter == null) return;

                        if (!IsSubMenu(parameter))
                        {
                            origCommand.Execute(parameter);
                        }
                        else
                        {
                            HandleSubMenu(item, parameter);
                        }
                    });
                });

        public IEnumerable<Xamarin.Forms.MenuItem> SubMenu
        {
            get { return (IEnumerable<Xamarin.Forms.MenuItem>)GetValue(MenuProperty); }
            set { SetValue(MenuProperty, value); }
        }

        public static readonly BindableProperty PageProperty =
            BindableProperty.Create("Page", typeof(Page), typeof(CascadeMenuItem), defaultValue: null);

        public Page Page
        {
            get { return (Page)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        public static Func<object, bool> IsSubMenu = (parameter) => false;

        private static async void HandleSubMenu(Xamarin.Forms.MenuItem item, object parameter)
        {
            var cascadeMenuItem = item as CascadeMenuItem;
            var menu = cascadeMenuItem.SubMenu;

            if (menu == null || !menu.Any()) return;

            var element = cascadeMenuItem.Page;

            if (element != null)
            {
                var result = await element.DisplayActionSheet(item.Text, cancel: null, destruction: null,
                                        buttons: menu
                                        .Select(i => i.Text)
                                        .ToArray());

                if (result != null)
                {
                    var selectedItem = menu.First(x => x.Text == result);

                    if (selectedItem.Command?.CanExecute(selectedItem) == true)
                    {
                        selectedItem.Command.Execute(parameter);
                    }
                }
            }
        }
    }
}
