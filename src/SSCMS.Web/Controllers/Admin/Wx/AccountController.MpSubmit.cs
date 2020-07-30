﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Wx
{
    public partial class AccountController
    {

        [HttpPost, Route(RouteMp)]
        public async Task<ActionResult<MpSubmitResult>> MpSubmit([FromBody] MpSubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, AuthTypes.SitePermissions.WxAccount))
            {
                return Unauthorized();
            }

            var (success, _, errorMessage) = await _wxManager.GetAccessTokenAsync(request.MpAppId, request.MpAppSecret);

            var account = await _wxAccountRepository.GetBySiteIdAsync(request.SiteId);
            if (success)
            {
                account.MpAppId = request.MpAppId;
                account.MpAppSecret = request.MpAppSecret;

                account.MpUrl = Request.Scheme + "://" + PageUtils.Combine(Request.Host.Host, Constants.ApiWxPrefix, Controllers.Wx.IndexController.Route.Replace("{siteId}", request.SiteId.ToString()));

                if (string.IsNullOrEmpty(account.MpToken))
                {
                    account.MpToken = StringUtils.GetRandomString(32);
                }
                if (string.IsNullOrEmpty(account.MpEncodingAESKey))
                {
                    account.MpIsEncrypt = true;
                    account.MpEncodingAESKey = StringUtils.GetRandomString(43);
                }

                await _wxAccountRepository.SetAsync(account);
                await _authManager.AddAdminLogAsync("修改微信公众号设置");
            }

            return new MpSubmitResult
            {
                Success = success,
                ErrorMessage = errorMessage,
                Account = account
            };
        }
    }
}
