# SignalRWebRtcDemo
## .NET 6 SignalR配合WebRtc进行实时视频通话
### 共有MVC和vue两个示例
### 1、MVC启动
#### 使用vscode/vs/rider 打开 SignalRWebRtcDemo/src/MvcDemo/ 
#### 无需修改直接启动
#### 打开两个web客户端 分别登录2个账号进行连接测试

### 2、Vue启动
#### 2.1、使用vscode/vs/rider 打开 SignalRWebRtcDemo/tree/main/src/VueDemo/ChatServer
#### Ctrl+F5启动SignalR信令服务器 
#### 2.2、使用vscode 打开 SignalRWebRtcDemo/tree/main/src/VueDemo/videochatvue 
####  2.2.1、先使用npm install安装依赖
####  2.2.2、在再使用npm run serve 启动vue项目
  启动完成后根据vue命令行提示网址进入系统 打开两个web客户端 分别登录2个账号进行连接测试
#### 2.3、如果出现SignalR连接错误 则检查App.vue 中SignalR连接地址端口是否和ChatServer一致，不一致则修改成一致即可

### 3、volcore例子启动 参照 http://www.volcore.xyz/#/login 文档启动例子 该例子直接把用户表改造成在线用户连接