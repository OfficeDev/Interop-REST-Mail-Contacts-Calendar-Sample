using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Meeting_Manager_Xamarin.Views;
using Microsoft.Identity.Client;
using Meeting_Manager_Xamarin.Droid;

[assembly: ExportRenderer(typeof(ConnectPage), typeof(MainPageRenderer))]
namespace Meeting_Manager_Xamarin.Droid
{
    class MainPageRenderer : PageRenderer
    {
        ConnectPage page;

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            page = e.NewElement as ConnectPage;
            var activity = this.Context as Activity;
            page.PlatformParameters = new PlatformParameters(activity);
        }

    }
}