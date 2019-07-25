---
page_type: sample
products:
- office-365
languages:
- csharp
extensions:
  contentType: samples
  createdDate: 9/16/2015 10:17:29 AM
---
# Aplicativo de Calendário de Contatos de Email de REST de Interoperabilidade

Este projeto adotou o [Código de Conduta do Código Aberto da Microsoft](https://opensource.microsoft.com/codeofconduct/). Para saber mais, confira as [Perguntas frequentes do Código de Conduta](https://opensource.microsoft.com/codeofconduct/faq/) ou contate [opencode@microsoft.com](mailto:opencode@microsoft.com) se tiver outras dúvidas ou comentários.

Este exemplo de aplicativo demonstra a interface REST (Representational State Transfer – Transferência de Estado Representacional) para o Office 365, incluindo a autenticação, interação com o calendário, consultas do catálogo de endereços e o envio de emails. O aplicativo pode ser criado para Android e para a Plataforma Universal do Windows. Para começar, [registre seu aplicativo com uma conta de desenvolvedor do Office 365](#registre-seu-aplicativo-com-uma-conta-de-desenvolvedor-do-office-365) e, em seguida, escolha para qual plataforma você deseja compilar. 

##Sumário

* [Sobre o Aplicativo de Calendário de Contatos de Email de REST de Interoperabilidade](#sobre-o-aplicativo-de-calendário-de-contatos-de-email-de-rest-de-interoperabilidade)

* [Registrar o Aplicativo com uma Conta de Desenvolvedor do Office 365](#registrar-o-aplicativo-com-uma-conta-de-desenvolvedor-do-office-365)

* Compilar o Aplicativo

  * [Criar o Aplicativo para a Plataforma Universal do Windows](/UWP)
  
  * [Compilar o aplicativo para Android](/Android)

##Sobre o Aplicativo de Calendário de Contatos de Email de REST de Interoperabilidade

No aplicativo, após fazer logon em uma conta do Office 365, você pode exibir seu calendário e criar reuniões individuais ou recorrentes no calendário. As reuniões podem ser agendadas com um determinado local, horário e um conjunto de convidados, em que os locais disponíveis e os participantes são consultados no Office 365. Cada convidado tem a opção de aceitar, recusar ou aceitar provisoriamente uma reunião ou enviar um email ao organizador. O organizador tem a opção de responder a todos ou encaminhar o convite da reunião e enviar uma mensagem de atraso aos convidados.

Se o aplicativo tiver sido criado com a Plataforma Universal do Windows, você conseguirá ver as solicitações e respostas ao vivo na API Universal do Microsoft Graph em um console na parte inferior do aplicativo.

O aplicativo básico consegue:

####Exibir seu Calendário

Android | UWP
--- | ---
![Página do Calendário](../img/app-calendar.jpg) | ![UWP da Página do Calendário](../img/app-calendar-uwp.jpg)

####Adicionar Detalhes da Reunião

Android | UWP
--- | ---
![Página Detalhes](../img/app-meeting-details.jpg) | ![Página Detalhes](../img/app-meeting-details-uwp.jpg)

####Enviar uma Mensagem para Outros Participantes da Reunião

Android | UWP
--- | ---
![Enviar uma mensagem](../img/app-reply-all.jpg) | ![Enviar uma mensagem](../img/app-reply-all-UWP.jpg)

####Modificar Detalhes da Reunião

Android | UWP
--- | ---
![modificar detalhes da reunião](../img/app-modify-meeting.jpg) | ![modificar detalhes da reunião](../img/app-modify-meeting-UWP.jpg)

####Criar uma Nova Reunião

Android | UWP
--- | ---
![criar nova reunião](../img/app-create-meeting.jpg) | ![Criar Nova Reunião](../img/app-create-meeting-uwp.jpg)

##Registrar o Aplicativo com uma Conta de Desenvolvedor do Office 365

1. Não importa o que você usa para configurar seu aplicativo, será preciso ter uma conta do Office 365 Developer e registrar seu aplicativo com ela. Para se inscrever em uma conta de desenvolvedor do Office 365:

  * [Participar do Programa de Desenvolvedores do Office 365 e obter uma assinatura gratuita 1 ano do Office 365](https://aka.ms/devprogramsignup).

  * Siga o link no email de confirmação e crie uma conta de desenvolvedor do Office 365.

  * Vá [aqui](https://msdn.microsoft.com/en-us/library/office/fp179924.aspx#o365_signup) para obter instruções detalhadas sobre como se inscrever em uma conta de desenvolvedor.

2. Após criar uma conta de desenvolvedor do Office 365, vá para [graph.microsoft.io](http://graph.microsoft.io/en-us/) para registrar seu aplicativo e clique em **Registro do Aplicativo** e em **Ferramenta de Registro do Aplicativo do Office 365** ou vá diretamente para a página de registro [dev.office.com/app-registration](http://dev.office.com/app-registration).

  ![Introdução](../img/ms-graph-get-started.jpg) 

  ![Próxima etapa](../img/ms-graph-get-started-2.jpg)

3. Nomeie seu aplicativo e escolha **Aplicativo Nativo** na linha **Tipo de aplicativo**. Escolha um URI de redirecionamento, a convenção de nomenclatura preferencial é: "seu domínio do Office 365 + um nome exclusivo para seu aplicativo". No entanto, isso não é obrigatório, porém deve ser formatado como um URI e ser exclusivo. Por exemplo, dei o nome https://greencricketcreations.onmicrosoft.com/MyCalendarApp ao meu aplicativo. O URI de redirecionamento não é um site real. Ele é mais de um identificador exclusivo do aplicativo. Defina as permissões após inserir um nome e o URI de redirecionamento. As permissões necessárias são:

  * Ler perfis do usuário
  * Ler contatos do usuário
  * Ler e gravar calendários do usuário
  * Ler calendários do usuário
  * Enviar email como o usuário
  * Leia e escrever emails do usuário

4. Depois de preencher o formulário, clique em **Registrar Aplicativo**.

  ![Registrar Aplicativo](../img/ms-graph-get-started-3.jpg)

5. Quando seu registro for concluído, você receberá uma ID de Cliente. Anote a ID de Cliente e o URI de redirecionamento pois você precisará deles para configurar seu aplicativo.

6. Se precisar de mais controle das opções de registro, você pode seguir estas [instruções detalhadas](https://github.com/jasonjoh/office365-azure-guides/blob/master/RegisterAnAppInAzure.md) para registrar seu aplicativo no Azure. Observe que essas instruções usam o portal clássico do Azure. Você pode acessar o [portal clássico do Azure aqui](https://manage.windowsazure.com/).

7. Agora você está pronto para criar seu aplicativo usando o [Android](/Android) ou a [Plataforma Universal do Windows](/UWP), ou ambos!

---

###Direitos autorais

Copyright © 2016 Microsoft. Todos os direitos reservados.