using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using JabbR_Core.Infrastructure;
using JabbR_Core.Models;
using JabbR_Core.Services;
using JabbR_Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using JabbR_Core.Configuration;
using Microsoft.Extensions.Options;


namespace JabbR_Core.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSettings _settings;
        public AccountController(IOptions<ApplicationSettings> settings,
                             IMembershipService membershipService,
                             //  IJabbrRepository repository,
                             IAuthenticationService authService,
                             IChatNotificationService notificationService,
                             IUserAuthenticator authenticator,
                             IEmailService emailService)
        {
            _settings = settings.Value;
        }
        [HttpGet]
        public IActionResult Index()
        {/*
             if (!IsAuthenticated)
             {
                 return HttpStatusCode.Forbidden;
             }

             ChatUser user = repository.GetUserById(Principal.GetUserId());*/

            //return GetProfileView(authService, user);

            return View();
        }


        [HttpGet("/login")]
        public IActionResult Login()
        {
            /*  if (IsAuthenticated)
              {
                  return this.AsRedirectQueryStringOrDefault("~/");
              }

              return View("login"/*, GetLoginViewModel(applicationSettings, repository, authService));*/
            return View();
        }


        [HttpPost("/login")]
        public IActionResult Login()
        {/*
            if (!HasValidCsrfTokenOrSecHeader)
            {
                return HttpStatusCode.Forbidden;
            }

            if (IsAuthenticated)
            {
                return this.AsRedirectQueryStringOrDefault("~/");
            }

            string username = Request.Form.username;
            string password = Request.Form.password;

            if (String.IsNullOrEmpty(username))
            {
                this.AddValidationError("username", LanguageResources.Authentication_NameRequired);
            }

            if (String.IsNullOrEmpty(password))
            {
                this.AddValidationError("password", LanguageResources.Authentication_PassRequired);
            }

            try
            {
                if (ModelValidationResult.IsValid)
                {
                    IList<Claim> claims;
                    if (authenticator.TryAuthenticateUser(username, password, out claims))
                    {
                        return this.SignIn(claims);
                    }
                }
            }
            catch
            {
                // Swallow the exception    
            }

            this.AddValidationError("_FORM", LanguageResources.Authentication_GenericFailure);

            return View["login", GetLoginViewModel(applicationSettings, repository, authService)];*/
            return View();

        }

        [HttpPost]

        public IActionResult Logout()
        {
            if (!IsAuthenticated)
            {
                return HttpStatusCode.Forbidden;
            }

            var response = true; // Response.AsJson(new { success = true });

            this.SignOut();

            return response;
        }


        [HttpGet]
        public IActionResult Register()
        {
            if (IsAuthenticated)
            {
                return this.AsRedirectQueryStringOrDefault("~/");
            }

            bool requirePassword = !Principal.Identity.IsAuthenticated;

            if (requirePassword &&
                !applicationSettings.AllowUserRegistration)
            {
                return HttpStatusCode.NotFound;
            }

            ViewBag.requirePassword = requirePassword;

            return View["register"];
        }

        [HttpPost]
        public IActionResult Create()
        {
            if (!HasValidCsrfTokenOrSecHeader)
            {
                return HttpStatusCode.Forbidden;
            }

            bool requirePassword = !Principal.Identity.IsAuthenticated;

            if (requirePassword &&
                !applicationSettings.AllowUserRegistration)
            {
                return HttpStatusCode.NotFound;
            }

            if (IsAuthenticated)
            {
                return this.AsRedirectQueryStringOrDefault("~/");
            }

            ViewBag.requirePassword = requirePassword;

            string username = Request.Form.username;
            string email = Request.Form.email;
            string password = Request.Form.password;
            string confirmPassword = Request.Form.confirmPassword;

            if (String.IsNullOrEmpty(username))
            {
                this.AddValidationError("username", LanguageResources.Authentication_NameRequired);
            }

            if (String.IsNullOrEmpty(email))
            {
                this.AddValidationError("email", LanguageResources.Authentication_EmailRequired);
            }

            try
            {
                if (requirePassword)
                {
                    ValidatePassword(password, confirmPassword);
                }

                if (ModelValidationResult.IsValid)
                {
                    if (requirePassword)
                    {
                        ChatUser user = membershipService.AddUser(username, email, password);

                        return this.SignIn(user);
                    }
                    else
                    {
                        // Add the required claims to this identity
                        var identity = Principal.Identity as ClaimsIdentity;

                        if (!Principal.HasClaim(ClaimTypes.Name))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Name, username));
                        }

                        if (!Principal.HasClaim(ClaimTypes.Email))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Email, email));
                        }

                        return this.SignIn(Principal.Claims);
                    }
                }
            }
            catch (Exception ex)
            {
                this.AddValidationError("_FORM", ex.Message);
            }

            return View["register"];
        }


        [HttpPost]
        public IActionResult Unlink()
        {
            if (!HasValidCsrfTokenOrSecHeader)
            {
                return HttpStatusCode.Forbidden;
            }

            if (!IsAuthenticated)
            {
                return HttpStatusCode.Forbidden;
            }

            string provider = Request.Form.provider;
            ChatUser user = repository.GetUserById(Principal.GetUserId());

            if (user.Identities.Count == 1 && !user.HasUserNameAndPasswordCredentials())
            {
                Request.AddAlertMessage("error", LanguageResources.Account_UnlinkRequiresMultipleIdentities);
                return Response.AsRedirect("~/account/#identityProviders");
            }

            var identity = user.Identities.FirstOrDefault(i => i.ProviderName == provider);

            if (identity != null)
            {
                repository.Remove(identity);

                Request.AddAlertMessage("success", String.Format(LanguageResources.Account_UnlinkCompleted, provider));
                return Response.AsRedirect("~/account/#identityProviders");
            }

            return HttpStatusCode.BadRequest;
        }


        [HttpPost]
        public IActionResult NewPassword()
        {
            if (!HasValidCsrfTokenOrSecHeader)
            {
                return HttpStatusCode.Forbidden;
            }

            if (!IsAuthenticated)
            {
                return HttpStatusCode.Forbidden;
            }

            string password = Request.Form.password;
            string confirmPassword = Request.Form.confirmPassword;

            ValidatePassword(password, confirmPassword);

            ChatUser user = repository.GetUserById(Principal.GetUserId());

            try
            {
                if (ModelValidationResult.IsValid)
                {
                    membershipService.SetUserPassword(user, password);
                    repository.CommitChanges();
                }
            }
            catch (Exception ex)
            {
                this.AddValidationError("_FORM", ex.Message);
            }

            if (ModelValidationResult.IsValid)
            {
                Request.AddAlertMessage("success", LanguageResources.Authentication_PassAddSuccess);
                return Response.AsRedirect("~/account/#changePassword");
            }

            return GetProfileView(authService, user);
        }


        [HttpPost]
        public IActionResult changePassword()
        {/*
                if (!HasValidCsrfTokenOrSecHeader)
                {
                    return HttpStatusCode.Forbidden;
                }

                if (!applicationSettings.AllowUserRegistration)
                {
                    return HttpStatusCode.NotFound;
                }

                if (!IsAuthenticated)
                {
                    return HttpStatusCode.Forbidden;
                }

                string oldPassword = Request.Form.oldPassword;
                string password = Request.Form.password;
                string confirmPassword = Request.Form.confirmPassword;

                if (String.IsNullOrEmpty(oldPassword))
                {
                    this.AddValidationError("oldPassword", LanguageResources.Authentication_OldPasswordRequired);
                }

                ValidatePassword(password, confirmPassword);

                ChatUser user = repository.GetUserById(Principal.GetUserId());

                try
                {
                    if (ModelValidationResult.IsValid)
                    {
                        membershipService.ChangeUserPassword(user, oldPassword, password);
                        repository.CommitChanges();
                    }
                }
                catch (Exception ex)
                {
                    this.AddValidationError("_FORM", ex.Message);
                }

                if (ModelValidationResult.IsValid)
                {
                    Request.AddAlertMessage("success", LanguageResources.Authentication_PassChangeSuccess);
                    return Response.AsRedirect("~/account/#changePassword");
                }

                return GetProfileView(authService, user);*/
            return View("index");
        }


        [HttpPost]
        public IActionResult changeUsername()
        {/*
                if (!HasValidCsrfTokenOrSecHeader)
                {
                    return HttpStatusCode.Forbidden;
                }

                if (!IsAuthenticated)
                {
                    return HttpStatusCode.Forbidden;
                }

                string username = Request.Form.username;
                string confirmUsername = Request.Form.confirmUsername;

                ValidateUsername(username, confirmUsername);

                /*ChatUser user = repository.GetUserById(Principal.GetUserId());
                string oldUsername = user.Name;

                try
                {
                    if (ModelValidationResult.IsValid)
                    {
                        membershipService.ChangeUserName(user, username);
                        repository.CommitChanges();
                    }
                }
                catch (Exception ex)
                {
                    this.AddValidationError("_FORM", ex.Message);
                }

                if (ModelValidationResult.IsValid)
                {
                    notificationService.OnUserNameChanged(user, oldUsername, username);

                    Request.AddAlertMessage("success", LanguageResources.Authentication_NameChangeCompleted);
                    return Response.AsRedirect("~/account/#changeUsername");
                }

                return GetProfileView(authService, user);*/
            return View("index");
        }

        
                [HttpGet("/requestresetpassword")]
                public IActionResult requestresetpassword(string id)
                {
                        if (IsAuthenticated)
                        {
                            return Response.AsRedirect("~/account/#changePassword");
                        }

                        if (!Principal.Identity.IsAuthenticated &&
                            !applicationSettings.AllowUserResetPassword ||
                            string.IsNullOrWhiteSpace(applicationSettings.EmailSender))
                        {
                            return HttpStatusCode.NotFound;
                        }
                        // 
                    return Redirect("~/account/#changePassword");
                }

                [HttpPost("/requestresetpassword")]
                public IActionResult requestresetpassword(string id, string password)
                {
                         if (!HasValidCsrfTokenOrSecHeader)
                          {
                              return HttpStatusCode.Forbidden;
                          }

                          if (IsAuthenticated)
                          {
                              return Response.AsRedirect("~/account/#changePassword");
                          }

                          if (!Principal.Identity.IsAuthenticated &&
                              !applicationSettings.AllowUserResetPassword ||
                              string.IsNullOrWhiteSpace(applicationSettings.EmailSender))
                          {
                              return HttpStatusCode.NotFound;
                          }

                          string username = Request.Form.username;

                          if (String.IsNullOrEmpty(username))
                          {
                              this.AddValidationError("username", LanguageResources.Authentication_NameRequired);
                          }

                          try
                          {
                              if (ModelValidationResult.IsValid)
                              {
                                  ChatUser user = repository.GetUserByName(username);

                                  if (user == null)
                                  {
                                      this.AddValidationError("username", String.Format(LanguageResources.Account_NoMatchingUser, username));
                                  }
                                  else if (String.IsNullOrWhiteSpace(user.Email))
                                  {
                                      this.AddValidationError("username", String.Format(LanguageResources.Account_NoEmailForUser, username));
                                  }
                                  else
                                  {
                                      membershipService.RequestResetPassword(user, applicationSettings.RequestResetPasswordValidThroughInHours);
                                      repository.CommitChanges();

                                      emailService.SendRequestResetPassword(user, this.Request.Url.SiteBase + "/account/resetpassword/");

                                      return View["requestresetpasswordsuccess", username];
                                  }
                              }
                          }
                          catch (Exception ex)
                          {
                              this.AddValidationError("_FORM", ex.Message);
                          } //

                    return View("requestresetpasswordsucess");
                }


                [HttpGet]
                public IActionResult resetpassword()
                {
                        if (!applicationSettings.AllowUserResetPassword ||
                            string.IsNullOrWhiteSpace(applicationSettings.EmailSender))
                        {
                            return HttpStatusCode.NotFound;
                        }

                        string resetPasswordToken = parameters.id;
                        string userName = membershipService.GetUserNameFromToken(resetPasswordToken);

                        // Is the token not valid, maybe some character change?
                        if (userName == null)
                        {
                            return View["resetpassworderror", LanguageResources.Account_ResetInvalidToken];
                        }
                        else
                        {
                            ChatUser user = repository.GetUserByRequestResetPasswordId(userName, resetPasswordToken);

                            // Is the token expired?
                            if (user == null)
                            {
                                return View["resetpassworderror", LanguageResources.Account_ResetExpiredToken];
                            }
                            else
                            {
                                return View["resetpassword", user.RequestPasswordResetId];
                            }
                        }
                    return View("resetpassword");
                }


                [HttpPost]
                public IActionResult resetpasswordPost()
                {
                        if (!HasValidCsrfTokenOrSecHeader)
                        {
                            return HttpStatusCode.Forbidden;
                        }

                        if (!applicationSettings.AllowUserResetPassword ||
                            string.IsNullOrWhiteSpace(applicationSettings.EmailSender))
                        {
                            return HttpStatusCode.NotFound;
                        }

                        string resetPasswordToken = parameters.id;
                        string newPassword = Request.Form.password;
                        string confirmNewPassword = Request.Form.confirmPassword;

                        ValidatePassword(newPassword, confirmNewPassword);

                        try
                        {
                            if (ModelValidationResult.IsValid)
                            {
                                string userName = membershipService.GetUserNameFromToken(resetPasswordToken);
                                ChatUser user = repository.GetUserByRequestResetPasswordId(userName, resetPasswordToken);

                                // Is the token expired?
                                if (user == null)
                                {
                                    return View["resetpassworderror", LanguageResources.Account_ResetExpiredToken];
                                }
                                else
                                {
                                    membershipService.ResetUserPassword(user, newPassword);
                                    repository.CommitChanges();

                                    return View["resetpasswordsuccess"];
                                }
                            }
                        }
                        catch (Exception ex)

                            this.AddValidationError("_FORM", ex.Message);
                        }

                        return View["resetpassword", resetPasswordToken];
                   
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
            if (String.IsNullOrEmpty(username))
            {
                //this.AddValidationError("username", LanguageResources.Authentication_NameRequired);
            }

            if (!String.Equals(username, confirmUsername))
            {
                //this.AddValidationError("confirmUsername", LanguageResources.Authentication_NameNonMatching);
            }
        }

        
          private dynamic GetProfileView(IAuthenticationService authService, ChatUser user)
          {
              return View("index", new ProfilePageViewModel(user, authService.GetProviders()));
          }

          private LoginViewModel GetLoginViewModel(ApplicationSettings applicationSettings,
                                                   IJabbrRepository repository,
                                                   IAuthenticationService authService)
          {
              ChatUser user = null;

              if (IsAuthenticated)
              {
                  user = repository.GetUserById(Principal.GetUserId());
              }

              var viewModel = new LoginViewModel(applicationSettings, authService.GetProviders(), user != null ? user.Identities : null);
              return viewModel;
          }

        
    }
}
