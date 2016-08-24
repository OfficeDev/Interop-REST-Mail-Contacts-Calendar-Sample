//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;

namespace MeetingManager.Models
{
    public class Command<T> : Prism.Commands.DelegateCommand<T>
    {
        public Command(Action<T> executeMethod) :
                    base(executeMethod)
        {
        }

        public Command(Action<T> executeMethod, Func<T, bool> canExecuteMethod) :
                    base(executeMethod, canExecuteMethod)
        {
        }
    }

    public class Command : Prism.Commands.DelegateCommand
    {
        public Command(Action executeMethod) :
                    base(executeMethod)
        {
        }

        public Command(Action executeMethod, Func<bool> canExecuteMethod) :
                    base(executeMethod, canExecuteMethod)
        {
        }

        public void ChangeCanExecute()
        {
            RaiseCanExecuteChanged();
        }
    }
}
