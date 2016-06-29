using MeetingManager.Models;
using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace MeetingManager
{
    internal class UI
    {
        internal static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        internal static T Deserialize<T>(object parameter)
        {
            return JsonConvert.DeserializeObject<T>((string)parameter);
        }

        internal static void GoBack()
        {
            App.Me.NavigationService.GoBack();
        }

        internal static TEvent GetEvent<TEvent>() where TEvent : EventBase, new()
        {
            return App.Me.EventAggregator.GetEvent<TEvent>();
        }

        internal static async Task<bool> YesNoDialog(string message)
        {
            var messageDialog = new MessageDialog(message);

            messageDialog.Commands.Add(new UICommand
            {
                Label = ResMan.GetString("YesCaption"),
                Id = 0
            });

            messageDialog.Commands.Add(new UICommand
            {
                Label = ResMan.GetString("NoCaption"),
                Id = 1
            });

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;

            var result = await messageDialog.ShowAsync();

            return (int)result.Id == 0;
        }

        internal static async Task MessageDialog(string message)
        {
            var messageDialog = new MessageDialog(message);

            messageDialog.Commands.Add(new UICommand
            {
                Label = ResMan.GetString("OKCaption"),
                Id = 0
            });

            messageDialog.DefaultCommandIndex = 0;

            await messageDialog.ShowAsync();
        }

        internal static async Task NavigateTo(string pageToken, object parameter = null)
        {
            var viewNameSpace = App.Me.GetType().Namespace + ".Views";
            var dialogTypeName = viewNameSpace + "." + pageToken + "Dialog";

            var dialogType = Type.GetType(dialogTypeName);

            if (dialogType != null)
            {
                var dlg = Activator.CreateInstance(dialogType) as ContentDialog;
                GetEvent<InitDialogEvent>().Publish(UI.SerializeParameter(parameter));
                await dlg.ShowAsync();
            }
            else 
            {
                UI.NavigateToPage(pageToken, parameter);
            }
        }

        private static void NavigateToPage(string pageToken, object parameter = null)
        {
            App.Me.NavigationService.Navigate(pageToken, SerializeParameter(parameter));
        }

        private static object SerializeParameter(object parameter)
        {
            if (parameter != null)
            {
                if (!(parameter is string) && !(parameter is bool))
                {
                    parameter = JsonConvert.SerializeObject(parameter);
                }
            }
            return parameter;
        }
    }
}
