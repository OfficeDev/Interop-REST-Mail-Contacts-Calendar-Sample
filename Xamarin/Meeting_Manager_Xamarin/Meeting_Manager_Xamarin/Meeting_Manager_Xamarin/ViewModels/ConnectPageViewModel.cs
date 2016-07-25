#if false
using Meeting_Manager_Xamarin.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class ConnectPageViewModel : BaseViewModel
    {
        public ConnectPageViewModel()
        {
        }

        public ICommand ConnectCommand => new Command(Connect);

        private async void Connect()
        {
            await UI.NavigateTo("Login");
        }
    }
}
#endif
