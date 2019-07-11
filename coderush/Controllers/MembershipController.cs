using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coderush.Data;
using coderush.Models;
using coderush.Services.Security;
using coderush.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace coderush.Controllers
{
    [Authorize(Roles = Services.App.Pages.Membership.RoleName)]
    public class MembershipController : Controller
    {
        private readonly Services.Security.ICommon _security;
        private readonly IdentityDefaultOptions _identityDefaultOptions;
        private readonly SuperAdminDefaultOptions _superAdminDefaultOptions;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        //dependency injection through constructor, to directly access services
        public MembershipController(
            Services.Security.ICommon security,
            IOptions<IdentityDefaultOptions> identityDefaultOptions,
            IOptions<SuperAdminDefaultOptions> superAdminDefaultOptions,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager
            )
        {
            _security = security;
            _identityDefaultOptions = identityDefaultOptions.Value;
            _superAdminDefaultOptions = superAdminDefaultOptions.Value;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {            
            List<ApplicationUser> users = new List<ApplicationUser>();
            users = _security.GetAllMembers();
            return View(users);
        }

        //display change profile screen if member founded, otherwise 404
        [HttpGet]
        public IActionResult ChangeProfile(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            IdentityUser appUser = new IdentityUser();
            appUser = _security.GetMemberByApplicationId(id);

            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        //post submited change profile request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitChangeProfile([Bind("Id,EmailConfirmed,Email,PhoneNumber")] ApplicationUser applicationUser)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[StaticString.StatusMessage] = "Error: Model state not valid.";
                    return RedirectToAction(nameof(ChangeProfile), new { id = applicationUser.Id });
                }

                ApplicationUser updatedUser = new ApplicationUser();
                updatedUser = _security.GetMemberByApplicationId(applicationUser.Id);
                if (updatedUser == null)
                {
                    TempData[StaticString.StatusMessage] = "Error: Can not found the member.";
                    return RedirectToAction(nameof(ChangeProfile), new { id = applicationUser.Id });
                }

                if (_identityDefaultOptions.IsDemo && _superAdminDefaultOptions.Email.Equals(applicationUser.Email))
                {
                    TempData[StaticString.StatusMessage] = "Error: Demo mode can not change super@admin.com data.";
                    return RedirectToAction(nameof(ChangeProfile), new { id = applicationUser.Id });
                }                

                updatedUser.Email = applicationUser.Email;
                updatedUser.PhoneNumber = applicationUser.PhoneNumber;
                updatedUser.EmailConfirmed = applicationUser.EmailConfirmed;

                _context.Update(updatedUser);
                await _context.SaveChangesAsync();

                TempData[StaticString.StatusMessage] = "Update success";
                return RedirectToAction(nameof(ChangeProfile), new { id = updatedUser.Id});
            }
            catch (Exception ex)
            {
                TempData[StaticString.StatusMessage] = "Error: " + ex.Message;
                return RedirectToAction(nameof(ChangeProfile), new { id = applicationUser.Id });
            }
            
        }

        //display change password screen if user founded, otherwise 404
        [HttpGet]
        public IActionResult ChangePassword(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = _security.GetMemberByApplicationId(id);
            if (member == null)
            {
                return NotFound();
            }

            ResetPassword cp = new ResetPassword();
            cp.Id = id;
            cp.UserName = member.UserName;

            return View(cp);
        }

        //post submitted change password request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitChangePassword([Bind("Id,OldPassword,NewPassword,ConfirmPassword")] ResetPassword changePassword)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[StaticString.StatusMessage] = "Error: Model state not valid.";
                    return RedirectToAction(nameof(ChangePassword), new { id = changePassword.Id });
                }

                var member = _security.GetMemberByApplicationId(changePassword.Id);

                if (member == null)
                {
                    TempData[StaticString.StatusMessage] = "Error: Can not found the member.";
                    return RedirectToAction(nameof(ChangePassword), new { id = changePassword.Id });
                }

                if (_identityDefaultOptions.IsDemo && _superAdminDefaultOptions.Email.Equals(member.Email))
                {
                    TempData[StaticString.StatusMessage] = "Error: Demo mode can not change super@admin.com data.";
                    return RedirectToAction(nameof(ChangePassword), new { id = changePassword.Id });
                }
                var tokenResetPassword = await _userManager.GeneratePasswordResetTokenAsync(member);
                var changePasswordResult = await _userManager.ResetPasswordAsync(member, tokenResetPassword, changePassword.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    TempData[StaticString.StatusMessage] = "Error: ";
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        TempData[StaticString.StatusMessage] = TempData[StaticString.StatusMessage] + " " + error.Description;
                    }
                    return RedirectToAction(nameof(ChangePassword), new { id = changePassword.Id });
                }

                TempData[StaticString.StatusMessage] = "Reset password success";
                return RedirectToAction(nameof(ChangePassword), new { id = changePassword.Id });
            }
            catch (Exception ex)
            {
                TempData[StaticString.StatusMessage] = "Error: " + ex.Message;
                return RedirectToAction(nameof(ChangePassword), new { id = changePassword.Id });
            }

        }

        //display change role screen if user founded, otherwise 404
        [HttpGet]
        public async Task<IActionResult> ChangeRole(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = _security.GetMemberByApplicationId(id);
            if (member == null)
            {
                return NotFound();
            }
            
            var registeredRoles = await _userManager.GetRolesAsync(member);            

            ChangeRoles changeRole = new ChangeRoles();
            changeRole.Id = id;
            changeRole.UserName = member.UserName;
            changeRole.IsTodoRegistered = registeredRoles.Contains("Todo") ? true : false;
            changeRole.IsMembershipRegistered = registeredRoles.Contains("Membership") ? true : false;
            changeRole.IsRoleRegistered = registeredRoles.Contains("Role") ? true : false;

            return View(changeRole);
        }

        //post submitted change role request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitChangeRole([Bind("Id", "IsTodoRegistered", "IsMembershipRegistered", "IsRoleRegistered")]ChangeRoles changeRoles)
        {
            try
            {                

                if (!ModelState.IsValid)
                {
                    TempData[StaticString.StatusMessage] = "Error: Model state not valid.";
                    return RedirectToAction(nameof(ChangeRole), new { id = changeRoles.Id });
                }

                var member = _security.GetMemberByApplicationId(changeRoles.Id);
                if (member == null)
                {
                    TempData[StaticString.StatusMessage] = "Error: Can not found the member.";
                    return RedirectToAction(nameof(ChangeRole), new { id = changeRoles.Id });
                }

                if (_identityDefaultOptions.IsDemo && _superAdminDefaultOptions.Email.Equals(member.Email))
                {
                    TempData[StaticString.StatusMessage] = "Error: Demo mode can not change super@admin.com data.";
                    return RedirectToAction(nameof(ChangeRole), new { id = changeRoles.Id });
                }

                //todo role
                if (changeRoles.IsTodoRegistered)
                {
                    await _userManager.AddToRoleAsync(member, "Todo");
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(member, "Todo");
                }

                //membership role
                if (changeRoles.IsMembershipRegistered)
                {
                    await _userManager.AddToRoleAsync(member, "Membership");
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(member, "Membership");
                }

                //role role
                if (changeRoles.IsRoleRegistered)
                {
                    await _userManager.AddToRoleAsync(member, "Role");
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(member, "Role");
                }

                TempData[StaticString.StatusMessage] = "Update success";
                return RedirectToAction(nameof(ChangeRole), new { id = changeRoles.Id });
            }
            catch (Exception ex)
            {
                TempData[StaticString.StatusMessage] = "Error: " + ex.Message;
                return RedirectToAction(nameof(ChangeRole), new { id = changeRoles.Id });
            }

        }

        //display member registration screen
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        //post submitted registration request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRegister([Bind("EmailConfirmed,Email,PhoneNumber,Password,ConfirmPassword")] Register register)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[StaticString.StatusMessage] = "Error: Model state not valid.";
                    return RedirectToAction(nameof(Register));
                }

                ApplicationUser newMember = new ApplicationUser();
                newMember.Email = register.Email;
                newMember.UserName = register.Email;
                newMember.PhoneNumber = register.PhoneNumber;
                newMember.EmailConfirmed = register.EmailConfirmed;
                newMember.isSuperAdmin = false;

                var result = await _userManager.CreateAsync(newMember, register.Password);
                if (result.Succeeded)
                {
                    TempData[StaticString.StatusMessage] = "Register new member success";
                    return RedirectToAction(nameof(ChangeProfile), new { id = newMember.Id });
                }
                else
                {
                    TempData[StaticString.StatusMessage] = "Error: Register new member not success";
                    return RedirectToAction(nameof(Register));
                }
                
            }
            catch (Exception ex)
            {
                TempData[StaticString.StatusMessage] = "Error: " + ex.Message;
                return RedirectToAction(nameof(Register));
            }

        }

    }
}