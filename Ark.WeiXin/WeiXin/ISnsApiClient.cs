using WebApi.WeiXin.Entities;
using WebApi.WeiXin.Sns;

namespace WebApi.WeiXin
{
    public interface ISnsApiClient
    {
        Task<JsCode2JsonResult?> JsCode2Json(
            string jsCode,
            string grantType = "authorization_code",
            int timeOut = 10000,
            CancellationToken cancellationToken = default
        );

        Task<AccessTokenResult?> GetTokenAsync(
            string grant_type = "client_credential",
            CancellationToken cancellationToken = default
        );

        Task<MsgSecCheckResult?> MsgSecCheckAsync(string accessToken, MsgSecCheck msgSec);

        //Task<MsgSecCheckResult?> MsgSecCheckAsync(
        //    string accessToken,
        //    string openid,
        //    string content,
        //    int version = 2,
        //    int scene = 3,
        //    string? title = null,
        //    string? nickname = null,
        //    string? signature = null,
        //    int timeOut = 10000,
        //    CancellationToken cancellationToken = default
        //);
    }
}
