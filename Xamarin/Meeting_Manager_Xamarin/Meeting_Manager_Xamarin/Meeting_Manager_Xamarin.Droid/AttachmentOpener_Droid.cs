using System;
using System.Linq;
using Android.Content;
using Meeting_Manager_Xamarin.Models;
using Xamarin.Forms;
using Meeting_Manager_Xamarin.Droid;
using Java.IO;
using Android.Widget;

[assembly: Dependency(typeof(AttachmentOpener_Droid))]

namespace Meeting_Manager_Xamarin.Droid
{
    public class AttachmentOpener_Droid : IAttachmentOpener
    {
        public void Open(FileAttachment attachment)
        {
            var context = Forms.Context;
            File file;

            try
            {
                var dir = Android.OS.Environment
                            .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
                file = new File(dir, attachment.Name);

                using (var stream = new FileOutputStream(file))
                {
                    stream.Write(attachment.ContentBytes);
                }
            }
            catch (Exception ex)
            {
                var message = $"Couldn't download {attachment.Name}: {ex.Message}";
                Toast.MakeText(context, message, ToastLength.Short).Show();
                return;
            }

            var uri = Android.Net.Uri.FromFile(file);

            var ext = Android.Webkit.MimeTypeMap.GetFileExtensionFromUrl(uri.ToString());
            var mimeType = Android.Webkit.MimeTypeMap.Singleton.GetMimeTypeFromExtension(ext);

            var intent = new Intent(Intent.ActionView);

            // Using just SetType method doesn't appear to work
            if (string.IsNullOrEmpty(mimeType))
            {
                intent.SetDataAndType(uri, "application/*");
            }
            else
            {
                intent.SetDataAndType(uri, mimeType);
            }
            
            var available = context.PackageManager
                                .QueryIntentActivities(intent, Android.Content.PM.PackageInfoFlags.MatchDefaultOnly);

            if (!available.Any())
            {
                var message = $"No apps found for {attachment.Name}";
                Toast.MakeText(Forms.Context, message, ToastLength.Short).Show();
            }
            else
            {
                try
                {
                    context.StartActivity(intent);
                }
                catch (Exception ex)
                {
                    var message = $"Couldn't start activity: {ex.Message}";
                    Toast.MakeText(Forms.Context, message, ToastLength.Short).Show();
                }
            }
        }
    }
}