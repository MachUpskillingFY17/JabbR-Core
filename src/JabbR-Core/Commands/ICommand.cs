//using System.Compsition;
using Microsoft.DotNet.Cli.Utils;

namespace JabbR_Core.Commands
{
    //[InheritedExport]
    public interface ICommand
    {
        void Execute(CommandContext context, CallerContext callerContext, string[] args);
    }
}