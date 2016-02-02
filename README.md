# Interop REST Mail Contacts Calendar Android App

Office Interoperability Examples - REST Mail, Contacts, and Calendar App

This example app demonstrates the Representational State Transfer (REST) interface for Office 365, including authentication, interacting with the calendar, querying the address book, and sending email. 

In the app, after you log into an Office 365 account, you can create single and recurring meetings on a calendar. Meetings can be scheduled with a given location, time slot, and a set of invitees, where the available locations and attendees are queried from Office 365. Each invitee has the option to accept, decline, or tentatively accept a meeting, or to email the organizer. An organizer has the option to reply all or forward the meeting invitation, and to send a "running late" message to the invitees.

###Table of Contents

* [System Requirements](#system-requirements)

* [Install Android SDK](#install-android-sdk)

* [Configuration](#configuration)

* [Register the App with Office 365 Developer Account](#register-the-app-with-office-365-developer-account)

* [Dependencies](#dependencies)

###System Requirements

To us the Interop REST Mail Contacts Calendar you need to install the Android SDK, which requires that your System meet the following requirements ([from the Android Developer Site](http://developer.android.com/sdk/index.html#Requirements)):

#####Windows

* Microsoft® Windows® 8/7/Vista (32 or 64-bit)
* 2 GB RAM minimum, 4 GB RAM recommended
* 400 MB hard disk space
* At least 1 GB for Android SDK, emulator system images, and caches
* 1280 x 800 minimum screen resolution
* Java Development Kit (JDK) 7
* Optional for accelerated emulator: Intel® processor with support for Intel® VT-x, Intel® EM64T (Intel® 64), and Execute Disable (XD) Bit functionality

#####Mac OS X

* Mac® OS X® 10.8.5 or higher, up to 10.9 (Mavericks)
* 2 GB RAM minimum, 4 GB RAM recommended
* 400 MB hard disk space
* At least 1 GB for Android SDK, emulator system images, and caches
* 1280 x 800 minimum screen resolution
* Java Runtime Environment (JRE) 6
* Java Development Kit (JDK) 7
* Optional for accelerated emulator: Intel® processor with support for Intel® VT-x, Intel® EM64T (Intel® 64), and Execute Disable (XD) Bit functionality
* On Mac OS, run Android Studio with Java Runtime Environment (JRE) 6 for optimized font rendering. You can then configure your project to use Java Development Kit (JDK) 6 or JDK 7.

#####Linux

* GNOME or KDE desktop
* GNU C Library (glibc) 2.15 or later
* 2 GB RAM minimum, 4 GB RAM recommended
* 400 MB hard disk space
* At least 1 GB for Android SDK, emulator system images, and caches
* 1280 x 800 minimum screen resolution
* Oracle® Java Development Kit (JDK) 7
* Tested on Ubuntu® 14.04, Trusty Tahr (64-bit distribution capable of running 32-bit applications).

###Install Android SDK

The Android SDK can be downloaded from the Android Developer site [here](http://developer.android.com/sdk/index.html). You may also need to install the [Java SE Development Kit 7u80](http://www.oracle.com/technetwork/java/javase/downloads/jdk7-downloads-1880260.html). If you have questions about installing the Java SDK, you can refer to this [tutorial](http://www.wikihow.com/Install-the-Java-Software-Development-Kit).

For the full installation instructions visit [the Android Developer Website](http://developer.android.com/sdk/installing/index.html).

###Configuration

1. Download or clone [The Interop REST Mail Calendar Android App](https://github.com/OfficeDev/Interop-REST-Mail-Contacts-Calendar-Sample).

2. Start Android Studio

3. Click on "Open an existing Android Studio project" and select the folder that contains the app, then open the "MeetingManagerAndroid" folder then the "Android" folder and select the build.gradle file.

4. Click Run > Run 'app' or click the green triangle play button. 

5. Click the down arrow next to the selection for "Android virtual device:" to select a different device or click the ellipsis next to it to add more devices, then click "Ok".

 * If you get the error message: 

    emulator: ERROR: x86 emulation currently requires hardware acceleration!
    Please ensure Intel HAXM is properly installed and usable.
    CPU acceleration status: HAX kernel module is not installed!

  Refer to this [Stackoverflow question](http://stackoverflow.com/questions/26355645/error-in-launching-avd) for how to install HAXM

6. Your app will now start in the emulator. But in order to connect the app to an account you first have to [Register the app with Office 365](#register-the-app-with-office-365-developer-account) and copy your "CLIENT ID" and "REDIRECT URI".

7. You can enter your "CLIENT ID" and "REDIRECT URI" by clicking on the three vertical dots in the upper right of the emulator screen and click "settings" and enter them there or navigate to "app" > "src/main" > "java" > "com/microsoft/office365/meetingmgr" > Constants.java and paste your "CLIENT ID" and "REDIRECT URI" as strings and save the file.

###Register the App with Office 365 Developer Account

1. To register your App you need to have an account with the Office 365 Dev Program. To sign up, visit [dev.office.com/devprogram](http://dev.office.com/devprogram). 

2. Once you have created an Office 365 Dev Account, go to [graph.microsoft.io](http://graph.microsoft.io/) to register your app and click "Get started" or you can go directly to the registration page [dev.office.com/app-registration](http://dev.office.com/app-registration).

3. Give your app a name and permissions and click "Register App" then enter the "CLIENT ID" and "REDIRECT URI". The permissions the app needs are:

  * Read user profiles
  * Read user contacts
  * Read and write user calendars
  * Read user calendars
  * Send mail as user
  * Read and write user mail

4. If you need more control of the registration options, you can follow these [detailed instructions](https://github.com/jasonjoh/office365-azure-guides/blob/master/RegisterAnAppInAzure.md) to register your app in Azure. Note that these instructions use the Azure classic portal. You can access the [Azure classic portal here](https://manage.windowsazure.com/).
