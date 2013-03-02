using System;

namespace UDPCOMMON
{
    #region 点对点消息

    /// <summary> 
    /// 点对点消息基类 
    /// </summary> 
    [Serializable]
    public abstract class P2P_MessageBase : MessageBase
    {
        // 
    }

    /// <summary> 
    /// 聊天消息 
    /// </summary> 
    [Serializable]
    public class P2P_TalkMessage : P2P_MessageBase
    {
        private readonly string message;

        public P2P_TalkMessage(string msg)
        {
            message = msg;
        }

        public string Message
        {
            get { return message; }
        }
    }

    /// <summary> 
    /// UDP打洞测试消息 
    /// </summary> 
    [Serializable]
    public class P2P_HolePunchingTestMessage : P2P_MessageBase
    {
        private readonly string _UserName;

        public P2P_HolePunchingTestMessage(string userName)
        {
            _UserName = userName;
        }

        public string UserName
        {
            get { return _UserName; }
        }
    }

    /// <summary> 
    /// 收到消息的回复确认 
    /// 如A与B想建立通话通道，些命令由B发出确认打洞成功 
    /// </summary> 
    [Serializable]
    public class P2P_HolePunchingResponse : P2P_MessageBase
    {
        private readonly string _UserName;

        public P2P_HolePunchingResponse(string userName)
        {
            _UserName = userName;
        }

        public string UserName
        {
            get { return _UserName; }
        }
    }

    #endregion

}
