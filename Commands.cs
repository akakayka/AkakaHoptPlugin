using AkakaHoptPlugin;
using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Plagin1
{
    internal class Commands : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "jobs";

        public string Help => "тебе не помогут";

        public string Syntax => "/jobs [mode] [player] [group]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "krytoiplugin.Jobs" };

        public bool CheckPermission(string permission, IRocketPlayer caller)
        {
            if (!Rocket.Core.R.Permissions.HasPermission(caller, permission))
                Rocket.Unturned.Chat.UnturnedChat.Say(caller, "У вас нет прав на использование этой команды", Color.red);
            return Rocket.Core.R.Permissions.HasPermission(caller, permission);
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
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

                            if (Plugin.groups.FirstOrDefault(x => ((UnturnedPlayer)rocketPlayer).Player.quests.groupID == x.uniqID) != null)
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
    }
}
