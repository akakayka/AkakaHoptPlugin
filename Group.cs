using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkakaHoptPlugin
{
    public class Group
    {
        public string Name { get; set; }
        public CSteamID uniqID { get; set; } = new CSteamID();
        private GroupInfo groupInfo { get; set; }

        public Group(string name)
        {
            Name = name;
        }

        public void Initz()
        {
            bool flag = false;

            if (uniqID == new CSteamID())
            {
                uniqID = GroupManager.generateUniqueGroupID();
                Console.WriteLine($"Set {Name} uniqID={uniqID}");
            }

            groupInfo = GroupManager.getOrAddGroup(this.uniqID, this.Name, out flag);
            Console.WriteLine($"{Name}({uniqID.ToString()}) wascreated={flag}");
        }

        public void DeleteGroup()
        {
            GroupManager.deleteGroup(uniqID);
        }

        public void AddPlayer(UnturnedPlayer player)
        {
            player.Player.quests.ServerAssignToGroup(uniqID, EPlayerGroupRank.MEMBER, true);
            Console.WriteLine(Name);
            Console.WriteLine(uniqID.ToString());
        }

    }
}
