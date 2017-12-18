using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace TermoHub.Models
{
    public class User : IdentityUser
    {
        public ICollection<Device> Devices { get; set; }
    }
}
