using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountSystem.Config;
using IdentityServer4.EntityFramework.Mappers;
using AccountSystem.Models;

namespace AccountSystem.Data
{
    public class SeedData
    {
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                {
                    var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                    context.Database.Migrate();
                    EnsureSeedData(context);
                }

                {
                    var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    context.Database.Migrate();

                    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var caizy = userMgr.FindByNameAsync("caizy").Result;
                    if (caizy == null)
                    {
                        caizy = new IdentityUser
                        {
                            UserName = "caizy",
                        };
                        var result = userMgr.CreateAsync(caizy, "Pass123$43").Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        if(!roleMgr.RoleExistsAsync(Role.AccountAdmin.ToString()).Result)
                        {
                            roleMgr.CreateAsync(new IdentityRole(Role.AccountAdmin.ToString()));
                        }
                        caizy = userMgr.FindByNameAsync(caizy.UserName).Result;
                        result = userMgr.AddToRoleAsync(caizy, Role.AccountAdmin.ToString()).Result;

                        result = userMgr.AddClaimsAsync(caizy, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "caizy"),
                        }).Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }
                    }
                }
            }
        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in InitConfig.GetClients().ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            //if (!context.IdentityResources.Any())
            //{
            //    Console.WriteLine("IdentityResources being populated");
            //    foreach (var resource in InitConfig.GetIdentityResources().ToList())
            //    {
            //        context.IdentityResources.Add(resource.ToEntity());
            //    }
            //    context.SaveChanges();
            //}

            if (!context.ApiResources.Any())
            {
                foreach (var resource in InitConfig.GetApiResources().ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
        }
    }
}
