using System;

namespace UDPCOMMON
{
    #region 客户端发送到服务器的消息

    /// <summary> 
    /// 客户端发送到服务器的消息基类 
    /// </summary> 
    [Serializable]
    public abstract class C2S_MessageBase : MessageBase
    {
        private readonly string _fromUserName;

        protected C2S_MessageBase(string fromUserName)
        {
            _fromUserName = fromUserName;
        }

        public string FromUserName
        {
            get { return _fromUserName; }
        }
    }

    /// <summary> 
    /// 用户登录消息 
    /// </summary> 
    [Serializable]
    public class C2S_LoginMessage : C2S_MessageBase
    {
        private readonly string _password;

        public C2S_LoginMessage(string userName, string password)
            : base(userName)
        {
            _password = password;
        }

        public string Password
        {
            get { return _password; }
        }
    }

    /// <summary> 
    /// 用户登出消息 
    /// </summary> 
    [Serializable]
    public class C2S_LogoutMessage : C2S_MessageBase
    {

        public C2S_LogoutMessage(string userName)
            : base(userName)
        { }
    }

    /// <summary> 
    /// 请求用户列表消息 
    /// </summary> 
    [Serializable]
    public class C2S_GetUsersMessage : C2S_MessageBase
    {
        public C2S_GetUsersMessage(string userName)
            : base(userName)
        { }
    }

    /// <summary> 
    /// 请求Purch Hole消息 
    /// </summary> 
    [Serializable]
    public class C2S_HolePunchingRequestMessage : C2S_MessageBase
    {
        protected string toUserName;

        public C2S_HolePunchingRequestMessage(string fromUserName, string toUserName)
            : base(fromUserName)
        {
            this.toUserName = toUserName;
        }

        public string ToUserName
        {
            get { return toUserName; }
        }
    }

    #endregion

}
