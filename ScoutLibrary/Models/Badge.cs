using System;
using System.Collections.Generic;
using System.Text;

namespace ScoutLibrary.Models
{
    public class Badge
    {
        public int BadgeId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        
        // Constructor
        public Badge(int badgeId, string name, string? description)
        {
            BadgeId = badgeId;
            Name = name;
            Description = description;
        }
        
        // Empty Constructor
        public Badge() {}
    }
}
