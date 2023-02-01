using AspAuthDemoApp.Feature.Authentication.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace AspAuthDemoApp.Data
{
    public class DatabaseSeeder
    {
        public static void PopulateDb(IApplicationBuilder app)
        {
            var serviceScope = app.ApplicationServices.CreateScope();
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var authDbContext = serviceScope.ServiceProvider.GetService<AuthDbContext>()!;
            SeedData(authDbContext, userManager, roleManager);
        }

        private static async void SeedData(AuthDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            Log.Information("Trying to apply auth migrations to Database");
            context.Database.Migrate();

            List<string> saccoRoles = new() { UserRoles.Admin, UserRoles.User };
            foreach (var role in saccoRoles)
            {
                var exists = await roleManager.RoleExistsAsync(role);
                if (!exists)
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = role });
                }
            }

            if (context.Users.Any())
            {
                Log.Information("Auth Migrations already applied to Database");
                return;   // DB has been seeded
            }

            var demoUser = new ApplicationUser
            {
                Id = "8c093b20-4f5e-4ec2-bfe2-a142e773d870",
                UserName = "johndoe",
                Email = "johndoe@gmail.com",
                PhoneNumber = "123456789",
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await userManager.CreateAsync(demoUser, "123@Demo");
            if (!result.Succeeded) return;
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Name, demoUser.UserName),
                    new Claim(JwtClaimTypes.Role, UserRoles.Admin)
                };
                await userManager.AddClaimsAsync(demoUser, claims);
                var isInRole = await userManager.IsInRoleAsync(demoUser, UserRoles.Admin);
                if (!isInRole)
                {
                    await userManager.AddToRoleAsync(demoUser, UserRoles.Admin.ToUpper());
                }
            }
        }

    }
}
