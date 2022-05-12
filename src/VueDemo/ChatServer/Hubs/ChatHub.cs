
using System.Text.Json;
using ChatServer.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace MvcDemo.Hubs
{
    public class ChatHub : Hub
    {

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }



        #region 开始关闭连接事件处理y

        /// <summary>
        /// 关闭连接自动清除连接通道
        /// </summary>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(System.Exception exception)
        {

            SignalRConnManagement.RomveConn(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public async Task Login(string userName)

        {
            var chk = SignalRConnManagement.IsExis(userName);
            if (chk)
            {
                SignalRConnManagement.UpDateConn(userName, Context.ConnectionId);
            }
            else
            {
                SignalRConnManagement.AddConn(userName, Context.ConnectionId);
            }
            var userList = SignalRConnManagement.GetUserList();
            var userListStr = JsonSerializer.Serialize(userList);
            await Clients.All.SendAsync("FriendsOnline", userListStr);
        }

        #endregion


        #region 信令服务器  

        /// <summary>
        /// 请求连接
        /// </summary>
        /// <param name="aName"></param>
        /// <param name="bName"></param>
        /// <returns></returns>
        public async Task VideoCall(string aName, string bName)
        {
            var bConnId = GetConnId(bName);
            await Clients.Client(bConnId).SendAsync("ReceiveVideoCall", $"{aName}请求视频连线", aName, bName);
        }


        /// <summary>
        /// B回复A 是否视频
        /// </summary>
        /// <returns></returns>
        public async Task IsAgreeVideo(string aName, string isAgree)
        {
            var aConnId = GetConnId(aName);
            await Clients.Client(aConnId).SendAsync("ReceiveIsAgreeVideo", isAgree);
        }
        /// <summary>
        ///  A准备与B发起连接
        /// </summary>
        /// <returns></returns>
        public async Task SendOffer(string bName, string offer)
        {
            //判断B是否在线
            var chk = SignalRConnManagement.IsExis(bName);
            if (chk)
            {
                //b的连接id
                var bConnId = GetConnId(bName);
                await Clients.Client(bConnId).SendAsync("ReceiveOffer", offer);
            }
        }
        /// <summary>
        /// B收到offer 向A回复answer
        /// </summary>
        /// <returns></returns>
        public async Task WasCallAnswer(string aName, string answer)
        {
            var aConnId = GetConnId(aName);
            await Clients.Client(aConnId).SendAsync("ReceiveWasCallAnswer", answer);
        }
        /// <summary>
        /// 双方candidate事件
        /// </summary>
        /// <returns></returns>
        public async Task SendCandidate(string name, string candidate)
        {
            var onnid = GetConnId(name);
            await Clients.Client(onnid).SendAsync("ReceiveCandidate", candidate);
        }
        #endregion


        /// <summary>
        /// 防止刷新页面后 连接id改变而出现消息传输bug 每次都先获取新的GetConnId
        /// </summary>
        /// <param name="itcode"></param>
        /// <returns></returns>
        private string GetConnId(string userName)
        {
            var conn = SignalRConnManagement.Connection.FirstOrDefault(x => x.UserName == userName);
            return conn.ConnId;
        }

    }
}