﻿using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Yuzuri.Commons;

namespace Yuzuri.Managers
{
    public class GuildManager
    {
        public GuildManager() { }

        public void WriteGuildData(YuzuGuild guild)
        {
            using StreamWriter w = File.CreateText($"data/Guilds/{guild.GuildId}.json");
            JsonSerializer searializer = new JsonSerializer();
            searializer.Serialize(w, guild);
            w.Close();
        }

        public static YuzuGuild ReadGuildData(ulong id)
        {
            using StreamReader r = new StreamReader($"data/Guilds/{id}.json");
            string json = r.ReadToEnd();
            YuzuGuild guild = JsonConvert.DeserializeObject<YuzuGuild>(json);
            r.Close();

            return guild;
        }

        public async Task GuildCheck(DiscordGuild guild)
        {
            try
            {

                YuzuGuild yuzuGuild;

                if (!File.Exists($"data/Guilds/{guild.Id}.json"))
                {
                    Console.WriteLine($"Checking {guild.Name} data... 404 NOT FOUND!");
                    yuzuGuild = new YuzuGuild(guild.Id);
                }
                else
                {
                    Console.WriteLine($"Checking {guild.Name} data... OK.");
                    yuzuGuild = GuildManager.ReadGuildData(guild.Id);
                }

                // Check if already exists
                bool hasRoomCategory = yuzuGuild.RoomId != 0;
                bool hasFloorsCategory = yuzuGuild.FloorId != 0;
                ulong resourcesChannel = yuzuGuild.Resources.Count == 0 ? 0 : yuzuGuild.Resources[0];
                /* ulong resourcesChannel = 0;
                 if (yuzuGuild.Resources.Count == 0)
                     resourcesChannel = yuzuGuild.Resources[0];*/

                if (!hasRoomCategory || !hasFloorsCategory || resourcesChannel == 0)
                {
                    foreach (KeyValuePair<ulong, DiscordChannel> channel in guild.Channels)
                    {
                        if (channel.Value.IsCategory)
                        {
                            // Skip if one already exists
                            switch (channel.Value.Name)
                            {
                                case "Player-Rooms":
                                    if (hasRoomCategory) break;

                                    Console.WriteLine($"{guild.Name}'s Player-Rooms... Found!");
                                    hasRoomCategory = true;
                                    yuzuGuild.RoomId = channel.Value.Id;
                                    break;
                                case "Floors":
                                    if (hasFloorsCategory) break;

                                    Console.WriteLine($"{guild.Name}'s Floors... Found!");
                                    hasFloorsCategory = true;
                                    yuzuGuild.FloorId = channel.Value.Id;
                                    break;
                            }

                        }
                        else if (channel.Value.Name.Equals("resources", StringComparison.OrdinalIgnoreCase) && resourcesChannel == 0)
                        {
                            Console.WriteLine($"{guild.Name}'s Resource channel... Found!");
                            resourcesChannel = channel.Value.Id;
                            yuzuGuild.Resources.Add(resourcesChannel);
                        }
                    }


                    // Add new categories
                    if (!hasRoomCategory)
                    {
                        DiscordOverwriteBuilder[] discordOverwrite = new DiscordOverwriteBuilder[1];
                        discordOverwrite[0] = new DiscordOverwriteBuilder(guild.EveryoneRole) { Denied = Permissions.AccessChannels };


                        Console.WriteLine($"{guild.Name}'s Player-Rooms... 404 NOT FOUND!");
                        var room = await guild.CreateChannelCategoryAsync("Player-Rooms", discordOverwrite).ConfigureAwait(false);
                        Console.WriteLine($"Generating {guild.Name}'s Player-Rooms...");

                        yuzuGuild.RoomId = room.Id;
                    }
                    if (!hasFloorsCategory)
                    {
                        DiscordOverwriteBuilder[] discordOverwrite = new DiscordOverwriteBuilder[1];
                        discordOverwrite[0] = new DiscordOverwriteBuilder(guild.EveryoneRole) { Denied = Permissions.AccessChannels };

                        Console.WriteLine($"{guild.Name}'s Floors... 404 NOT FOUND!");
                        var floor = await guild.CreateChannelCategoryAsync("Floors", discordOverwrite).ConfigureAwait(false);
                        Console.WriteLine($"Generating {guild.Name}'s Floors...");

                        yuzuGuild.FloorId = floor.Id;
                    }
                }
                else
                {
                    Console.WriteLine($"{guild.Name}'s Categories... OK.");
                }

                // Check if already exists
                bool hasPlayerRole = yuzuGuild.RoleId != 0;

                // if it does skip
                if (!hasPlayerRole)
                {
                    foreach (KeyValuePair<ulong, DiscordRole> roles in guild.Roles)
                    {

                        if (roles.Value.Name == "Player")
                        {
                            Console.WriteLine($"{guild.Name}'s Player Role... Found!");
                            hasPlayerRole = true;
                            yuzuGuild.RoleId = roles.Value.Id;
                            break;
                        }
                    }

                    // Add new role
                    if (!hasPlayerRole)
                    {
                        Console.WriteLine($"{guild.Name}'s Player Role... 404 NOT FOUND!");
                        var role = await guild.CreateRoleAsync("Player").ConfigureAwait(false);
                        Console.WriteLine($"Generating {guild.Name}'s Player Role...");

                        yuzuGuild.RoleId = role.Id;


                    }
                }
                else
                {

                    Console.WriteLine($"{guild.Name}'s Roles... OK.");

                    // Download files from resouces channel
                    try
                    {
                        if (resourcesChannel != 0)
                        {
                            DiscordChannel resources = await Bot.Client.GetChannelAsync(resourcesChannel).ConfigureAwait(false);

                            Console.WriteLine($"Checking Resources... Found! Extracting data");

                            foreach (DiscordMessage msg in await resources.GetMessagesAsync().ConfigureAwait(false))
                            {

                                // IF already extracted skip
                                if (!yuzuGuild.Resources.Contains(msg.Id))
                                {
                                    if (msg.Attachments.Count != 0)
                                    {

                                        DiscordAttachment discordAttachment = msg.Attachments[0];


                                        using WebClient client = new WebClient();
                                        await client.DownloadFileTaskAsync(new Uri(discordAttachment.Url), $"{discordAttachment.FileName}").ConfigureAwait(false);

                                        if (discordAttachment.MediaType.Equals("application/zip", StringComparison.OrdinalIgnoreCase))
                                            ZipFile.ExtractToDirectory(discordAttachment.FileName, $"{Directory.GetCurrentDirectory()}/{msg.Content}", true);
                                        File.Delete(discordAttachment.FileName);

                                        yuzuGuild.Resources.Add(msg.Id);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Checking Resources... 404 NOT FOUND! Skipping...");
                        Console.WriteLine(ex);
                    }

                }

                Console.WriteLine($"Generating {guild.Name} data... Done.");
                WriteGuildData(yuzuGuild);


            }
            catch (Exception ex)
            {
                Console.WriteLine("Guild List could not be retrieved");
                Console.WriteLine(ex);
            }
        }
    }
}
