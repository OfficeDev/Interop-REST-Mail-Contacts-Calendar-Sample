//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

namespace Meeting_Manager_Xamarin.Views
{
    public sealed class OnPlatformEx<T>
    {
        public OnPlatformEx()
        {
            Android = default(T);
            iOS = default(T);
            WinPhone = default(T);
            Windows = default(T);
            Other = default(T);
        }

        public T Android { get; set; }

        public T iOS { get; set; }

        public T WinPhone { get; set; }

        public T Windows { get; set; }

        public T Other { get; set; }

        public static implicit operator T(OnPlatformEx<T> onPlatform)
        {
            switch (Xamarin.Forms.Device.OS)
            {
                case Xamarin.Forms.TargetPlatform.Android:
                    return onPlatform.Android;

                case Xamarin.Forms.TargetPlatform.iOS:
                    return onPlatform.iOS;

                case Xamarin.Forms.TargetPlatform.WinPhone:
                    return onPlatform.WinPhone;

                case Xamarin.Forms.TargetPlatform.Windows:
                    return onPlatform.Windows;

                default:
                    return onPlatform.Other;
            }
        }
    }
}
