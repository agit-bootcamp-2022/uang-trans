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
using uang_trans.Input;
using uang_trans.Input.Role;
using uang_trans.Input.User;
using uang_trans.Input.Profile;
using uang_trans.Models;
using AutoMapper;
using uang_trans.Input.Wallet;

// using System.IdentityModel.Tokens.Jwt;

namespace uang_trans.GraphQL
{
    public class Mutation
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public Mutation(IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<Register> RegisterUserAsync([Service] AppDbContext context,
                                                      [Service] UserManager<IdentityUser> userManager,
                                                      Register input)
        {
            try
            {
                var newUser = new IdentityUser
                {
                    UserName = input.Username,
                    Email = input.Username

                };

                var result = await userManager.CreateAsync(newUser, input.Password);

                var user = await userManager.FindByNameAsync(input.Username);

                await userManager.SetLockoutEnabledAsync(user, false);

                await userManager.AddToRoleAsync(user, "Customer");

                if (!result.Succeeded)
                {
                    throw new Exception("Gagal Menambahkan User");
                }
                var userResult = await userManager.FindByNameAsync(newUser.Email);


                var userEntity = new Customer
                {
                    Username = input.Username,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    Email = input.Email,
                    CreatedDate = DateTime.Now
                };

                Console.WriteLine(userEntity);

                context.Customers.Add(userEntity);
                await context.SaveChangesAsync();

                context.Entry(userEntity).GetDatabaseValues();
                int id = userEntity.Id;

                var walletEntity = new Wallet
                {
                    CustomerId = id,
                    Balance = 0,
                    CreatedDate = DateTime.Now

                };

                Console.WriteLine(walletEntity);

                context.Wallets.Add(walletEntity);
                await context.SaveChangesAsync();

                return input;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }

        // [Authorize(Roles = new [] {"Admin"})]
        public async Task<Register> RegisterAdminAsync([Service] AppDbContext context,
                                                   [Service] UserManager<IdentityUser> userManager,
                                                   Register input)
        {
            try
            {
                var newAdmin = new IdentityUser
                {
                    UserName = input.Username,
                    Email = input.Email
                };

                var result = await userManager.CreateAsync(newAdmin, input.Password);
                if (!result.Succeeded)
                {
                    throw new Exception("failed to add admin user");
                }

                var user = await userManager.FindByNameAsync(input.Username);

                await userManager.SetLockoutEnabledAsync(user, false);

                await userManager.AddToRoleAsync(user, "Admin");

                var userEntity = new Customer
                {
                    Username = input.Username,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    Email = input.Email,
                    CreatedDate = DateTime.Now

                };

                Console.WriteLine(userEntity);

                context.Customers.Add(userEntity);
                await context.SaveChangesAsync();

                return input;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }

        }

        public async Task<AddRoleToUser> AddToRoleAsync([Service] AppDbContext context,
                                         [Service] UserManager<IdentityUser> userManager,
                                         AddRoleToUser input)
        {
            var user = await userManager.FindByNameAsync(input.Username);
            try
            {
                var result = await userManager.AddToRoleAsync(user, input.Rolename);
                if (!result.Succeeded)
                {

                    StringBuilder errMsg = new StringBuilder(String.Empty);
                    foreach (var err in result.Errors)
                    {
                        errMsg.Append(err.Description + " ");
                    }
                    throw new Exception($"{errMsg}");
                }

                return input;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UserToken> LoginUserAsync([Service] AppDbContext context,
                                                    [Service] IOptions<TokenSettings> tokenSettings,
                                                    [Service] UserManager<IdentityUser> userManager,
                                                    LoginUserInput input)
        {
            var identityUser = await userManager.FindByNameAsync(input.Username);
            var userFind = await userManager.CheckPasswordAsync(identityUser, input.Password);
            if (!userFind)
                return null;
            var user = await context.Customers.Where(u => u.Username == input.Username).SingleOrDefaultAsync();
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("Id", user.Id.ToString()));
            var roles = await userManager.GetRolesAsync(identityUser);

            foreach (var role in roles)
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
            if (role.Result)
            {
                return await Task.FromResult(new TransactionStatus(false, "Role already exist"));
            }
            await roleManager.CreateAsync(new IdentityRole(input.RoleName));
            return await Task.FromResult(new TransactionStatus(true, "Add Role Success"));
        }

        // [Authorize(Roles = new [] {"Customer"})]
        public async Task<WalletBalance> TopUp([Service] AppDbContext context,
                                                WalletInput input)
        {
            var custId = _httpContextAccessor.HttpContext.User.FindFirst("Id").Value;
            var userWallet = await context.Wallets.Where(w => w.CustomerId == Convert.ToInt32(custId)).SingleOrDefaultAsync();
            if (userWallet == null) return new WalletBalance("Wallet Not Found", 0);

            userWallet.Balance += input.Balance;

            var walletMutation = new WalletMutation
            {
                WalletId = userWallet.Id,
                Amount = input.Balance,
                MutationType = MutationType.Credit,
                CreatedDate = DateTime.Now,
            };

            await context.WalletMutations.AddAsync(walletMutation);

            await context.SaveChangesAsync();
            return new WalletBalance("Success", userWallet.Balance);
        }

        [Authorize(Roles = new[] { "Customer" })]
        public async Task<ProfileResult> UpdateProfileAsync([Service] AppDbContext context, ProfileInput input)
        {
            var custId = _httpContextAccessor.HttpContext.User.FindFirst("Id").Value;
            var customer = await context.Customers.Where(cust => cust.Id == Convert.ToInt32(custId)).SingleOrDefaultAsync();
            if (customer == null)
            {
                return await Task.FromResult(new ProfileResult("Profile not Found", null));
            }

            customer.FirstName = input.FirstName;
            customer.LastName = input.LastName;
            customer.Email = input.Email;
            customer.CreatedDate = DateTime.Now;
            await context.SaveChangesAsync();

            var data = _mapper.Map<ProfileOutput>(customer);

            return (new ProfileResult(Message: $"Update profile for Id {customer.Id} success", Data: data));
        }

        public async Task<WalletBalance> UpdateWalletAsync([Service] AppDbContext context, WalletInput input)
        {
            var custId = _httpContextAccessor.HttpContext.User.FindFirst("Id").Value;
            var wallet = await context.Wallets.Where(w => w.CustomerId == Convert.ToInt32(custId)).SingleOrDefaultAsync();

            if (wallet == null) return new WalletBalance("Wallet Not Found", 0);

            wallet.Balance = input.Balance;
            await context.SaveChangesAsync();

            return new WalletBalance("Success", wallet.Balance);
        }

        public async Task<TransactionStatus> LockUserAsync([Service] AppDbContext context,
                                                           [Service] UserManager<IdentityUser> userManager,
                                                           LockUser input)
        {
            var user = await userManager.FindByNameAsync(input.Username);

            await userManager.SetLockoutEnabledAsync(user, true);

            return await Task.FromResult(new TransactionStatus(true, "Lock User Success"));
        }

        public async Task<TransactionStatus> UnlockUserAsync([Service] AppDbContext context,
                                                             [Service] UserManager<IdentityUser> userManager,
                                                             LockUser input)
        {
            var user = await userManager.FindByNameAsync(input.Username);

            await userManager.SetLockoutEnabledAsync(user, false);

            return await Task.FromResult(new TransactionStatus(true, "Unlock User Success"));
        }
    }
}