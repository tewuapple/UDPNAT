using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Collections;

namespace UDPCOMMON
{
    /// <summary> 
    /// 显示跟踪消息的事件委托 
    /// </summary> 
    public delegate void WriteLogHandle(string msg);

    /// <summary> 
    /// 刷新在线用户的事件委托 
    /// </summary> 
    public delegate void UserChangedHandle(UserCollection users);

    public class Globals
    {
        /// <summary> 
        /// 服务器侦听端口 
        /// </summary> 
        public const int SERVER_PORT = 21134;

        /// <summary> 
        /// 本地侦听端口 
        /// </summary> 
        public const int LOCAL_PORT = 19786;
    }

    /// <summary> 
    /// User 的摘要说明。 
    /// </summary> 
    [Serializable]
    public class User
    {
        protected string _userName;
        protected IPEndPoint _netPoint;
        protected bool _conntected;

        public User(string UserName, IPEndPoint NetPoint)
        {
            _userName = UserName;
            _netPoint = NetPoint;
        }

        public string UserName
        {
            get { return _userName; }
        }

        public IPEndPoint NetPoint
        {
            get { return _netPoint; }
            set { _netPoint = value; }
        }

        public bool IsConnected //打洞标记
        {
            get { return _conntected; }
            set { _conntected = value; }
        }

        public string FullName
        {
            get { return ToString(); }
        }

        public override string ToString()
        {
            return _userName + "- [" + _netPoint.Address + ":" + _netPoint.Port.ToString(CultureInfo.InvariantCulture) +
                   "] " +
                   (_conntected ? "Y" : "N");
        }
    }

    /// <summary> 
    /// 在线用户列表 
    /// </summary> 
    [Serializable]
    public class UserCollection : CollectionBase
    {
        public void Add(User user)
        {
            InnerList.Add(user);
        }

        public void Remove(User user)
        {
            InnerList.Remove(user);
        }

        public User this[int index]
        {
            get { return (User) InnerList[index]; }
        }

        public User Find(string userName)
        {
            return
                this.Cast<User>().FirstOrDefault(
                    user => String.Compare(userName, user.UserName, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }

    /// <summary> 
    /// 序列化反序列化对象 
    /// </summary> 
    public class ObjectSerializer
    {
        public static byte[] Serialize(object obj)
        {
            var binaryF = new BinaryFormatter();
            var ms = new MemoryStream(1024*10);
            binaryF.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[(int) ms.Length];
            ms.Read(buffer, 0, buffer.Length);
            ms.Close();
            return buffer;
        }

        public static object Deserialize(byte[] buffer)
        {
            var binaryF = new BinaryFormatter();
            var ms = new MemoryStream(buffer, 0, buffer.Length, false);
            object obj = binaryF.Deserialize(ms);
            ms.Close();
            return obj;
        }
    }

}
