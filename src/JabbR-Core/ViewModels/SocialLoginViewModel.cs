using System;
using System.Linq;
using System.Collections.Generic;
using JabbR_Core.Data.Models;

namespace JabbR_Core.ViewModels
{ 
    public class SocialLoginViewModel
    {
        public SocialLoginViewModel(/*IEnumerable<IAuthenticationProvider> configuredProviders,*/ IEnumerable<ChatUserIdentity> userIdentities)
        {
            //ConfiguredProviders = configuredProviders != null ? configuredProviders.Select(x => x.Name) : Enumerable.Empty<string>();
            _userIdentities = userIdentities;
           
        }

        private readonly IEnumerable<ChatUserIdentity> _userIdentities;
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