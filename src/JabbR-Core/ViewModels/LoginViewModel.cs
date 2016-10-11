using System.Collections.Generic;
using JabbR_Core.Data.Models;
using JabbR_Core.Services;
//using SimpleAuthentication.Core;

namespace JabbR_Core.ViewModels
{
    public class LoginViewModel
    {
        public LoginViewModel(ApplicationSettings settings,/* IEnumerable<IAuthenticationProvider> configuredProviders, */IEnumerable<ChatUserIdentity> userIdentities)
        {
            SocialDetails = new SocialLoginViewModel(/*configuredProviders,*/ userIdentities);
            AllowUserRegistration = settings.AllowUserRegistration;
            AllowUserResetPassword = settings.AllowUserResetPassword;
            HasEmailSender = !string.IsNullOrWhiteSpace(settings.EmailSender);
        }

        public bool AllowUserRegistration { get; set; }
        public bool AllowUserResetPassword { get; set; }
        public bool HasEmailSender { get; set; }
        public SocialLoginViewModel SocialDetails { get; private set; }
    }
}