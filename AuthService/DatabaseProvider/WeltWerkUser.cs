using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.DatabaseProvider
{
    public class WeltWerkUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}
