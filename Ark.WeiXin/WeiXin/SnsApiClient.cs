using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebApi.Models;
using WebApi.WeiXin.Entities;
using WebApi.WeiXin.Sns;

namespace WebApi.WeiXin
{
    public class SnsApiClient : ISnsApiClient
    {
        private readonly WeixinSetting _weixinSetting;
        private readonly IHttpClientFactory _httpClientFactory;

        public SnsApiClient(
            IOptionsMonitor<WeixinSetting> weixinSetting,
            IHttpClientFactory httpClientFactory
        )
        {
            _weixinSetting = weixinSetting.CurrentValue;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JsCode2JsonResult?> JsCode2Json(
            string jsCode,
            string grantType = "authorization_code",
            int timeOut = 10000,
            CancellationToken cancellationToken = default
        )
        {
            string urlFormat = string.Format(
                "{0}/sns/jscode2session?appid={1}&secret={2}&js_code={3}&grant_type={4}",
                _weixinSetting.Website,
                _weixinSetting.WxOpenAppId,
                _weixinSetting.WxOpenAppSecret,
                jsCode,
                grantType
            );

            var httpClient = _httpClientFactory.CreateClient();

            return await httpClient.GetFromJsonAsync<JsCode2JsonResult>(
                urlFormat,
                cancellationToken
            );
        }

        /// <summary>
        /// 【异步方法】获取凭证接口
        /// </summary>
        /// <param name="grant_type">获取access_token填写client_credential</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AccessTokenResult?> GetTokenAsync(
            string grant_type = "client_credential",
            CancellationToken cancellationToken = default
        )
        {
            //注意：此方法不能再使用ApiHandlerWapper.TryCommonApi()，否则会循环
            var urlFormat = string.Format(
                "{0}/cgi-bin/token?grant_type={1}&appid={2}&secret={3}",
                _weixinSetting.Website,
                grant_type,
                _weixinSetting.WxOpenAppId,
                _weixinSetting.WxOpenAppSecret
            );

            var httpClient = _httpClientFactory.CreateClient();

            AccessTokenResult? result = await httpClient.GetFromJsonAsync<AccessTokenResult>(
                urlFormat,
                cancellationToken
            ); //此处为最原始接口，不再使用重试获取的封装

            if (result?.errcode == ReturnCode.请求成功)
            {
                return result;
            }

            return result;
        }

        public async Task<MsgSecCheckResult?> MsgSecCheckAsync(
            string accessToken,
            MsgSecCheck msgSec
        )
        {
            //            var urlFormat = string.Format(
            //    "wxa/msg_sec_check?access_token={0}",
            //    accessToken
            //);

            //        var client = new RestClient(_weixinSetting.Website);
            //        client.AddDefaultHeader("Connection", "keep-alive");
            //        client.AddDefaultHeader("Content-Type", "application/json; charset=UTF-8");
            //        client.AddDefaultHeader("Accept", "application/json, text/plain, */*");
            //        client.AddDefaultHeader("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
            //        client.AddDefaultHeader("Origin", "https://api.weixin.qq.com");

            //        var response = await client.PostJsonAsync<MsgSecCheck, MsgSecCheckResult>(
            //urlFormat, msgSec);

            //return response;

            var urlFormat = string.Format(
    "{0}/wxa/msg_sec_check?access_token={1}",
    _weixinSetting.Website,
    accessToken
);

            var httpClient = _httpClientFactory.CreateClient();
            string strJson = JsonSerializer.Serialize(msgSec, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            //byte[] bytes = Encoding.UTF8.GetBytes(strJson);
            //string utf8_String = Encoding.UTF8.GetString(bytes);


            var itemJson = new StringContent(
                strJson,
                Encoding.UTF8,
                "application/json"
            );



            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );



            ////httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
            ////httpClient.DefaultRequestHeaders.Accept.Add(
            ////    new MediaTypeWithQualityHeaderValue("application/json")
            ////);
            ////httpClient.DefaultRequestHeaders.Accept.Add(
            ////    new MediaTypeWithQualityHeaderValue("text/plain")
            ////);
            ////httpClient.DefaultRequestHeaders.Accept.Add(
            ////    new MediaTypeWithQualityHeaderValue("*/*")
            ////);


            ////httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh-CN"));
            ////httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh"));

            ////httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(""));

            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AcceptLanguage", "zh-CN,zh;q=0.9,en;q=0.8");

            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("ContentType", "application/json");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://api.weixin.qq.com");

            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("UserAgent", "Mozilla/5.0 (Linux; U; Android 10; zh-cn; Redmi K20 Pro Build/QKQ1.190825.002) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/71.0.3578.141 Mobile Safari/537.36 XiaoMi/MiuiBrowser/11.8.14");


            using var response = await httpClient.PostAsync(urlFormat, itemJson);

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MsgSecCheckResult>();
            }
            else
            {
                return new MsgSecCheckResult { };
            }
        }

