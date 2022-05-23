using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRHelper;
using VOL.Core.ManageUser;

namespace VOL.WebApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }



        #region 开始关闭连接事件处理

        public override Task OnConnectedAsync()
        {
            var userid = UserContext.Current.UserId;
            var username = UserContext.Current.UserName;
            var chk = SignalRConnManagement.IsExis(userid);
            if (chk)
            {
                SignalRConnManagement.UpDateConn(userid, Context.ConnectionId);
            }
            else
            {
                SignalRConnManagement.AddConn(userid, Context.ConnectionId);
            }
            Clients.All.SendAsync("FriendsOnline", username);
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// 关闭连接自动清除连接通道
        /// </summary>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {

            SignalRConnManagement.RomveConn(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }


        #endregion


        #region 信令服务器  

        /// <summary>
        /// 请求连接
        /// </summary>
        /// <param name="aName"></param>
        /// <param name="bName"></param>
        /// <returns></returns>
        public async Task VideoCall(int auserid,int buserid, string name)
        {
            var bConnId = GetConnId(buserid);
            await Clients.Client(bConnId).SendAsync("ReceiveVideoCall", $"{name}请求视频连线", auserid, buserid);
        }


        /// <summary>
        /// B回复A 是否视频
        /// </summary>
        /// <returns></returns>
        public async Task IsAgreeVideo(int auserid, string isAgree)
        {
            var aConnId = GetConnId(auserid);
            await Clients.Client(aConnId).SendAsync("ReceiveIsAgreeVideo", isAgree);
        }
        /// <summary>
        ///  A准备与B发起连接
        /// </summary>
        /// <returns></returns>
        public async Task SendOffer(int buserid, string offer)
        {
            //判断B是否在线
            var chk = SignalRConnManagement.IsExis(buserid);
            if (chk)
            {
                //b的连接id
                var bConnId = GetConnId(buserid);
                await Clients.Client(bConnId).SendAsync("ReceiveOffer",offer);
            }
        }
        /// <summary>
        /// B收到offer 向A回复answer
        /// </summary>
        /// <returns></returns>
        public async Task WasCallAnswer(int auserid, string answer)
        {
            var aConnId = GetConnId(auserid);
            await Clients.Client(aConnId).SendAsync("ReceiveWasCallAnswer",  answer);
        }
        /// <summary>
        /// 双方candidate事件
        /// </summary>
        /// <returns></returns>
        public async Task SendCandidate(int buserid, string candidate)
        {
            var onnid = GetConnId(buserid);
            await Clients.Client(onnid).SendAsync("ReceiveCandidate", candidate);
        }
        #endregion


        /// <summary>
        /// 防止刷新页面后 连接id改变而出现消息传输bug 每次都先获取新的GetConnId
        /// </summary>
        /// <param name="itcode"></param>
        /// <returns></returns>
        private string GetConnId(int userId)
        {
            var conn = SignalRConnManagement.Connection.FirstOrDefault(x => x.UserId == userId);
            return conn.ConnId;
        }

    }
}

