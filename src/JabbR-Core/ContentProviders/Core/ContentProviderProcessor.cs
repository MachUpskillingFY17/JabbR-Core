using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Hubs;
using JabbR_Core.Data.Repositories;

namespace JabbR_Core.ContentProviders.Core
{
    public class ContentProviderProcessor
    {
        private IJabbrRepository _repository;
        private IResourceProcessor _processor;

        public ContentProviderProcessor(IJabbrRepository repository,
                                        IResourceProcessor processor)
        {
            _repository = repository;
            _processor = processor;
        }

        public void ProcessUrls(IEnumerable<string> links,
                                IHubConnectionContext<dynamic> clients,
                                string roomName,
                                string messageId)
        {            
            var contentTasks = links.Select(_processor.ExtractResource).ToArray();

            Task.Factory.ContinueWhenAll(contentTasks, tasks =>
            {
                foreach (var task in tasks)
                {
                    if (task.IsFaulted)
                    {
                        Trace.TraceError(task.Exception.GetBaseException().Message);
                        continue;
                    }

                    if (task.Result == null || String.IsNullOrEmpty(task.Result.Content))
                    {
                        continue;
                    }

                    // Update the message with the content

                    // REVIEW: Does it even make sense to get multiple results?
                    using (_repository)
                    {
                        var message = _repository.GetMessageById(messageId);

                        // Should this be an append?
                        message.HtmlContent = task.Result.Content;

                        _repository.CommitChanges();
                    }

                    // Notify the room
                    clients.Group(roomName).addMessageContent(messageId, task.Result.Content, roomName);
                }
            });
        }
    }
}