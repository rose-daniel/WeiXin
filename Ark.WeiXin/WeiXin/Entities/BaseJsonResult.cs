namespace WebApi.WeiXin.Entities
{
    //
    // 摘要:
    //     WxJsonResult 等 Json 结果的基类（抽象类），子类必须具有不带参数的构造函数
    [Serializable]
    public abstract class BaseJsonResult : IJsonResult
    {
        //
        // 摘要:
        //     返回结果信息
        public virtual string errmsg { get; set; }

        //
        // 摘要:
        //     errcode的
        public abstract int ErrorCodeValue { get; }

        public virtual object P2PData { get; set; }
    }
}
