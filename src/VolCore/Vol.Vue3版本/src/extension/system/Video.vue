<template>
  <el-button type="default" size="mini" @click="model = true">打开弹出框</el-button>
  <vol-box :lazy="true" v-model="model" title="弹出框3" :height="400" :width="700" :padding="15">
    <div>
      <el-row :gutter="10">
        <el-col :span="12">
          <h2>我</h2>
          <video ref="localVideo" autoplay playsinline controls></video>
        </el-col>
        <el-col :span="12">
          <h2>对方</h2>
          <video ref="remoteVideo" autoplay playsinline controls></video>
        </el-col>

      </el-row>
    </div>
    <button @click="connVideo" v-if="isConn">开始连接</button>
  </vol-box>
</template>
<script>
import VolBox from "@/components/basic/VolBox.vue";
//这里使用的vue2语法，也可以写成vue3语法
export default {
  components: { "vol-box": VolBox },
  methods: {},
  data() {
    return {
      model: false,
      //userid: 0,
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
      remoteVideo: null,
      userName:0,
      myuserName:"",
      aName: 0,
      bName: 0,
      isFirst: true,
      isConn:false
    };
  },
  methods: {
    open(myuserid,touserid,myuserName,isConn) {
      this.model = true;
      this.aName = myuserid;
      this.bName = touserid;
      this.userName = myuserid;
      this.myuserName = myuserName;
      this.isConn = isConn;
      this.initVideo();
      //this.connVideo();
    },
    connVideo() {
      
      
      this.signaR.SR.invoke("VideoCall", this.aName,this.bName,this.myuserName ).catch(function (err) {
        return console.error(err.toString());
      });
    },
    initVideo() {

      var that = this;
      this.yourConn = new RTCPeerConnection(this.configuration);

      if (this.isFirst) {
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
        this.isFirst = false;
      }


      // 监听ice候选 并向对方发送ice
      this.yourConn.onicecandidate = function (event) {
        var other = that.userName == that.aName ? that.bName : that.aName;
        if (event.candidate) {
          that.signaR.SR.invoke("SendCandidate", other, JSON.stringify(event.candidate)).catch(function (err) {
            return console.log(err.toString());
          });
        }
      };

      //监听对面是否同意视频
      this.signaR.SR.on("ReceiveIsAgreeVideo", function (isAgree) {
        if (isAgree != "是") {
          alert("对方不同意视频")
        } else {

          //A 发送 B WebRtc请求 SendOffer
          that.yourConn.createOffer({
            offerToReceiveVideo: 1,
            offerToReceiveAudio: 1
          }).then(offer => {
            //设置视频源触发icecandidate事件
            console.log(offer);
            console.log(that.bName); 
            that.yourConn.setLocalDescription(offer);
            that.signaR.SR.invoke("SendOffer", that.bName, JSON.stringify(offer)).catch(function (err) {
              return console.log(err.toString());
            });
          });
        }
      });

      //B接收Offer并向A回复Answer
      this.signaR.SR.on("ReceiveOffer", function (offerStr) {
        var offer = JSON.parse(offerStr);
        that.yourConn.setRemoteDescription(new RTCSessionDescription(offer));
        ///收到后应答请求
        that.yourConn.createAnswer().then(answer => {
          ///更改本地描述，抛出icecandidate事件
          that.yourConn.setLocalDescription(answer).then(() => {
            that.signaR.SR.invoke("WasCallAnswer", that.aName, JSON.stringify(answer)).catch(function (err) {
              return console.error(err.toString());
            });
          });
        });
      });

      //A接收BAnswer 修改远程描述
      this.signaR.SR.on("ReceiveWasCallAnswer", function (answerStr) {
        var answer = JSON.parse(answerStr);
        //更改远程描述
        that.yourConn.setRemoteDescription(new RTCSessionDescription(answer));
      });

      //处理Candidate事件
      this.signaR.SR.on("ReceiveCandidate", function (candidateStr) {
        var candidate = JSON.parse(candidateStr);
        console.log(candidateStr)
        //更改远程描述
        that.yourConn.addIceCandidate(new RTCIceCandidate(candidate));
      });
    }
  },



 
};
</script>
