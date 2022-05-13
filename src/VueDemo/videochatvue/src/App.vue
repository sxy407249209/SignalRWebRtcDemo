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
    };
  },
  created() {
    this.init();

  },
  methods: {
    init() {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7175/chathub", {})
        .configureLogging(signalR.LogLevel.Information)
        .build();
      this.connection.on("FriendsOnline", (userNameListStr) => {
        this.userList = JSON.parse(userNameListStr);
      });
      this.connection.start();
      this.yourConn = new RTCPeerConnection(this.configuration);
      navigator.mediaDevices.getUserMedia(this.setting).then(myStream => {
        this.stream = myStream;
        this.localVideo = this.$refs.localVideo;
        this.localVideo.srcObject = this.stream;
        this.yourConn.addStream(this.stream);
      }).catch(error => console.log(error));
    },
    login() {
      this.connection.invoke("Login", this.userName).catch(function (err) {
        return console.error(err.toString());
      });
    },
    connVideo(username) {
      console.log(username);
    },


  }
}
</script>

<style>
</style>
