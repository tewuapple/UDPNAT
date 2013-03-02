using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using UDPCOMMON;
using ThreadState = System.Threading.ThreadState;

namespace UDPNATCLIENT
{
    /// <summary> 
    /// 客户端业务类 
    /// </summary> 
    public class Client : IDisposable
    {
        //private const int MAX_RETRY_SEND_MSG = 1; //打洞时连接次数,正常情况下一次就能成功 

        private readonly UdpClient _client;//客户端监听 
        private readonly IPEndPoint _hostPoint; //主机IP 
        private IPEndPoint _remotePoint; //接收任何远程机器的数据 
        private readonly UserCollection _userList;//在线用户列表 
        private readonly Thread _listenThread; //监听线程 
        private string _LocalUserName; //本地用户名 
        //private bool _HoleAccepted = false; //A->B,接收到B用户的确认消息 

        private WriteLogHandle _OnWriteMessage;
        public WriteLogHandle OnWriteMessage
        {
            get { return _OnWriteMessage; }
            set { _OnWriteMessage = value; }
        }

        private UserChangedHandle _UserChangedHandle;
        public UserChangedHandle OnUserChanged
        {
            get { return _UserChangedHandle; }
            set { _UserChangedHandle = value; }
        }

        /// <summary> 
        ///显示跟踪记录 
        /// </summary> 
        /// <param name="log"></param> 
        private void DoWriteLog(string log)
        {
            if (_OnWriteMessage != null)
                ((Control) _OnWriteMessage.Target).Invoke(_OnWriteMessage, log);
        }

        /// <summary> 
        /// 构造器 
        /// </summary> 
        public Client()
        {
            string serverIP = GetServerIP();
            _remotePoint = new IPEndPoint(IPAddress.Any, 0); //任何与本地连接的用户IP地址。 
            _hostPoint = new IPEndPoint(IPAddress.Parse(serverIP), Globals.SERVER_PORT); //服务器地址 
            _client = new UdpClient();//不指定端口，系统自动分配 
            _userList = new UserCollection();
            _listenThread = new Thread(Run);
        }

        /// <summary> 
        /// 获取服务器IP，INI文件内设置 
        /// </summary> 
        /// <returns></returns> 
        private string GetServerIP()
        {
            string file = Application.StartupPath + "\\ip.ini";
            string ip = File.ReadAllText(file);
            return ip.Trim();
        }

        /// <summary> 
        /// 启动客户端 
        /// </summary> 
        public void Start()
        {
            if (_listenThread.ThreadState == ThreadState.Unstarted)
            {
                _listenThread.Start();
            }
        }

        /// <summary> 
        /// 客户登录 
        /// </summary> 
        public void Login(string userName, string password)
        {
            _LocalUserName = userName;

            // 发送登录消息到服务器 
            C2S_LoginMessage loginMsg = new C2S_LoginMessage(userName, password);
            SendMessage(loginMsg, _hostPoint);
        }

        /// <summary> 
        /// 登出 
        /// </summary> 
        public void Logout()
        {
            C2S_LogoutMessage lgoutMsg = new C2S_LogoutMessage(_LocalUserName);
            SendMessage(lgoutMsg, _hostPoint);

            Dispose();
            Environment.Exit(0);
        }

        /// <summary> 
        /// 发送请求获取用户列表 
        /// </summary> 
        public void DownloadUserList()
        {
            C2S_GetUsersMessage getUserMsg = new C2S_GetUsersMessage(_LocalUserName);
            SendMessage(getUserMsg, _hostPoint);
        }

        /// <summary> 
        /// 显示在线用户 
        /// </summary> 
        /// <param name="users"></param> 
        private void DisplayUsers(UserCollection users)
        {
            if (_UserChangedHandle != null)
                ((Control) _UserChangedHandle.Target).Invoke(_UserChangedHandle, users);
        }

