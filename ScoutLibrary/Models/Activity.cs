using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ScoutLibrary.Models
{
    public class Activity
    {
        public int? ActivityId { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }
        public string? Description { get; set; }
        public int? MaxCount { get; set; }
        public int? MinAge { get; set; }
        public List<Badge>? RequiredBadges { get; set; }
        public List<Member>? Members { get; set; }

        public Activity() { }
        public Activity(string name, DateTime dateTime, string? description, int? maxCount, int? minAge, List<Badge>? reqBadges, List<Member>? members)
        {
            Name = name;
            DateTime = dateTime;
            Description = description;
            MaxCount = maxCount;
            MinAge = minAge;
            RequiredBadges = reqBadges;
            Members = members;
        }
        
        public override string ToString()
        {
            return $"Aktivitet: {Name}. Dato og tid: {DateTime}. Beskrivelse: {Description}.";
        }

    }

}
