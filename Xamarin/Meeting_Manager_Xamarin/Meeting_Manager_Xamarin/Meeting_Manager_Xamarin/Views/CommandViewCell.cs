//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System.Windows.Input;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    class CommandViewCell : ViewCell
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(CommandViewCell), null, BindingMode.Default, null,
                propertyChanged: (sender, oldValue, newValue) =>
                {
                });

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        protected override void OnTapped()
        {
            base.OnTapped();

            if (Command != null && Command.CanExecute(this))
            {
                Command.Execute(this);
            }
        }
    }
}
