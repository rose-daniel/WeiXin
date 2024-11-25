using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Voy.Toolkit.Common.Models;
using Voy.Toolkit.Common.Utils;
using WebApi.WeiXin;
using WebApi.WeiXin.Entities;

namespace Ark.WeiXin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISnsApiClient _snsApiClient;

        public AuthController(ISnsApiClient snsApiClient) { _snsApiClient = snsApiClient; }

        [HttpGet("WxLogin")]
        [AllowAnonymous]
        public async Task<ResponseResult> WxLogin(string jsCode = "")
        {
            var jsonResult = await _snsApiClient.JsCode2Json(jsCode);

            if(jsonResult != null && jsonResult.errcode == ReturnCode.请求成功)
            {
                return new ResponseResult { Data = jsonResult.openid };
            }

            return new ResponseResult { Status = ResultStatus.Fail };
        }

        [HttpGet("WxToken")]
        [AllowAnonymous]
        public async Task<ResponseResult> WxToken()
        {
            var jsonResult = await _snsApiClient.GetTokenAsync();

            if(jsonResult != null && jsonResult.errcode == ReturnCode.请求成功)
            {
                return new ResponseResult { Data = jsonResult.access_token };
            }

            return new ResponseResult { Status = ResultStatus.Fail };
        }


        /// <summary>
        /// 文本内容安全识别
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("MsgSecCheck")]
        [AllowAnonymous]
        public async Task<ResponseResult> MsgSecCheckAsync([FromBody] MsgSecCheckParam param)
        {
            if(string.IsNullOrWhiteSpace(param.accessToken))
            {
                return new ResponseResult { Status = ResultStatus.Error, Message = "参数错误#1" };
            }

            if(string.IsNullOrWhiteSpace(param.openid))
            {
                return new ResponseResult { Status = ResultStatus.Error, Message = "参数错误#2" };
            }

            if(string.IsNullOrWhiteSpace(param.content))
            {
                return new ResponseResult { Status = ResultStatus.Fail, Message = "请输入帖子内容" };
            }
            string content = param.content.Trim();

            byte[] bytes = Encoding.UTF8.GetBytes(content);
            int byteCount = bytes.Length;
            if(byteCount > 500 * 1024) // 500KB
            {
                return new ResponseResult { Status = ResultStatus.Fail, Message = "帖子内容，长度不能超过 500KB" };
            }

            //string inputString = System.Text.Encoding.UTF8.GetString(bytes);

            var jsonResult = await _snsApiClient.MsgSecCheckAsync(
                param.accessToken,
                new MsgSecCheck { content = content, openid = param.openid, scene = 1, version = 2 });

            if(jsonResult != null && jsonResult.errcode == ReturnCode.请求成功)
            {
                if(jsonResult.result.label == 100)
                {
                    return new ResponseResult { Data = jsonResult.result.label };
                } else
                {
                    return new ResponseResult { Message = "发布失败，帖子内容中存在敏感关键词,请编辑内容重新发布", Status = ResultStatus.Fail };

                    //string? labelName = Enum.GetName(typeof(LabelCode), jsonResult.result.label);

                    //return new ResponseResult
                    //{
                    //    Data = jsonResult.detail,
                    //    Message = "存在[" + labelName + "]信息",
                    //    Status = ResultStatus.Fail
                    //};
                }
            } else
            {
                return new ResponseResult { Message = "请求失败", Status = ResultStatus.Fail };
            }
        }
    }
}
