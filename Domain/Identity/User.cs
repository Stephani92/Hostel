

using System.Collections.Generic;
using System.Security.Claims;
using Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.Reposi
{
    public class User : IdentityUser
    {
        public List<Job> Jobs { get; set; }

    }
}