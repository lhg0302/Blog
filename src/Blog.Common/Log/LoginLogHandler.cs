﻿using Blog.Common.Auth;
using Blog.Common.Net;
using System;

namespace Blog.Common.Log
{
    /// <summary>
    /// 系统用户登录日志处理
    /// </summary>
    public class LoginLogHandler : LogHandler<LoginLog>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loginId">登录id</param>
        public LoginLogHandler(string loginId) : base(LogMode.LoginLog)
        {
            AuthorizationUser currentUser = null;
            var current = HttpContextHelper.Current;
            if (current != null)
            {
                currentUser = AuthenticationHelper.Current();
            }
            if (currentUser == null)
            {
                currentUser = new AuthorizationUser()
                {
                    RealName = "匿名用户"
                };
            }
            var request = current.Request;
            LogInfo = new LoginLog
            {
                LoginId = loginId,
                CreateAccountId = currentUser.AccountId,
                ServerHost = HttpHelper.GetServerIp(),
                ClientHost = HttpHelper.GetClientIp(),
                UserAgent = HttpHelper.UserAgent(),
                OsVersion = HttpHelper.GetOsVersion(),
                LoginTime = DateTime.Now,
                IpAddressName = HttpHelper.GetAddressByApi()
            };

        }
    }
}
