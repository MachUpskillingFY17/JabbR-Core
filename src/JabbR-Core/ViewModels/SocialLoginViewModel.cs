using System;
using System.Collections.Generic;
using System.Linq;
using JabbR_Core.Models;
//using SimpleAuthentication.Core;

namespace JabbR_Core.ViewModels
{
    public class SocialLoginViewModel
    {
        public SocialLoginViewModel(/*IEnumerable<IAuthenticationProvider> configuredProviders,*/ IEnumerable<ChatUserIdentities> userIdentities)
        {
            //ConfiguredProviders = configuredProviders != null ? configuredProviders.Select(x => x.Name) : Enumerable.Empty<string>();
            _userIdentities = userIdentities;
           
        }

        private readonly IEnumerable<ChatUserIdentities> _userIdentities;
        public IEnumerable<string> ConfiguredProviders { get; private set; }

        public bool IsAlreadyLinked(string providerName)
        {
              if (_userIdentities == null || !_userIdentities.Any())
              {
                  return false;
              }

              return _userIdentities.Any(x => x.ProviderName.Equals(providerName, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}