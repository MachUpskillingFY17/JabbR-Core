using System;
using System.Collections.Generic;
using System.Linq;
using JabbR_Core.Infrastructure;
using JabbR_Core.Data.Models;
using JabbR_Core.Services;
using JabbR_Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using JabbR_Core.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace JabbR_Core.Controllers
{
    [Authorize()]
    public class NotificationsController : Controller
    {
        private readonly IJabbrRepository _repository;
        private IChatService _chatService;
        private IChatNotificationService _notificationService;
        public List<NotificationViewModel> test;
        private readonly UserManager<ChatUser> _userManager;
        private readonly SignInManager<ChatUser> _signInManager;
        Microsoft.AspNetCore.Http.HttpContext context;
        private ApplicationSettings _settings;
        private IMembershipService _membershipService;

        private IHttpContextAccessor _context;
        public NotificationsController(UserManager<ChatUser> userManager,
            SignInManager<ChatUser> signInManager,
            ApplicationSettings applicationSettings,
            IHttpContextAccessor context,
            IJabbrRepository repository,
            IChatNotificationService notificationService,
        IOptions<ApplicationSettings> settings
            )
        {
            _context = context;
            _settings = applicationSettings;
            _repository = repository;
            _userManager = userManager;
            _signInManager = signInManager;
            _notificationService = notificationService;

        }

        [HttpGet]
        public IActionResult Index()
        {
            
            if (!User.Identity.IsAuthenticated)
            {
                // return Forbidden view
                Response.StatusCode = 403; // HttpStatusCode.Forbidden
                //return this.Redirect("~/Account/Forbidden/");
                return this.Redirect("~/Account/Login/");
            }

            var request = new NotificationRequestModel();

            var id = _context.HttpContext.User.GetUserId();
            ChatUser user = _repository.GetUserById(id);
            int unreadCount = _repository.GetUnreadNotificationsCount(user);
            List<NotificationViewModel> notifications = GetNotifications(_repository, user, all: request.All, page: request.Page, roomName: request.Room);
            var viewModel = new NotificationsViewModel
            {
                ShowAll = request.All,
                UnreadCount = unreadCount,
                Notifications = notifications,
            };


            return View("Index", viewModel);


        }


        [HttpPost]
        public IActionResult MarkAsRead(int notificationId)
        {

            if (!User.Identity.IsAuthenticated)
            {
                // return Forbidden view
                Response.StatusCode = 403; // HttpStatusCode.Forbidden
                //return this.Redirect("~/Account/Forbidden/");
                return this.Redirect("~/Account/Login/");
            }

            Notification notification = _repository.GetNotificationById(notificationId);

            if (notification == null)
            {
                return new NotFoundResult();
            }

            var userId = _context.HttpContext.User.GetUserId();
            ChatUser user = _repository.GetUserById(userId);
            

            if (notification.UserKeyNavigation != user)
            {
                return new UnauthorizedResult();
            }

            notification.Read = true;
            _repository.CommitChanges();

            UpdateUnreadCountInChat(_repository, _notificationService, user);

            return new JsonResult(new { success = true });

        }

          [HttpPost]
          public IActionResult MarkAllAsRead()
          {
            if (!User.Identity.IsAuthenticated)
            {
                // return Forbidden view
                Response.StatusCode = 403; // HttpStatusCode.Forbidden
                //return this.Redirect("~/Account/Forbidden/");
                return this.Redirect("~/Account/Login/");
            }
            var userId = _context.HttpContext.User.GetUserId();
            ChatUser user = _repository.GetUserById(userId);
            IList<Notification> unReadNotifications = _repository.GetNotificationsByUser(user).Unread().ToList();

            if (!unReadNotifications.Any())
            {
                return new NotFoundResult();
            }

            foreach (var notification in unReadNotifications)
            {
                notification.Read = true;
            }

            _repository.CommitChanges();

            UpdateUnreadCountInChat(_repository, _notificationService, user);

            return new JsonResult(new { success = true });

        }

          private static void UpdateUnreadCountInChat(IJabbrRepository _repository, IChatNotificationService notificationService,
                                                      ChatUser user)
          {
              var unread = _repository.GetUnreadNotificationsCount(user);
              notificationService.UpdateUnreadMentions(user, unread);
          }

        private static List<NotificationViewModel> GetNotifications(IJabbrRepository repository, ChatUser user, bool all = false,
                                                                          int page = 1, int take = 20, string roomName = "light_meow")
        {
            IQueryable<Notification> notificationsQuery = repository.GetNotificationsByUser(user);

            if (!all)
            {
                notificationsQuery = notificationsQuery.Unread();
            }

            if (!String.IsNullOrWhiteSpace(roomName))
            {
                var room = repository.VerifyRoom(roomName);

                if (room != null)
                {
                    notificationsQuery = notificationsQuery.ByRoom(roomName);
                }
            }



            return notificationsQuery.OrderByDescending(n => n.MessageKeyNavigation.When)
                                     .Select(n => new NotificationViewModel()
                                     {
                                         NotificationKey = n.Key,
                                         FromUserName = n.MessageKeyNavigation.UserKeyNavigation.Name,
                                         FromUserImage = "Image", 
                                          Message = n.MessageKeyNavigation.Content,
                                         HtmlEncoded = n.MessageKeyNavigation.HtmlEncoded,
                                         RoomName = n.RoomKeyNavigation.Name,
                                         Read = n.Read,
                                         When = n.MessageKeyNavigation.When
                                     })
                                     .ToList();

        }

        private class NotificationRequestModel
        {
            public NotificationRequestModel()
            {
                All = true;
                Page = 1;
                Room = null;
            }

            public bool All { get; set; }
            public int Page { get; set; }
            public string Room { get; set; }
        }

    }
}