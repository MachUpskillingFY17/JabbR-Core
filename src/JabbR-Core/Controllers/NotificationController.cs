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

namespace JabbR_Core.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly IJabbrRepository _repository;
        private IChatService chatService;
        private IChatNotificationService notificationService;
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
            IOptions<ApplicationSettings> settings
            )
        {
            test = new List<NotificationViewModel>();
            _context = context;
            _settings = applicationSettings;
            _repository = repository;
            _userManager = userManager;
            _signInManager = signInManager;

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


            return View("index", viewModel);


        }


        [HttpPost]
        public IActionResult MarkAsRead()//(int num)
        {

            if (!User.Identity.IsAuthenticated)
            {
                // return Forbidden view
                Response.StatusCode = 403; // HttpStatusCode.Forbidden
                //return this.Redirect("~/Account/Forbidden/");
                return this.Redirect("~/Account/Login/");
            }

          /*  int notificationId = Request.Form.notificationId;

            Notification notification = repository.GetNotificationById(notificationId);

            if (notification == null)
            {
                return HttpStatusCode.NotFound;
            }

            ChatUser user = repository.GetUserById(Principal.GetUserId());

            if (notification.UserKey != user.Key)
            {
                return HttpStatusCode.Forbidden;
            }

            notification.Read = true;
            //      _repository.CommitChanges();

            UpdateUnreadCountInChat(_repository, notificationService, user);

            //    var response = Response.AsJson(new { success = true });

            // return response;*/
            return View("index");
        }

        /*  [HttpPost]
          public IActionResult MarkAllAsRead()
          {
                  if (!IsAuthenticated)
                  {
                      return HttpStatusCode.Forbidden;
                  }

                  ChatUser user = repository.GetUserById(Principal.GetUserId());
                  IList<Notification> unReadNotifications = repository.GetNotificationsByUser(user).Unread().ToList();

                  if (!unReadNotifications.Any())
                  {
                      return HttpStatusCode.NotFound;
                  }

                  foreach (var notification in unReadNotifications)
                  {
                      notification.Read = true;
                  }

                  repository.CommitChanges();

                  UpdateUnreadCountInChat(repository, notificationService, user);

                  var response = Response.AsJson(new { success = true });

                  return response;
              };

              ChatUser user = _repository.GetUserById("1");
              IList<Notification> unReadNotifications = _repository.GetNotificationsByUser(user).Unread().ToList();

              foreach (var notification in unReadNotifications)
              {
                  notification.Read = true;
              }

              repository.CommitChanges();

              UpdateUnreadCountInChat(_repository, notificationService, user);

              //added this
              var viewModel = new NotificationsViewModel
              {
                  ShowAll = false,//request.All
                  UnreadCount = 0,
                  //  TotalCount = 2, //was not here
                  Notifications = null //notifications
              };

              return View("index", viewModel);
          }

          private static void UpdateUnreadCountInChat(InMemoryRepository repository, IChatNotificationService notificationService,
                                                      ChatUser user)
          {
              var unread = repository.GetUnreadNotificationsCount(user);
              notificationService.UpdateUnreadMentions(user, unread);
          }*/

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
                                         FromUserImage = "Image", //n.Message.User.Hash,
                                          Message = n.MessageKeyNavigation.Content,
                                         HtmlEncoded = n.MessageKeyNavigation.HtmlEncoded,
                                         RoomName = n.RoomKeyNavigation.Name,
                                         Read = n.Read,
                                         When = n.MessageKeyNavigation.When
                                     })
                                     .ToList();

            //To test comment out the first return statement and uncomment the one below, so you have dummy notifications. For testing index
            //You should just be seeing the difference between Unread Notifications and All Notifications Tab. No functionality in this branch

            /*   var vm = new NotificationViewModel()
               {
                   NotificationKey = 1,
                   FromUserName = "Jack",
                   FromUserImage = "Image",
                   Message = "This is the unread test message",
                   HtmlEncoded = true,
                   RoomName = "light-meow",
                   Read = false,
                   When = DateTimeOffset.Now
               };

               var vm2 = new NotificationViewModel()
               {
                   NotificationKey = 2,
                   FromUserName = "Jack",
                   FromUserImage = "Image",
                   Message = "This is the read test message ",
                   HtmlEncoded = true,
                   RoomName = "light-meow",
                   Read = true,
                   When = DateTimeOffset.Now
               };
               List<NotificationViewModel> notifications = new List<NotificationViewModel>();

               notifications.Add(vm);
               notifications.Add(vm2);

               return notifications;*/
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