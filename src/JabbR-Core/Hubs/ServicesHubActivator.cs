using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace JabbR_Core.Hubs
{
    public class ServicesHubActivator : IHubActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public ServicesHubActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (descriptor.HubType == null)
            {
                return null;
            }

            return _serviceProvider.GetRequiredService(descriptor.HubType) as IHub;
        }
    }


}
