using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace TfgApi.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Routine> Routines { get; set; } = new List<Routine>();
    }
}