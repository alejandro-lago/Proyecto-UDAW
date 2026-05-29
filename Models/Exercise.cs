using System;
using System.Collections.Generic;

namespace TfgApi.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string ExternalApiId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> BodyParts { get; set; } = new List<string>();
        public List<string> TargetMuscles { get; set; } = new List<string>();
        public List<string> SecondaryMuscles { get; set; } = new List<string>();
        public List<string> Equipments { get; set; } = new List<string>();

        public string? GifUrl { get; set; }
        public List<string> Instructions { get; set; } = new List<string>();
        public bool IsActive { get; set; } = true;
    }
}