        ///// <summary>
        ///// 文本内容安全识别
        ///// <para>该接口用于检查一段文本是否含有违法违规内容。</para>
        ///// <para>https://developers.weixin.qq.com/miniprogram/dev/OpenApiDoc/sec-center/sec-check/msgSecCheck.html</para>
        ///// </summary>
        ///// <param name="accessToken">AccessToken</param>
        ///// <param name="openid">用户的openid（用户需在近两小时访问过小程序）</param>
        ///// <param name="content">要检测的文本内容，长度不超过 500KB，编码格式为utf-8</param>
        ///// <param name="version">接口版本号，2.0版本为固定值2</param>
        ///// <param name="scene">场景枚举值（1 资料；2 评论；3 论坛；4 社交日志）</param>
        ///// <param name="title">非必填 文本标题，需使用UTF-8编码</param>
        ///// <param name="nickname">非必填 用户昵称，需使用UTF-8编码</param>
        ///// <param name="signature">非必填 个性签名，该参数仅在资料类场景有效(scene=1)，需使用UTF-8编码</param>
        ///// <param name="timeOut"></param>
        ///// <param name="cancellationToken"></param>
        ///// <returns></returns>
        //public async Task<MsgSecCheckResult?> MsgSecCheckAsync(
        //    string accessToken,
        //    string openid,
        //    string content,
        //    int version = 2,
        //    int scene = 1,
        //    string? title = null,
        //    string? nickname = null,
        //    string? signature = null,
        //    int timeOut = 10000,
        //    CancellationToken cancellationToken = default
        //)
        //{
        //    var urlFormat = string.Format(
        //        "{0}/wxa/msg_sec_check?access_token={1}",
        //        _weixinSetting.Website,
        //        accessToken
        //    );

        //    //var userData = new MsgSecCheck
        //    //{
        //    //    content = content,
        //    //    version = version,
        //    //    scene = scene,
        //    //    openid = openid,
        //    //    //title = title,
        //    //    //nickname = nickname,
        //    //    //signature = signature,
        //    //};


        //    //using (var client = new HttpClient())
        //    //{
        //    //    //client.BaseAddress = new Uri(urlFormat);
        //    //    var request = new HttpRequestMessage(new HttpMethod("Post"), urlFormat);

        //    //    string json = JsonSerializer.Serialize(userData);

        //    //    if (!string.IsNullOrEmpty(json))
        //    //    {
        //    //        request.Content = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded");
        //    //    }
        //    //    var response = await client.SendAsync(request);
        //    //    response.EnsureSuccessStatusCode();
        //    //    return await response.Content.ReadFromJsonAsync<MsgSecCheckResult>();
        //    //}

        //    //using (var httpClient = _httpClientFactory.CreateClient())
        //    //{
        //    //    httpClient.Timeout = new TimeSpan(1, 0, 0, 0, 0);
        //    //    //httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
        //    //    //httpClient.DefaultRequestHeaders.Add("Keep-Alive", "timeout=600");
        //    //    httpClient.DefaultRequestHeaders.Add("ContentType", "application/json; charset=UTF-8");
        //    //    // httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

        //    //    //httpClient.DefaultRequestHeaders.Add(new MediaTypeWithQualityHeaderValue("application/json"));


        //    //    // 将数据对象序列化为JSON字符串
        //    //    byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(userData);

        //    //    //string json = SerializeObjectToJson(userData);

        //    //    //string json = JsonSerializer.Serialize(userData);

        //    //    // 创建HttpContent对象
        //    //    //HttpContent hpContent = new StringContent(json, Encoding.UTF8, "application/json");

        //    //    HttpResponseMessage response = await httpClient.PostAsync(
        //    //        urlFormat,
        //    //        new ByteArrayContent(jsonUtf8Bytes)
        //    //    );




        //    using StringContent jsonContent =
        //        new(
        //            JsonSerializer.Serialize(
        //                new
        //                {
        //                    content = content,
        //                    version = version,
        //                    scene = scene,
        //                    openid = openid
        //                }
        //            ),
        //            Encoding.UTF8,
        //            "application/json"
        //        );

        //    var httpClient = _httpClientFactory.CreateClient();

        //    //httpClient.DefaultRequestHeaders.Add("ContentType", "application/json; charset=UTF-8");
        //    //httpClient.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36 Edg/128.0.0.0");
        //    using HttpResponseMessage response = await httpClient.PostAsync(urlFormat, jsonContent);

        //    //var httpClient = _httpClientFactory.CreateClient();

        //    //using HttpResponseMessage response = await httpClient.PostAsJsonAsync<MsgSecCheck>(urlFormat, new MsgSecCheck
        //    //{
        //    //    content = content,
        //    //    version = version,
        //    //    scene = scene,
        //    //    openid = openid,
        //    //    //title = title,
        //    //    //nickname = nickname,
        //    //    //signature = signature,
        //    //});


        //    response.EnsureSuccessStatusCode();


        //    if (response.IsSuccessStatusCode)
        //    {
        //        return await response.Content.ReadFromJsonAsync<MsgSecCheckResult>();
        //    }
        //    else
        //    {
        //        return new MsgSecCheckResult { };
        //    }
        //    //}
        //}

        //public string SerializeObjectToJson(object obj)
        //{
        //    JsonSerializer serializer = new JsonSerializer();
        //    using (var stream = new MemoryStream())
        //    {
        //        using (var writer = new StreamWriter(stream, Encoding.UTF8))
        //        {
        //            serializer.Serialize(writer, obj);
        //        }
        //        stream.Position = 0;
        //        using (var reader = new StreamReader(stream))
        //        {
        //            return reader.ReadToEnd();
        //        }
        //    }
        //}
    }
}
