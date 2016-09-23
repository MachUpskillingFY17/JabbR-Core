//using System.Compsition;
using Microsoft.DotNet.Cli.Utils;
using System;

namespace JabbR_Core.Commands
{
    //[InheritedExport]
    public interface ICommand
    {
        void Execute(CommandContext context, CallerContext callerContext, string[] args);
    }
}