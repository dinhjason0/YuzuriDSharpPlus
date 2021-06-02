using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Yuzuri.Commands
{
    public class MiscCommands : BaseCommandModule
    {

        [Command("info"), Description("Check for info about general information")]
        public async Task Info(CommandContext ctx, string arg)
        {
            switch (arg.ToLower())
            {
                case "status":
                case "statuses":
                    await ctx.Channel.SendMessageAsync("A player can be classified into three statuses `Alive`, `Unconscious` and `Dead`").ConfigureAwait(false);
                    break;
            }
        }
    }
}
