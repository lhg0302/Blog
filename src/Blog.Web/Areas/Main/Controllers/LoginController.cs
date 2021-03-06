﻿using Blog.Common.Auth;
using Blog.Common.Log;
using Blog.Common.Utils;
using Blog.Entities;
using Blog.Entities.Dtos;
using Blog.IServices;
using Blog.Web.Filter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Blog.Web.Areas.Main.Controllers
{
    [ExceptionGlobalFilter]
    [Area("Main")]
    public class LoginController : Controller
    {
        private readonly ISysAccountService _sysAccountService;
        private readonly ISysLoginLogService _sysLoginLogService;
        public LoginController(ISysAccountService sysAccountService, ISysLoginLogService sysLoginLogService)
        {
            _sysAccountService = sysAccountService;
            _sysLoginLogService = sysLoginLogService;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public FileContentResult ValidateCode()
        {
            VerifyCodeUtil code = new VerifyCodeUtil();
            code.SetHeight = 36;
            code.SetForeNoisePointCount = 4;
            var byteImg = code.GetVerifyCodeImage();
            HttpContext.Session.SetString("VerifyCode", code.SetVerifyCodeText);
            return File(byteImg, @"image/jpeg");
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="code">验证码</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Login(string userName, string password, string code)
        {
            OperateResult<string> result = new OperateResult<string>();
            string verifyCode = HttpContext.Session.GetString("VerifyCode");
            if (verifyCode == null)
            {
                result.Message = "验证码已过期";
            }
            else if (code.ToLower() != verifyCode.ToString().ToLower())
            {
                result.Message = "验证码有误";
            }
            else
            {
                //清除验证码
                HttpContext.Session.Remove("VerifyCode");
                var operateResult = _sysAccountService.Login(userName, password);
                AuthorizationUser auth = operateResult.Data;
                if (auth != null)
                {
                    await AuthenticationHelper.SetAuthCookie(auth);
                    result.Status = ResultStatus.Success;
                    result.Data = "/Main/Home/Index";

                    #region 记录登录日志
                    LoginLogHandler loginLog = new LoginLogHandler(auth.LoginId);
                    loginLog.WriteLog();
                    #endregion
                }
                result.Message = operateResult.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> SignOut()
        {
            AuthorizationUser user = AuthenticationHelper.Current();
            if (user != null)
            {
                SysLoginLog log = _sysLoginLogService.FindEntity(user.LoginId);
                if (log != null)
                {
                    log.SignOutTime = DateTime.Now;
                    TimeSpan ts = log.SignOutTime.Value - log.LoginTime;
                    log.StandingTime = ts.TotalMinutes;
                    await _sysLoginLogService.UpdateAsync(log);
                }
            }
            await AuthenticationHelper.SignOut();
            return Json(new OperateResult("退出成功", ResultStatus.SignOut));
        }
    }
}