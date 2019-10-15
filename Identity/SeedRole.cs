
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.Reposi;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Identity
{
    public static class SeedRole {
        public static async Task InitializeAsync(MyDBContext context,IServiceProvider serviceProvider) {
           
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            List<string> roles = new List<string>(){
                "admin",
                "userJr",
                "userSn"
            };
            IdentityResult roleResult;

            foreach (var roleName in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                   roleResult = await roleManager.CreateAsync(new Role(){
                        Name = roleName
                    });
                }
                
            }
        }
        
    }
}


