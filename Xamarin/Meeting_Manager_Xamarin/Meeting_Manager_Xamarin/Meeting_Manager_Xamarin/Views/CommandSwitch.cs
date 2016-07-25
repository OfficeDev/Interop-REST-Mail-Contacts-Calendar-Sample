//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System.Windows.Input;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    class CommandSwitch : Switch
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(CommandSwitch), null, BindingMode.Default, null, null);

        public CommandSwitch()
        {
            this.Toggled += OnToggled;
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private void OnToggled(object sender, ToggledEventArgs args)
        {
            if (Command != null && Command.CanExecute(this))
            {
                Command.Execute(this);
            }
        }
    }
}
