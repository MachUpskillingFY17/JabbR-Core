using JabbR_Core.Models;
using JabbR_Core.Services;

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