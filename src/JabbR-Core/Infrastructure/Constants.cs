﻿using System;
namespace JabbR_Core.Infrastructure
{
    public static class Constants
    {
        public static readonly string AuthResultCookie = "jabbr.authResult";
        //public static readonly Version JabbRVersion = typeof(Constants).Assembly.GetName().Version;
        public static readonly string JabbRAuthType = "JabbR";
    }

    public static class JabbRClaimTypes
    {
        public const string Identifier = "urn:jabbr:id";
        public const string Admin = "urn:jabbr:admin";
        public const string PartialIdentity = "urn:jabbr:partialid";
    }

    public static class AcsClaimTypes
    {
        public static readonly string IdentityProvider = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/IdentityProvider";
    }

    public static class ContentTypes
    {
        public const string Html = "text/html";
        public const string Text = "text/plain";
    }
}