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
using uang_trans.Input.Transaction;

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

        public async Task<TransactionStatus> RegisterUserAsync([Service] AppDbContext context,
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

                return await Task.FromResult(new TransactionStatus(true, "Add User Success"));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }

        // [Authorize(Roles = new [] {"Admin"})]
        public async Task<TransactionStatus> RegisterAdminAsync([Service] AppDbContext context,
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

                return await Task.FromResult(new TransactionStatus(true, "Add New Admin User Success!!"));
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

        [Authorize(Roles = new [] {"Customer"})]
        public async Task<TransactionCreateOutput> CreateTransaction([Service] AppDbContext context,
                                                                    TransactionCreateInput input)
        {
            var custId = _httpContextAccessor.HttpContext.User.FindFirst("Id").Value;
            try
            {
                var buyerWallet = context.Wallets.Where(buy => buy.CustomerId == input.BuyerId).SingleOrDefault();
                if (buyerWallet == null) return new TransactionCreateOutput("Buyer wallet Not Found", 0);
                if (buyerWallet.Balance < input.AmountBuyer) return new TransactionCreateOutput("Insufficient User Balance. Please Topup First", 0);

                var courierWallet = context.Wallets.Where(cour => cour.CustomerId == input.CourierId).SingleOrDefault();
                if (courierWallet == null) return new TransactionCreateOutput("Courier wallet Not Found", 0);

                foreach (var seller in input.Sellers)
                {
                    var sellerWallet = context.Wallets.Where(a => a.CustomerId == seller.SellerId).SingleOrDefault();
                    if (sellerWallet == null) return new TransactionCreateOutput("Seller wallet Not Found", 0);
                }

                var transaction = new Transaction
                {
                    BuyerId = input.BuyerId,
                    AmountBuyer = input.AmountBuyer,
                    CourierId = input.CourierId,
                    AmountCourier = input.AmountCourier,
                    TransactionStatus = Status.Paid
                };

                context.Transactions.Add(transaction);

                buyerWallet.Balance -= input.AmountBuyer;
                courierWallet.Balance += input.AmountCourier;
                
                await context.SaveChangesAsync();

                var mutationBuyer = new WalletMutationCreateInput(input.BuyerId, input.AmountBuyer, MutationType.Debit);
                var mutationCourier = new WalletMutationCreateInput(input.CourierId, input.AmountCourier, MutationType.Credit);

                await CreateWalletMutationDebitCredit(context, mutationBuyer);
                await CreateWalletMutationDebitCredit(context, mutationCourier);
                
                var sellers = _mapper.Map<List<Seller>>(input.Sellers);

                foreach(var a in sellers)
                {
                    a.TransactionId = transaction.Id;
                    context.Sellers.Add(a);
                }

                await context.SaveChangesAsync();
                return new TransactionCreateOutput("Success", transaction.Id);
            }
            catch (Exception ex)
            {
                return new TransactionCreateOutput($"Error: {ex.Message}", 0);
            }
        }

        public async Task<TransactionStatus> CreateWalletMutationDebitCredit([Service] AppDbContext context,
                                                                            WalletMutationCreateInput input)
        {
            var customerWallet = await context.Wallets.Where(w => w.CustomerId == input.CustomerId).SingleOrDefaultAsync();

            var walletMutation = new WalletMutation
            {
                WalletId = customerWallet.Id,
                Amount = input.Amount,
                MutationType = input.MutationType,
                CreatedDate = DateTime.Now
            };

            context.WalletMutations.Add(walletMutation);
            await context.SaveChangesAsync();
            return new TransactionStatus(true, "Success");
        }                                                          

        [Authorize(Roles = new [] {"Customer"})]
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