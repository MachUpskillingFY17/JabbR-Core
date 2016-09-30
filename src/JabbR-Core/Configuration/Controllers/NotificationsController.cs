﻿using System;
using System.Collections.Generic;
using System.Linq;
using JabbR_Core.Infrastructure;
using JabbR_Core.Models;
using JabbR_Core.Services;
using JabbR_Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
//using PagedList;

namespace JabbR_Core.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly InMemoryRepository _repository;
        private IJabbrRepository repository;
        private IChatService chatService;
        private IChatNotificationService notificationService;
        public NotificationsController()
        {
            _repository = new InMemoryRepository();

        }

        [HttpGet]
        public IActionResult Index()
        {
            /*   if (!IsAuthenticated)
               {
                   return Response.AsRedirect(String.Format("~/account/login?returnUrl={0}", HttpUtility.UrlEncode(Request.Path)));
               }

               var request = this.Bind<NotificationRequestModel>();

               ChatUser user = repository.GetUserById(Principal.GetUserId());
               int unreadCount = repository.GetUnreadNotificationsCount(user);
               IPagedList<NotificationViewModel> notifications = GetNotifications(repository, user, all: request.All, page: request.Page, roomName: request.Room);*/

            ChatUser user = _repository.GetUserById("1");

            /*    var viewModel = new NotificationsViewModel
                {
                    ShowAll = request.All,
                    UnreadCount = unreadCount,
                    Notifications = notifications,
                };*/

            //  return View["index", viewModel];

            return View();
        }


        [HttpPost]
        public IActionResult MarkAsRead()
        {
            /*
                if (!IsAuthenticated)
                {
                    return HttpStatusCode.Forbidden;
                }

                int notificationId = Request.Form.notificationId;

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
                repository.CommitChanges();

                UpdateUnreadCountInChat(repository, notificationService, user);

                var response = Response.AsJson(new { success = true });

                return response;*/
            return View("index");
        }

        [HttpPost]
        public IActionResult MarkAllAsRead()
        {/*
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
            };*/
            return View("index");
        }

        private static void UpdateUnreadCountInChat(IJabbrRepository repository, IChatNotificationService notificationService,
                                                    ChatUser user)
        {
            var unread = repository.GetUnreadNotificationsCount(user);
            notificationService.UpdateUnreadMentions(user, unread);
        }

        /*   private static IPagedList<NotificationViewModel> GetNotifications(IJabbrRepository repository, ChatUser user, bool all = false,
                                                                             int page = 1, int take = 20, string roomName = null)
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

               return notificationsQuery.OrderByDescending(n => n.Message.When)
                                        .Select(n => new NotificationViewModel()
                                        {
                                            NotificationKey = n.Key,
                                            FromUserName = n.Message.User.Name,
                                            FromUserImage = n.Message.User.Hash,
                                            Message = n.Message.Content,
                                            HtmlEncoded = n.Message.HtmlEncoded,
                                            RoomName = n.Room.Name,
                                            Read = n.Read,
                                            When = n.Message.When
                                        })
                                        .ToPagedList(page, take);
           }*/

        private class NotificationRequestModel
        {
            public NotificationRequestModel()
            {
                All = false;
                Page = 1;
                Room = null;
            }

            public bool All { get; set; }
            public int Page { get; set; }
            public string Room { get; set; }
        }
    }
}