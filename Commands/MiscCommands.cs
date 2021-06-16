using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Yuzuri.Commands
{
    public class MiscCommands
    {
        
        
        public async Task SayTestingAsync(InteractionContext ctx, string toSay)
        {
            await ctx.Interaction.Channel.SendMessageAsync("Test" + toSay).ConfigureAwait(false);
        }
    }
}
