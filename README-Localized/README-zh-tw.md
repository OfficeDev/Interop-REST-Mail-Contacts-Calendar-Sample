# Interop REST 郵件連絡人行事曆應用程式

此專案已採用 [Microsoft 開放原始碼執行](https://opensource.microsoft.com/codeofconduct/)。如需詳細資訊，請參閱[程式碼執行常見問題集](https://opensource.microsoft.com/codeofconduct/faq/)，如果有其他問題或意見，請連絡 [opencode@microsoft.com](mailto:opencode@microsoft.com)。

這個範例應用程式會示範適用於 Office 365 的具象狀態傳輸 (REST) 介面，包括驗證、與行事曆互動、查詢通訊錄，以及傳送電子郵件。 應用程式可以針對 Android 和通用 Windows 平台建置。 若要開始，[使用 Office 365 開發人員帳戶註冊您的應用程式](#使用-office-365-開發人員帳戶註冊您的應用程式)，然後選擇您想要建置的平台。 

##目錄

* [關於 Interop REST 郵件連絡人行事曆應用程式](#關於-interop-rest-郵件連絡人行事曆應用程式)

* [使用 Office 365 開發人員帳戶註冊應用程式](#使用-office-365-開發人員帳戶註冊應用程式)

* 建置應用程式

  * [針對通用 Windows 平台建置應用程式](/UWP)
  
  * [針對 Android 建置應用程式](/Android)

##關於 Interop REST 郵件連絡人行事曆應用程式

在應用程式中，在您登入 Office 365 帳戶之後，您可以檢視您的行事曆，並且在行事曆上建立單一或週期性會議。 會議可以排程在指定的位置、時段，以及一組受邀者，其中可用的位置和出席者是從 Office 365 查詢。 每個受邀者可以選擇接受、拒絕或暫且接受會議，或傳送電子郵件給召集人。 召集人可以選擇全部回覆或轉寄會議邀請，以及將即將遲到的訊息傳送給受邀者。

如果是使用通用 Windows 平台建置應用程式，您可以從應用程式底部主控台中的 Microsoft Graph Universal API 看到即時要求和回應。

基本應用程式可以：

####檢視您的行事曆

Android | UWP
--- | ---
![行事曆頁面](../img/app-calendar.jpg) | ![行事曆頁面 UWP](../img/app-calendar-uwp.jpg)

####檢視會議詳細資料

Android | UWP
--- | ---
![詳細資料頁面](../img/app-meeting-details.jpg) | ![詳細資料頁面](../img/app-meeting-details-uwp.jpg)

####將訊息傳送給其他與會者

Android | UWP
--- | ---
![傳送訊息](../img/app-reply-all.jpg) | ![傳送訊息](../img/app-reply-all-UWP.jpg)

####修改會議詳細資料

Android | UWP
--- | ---
![修改會議詳細資料](../img/app-modify-meeting.jpg) | ![修改會議詳細資料](../img/app-modify-meeting-UWP.jpg)

####建立新的會議

Android | UWP
--- | ---
![建立新的會議](../img/app-create-meeting.jpg) | ![建立新的會議](../img/app-create-meeting-uwp.jpg)

##使用 Office 365 開發人員帳戶註冊應用程式

1. 不論您使用什麼項目來設定您的應用程式，您必須擁有 Office 365 開發人員帳戶，並且註冊您的應用程式。 若要註冊 Office 365 開發人員帳戶：

  * [參加 Office 365 開發人員計劃，並取得 Office 365 的免費 1 年訂用帳戶](https://aka.ms/devprogramsignup)。

  * 遵循確認電子郵件中的連結，並且建立 Office 365 開發人員帳戶。

  * 如需註冊開發人員帳戶的詳細指示，請移至[這裡](https://msdn.microsoft.com/en-us/library/office/fp179924.aspx#o365_signup)。

2. 一旦建立 Office 365 開發人員帳戶，前移至 [graph.microsoft.io](http://graph.microsoft.io/en-us/)，註冊您的應用程式，按一下 [應用程式註冊]，然後按一下 [Office 365 應用程式註冊工具]，或者您可以直接移至註冊頁面 [dev.office.com/app-registration](http://dev.office.com/app-registration)。

  ![開始使用](../img/ms-graph-get-started.jpg) 

  ![後續步驟](../img/ms-graph-get-started-2.jpg)

3. 提供您的應用程式名稱，並且在 [應用程式類型] 行中選取 [原生應用程式]。 然後選擇重新導向 URI，慣用的命名慣例是：「您的 Office 365 網域 + 您的應用程式的唯一名稱」，但這並非必要，不過，必須格式化為 URI 且是唯一的。 例如，我將我的應用程式命名為 https://greencricketcreations.onmicrosoft.com/MyCalendarApp。 重新導向 URI 不是實際的網站；比較像是您的應用程式的唯一識別碼。 一旦您輸入名稱，重新導向 URI 會設定權限。 必要權限為：

  * 讀取使用者設定檔
  * 讀取使用者連絡人
  * 讀取和寫入使用者行事曆
  * 讀取使用者連絡人
  * 以使用者的身分傳送郵件
  * 讀取和寫入使用者郵件

4. 一旦您填寫表單，按一下 [註冊應用程式]。

  ![註冊應用程式](../img/ms-graph-get-started-3.jpg)

5. 完成您的註冊時，您會收到用戶端識別碼。 請記下用戶端識別碼和重新導向 URI，您需要這些項目來設定您的應用程式。

6. 如果您需要更多對於註冊選項的控制，您可以遵循這些[詳細指示](https://github.com/jasonjoh/office365-azure-guides/blob/master/RegisterAnAppInAzure.md)，在 Azure 中註冊您的應用程式。 請注意，這些指示會使用 Azure 傳統入口網站。 您可以在以下位置存取 [Azure 傳統入口網站](https://manage.windowsazure.com/)。

7. 現在您可以使用 [Android](/Android) 或 [通用 Windows 平台](/UWP)或兩者，建置您的應用程式！

---

###著作權

Copyright (c) 2016 Microsoft.著作權所有，並保留一切權利。