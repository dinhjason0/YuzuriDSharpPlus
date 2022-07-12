using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuzuri.Commons;
using Yuzuri.Managers;

namespace Yuzuri.Commands
{
    internal class BattleCommands : BaseCommandModule
    {
        public GuildManager GuildManager { get; private set; }
        public Random RNG { get; private set; }

        public BattleCommands(IServiceProvider provider)
        {
            GuildManager = provider.GetRequiredService<GuildManager>();
            RNG = provider.GetRequiredService<Random>();
        }

        //[SlashCommand("battle", "Start a Fire Water Leaf duel with a user")]
        [Command("battle")]
        public async Task Start(CommandContext ctx, DiscordMember targetUser)
        {
            if (targetUser == null)
            {
                await ctx.Channel.SendMessageAsync("Unknown player").ConfigureAwait(false);
                return;
            }

            YuzuGuild yuzuGuild = GuildManager.ReadGuildData(ctx.Guild.Id);

            if (yuzuGuild.FloorId != ctx.Channel.Id)
            {
                await ctx.Channel.SendMessageAsync("Must be used in the new battle room channel");
                return;
            }

            DiscordMember member = (DiscordMember)ctx.User;
            DiscordGuild server = ctx.Guild;
            // Category
            DiscordChannel category = ctx.Channel.Parent;
            var interactivity = ctx.Client.GetInteractivity();

            int roll = RNG.Next(0, 2);
            //int roll = 0;

            DiscordMember[] player = new DiscordMember[2];
            if (roll == 0)
            {
                player[0] = member;
                player[1] = targetUser;
            }
            else
            {
                player[0] = targetUser;
                player[1] = member;
            }

            DiscordOverwriteBuilder[] battleLobbyOverwrites = new DiscordOverwriteBuilder[] {
                new DiscordOverwriteBuilder(player[0]) { Denied = Permissions.AccessChannels },
                new DiscordOverwriteBuilder(player[1]) { Denied = Permissions.AccessChannels },
            };

            DiscordChannel lobby = await server.CreateTextChannelAsync($"{player[0].DisplayName} V.S {player[1].DisplayName}",
                parent: category,
                overwrites: battleLobbyOverwrites)
                .ConfigureAwait(false);


            DiscordOverwriteBuilder[] memberBattleRoom = new DiscordOverwriteBuilder[] {
                new DiscordOverwriteBuilder(player[1]) { Denied = Permissions.AccessChannels },
                new DiscordOverwriteBuilder(server.EveryoneRole) { Denied = Permissions.SendMessages }
            };

            DiscordChannel memberRoom = await server.CreateTextChannelAsync($"{player[0].DisplayName}\' Battle Room",
                parent: category,
                overwrites: memberBattleRoom)
                .ConfigureAwait(false);

            DiscordOverwriteBuilder[] targetBattleRoom = new DiscordOverwriteBuilder[]
            {
                new DiscordOverwriteBuilder(player[0]) { Denied = Permissions.AccessChannels },
                new DiscordOverwriteBuilder(server.EveryoneRole) { Denied = Permissions.SendMessages }
            };

            DiscordChannel targetRoom = await server.CreateTextChannelAsync($"{player[1].DisplayName}\'s Battle Room",
                parent: category,
                overwrites: targetBattleRoom)
                .ConfigureAwait(!false);

            await ctx.Channel.SendMessageAsync($"A new duel between {player[0].Mention} & {player[1].Mention} has started!" +
                $"\nSpectators can view it from {lobby.Mention}").ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder()
            {
                Title = $"{player[0].DisplayName} V.S {player[1].DisplayName}",
                Description = $"Status:  `{player[1].DisplayName} is thinking...`",
            };

            var lobbyEmbed = new DiscordEmbedBuilder(embed);

            int memberScore = 0;
            int targetScore = 0;
            int turn = 0;

            DiscordButtonComponent fireButton = new DiscordButtonComponent(ButtonStyle.Danger, "fire", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":fire:")));
            DiscordButtonComponent waterButton = new DiscordButtonComponent(ButtonStyle.Primary, "water", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":ocean:")));
            DiscordButtonComponent leafButton = new DiscordButtonComponent(ButtonStyle.Success, "leaf", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":deciduous_tree:")));

            Dictionary<string, string> emojis = new Dictionary<string, string>();
            emojis.Add("fire", "fire");
            emojis.Add("water", "ocean");
            emojis.Add("leaf", "deciduous_tree");

            DiscordComponent[] discordComponent = new DiscordComponent[] { fireButton, waterButton, leafButton };

            DiscordMessage lobbyMsg = await new DiscordMessageBuilder()
                    .WithEmbed(embed)
                    .SendAsync(lobby)
                    .ConfigureAwait(false); ;
            DiscordMessage targetMsg = await new DiscordMessageBuilder()
                    .WithEmbed(embed)
                    .SendAsync(targetRoom)
                    .ConfigureAwait(false); ;
            DiscordMessage memberMsg = await new DiscordMessageBuilder()
                    .WithEmbed(embed)
                    .SendAsync(memberRoom)
                    .ConfigureAwait(false); ;



            while (memberScore < 3 && targetScore < 3)
            {
                string targetAction = "";
                string memberAction = "";

                targetMsg = await targetMsg.ModifyAsync(new DiscordMessageBuilder()
                  .WithEmbed(embed)
                  .AddComponents(discordComponent));
                
                var targetResponse = await interactivity.WaitForButtonAsync(targetMsg, player[1], timeoutOverride: TimeSpan.FromMinutes(5)).ConfigureAwait(false);

                targetAction = targetResponse.Result.Interaction.Data.CustomId;
                await targetResponse.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                embed.ClearFields();
                lobbyEmbed.ClearFields();

                lobbyEmbed.AddField($"{player[1].DisplayName}", $"{DiscordEmoji.FromName(ctx.Client, $":{emojis[targetAction]}:")}", true);
                lobbyEmbed.Description = $"Status:  `{player[0].DisplayName} is thinking...`";

                embed.AddField($"{player[1].DisplayName}", "Has picked their action");
                embed.Description = $"Status:  `{player[0].DisplayName} is thinking...`";
                
                await lobbyMsg.ModifyAsync(embed: lobbyEmbed.Build()).ConfigureAwait(false);
                await targetMsg.ModifyAsync(embed: lobbyEmbed.Build()).ConfigureAwait(false);

                memberMsg = await memberMsg.ModifyAsync(new DiscordMessageBuilder()
                  .WithEmbed(embed)
                  .AddComponents(discordComponent));

                var memberResponse = await interactivity.WaitForButtonAsync(memberMsg, player[0], timeoutOverride: TimeSpan.FromMinutes(5)).ConfigureAwait(false);
                
                memberAction = memberResponse.Result.Interaction.Data.CustomId;
                await memberResponse.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                lobbyEmbed.AddField($"{player[0].DisplayName}", $"{DiscordEmoji.FromName(ctx.Client, $":{emojis[memberAction]}:")}", true);

                if ((targetAction == "fire" && memberAction == "water") ||
                    (targetAction == "water" && memberAction == "leaf") ||
                    (targetAction == "leaf" && memberAction == "fire"))
                {
                    memberScore++;
                    lobbyEmbed.Description = $"{player[0].DisplayName} won a round!";
                }
                else if ((targetAction == "fire" && memberAction == "leaf") ||
                    (targetAction == "water" && memberAction == "fire") ||
                    (targetAction == "leaf" && memberAction == "water"))
                {
                    targetScore++;
                    lobbyEmbed.Description = $"{player[1].DisplayName} won a round!";
                }
                else
                {
                    lobbyEmbed.Description = $"Its a draw!";
                }

                embed.Title = $"{player[0].DisplayName}({memberScore}) V.S {player[1].DisplayName}({targetScore})";

                await lobbyMsg.ModifyAsync(embed: lobbyEmbed.Build()).ConfigureAwait(false);
                await targetMsg.ModifyAsync(embed: lobbyEmbed.Build()).ConfigureAwait(false);
                await memberMsg.ModifyAsync(embed: lobbyEmbed.Build()).ConfigureAwait(false);

                embed.Description = $"Status:  `{player[1].DisplayName} is thinking...`";

                embed.ClearFields();
                lobbyEmbed.ClearFields();

                await Task.Delay(2000).ConfigureAwait(false);

                lobbyEmbed = new DiscordEmbedBuilder(embed);
                await lobbyMsg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                await targetMsg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                await memberMsg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
            }

            if (memberScore > targetScore)
            {
                lobbyEmbed.Description = $"{player[0].DisplayName} Wins!";
            }
            else
            {
                lobbyEmbed.Description = $"{player[1].DisplayName} Wins!";
            }

            await lobbyMsg.ModifyAsync(embed: lobbyEmbed.Build()).ConfigureAwait(false);
            await targetMsg.ModifyAsync(embed: lobbyEmbed.Build()).ConfigureAwait(false);
            await memberMsg.ModifyAsync(embed: lobbyEmbed.Build()).ConfigureAwait(false);

            await Task.Delay(5000);

            await targetRoom.DeleteAsync().ConfigureAwait(false);
            await memberRoom.DeleteAsync().ConfigureAwait(false);
            await lobby.DeleteAsync().ConfigureAwait(false);
        }

    }
}
