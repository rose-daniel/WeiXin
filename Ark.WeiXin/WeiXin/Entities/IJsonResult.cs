namespace WebApi.WeiXin.Entities
{
    //
    // 摘要:
    //     所有 JSON 格式返回值的API返回结果接口
    public interface IJsonResult
    {
        //
        // 摘要:
        //     返回结果信息
        string errmsg { get; set; }

        //
        // 摘要:
        //     errcode的
        int ErrorCodeValue { get; }

        object P2PData { get; set; }
    }
}
