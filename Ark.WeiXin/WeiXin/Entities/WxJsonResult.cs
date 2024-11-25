namespace WebApi.WeiXin.Entities
{
    //
    // 摘要:
    //     公众号 JSON 返回结果（用于菜单接口等），子类必须具有不带参数的构造函数
    [Serializable]
    public class WxJsonResult : BaseJsonResult
    {
        public ReturnCode errcode { get; set; }

        //
        // 摘要:
        //     返回消息代码数字（同errcode枚举值）
        public override int ErrorCodeValue => (int)errcode;

        //
        // 摘要:
        //     无参数的构造函数
        public WxJsonResult()
        {
        }

        public override string ToString()
        {
            return $"WxJsonResult：{{errcode:'{(int)errcode}',errcode_name:'{errcode.ToString()}',errmsg:'{errmsg}'}}";
        }
    }
}
