using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
// using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using uang_trans.Input.Role;
using uang_trans.Input.User;
using uang_trans.Models;
// using System.IdentityModel.Tokens.Jwt;

namespace uang_trans.GraphQL
{
    public class Mutation
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Mutation(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserToken> LoginUserAsync([Service] AppDbContext context, 
                                                    [Service] IOptions<TokenSettings> tokenSettings, 
                                                    [Service] UserManager<IdentityUser> userManager,
                                                    LoginUserInput input) 
        {
            var identityUser = await userManager.FindByNameAsync(input.Username);
            var userFind = await userManager.CheckPasswordAsync(identityUser, input.Password);
            if(!userFind)
                return null;
            var user = await context.Customers.Where(u=> u.Username == input.Username).SingleOrDefaultAsync();
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("Id", user.Id.ToString()));
            var roles = await userManager.GetRolesAsync(identityUser);

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var tokenHandler = new JwtSecurityTokenHandler();
            var expired = DateTime.Now.AddHours(3);
            var jwtToken = new JwtSecurityToken(
                issuer: tokenSettings.Value.Issuer,
                audience: tokenSettings.Value.Audience,
                expires: expired,   
                claims: claims,
                signingCredentials: credentials
            );
            var token = tokenHandler.WriteToken(jwtToken);
            return await Task.FromResult(new UserToken(token, expired.ToString(), null));
        }

        // [Authorize(Roles = new [] {"Admin"})]
        public async Task<TransactionStatus> CreateRoleAsync([Service] AppDbContext context,
                                                             [Service] RoleManager<IdentityRole> roleManager, 
                                                             CreateRoleInput input)
        {
            var role = roleManager.RoleExistsAsync(input.RoleName);
            if(role.Result)
            {
                return await Task.FromResult(new TransactionStatus(false, "Role already exist"));
            }
            await roleManager.CreateAsync(new IdentityRole(input.RoleName));
            return await Task.FromResult(new TransactionStatus(true, "Add Role Success"));
        }

        // [Authorize(Roles = new [] {"Customer"})]
        public async Task<WalletBalance> TopUp([Service] AppDbContext context)
        {
            var custId = _httpContextAccessor.HttpContext.User.FindFirst("Id").Value;
            var user = await context.Wallets.Where(w => w.CustomerId == Convert.ToInt32(custId)).SingleOrDefaultAsync();
            if(user == null) return new WalletBalance(0);
            return new WalletBalance(user.Balance);
        }
    }
}