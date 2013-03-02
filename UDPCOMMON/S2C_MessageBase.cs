using System;

namespace UDPCOMMON
{
    #region 服务器发送到客户端消息

    /// <summary> 
    /// 服务器发送到客户端消息基类 
    /// </summary> 
    [Serializable]
    public abstract class S2C_MessageBase : MessageBase
    {
    }

    /// <summary> 
    /// 请求用户列表应答消息 
    /// </summary> 
    [Serializable]
    public class S2C_UserListMessage : S2C_MessageBase
    {
        private readonly UserCollection userList;

        public S2C_UserListMessage(UserCollection users)
        {
            userList = users;
        }

        public UserCollection UserList
        {
            get { return userList; }
        }
    }

    /// <summary> 
    /// 转发请求Purch Hole消息 
    /// </summary> 
    [Serializable]
    public class S2C_HolePunchingMessage : S2C_MessageBase
    {
        protected System.Net.IPEndPoint _remotePoint;

        public S2C_HolePunchingMessage(System.Net.IPEndPoint point)
        {
            _remotePoint = point;
        }

        public System.Net.IPEndPoint RemotePoint
        {
            get { return _remotePoint; }
        }
    }

    /// <summary> 
    /// 服务器通知所有在线用户， 
    /// </summary> 
    [Serializable]
    public class S2C_UserAction : S2C_MessageBase
    {
        protected User _User;
        protected UserAction _Action;

        public S2C_UserAction(User user, UserAction action)
        {
            _User = user;
            _Action = action;
        }

        public User User
        {
            get { return _User; }
        }

        public UserAction Action
        {
            get { return _Action; }
        }
    }

    #endregion

}
