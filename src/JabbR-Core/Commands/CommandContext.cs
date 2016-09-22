using JabbR_Core.Services;
using JabbR_Core.Data.Repositories;

namespace JabbR_Core.Commands
{
    public class CommandContext
    {
        public IJabbrRepository Repository { get; set; }
        public ICache Cache { get; set; }
        public IChatService Service { get; set; }
        public INotificationService NotificationService { get; set; }
    }
}