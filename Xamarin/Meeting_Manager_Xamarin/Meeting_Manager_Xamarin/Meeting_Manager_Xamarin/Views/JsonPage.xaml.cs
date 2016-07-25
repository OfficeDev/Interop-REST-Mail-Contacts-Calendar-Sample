//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    public partial class JsonPage : ContentPage
    {
        public JsonPage()
        {
            InitializeComponent();

            this.Subscribe<JsonData>(ShowJson);
        }

        private void ShowJson(object sender, JsonData data)
        {
            JsonWindow.Text = Prettify(data.Payload);
        }

        private string Prettify(string text)
        {
            try
            {
                var instance = JSON.Deserialize<object>(text);
                return JSON.Serialize(instance, Formatting.Indented);
            }
            catch
            {
                return text;
            }
        }
    }
}
