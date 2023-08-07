using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AkakaHoptPlugin
{
    public class GroupPermission
    {
        public string GroupName { get; set; }
        public List<string> Permissions { get; set; }

        public GroupPermission()
        { }

        public GroupPermission(string name, List<string> permissions)
        {
            GroupName = name;
            Permissions = permissions;
        }
    }
}
