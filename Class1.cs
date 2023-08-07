using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDG.Unturned;
using Steamworks;
using Rocket.API;
using System.Net.Sockets;
using Rocket.Core.Commands;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace AkakaHoptPlugin
{
    public class Plugin : RocketPlugin
    {
        public static List<Group> groups { get; set; } = new List<Group>();
        private static List<GroupPermission> GroupsPermissions { get; set; }

        private string pathConfig { get; } = "Plugins//Plagin1//config.json";
        private string pathData { get; } = "Plugins//Plagin1//groups.data";

        protected override void Load()
        {
            swastika(20, 20);

            LoadPermissions();
            LoadGroups();

            Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(this.SuperAye));

            Rocket.Unturned.U.Events.OnPlayerConnected += OnPlayerConnected;
        }

        private void LoadPermissions()
        {
            if(!File.Exists(pathConfig))
            {
                string json = JsonConvert.SerializeObject(new List<GroupPermission> { new GroupPermission("TestGroupName", new List<string> { "test1", "test2" }) });

                using (StreamWriter sw = new StreamWriter(pathConfig, false, Encoding.Default))
                    sw.Write(json);
            }

            string data = File.ReadAllText(pathConfig);

            GroupsPermissions = JsonConvert.DeserializeObject<List<GroupPermission>>(data);
        }

        

        private void LoadGroups()
        {
            if (File.Exists(pathData))
            {
                string json = File.ReadAllText(pathData);

                groups = JsonConvert.DeserializeObject<List<Group>>(json);
            }

            foreach (var groupName in GroupsPermissions.Select(x => x.GroupName))
            {
                if (!groups.Select(x => x.Name).Contains(groupName))
                { 
                    groups.Add(new Group(groupName));
                }
            }
        }
        static void swastika(int row, int col)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Console.WriteLine("SUPER PLAGIN LOADING.....");
                    Console.WriteLine("PLEASE WAIT............................");
                }

                Console.WriteLine();
            }
        }

        [RocketCommand("jobs", "Adds player to a group", "<player> <group>", AllowedCaller.Player)]
        public void AddGroupCommand(IRocketPlayer caller, string[] command)
        {
            // jobs add kotov
            //       0   1
            try
            {
                switch (command[0])
                {
                    case "add":
                        if (CheckPermission($"krytoiplugin.Add.{command[2]}", caller))
                        {
                            IRocketPlayer rocketPlayer = UnturnedPlayer.FromName(command[1]);
                            JobComponent component = ((UnturnedPlayer)rocketPlayer).GetComponent<JobComponent>();
                            component.permissionRequest = command[2];
                            Rocket.Unturned.Chat.UnturnedChat.Say(caller, $"Приглашение игроку {rocketPlayer.DisplayName} на вступление в {command[2]} было отослано", Color.yellow);
                            Rocket.Unturned.Chat.UnturnedChat.Say(rocketPlayer, $"Игрок {caller.DisplayName} приглашает вас вступить в {command[2]}");
                        }
                        break;
                    case "join":
                        if (CheckPermission("krytoiplugin.Join", caller))
                        {
                            JobComponent selfComponent = ((UnturnedPlayer)caller).GetComponent<JobComponent>();
                            Rocket.Core.R.Permissions.AddPlayerToGroup(selfComponent.permissionRequest, caller);
                            Rocket.Unturned.Chat.UnturnedChat.Say(caller, $"Приглашение на вступление в группу было принято", Color.yellow);
                        }
                        break;
                    case "reject":
                        if (CheckPermission("krytoiplugin.Reject", caller))
                        {
                            JobComponent rejectComponent = ((UnturnedPlayer)caller).GetComponent<JobComponent>();
                            rejectComponent.permissionRequest = String.Empty;
                            Rocket.Unturned.Chat.UnturnedChat.Say(caller, $"Приглашение на вступление в группу было отклонено", Color.yellow);
                        }
                        break;
                    case "remove":
                        if (CheckPermission($"krytoiplugin.Remove.{command[2]}", caller))
                        {
                            IRocketPlayer rocketPlayer = UnturnedPlayer.FromName(command[1]);
                            JobComponent removeComponent = ((UnturnedPlayer)rocketPlayer).GetComponent<JobComponent>();
                            removeComponent.permissionRequest = String.Empty;
                            Rocket.Core.R.Permissions.RemovePlayerFromGroup(command[2], rocketPlayer);
                            Rocket.Unturned.Chat.UnturnedChat.Say(rocketPlayer, $"Игрок {caller.DisplayName} исключил вас из {command[2]}");
                            Rocket.Unturned.Chat.UnturnedChat.Say(caller, $"Вы исключили {command[1]} из {command[2]}", Color.yellow);

                            if(groups.FirstOrDefault(x => ((UnturnedPlayer)rocketPlayer).Player.quests.groupID == x.uniqID) != null)
                            {
                                ((UnturnedPlayer)rocketPlayer).Player.quests.leaveGroup(true);
                            }
                        }
                        break;
                }
            }
            catch
            {
                Rocket.Unturned.Chat.UnturnedChat.Say(caller, $"Правильно используйте команду! Пример: '/jobs add [user] [group]'", Color.red);
            }
        }

        public bool CheckPermission(string permission, IRocketPlayer caller) 
        {
            if (!Rocket.Core.R.Permissions.HasPermission(caller, permission))
                Rocket.Unturned.Chat.UnturnedChat.Say(caller, "У вас нет прав на использование этой команды", Color.red);
            return Rocket.Core.R.Permissions.HasPermission(caller, permission);
        } 



        private void SuperAye(int level)
        {
            if (level > Level.BUILD_INDEX_SETUP + 1)
            {
                foreach (var group in groups)
                {
                    group.Initz();
                }
                Save();
            }
        }


        private void OnPlayerConnected(UnturnedPlayer player)
        {
            player.Player.gameObject.AddComponent<JobComponent>();

            foreach(var groupName in GroupsPermissions)
                foreach(var group in groupName.Permissions)
                {
                    var unturnedGroup = groups.Where(x => x.Name == groupName.GroupName).FirstOrDefault();
                    Console.WriteLine(Rocket.Core.R.Permissions.GetPermissions(player, group).Count);
                    if(Rocket.Core.R.Permissions.GetPermissions(player, group).Count != 0)
                    {
                        unturnedGroup.AddPlayer(player);
                        Console.WriteLine(groupName.GroupName);
                        break;
                    }
                    else if(player.Player.quests.groupID == unturnedGroup.uniqID)
                    {
                        player.Player.quests.leaveGroup(true);
                    }
                }
            foreach(var group in groups)
            {
                Console.WriteLine(group.Name);
            }
        }

        internal void Save()
        {
            if (groups.Count > 0)
            {
                using (StreamWriter streamWriter = new StreamWriter(pathData, false, Encoding.UTF8))
                {
                    streamWriter.WriteLine(JsonConvert.SerializeObject(groups));
                }
            }
        }

        protected override void Unload()
        {
            Rocket.Unturned.U.Events.OnPlayerConnected -= OnPlayerConnected;
        }

    }
}
