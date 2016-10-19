using System;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace JabbR_Core.Hubs
{
    public class ServicesHubActivator : IHubActivator
    {
        private readonly IServiceProvider _serviceProvider;
        private Chat _chat;

        public ServicesHubActivator(IServiceProvider serviceProvider, Chat chat)
        {
            _serviceProvider = serviceProvider;
            _chat = chat;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            if (descriptor.HubType == null)
            {
                return null;
            }

            // This resolves from the DI container
            return _chat as IHub;

            //return ActivatorUtilities.CreateInstance<Chat>(_serviceProvider);

            // This doesn't
            //return ActivatorUtilities.CreateInstance(_serviceProvider, descriptor.HubType) as IHub;
        }
    }
}
