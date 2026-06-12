using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Extension
{
    public class NavMenuItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; } = "";
        public string Href { get; set; } = "";
        public string Icon { get; set; } = "";
        public string IconColor { get; set; } = "text-gray-400";
        public bool IsActive { get; set; }

        public List<string> AllowedRoles { get; set; } = [];

        public List<NavMenuItem>? SubItems { get; set; } 

    }
}