        //运行线程 
        private void Run()
        {
            try
            {
                byte[] buffer;//接受数据用 
                while (true)
                {
                    buffer = _client.Receive(ref _remotePoint);//_remotePoint变量返回当前连接的用户IP地址 

                    object msgObj = ObjectSerializer.Deserialize(buffer);
                    Type msgType = msgObj.GetType();
                    DoWriteLog("接收到消息:" + msgType + " From:" + _remotePoint);

                    if (msgType == typeof(S2C_UserListMessage))
                    {
                        // 更新用户列表 
                        S2C_UserListMessage usersMsg = (S2C_UserListMessage)msgObj;
                        _userList.Clear();

                        foreach (User user in usersMsg.UserList)
                            _userList.Add(user);

                        DisplayUsers(_userList);
                    }
                    else if (msgType == typeof(S2C_UserAction))
                    {
                        //用户动作，新用户登录/用户登出 
                        S2C_UserAction msgAction = (S2C_UserAction)msgObj;
                        if (msgAction.Action == UserAction.Login)
                        {
                            _userList.Add(msgAction.User);
                            DisplayUsers(_userList);
                        }
                        else if (msgAction.Action == UserAction.Logout)
                        {
                            User user = _userList.Find(msgAction.User.UserName);
                            if (user != null) _userList.Remove(user);
                            DisplayUsers(_userList);
                        }
                    }
                    else if (msgType == typeof(S2C_HolePunchingMessage))
                    {
                        //接受到服务器的打洞命令 
                        S2C_HolePunchingMessage msgHolePunching = (S2C_HolePunchingMessage)msgObj;

                        //NAT-B的用户给NAT-A的用户发送消息,此时UDP包肯定会被NAT-A丢弃， 
                        //因为NAT-A上并没有A->NAT-B的合法Session, 但是现在NAT-B上就建立了有B->NAT-A的合法session了! 
                        P2P_HolePunchingTestMessage msgTest = new P2P_HolePunchingTestMessage(_LocalUserName);
                        SendMessage(msgTest, msgHolePunching.RemotePoint);
                    }
                    else if (msgType == typeof(P2P_HolePunchingTestMessage))
                    {
                        //UDP打洞测试消息 
                        //_HoleAccepted = true; 
                        P2P_HolePunchingTestMessage msgTest = (P2P_HolePunchingTestMessage)msgObj;
                        UpdateConnection(msgTest.UserName, _remotePoint);

                        //发送确认消息 
                        P2P_HolePunchingResponse response = new P2P_HolePunchingResponse(_LocalUserName);
                        SendMessage(response, _remotePoint);
                    }
                    else if (msgType == typeof(P2P_HolePunchingResponse))
                    {
                        //_HoleAccepted = true;//打洞成功 
                        P2P_HolePunchingResponse msg = msgObj as P2P_HolePunchingResponse;
                        Debug.Assert(msg != null, "msg != null");
                        UpdateConnection(msg.UserName, _remotePoint);

                    }
                    else if (msgType == typeof(P2P_TalkMessage))
                    {
                        //用户间对话消息 
                        P2P_TalkMessage workMsg = (P2P_TalkMessage)msgObj;
                        DoWriteLog(workMsg.Message);
                    }
                    else
                    {
                        DoWriteLog("收到未知消息!");
                    }
                }
            }
            catch (Exception ex) { DoWriteLog(ex.Message); }
        }

        private void UpdateConnection(string user, IPEndPoint ep)
        {
            User remoteUser = _userList.Find(user);
            if (remoteUser != null)
            {
                remoteUser.NetPoint = ep;//保存此次连接的IP及端口 
                remoteUser.IsConnected = true;
                DoWriteLog(string.Format("您已经与{0}建立通信通道,IP:{1}!",
                remoteUser.UserName, remoteUser.NetPoint));
                DisplayUsers(_userList);
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            try
            {
                _listenThread.Abort();
                _client.Close();
            }
            catch
            {
                
            }
        }

        #endregion

        public void SendMessage(MessageBase msg, User user)
        {
            SendMessage(msg, user.NetPoint);
        }

        public void SendMessage(MessageBase msg, IPEndPoint remoteIP)
        {
            if (msg == null) return;
            DoWriteLog("正在发送消息给->" + remoteIP + ",内容:" + msg);
            byte[] buffer = ObjectSerializer.Serialize(msg);
            _client.Send(buffer, buffer.Length, remoteIP);
            DoWriteLog("消息已发送.");
        }

        /// <summary> 
        /// UDP打洞过程 
        /// 假设A想连接B.首先A发送打洞消息给Server,让Server告诉B有人想与你建立通话通道,Server将A的IP信息转发给B 
        /// B收到命令后向A发一个UDP包,此时B的NAT会建立一个与A通讯的Session. 然后A再次向B发送UDP包B就能收到了 
        /// </summary> 
        public void HolePunching(User user)
        {
            //A:自己; B:参数user 
            //A发送打洞消息给服务器,请求与B打洞 
            C2S_HolePunchingRequestMessage msg = new C2S_HolePunchingRequestMessage(_LocalUserName, user.UserName);
            SendMessage(msg, _hostPoint);

            Thread.Sleep(2000);//等待对方发送UDP包并建立Session 

            //再向对方发送确认消息，如果对方收到会发送一个P2P_HolePunchingResponse确认消息，此时打洞成功 
            P2P_HolePunchingTestMessage confirmMessage = new P2P_HolePunchingTestMessage(_LocalUserName);
            SendMessage(confirmMessage, user);
        }
    }
}
