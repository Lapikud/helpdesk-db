// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using FreeIPA.DotNet;
using FreeIPA.DotNet.Models;
using FreeIPA.DotNet.Models.Login;
using FreeIPA.DotNet.Models.RPC;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly AppDbContext _context;

        public LoginModel(ILogger<LoginModel> logger,
            AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
                ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
            [Display(Name = nameof(Username), ResourceType = typeof(Base.Resources.Common))]
            public string Username { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
                ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
            [DataType(DataType.Password)]
            [Display(Name = nameof(Password), ResourceType = typeof(Base.Resources.Common))]
            public string Password { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                IpaResultModel<IpaLoginResponseModel> loginResult = null;
                try
                {
                    var ipaClient = new IpaClient("https://ipa.lapikud.ee");
                    loginResult = await ipaClient.LoginWithPassword(new IpaLoginRequestModel()
                    {
                        Username = Input.Username,
                        Password = Input.Password
                    });
                    if (loginResult.Success)
                    {
                        _logger.LogInformation($"User: {Input.Username} logged in.");

                        var user = _context.Users.FirstOrDefault(u => u.Username == Input.Username);
                        var groups = new List<string>();

                        var rpcRequest = new IpaRpcRequestModel
                        {
                            Id = 0,
                            Method = "user_show",
                            Parameters = new object[]
                            {
                                Array.Empty<string>(), new
                                {
                                    uid = Input.Username,
                                    all = false,
                                    raw = false,
                                    rights = true
                                }
                            },
                            Version = "2.251"
                        };

                        var rpcResult = await ipaClient.SendRpcRequest(rpcRequest);

                        if (rpcResult.Success)
                        {
                            // RPC request successful, use the results
                            var response = rpcResult.Data!.Result!;
                            var doc = JsonDocument.Parse(response.ToString()!);
                            var allGroups = doc.RootElement
                                .GetProperty("result")
                                .GetProperty("memberof_group")
                                .EnumerateArray()
                                .Select(e => e.GetString())
                                .ToList();

                            foreach (var group in allGroups)
                            {
                                if (group is "admins" or "members" or "pixels" or "helpdesk_db_admins")
                                {
                                    groups.Add(group);
                                }

                                Console.WriteLine($"RPC response groups: {group}");
                            }
                        }
                        else
                        {
                            // RPC request failed, check the error message
                            var error = rpcResult.Data!.Error;
                            var errorMessage = rpcResult.Message;
                            Console.WriteLine($"RPC request failed: {error}");
                            Console.WriteLine($"RPC request failed message: {errorMessage}");
                        }


                        if (user == null)
                        {
                            user = new AppUser
                            {
                                Id = Guid.NewGuid(),
                                Username = Input.Username,
                            };

                            await _context.Users.AddAsync(user);
                        }
                        
                        if (!_context.Owners.Any(o => o.OwnerName.Equals(user.Username)))
                        {
                            await _context.Owners.AddAsync(new Owner()
                                { OwnerName = user.Username, Comment = "member" });
                        }

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Username!),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        };

                        var existingUserRoles = _context.UserRoles.Where(ur => ur.UserId.Equals(user.Id)).ToList();
                        foreach (var userRole in existingUserRoles)
                        {
                            var role = _context.Roles.First(r => r.Id == userRole.RoleId);

                            if (!groups.Contains(role.Name))
                            {
                                _context.UserRoles.Remove(userRole);
                            }
                        }
                        foreach (var group in groups)
                        {
                            var role = _context.Roles.First(r => r.Name!.Equals(group));
                            if (_context.UserRoles.FirstOrDefault(ur =>
                                    ur.UserId.Equals(user.Id) && ur.RoleId.Equals(role.Id)) == null)
                            {
                                var userRole = new AppUserRole
                                {
                                    Id = Guid.NewGuid(),
                                    UserId = user.Id,
                                    RoleId = role.Id
                                };
                                await _context.UserRoles.AddAsync(userRole);
                            }
                            claims.Add(new Claim(ClaimTypes.Role, group));
                        }

                        await _context.SaveChangesAsync();

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal
                        );

                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, Base.Resources.Errors.IdentityErrors.InvalidLogin);
                        return Page();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}