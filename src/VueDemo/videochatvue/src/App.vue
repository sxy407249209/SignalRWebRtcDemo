<template>
  <div id="app">

    <div> <span>用户名</span> <input type="text" v-model="userName"></div>
    <button @click="login">登录</button>

    <div>
      <h2>我</h2>
      <video height="240" ref="localVideo" autoplay playsinline controls></video>


    </div>
    <div>
      <h2>对面</h2>
      <video height="240" ref="remoteVideo" autoplay playsinline controls></video>
    </div>

    <div>
      <ul v-for="(value, key) in userList" :key="key">
        <li>用户：{{ value.UserName }} <button v-if="value.UserName != userName"
            @click="connVideo(value.UserName)">连接</button>
        </li>
      </ul>
    </div>


  </div>

</template>

<script>
const signalR = require("@microsoft/signalr");
export default {
  name: 'App',
  components: {

  },
  data() {
    return {
      connection: null,
      userName: "",
      userList: [],
      yourConn: null,
      stream: null,
      configuration: {
        "iceServers": [{ "url": "stun:stun2.1.google.com:19302" }]
      },
      setting: {
        video: true,
        audio: true
      },
      localVideo: null,
      remoteVideo:null,
      aName: "",
      bName: "",
    };
  },
  created() {
    this.init();

  },
  methods: {
    init() {
      var that = this;
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7175/chathub", {})
        .configureLogging(signalR.LogLevel.Information)
        .build();
      this.connection.on("FriendsOnline", (userNameListStr) => {
        that.userList = JSON.parse(userNameListStr);
      });
      this.connection.start();
      this.yourConn = new RTCPeerConnection(this.configuration);
      navigator.mediaDevices.getUserMedia(this.setting).then(myStream => {
        this.stream = myStream;
        this.localVideo = this.$refs.localVideo;
        this.localVideo.srcObject = this.stream;
        this.yourConn.addStream(this.stream);
      }).catch(error => console.log(error));

      this.yourConn.addEventListener('addstream', (event) => {
        that.remoteVideo = that.$refs.remoteVideo;
        that.remoteVideo.srcObject = event.stream;
      });

      // 监听ice候选 并向对方发送ice
      this.yourConn.onicecandidate = function (event) {
        var other = that.userName == that.aName ? that.bName : that.aName;
        if (event.candidate) {
          that.connection.invoke("SendCandidate", other, JSON.stringify(event.candidate)).catch(function (err) {
            return console.log(err.toString());
          });
        }
      };

      //收到连接请求 并出处理
      this.connection.on("ReceiveVideoCall", function (msg, aName, bName) {
        that.aName = aName;
        that.bName = bName;
        if (window.confirm(msg)) {
          //发给A同意
          that.connection.invoke("IsAgreeVideo", that.aName, "是").catch(function (err) {
            return console.error(err.toString());
          });
        } else {
          //发给A不同意
          that.connection.invoke("IsAgreeVideo", that.aName, "否").catch(function (err) {
            return console.error(err.toString());
          });
        }
      });

      //监听对面是否同意视频
      this.connection.on("ReceiveIsAgreeVideo", function (isAgree) {
        if (isAgree != "是") {
          alert("对方不同意视频")
        } else {
          //A 发送 B WebRtc请求 SendOffer
          that.yourConn.createOffer({
            offerToReceiveVideo: 1,
            offerToReceiveAudio: 1
          }).then(offer => {
            //设置视频源触发icecandidate事件
            that.yourConn.setLocalDescription(offer);
            that.connection.invoke("SendOffer", that.bName, JSON.stringify(offer)).catch(function (err) {
              return console.log(err.toString());
            });
          });
        }
      });

      //B接收Offer并向A回复Answer
      this.connection.on("ReceiveOffer", function (offerStr) {
        var offer = JSON.parse(offerStr);
        that.yourConn.setRemoteDescription(new RTCSessionDescription(offer));
        ///收到后应答请求
        that.yourConn.createAnswer().then(answer => {
          ///更改本地描述，抛出icecandidate事件
          that.yourConn.setLocalDescription(answer).then(() => {
            that.connection.invoke("WasCallAnswer", that.aName, JSON.stringify(answer)).catch(function (err) {
              return console.error(err.toString());
            });
          });
        });
      });

      //A接收BAnswer 修改远程描述
      this.connection.on("ReceiveWasCallAnswer", function (answerStr) {
        var answer = JSON.parse(answerStr);
        //更改远程描述
        that.yourConn.setRemoteDescription(new RTCSessionDescription(answer));
      });

      //处理Candidate事件
      this.connection.on("ReceiveCandidate", function (candidateStr) {
        var candidate = JSON.parse(candidateStr);
        console.log(candidateStr)
        //更改远程描述
        that.yourConn.addIceCandidate(new RTCIceCandidate(candidate));
      });


    },
    //登录
    login() {
      this.connection.invoke("Login", this.userName).catch(function (err) {
        return console.error(err.toString());
      });
    },
    //请求连接
    connVideo(bname) {
      this.aName = this.userName;
      this.bName = bname;
      this.connection.invoke("VideoCall", this.aName, this.bName).catch(function (err) {
        return console.error(err.toString());
      });
    },

  }
}
</script>

<style>
</style>
