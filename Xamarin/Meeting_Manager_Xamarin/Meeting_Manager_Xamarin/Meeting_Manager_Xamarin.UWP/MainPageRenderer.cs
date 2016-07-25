using Meeting_Manager_Xamarin.UWP;
//using Microsoft.Identity.Client;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Meeting_Manager_Xamarin.Views.ConnectPage), typeof(MainPageRenderer))]
namespace Meeting_Manager_Xamarin.UWP
{
    class MainPageRenderer : PageRenderer
    {
        Views.ConnectPage page;
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            page = e.NewElement as Views.ConnectPage;
//            page.PlatformParameters = new PlatformParameters(true);
        }
    }
}
