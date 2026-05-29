using System;

namespace TfgApi.Models
{
    public class DayOfWeek
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}