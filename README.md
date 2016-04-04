# Interop REST Mail Contacts Calendar App

Office Interoperability Examples - REST Mail, Contacts, and Calendar App

This example app demonstrates the Representational State Transfer (REST) interface for Office 365, including authentication, interacting with the calendar, querying the address book, and sending email. The app can be built for Android or the Universal Windows Platform. To get started, register your app with an Office 365 Developer Account, then choose which platform(s) you want to build for. 

##Table of Contents

* [Register the App with Office 365 Developer Account](#register-the-app-with-office-365-developer-account)

* [Build Your App for the Universal Windows Platform](/UWP)

  * [Install Visual Studio](/UWP#install-visual-studio)

* [Build Your App for Android](/Android)

  * [Android Studio Configuration](/Android#android-studio-configuration)

    * [Install Android SDK](/Android#install-android-sdk)

    * [Android Configuration](/Android#android-configuration)

  * [Visual Studio Emulator Configuration](/Android#visual-studio-emulator-configuration)
  
  * [test](/Android#visual-studio-emulator-configuration)

    * [Install Visual Studio Android Emulator](/Android#install-visual-studio-android-emulator)

    * [Launch the App in Visual Studio Android Emulator](/Android#launch-the-app-in-visual-studio-android-emulator)

In the app, after you log into an Office 365 account, you can view your calendar and create single or recurring meetings on your calendar. Meetings can be scheduled with a given location, time slot, and a set of invitees, where the available locations and attendees are queried from Office 365. Each invitee has the option to accept, decline, or tentatively accept a meeting, or to email the organizer. An organizer has the option to reply all or forward the meeting invitation, and to send a "running late" message to the invitees.

If the app is built with the Universal Windows Platform you will also see the console with requests and responses from the Microsoft Graph Universal API.

The basic app is able to:

####View Your Calendar

Android | UWP
--- | ---
![Calendar Page](/img/app-calendar.jpg) | ![Calendar Page UWP](/img/app-calendar-uwp.jpg)

####View Meeting Details

Android | UWP
--- | ---
![Details Page](/img/app-meeting-details.jpg) | ![Details Page](/img/app-meeting-details-uwp.jpg)

####Send a Message to Other Meeting Attendees

Android | UWP
--- | ---
![Send a message](/img/app-reply-all.jpg) | ![Send a message](/img/app-reply-all-UWP.jpg)

####Modify Meeting Details

Android | UWP
--- | ---
![modify meeting details](/img/app-modify-meeting.jpg) | ![modify meeting details](/img/app-modify-meeting-UWP.jpg)

####Create a New Meeting

Android | UWP
--- | ---
![create new meeting](/img/app-create-meeting.jpg) | ![Create New Meeting](/img/app-create-meeting-uwp.jpg)

##Register the App with Office 365 Developer Account

1. No matter what you use to configure your app, you will need to register it with an Office 365 Developer Account. To register your app you need to have an account with the Office 365 Dev Program. To sign up, visit [dev.office.com/devprogram](http://dev.office.com/devprogram) or you can go directly to the [profile creation page](https://profile.microsoft.com/RegSysProfileCenter/wizardnp.aspx?wizid=14b845d0-938c-45af-b061-f798fbb4d170&lcid=1033) and create a profile. Once you have created your profile, you also need to sign up for an Office 365 developer account. The link to create your account will be in the confirmation email you receive after creating your profile. You can view detailed instructions on signing up for a developer account [here](https://msdn.microsoft.com/en-us/library/office/fp179924.aspx#o365_signup).

2. Once you have created an Office 365 Dev Account, go to [graph.microsoft.io](http://graph.microsoft.io/en-us/) to register your app and click "App Registration" then click "Office 365 App Registration Tool" or you can go directly to the registration page [dev.office.com/app-registration](http://dev.office.com/app-registration).

  ![Get started](/img/ms-graph-get-started.jpg) | ![Next step](/img/ms-graph-get-started-2.jpg)
  --- | ---

3. Give your app a name and select "Native App" in the "App type" line. Then pick a "Redirect URI" the preferred naming convention is: "your Office 365 domain + a unique name for your app", but it is not required, it must however be formatted as a URI and be unique. For example I named my app https://greencricketcreations.onmicrosoft.com/MyCalendarApp. The Redirect URI isn't a real website, it is more of a unique identifier for your app. Once you have entered a name and Redirect URI set the permissions. The necessary permissions are:

  * Read user profiles
  * Read user contacts
  * Read and write user calendars
  * Read user calendars
  * Send mail as user
  * Read and write user mail

4. Once you have filled out the form, click "Register App".

  ![Register App](/img/ms-graph-get-started-3.jpg)

5. When your registration is complete, you will receive a "Client ID". Make note of the "Client ID" and "Redirect URI" you will need them to configure your app.

6. If you need more control of the registration options, you can follow these [detailed instructions](https://github.com/jasonjoh/office365-azure-guides/blob/master/RegisterAnAppInAzure.md) to register your app in Azure. Note that these instructions use the Azure classic portal. You can access the [Azure classic portal here](https://manage.windowsazure.com/).

7. Now you're ready to build your app with [Android](/Android) or [the Universal Windows Platform](/UWP) or both!

---

###Copyright

Copyright (c) 2016 Microsoft. All rights reserved.
