Originally based on [hajekj/aad-b2b-multitenant](https://github.com/hajekj/aad-b2b-multitenant) sample. For full explanation about how this code works, please see following [blog post](https://hajekj.net/2017/07/24/creating-a-multi-tenant-application-which-supports-b2b-users/).

# Setup instructions
1. [Create an Azure AD application](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-integrating-applications#adding-an-application) in the [Portal](https://portal.azure.com).
2. Configure application's permissions to have access to *Windows Azure Service Management API* and also *Microsoft Graph* (add permissions to sign-in the user and read user's profile, read basic profiles of users and also access directory as currently signed in user)
3. Get the application's client id, client secret and configure the reply url to *http://localhost:5000/signin-oidc*
4. Replace the client id in the *appsettings.json* and place the client secret into [user secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets#secret-manager) or environmental variables if deploying to Azure.
5. In order for this to work, due to current Microsoft Graph permission model, administrator in the foreign tenants has to approve the application. Either by simply signing to it and appending `&prompt=admin_consent` to the login URL or through the Azure Portal, in Enterprise Applications by clicking Grant Consent (we will eventually streamline this process to make the onboarding process more easier).
