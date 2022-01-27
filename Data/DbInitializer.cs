using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using uang_trans.Models;

namespace uang_trans.Data
{
    public class DbInitializer
    {
        private AppDbContext _context;

        public DbInitializer(AppDbContext context)
        {
            _context = context;
        }

        public async Task Initialize(UserManager<IdentityUser> userManager, bool isProd)
        {
            if(isProd)
            {  
                Console.Write("--> Menjalankan migrasi");
                try
                {
                    _context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Gagal Menjalankan Migrasi dengan error: {ex.Message}");
                }
            }

            var roleStore = new RoleStore<IdentityRole>(_context);

            if (userManager.FindByEmailAsync("admin@admin.com").Result == null)
            {
                if (!_context.Roles.Any(r => r.Name == "Admin"))
                {
                    await roleStore.CreateAsync(new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" });
                }
                if (!_context.Roles.Any(r => r.Name == "Customer"))
                {
                    await roleStore.CreateAsync(new IdentityRole { Name = "Customer", NormalizedName = "CUSTOMER" });
                }

                IdentityUser user = new IdentityUser
                {
                    UserName = "Admin",
                    Email = "admin@admin.com"
                };
                IdentityResult result = userManager.CreateAsync(user, "Admin@123").Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                    userManager.SetLockoutEnabledAsync(user, false).Wait();
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}