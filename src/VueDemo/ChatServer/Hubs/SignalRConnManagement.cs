namespace ChatServer.Hubs
{
    /// <summary>
    /// Signal连接处理
    /// </summary>
    public static class SignalRConnManagement
    {
        public static List<SignalRConn> Connection { get; set; } = new List<SignalRConn>();
        /// <summary>
        /// 判断该用户是否已添加过连接
        /// </summary>
        /// <param name="itcode"></param>
        /// <returns></returns>
        public static bool IsExis(string userName )
        {
            var signalRConn = Connection.FirstOrDefault(x => x.UserName == userName);
            if (signalRConn != null)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 添加新连接到缓存
        /// </summary>
        /// <param name="connid"></param>
        public static void AddConn(string userName,string connId)
        {          
            SignalRConn signalRConn = new SignalRConn
            {
                ConnId = connId,
                UserName = userName,
            };
            Connection.Add(signalRConn);
        }
        /// <summary>
        /// 从缓存中删除连接
        /// </summary>
        /// <param name="itcode"></param>
        /// <param name="connid"></param>
        public static void RomveConn(string ConnId)
        {          
            var signalRConn = Connection.FirstOrDefault(x => x.ConnId == ConnId);
            if (signalRConn != null)
            {
                Connection.Remove(signalRConn);
            }

        }
        /// <summary>
        /// 更新不一致连接
        /// </summary>
        /// <param name="connid"></param>
        public static void UpDateConn(string userName, string connId)
        {          
            var old = Connection.FirstOrDefault(x => x.UserName == userName);
            old.ConnId = connId;
        }

        /// <summary>
        /// 获取所有连接
        /// </summary>
        public static List<SignalRConn> GetUserList()
        {
            return Connection;
        }

    }

    public class SignalRConn
    {
        /// <summary>
        /// 连接id
        /// </summary>
        public string  ConnId { get; set; }
        /// <summary>
        /// 用户唯一标识
        /// </summary>
        public string UserName { get; set; }
    }
}