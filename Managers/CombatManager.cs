using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuzuri.Commons;
using Yuzuri.Helpers;

namespace Yuzuri.Managers
{
    public class CombatManager
    {
        public CombatManager() { }

        public static string[] GetFloorMobs(string floor)
        {
            string[] mobFiles = Directory.GetFiles("data/Floors/");

            string[] floorMobs = mobFiles.Where(x => x.StartsWith($"F{floor}")).ToArray();

            return floorMobs;
        }

        public static Monster GetMonster(string file)
        {
            using StreamReader r = new StreamReader($"data/Players/{file}.json");
            string json = r.ReadToEnd();
            Monster monster = JsonConvert.DeserializeObject<Monster>(json);
            r.Close();
            return monster;
        }

        public static async Task StartCombat(InteractionContext ctx)
        {
            bool inCombat = true;

            //Monster mob = GetMonster(ctx.Channel.Name.Split('-')[1]);
            Monster mob = new Monster()
            {
                Name = "Slime",
                MaxHP = 20,
                HP = 20,
                SPD = 3,
                STR = 1,
                ImgUrl = "https://static.wikia.nocookie.net/tensei-shitara-slime-datta-ken/images/3/34/Rimuru_Slime_Anime.png/revision/latest?cb=20180922214304"
            };

            CombatPhase phase = CombatPhase.OffTurn;
            int counter = 0;
            Dictionary<ulong, Actions> actions = new Dictionary<ulong, Actions>();
            do
            {
                Console.WriteLine("Test1");
                
                counter++;
                switch (phase)
                {
                    case CombatPhase.OffTurn:
                        actions = await GetButtonInputs(actions, ctx, mob, phase, DiscordComponentHelper.OffTurnComponents).ConfigureAwait(false);
                        Console.WriteLine("OffTurn");
                        phase = CombatPhase.AttackTurn;
                        break;
                    case CombatPhase.AttackTurn:
                        actions = await GetButtonInputs(actions, ctx, mob, phase, DiscordComponentHelper.AttackTurnComponents).ConfigureAwait(false);
                        Console.WriteLine("Combat Phase");
                        foreach (ulong id in actions.Keys)
                        {
                            Player player = PlayerManager.ReadPlayerData(id);
                            Actions action = actions[id];

                            Tuple<double, double> stance = action.GetAction1Multiplier();
                            double attack = action.GetAction2Mulitplier();

                            mob.HP -= Convert.ToInt32(((player.STR + player.Equipped[Player.EquippedSlots.MainHand].STR) * attack) * (player.HIT * stance.Item1));
                        }
                        
                        phase = CombatPhase.AfterTurn;
                        break;
                    case CombatPhase.AfterTurn:
                        actions = await GetButtonInputs(actions, ctx, mob, phase, DiscordComponentHelper.OffTurnComponents).ConfigureAwait(false);
                        Console.WriteLine("AfterTurn");
                        phase = CombatPhase.OffTurn;

                        actions.Clear();
                        break;
                }

                if (mob.HP < 0)
                {
                    inCombat = false;
                }
                if (counter > 13) inCombat = false;
            }
            while (inCombat);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Combat done! You win nothing, but depression and suffering")).ConfigureAwait(false);
        }

        public static async Task<Dictionary<ulong, Actions>> GetButtonInputs(Dictionary<ulong, Actions> actions, InteractionContext ctx, Monster mob, CombatPhase phase, DiscordComponent[] components)
        {
            //Dictionary<ulong, Actions> actions = new Dictionary<ulong, Actions>();

            var timeout = DateTime.Now + TimeSpan.FromSeconds(15);

            while (DateTime.Now < timeout)
            {
                try
                {
                    Console.WriteLine("Button Press");
                    DiscordWebhookBuilder builder = new DiscordWebhookBuilder()
                        .WithContent($"{phase} HP: {mob.HP}")
                        .AddComponents(components);

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    {
                        Title = $"{mob.Name}: {mob.HP} HP",
                        ImageUrl = mob.ImgUrl,
                        Footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            Text = $"Actions: {string.Join(" | ", actions.Select(k => $"**{PlayerManager.ReadPlayerData(k.Key).Name}** {k.Value}"))}"
                        }

                    };

                    builder.AddEmbed(embed);

                    var msg = await ctx.EditResponseAsync(builder).ConfigureAwait(false);

                    var result = await msg.WaitForButtonAsync(TimeSpan.FromSeconds(15)).ConfigureAwait(false);

                    if (!result.TimedOut)
                    {
                        await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                        
                        if (phase == CombatPhase.OffTurn && !actions.ContainsKey(result.Result.Interaction.User.Id))
                            actions[result.Result.Interaction.User.Id] = new Actions();
                        if (actions.ContainsKey(result.Result.Interaction.User.Id))
                        {

                            switch (phase)
                            {
                                case CombatPhase.OffTurn:
                                    actions[result.Result.Interaction.User.Id].Action1 = result.Result.Interaction.Data.CustomId;
                                    break;
                                case CombatPhase.AttackTurn:
                                    actions[result.Result.Interaction.User.Id].Action2 = result.Result.Interaction.Data.CustomId;
                                    break;
                                case CombatPhase.AfterTurn:
                                    actions[result.Result.Interaction.User.Id].Action3 = result.Result.Interaction.Data.CustomId;
                                    break;
                            }
                        }

                        Console.WriteLine($"{result.Result.Interaction.User.Id} {actions[result.Result.Interaction.User.Id]}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return actions;
        }

        public class Actions
        {
            public Actions()
            {
                Action1 = "None";
                Action2 = "None";
                Action3 = "None";
            }

            public string Action1 { get; set; }
            public string Action2 { get; set; }
            public string Action3 { get; set; }

            public Tuple<double, double> GetAction1Multiplier()
            {
                // Dmg, Reduction
                return Action1 switch
                {
                    "Block" => new Tuple<double, double>(0.3, 0.8),
                    "Parry" => new Tuple<double, double>(0.85, 0.15),
                    "Dodge" => new Tuple<double, double>(0.5, 1),
                    "Attack" => new Tuple<double, double>(1.15, 0.0),
                    _ => new Tuple<double, double>(1.0, 0.0)
                };
            }

            public double GetAction2Mulitplier()
            {
                return Action2 switch
                {
                    "LightAttack" => 0.8,
                    "NormalAttack" => 1.0,
                    "HeavyAttack" => 1.3,
                    _ => 1.0
                };
            }

            
            public override string ToString()
            {
                return $"[{Action1}, {Action2}, {Action3}]";
            }
        }

        /// <summary>
        /// OffTurn = Battle Stance
        /// AttackTurn = Attack type
        /// AfterTurn = Dots/Other effects resolve/Dmged by boss
        /// </summary>
        public enum CombatPhase
        {
            None = 0,
            //PreOffTurn = 1,
            OffTurn = 2,
            AttackTurn = 3,
            //OffAfterTurn = 4,
            AfterTurn = 5,
            //PostTurn = 6,
            End = -1
        }
    }
}
