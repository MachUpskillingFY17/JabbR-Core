using System;
using System.Net;
using System.Linq;
using System.Text;
using JabbR_Core.Services;
using JabbR_Core.ViewModels;
using System.Threading.Tasks;
using JabbR_Core.Data.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using JabbR_Core.Data.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace JabbR_Core.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // private IAuthenticationService _authService;
        private ApplicationSettings _settings;
        private IMembershipService _membershipService;

        private readonly IJabbrRepository _repository;
        private readonly IEmailSender _emailSender;

        private readonly UserManager<ChatUser> _userManager;
        private readonly SignInManager<ChatUser> _signInManager;
        Microsoft.AspNetCore.Http.HttpContext context;

        private IHttpContextAccessor _context;

        public AccountController(
            UserManager<ChatUser> userManager,
            SignInManager<ChatUser> signInManager,
            ApplicationSettings applicationSettings,
            IHttpContextAccessor context,
            IJabbrRepository repository,
            IOptions<ApplicationSettings> settings,
            IEmailSender emailsender

            // IOptions<ApplicationSettings> settings,
            // IMembershipService membershipService,
            // IAuthenticationService authService
            // IChatNotificationService notificationService,
            // IUserAuthenticator authenticator,
            // IEmailService emailService
            )
        {
            // _settings = settings.Value;
            // _authService = authService;
            // _membershipService = membershipService;

            _context = context;
            _settings = applicationSettings;
            _repository = repository;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailsender;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index(ManageMessageId? message = null, string otherMessages = "")
        {
            /*This is for the error messages to be on the page*/
            ViewData["StatusMessage"] =
              message == ManageMessageId.ChangeUsernameSuccess ? "Your username has been changed."
               : message == ManageMessageId.Error ? "An error has occurred."
               : message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
               : message == ManageMessageId.ChangePasswordFailure ? "Failure to change password."
               : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
               : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
               : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
               : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
               : message == ManageMessageId.ErrorChangeUsername ? "Your username was not changed. Please be sure you entered the correct information"
               : message == ManageMessageId.WrongPassword ? "You entered the wrong password. Please try again"
               : message == ManageMessageId.CustomMessage ? otherMessages
               : "";

            /*regular index code*/
            if (!User.Identity.IsAuthenticated)
            {
                // return Forbidden view
                Response.StatusCode = 403; // HttpStatusCode.Forbidden
                //return this.Redirect("~/Account/Forbidden/");
                return this.Redirect("~/Account/Login/");

            }

            // HttpContextAccessor DI works when Singelton (Scoped injects null)
            var id = _context.HttpContext.User.GetUserId();
            ChatUser user = _repository.GetUserById(id);

            return GetProfileView(user);
        }

        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }

        //
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            // check if the user IsAuthenticated
            if (User.Identity.IsAuthenticated)
            {
                // if so, no reason to login. Redirect to home page.
                return this.Redirect("~/");
            }
            return View(GetLoginViewModel(_settings, _repository/*, authService*/));
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // check if the user IsAuthenticated
            if (User.Identity.IsAuthenticated)
            {
                // if so, no reason to login. Redirect to home page.
                return this.Redirect("~/");
            }

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                // 3rd paramater (isPersisted:) holds cookie after browser is closed
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // user logged in
                    // Redirect to home page - Lobby
                    return this.Redirect("~/");
                }
                if (result.RequiresTwoFactor)
                {
                    // TODO: Future implemntation
                    // return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    // user account locked out
                    // TODO: Fiture implemtation of Lockout View
                    // return View("Lockout");
                    return View(GetLoginViewModel(_settings, _repository/*, authService*/));
                }
                else
                {
                    // If we got this far, something failed, redisplay form
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(GetLoginViewModel(_settings, _repository/*, authService*/));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(GetLoginViewModel(_settings, _repository/*, authService*/));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();

            // redirect to AccountLogin since you are no longer authenticated
            return this.Redirect("~/account/login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("~/");
            }

            if (!_settings.AllowUserRegistration)
            {
                return View(HttpStatusCode.NotFound);
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View("register");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {

                if (!_settings.AllowUserRegistration)
                {
                    return View(HttpStatusCode.NotFound);
                }

                if (User.Identity.IsAuthenticated)
                {
                    return Redirect("~/");
                }

                try
                {
                    var user = new ChatUser { Name = model.Name, UserName = model.Name, Email = model.Email, LastActivity = DateTime.UtcNow };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddClaimsAsync(user, new List<Claim>() { new Claim(JabbRClaimTypes.Identifier, user.Id) });
                        // Send an email with this link
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                        await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                            $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                    AddErrors(result);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Unlink()
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return View(HttpStatusCode.Forbidden);
        //    }

        //    string provider = Request.Form.provider;
        //    ChatUser user = _repository.GetUserById(_context.HttpContext.User.GetUserId());

        //    if (user.ChatUserIdentities.Count == 1 && !user.HasUserNameAndPasswordCredentials())
        //    {
        //        Request.AddAlertMessage("error", LanguageResources.Account_UnlinkRequiresMultipleIdentities);
        //        return Redirect("~/account/#identityProviders");
        //    }

        //    var identity = user.ChatUserIdentities.FirstOrDefault(i => i.ProviderName == provider);

        //    if (identity != null)
        //    {
        //        _repository.Remove(identity);

        //        Request.AddAlertMessage("success", String.Format(LanguageResources.Account_UnlinkCompleted, provider));
        //        return Redirect("~/account/#identityProviders");
        //    }

        //    return View(HttpStatusCode.BadRequest);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)//string oldPassword, string password, string confirmPassword)
        {
            // check if the user IsAuthenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Don't allow if not authenticated
                return View(HttpStatusCode.Forbidden);

            }
            //Check if user filled out form corrrectly
            try
            {
                if (ModelState.IsValid)
                {
                    // HttpContextAccessor DI works when Singelton (Scoped injects null)
                    var id = _context.HttpContext.User.GetUserId();
                    ChatUser actualUser = _repository.GetUserById(id);

                    //var actualUser = await _userManager.FindByNameAsync(User.Identity.Name);
                    if (actualUser == null)
                    {
                        return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
                    }

                    //string forceToken = await _userManager.GenerateChangeEmailTokenAsync(actualUser, null);
                    var result = await _userManager.ChangePasswordAsync(actualUser, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return new RedirectResult(Url.Action("Index", new { Message = ManageMessageId.ChangePasswordSuccess }) + "#changePassword");
                    }
                    AddErrors(result);
                }
                else
                {
                    return new RedirectResult(Url.Action("Index", new { Message = ManageMessageId.ChangePasswordFailure }) + "#changePassword");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
            }

            //If we got this far something's wrong
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });//, errors });

            //ModelState.AddModelError(string.Empty, "Error changing password.");
            //return GetProfileView(/*_authService, */user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUsername(ChangeUsernameViewModel model)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return View(HttpStatusCode.Forbidden);
            }

            // HttpContextAccessor DI works when Singelton (Scoped injects null)
            var id = _context.HttpContext.User.GetUserId();

            ChatUser user = _repository.GetUserById(id);
            string oldUsername = user.Name; //user.UserName

            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.password, false);
                    if (result.Succeeded)
                    {
                        user.UserName = model.username;
                        user.Name = model.username;
                        _repository.CommitChanges();

                        //  _notificationService.OnUserNameChanged(user, oldUsername, model.username);

                        return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangeUsernameSuccess });
                    }
                    else
                        return new RedirectResult(Url.Action("Index", new { Message = ManageMessageId.WrongPassword }) + "#changeUsername");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
            }

            return new RedirectResult(Url.Action("Index", new { Message = ManageMessageId.ErrorChangeUsername }) + "#changeUsername");

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ChatUser { Name = model.Name, UserName = model.Name, Email = model.Email, LastActivity = DateTime.UtcNow };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _userManager.AddClaimsAsync(user, new List<Claim>() { new Claim(JabbRClaimTypes.Identifier, user.Id) });
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private void ValidatePassword(string password, string confirmPassword)
        {
            if (String.IsNullOrEmpty(password))
            {
                //this.AddValidationError("password", LanguageResources.Authentication_PassRequired);
            }

            if (!String.Equals(password, confirmPassword))
            {
                //this.AddValidationError("confirmPassword", LanguageResources.Authentication_PassNonMatching);
            }
        }

        private void ValidateUsername(string username, string confirmUsername)
        {
            /*  if (String.IsNullOrEmpty(username))
          {
              this.AddValidationError("confirmUsername", LanguageResources.Authentication_NameNonMatching);
          }*/

            var changeUsername = new RegularExpressionAttribute(username);
            if (!changeUsername.IsValid(confirmUsername)) //(String.IsNullOrEmpty(username))
            {
                changeUsername.FormatErrorMessage(LanguageResources.Authentication_NameNonMatching);
                // IsValid("usernam")
                //this.AddValidationError("username", LanguageResources.Authentication_NameRequired);
            }
        }

        private dynamic GetProfileView(/*IAuthenticationService authService,*/ ChatUser user)
        {
            return View(new ProfilePageViewModel(user/*, authService.GetProviders()*/));
        }

        private LoginViewModel GetLoginViewModel(ApplicationSettings applicationSettings,
                                                 IJabbrRepository repository)
        {
            ChatUser user = null;

            if (User.Identity.IsAuthenticated)
            {
                var id = _context.HttpContext.User.GetUserId();
                user = _repository.GetUserById(id);
            }

            var viewModel = new LoginViewModel(applicationSettings, /*authService.GetProviders(),*/ user != null ? user.ChatUserIdentities : null);
            return viewModel;
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private String AddErrors(IdentityResult result)
        {
            var sb = new StringBuilder();
            foreach (var error in result.Errors)
            {
                //Need ModelState validation for the register page
                ModelState.AddModelError(string.Empty, error.Description);

                //As of now we're not using full views only partials for account controller, so 
                //we'd lose modelstate on redirect to the Index view to show the errors, since we 
                //have to return the Index view in order to see any of the partials like ChangePassword
                sb.Append(error.Description);
                sb.Append("<br />");
            }

            return sb.ToString();
        }

        public enum ManageMessageId
        {
            ChangeUsernameSuccess,
            Error,
            ChangePasswordSuccess,
            ChangePasswordFailure,
            SetPasswordSuccess,
            SetTwoFactorSuccess,
            AddPhoneSuccess,
            RemovePhoneSuccess,
            AddLoginSuccess,
            RemoveLoginSuccess,
            ErrorChangeUsername,
            WrongPassword,
            CustomMessage

        }
    }
}
