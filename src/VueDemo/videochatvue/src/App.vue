<template>
  <div id="app">
    <img alt="Vue logo" src="./assets/logo.png">
    <HelloWorld msg="Welcome to Your Vue.js App" />
     <button @click="sendMsg">发送测试</button>
  </div>
 
</template>

<script>
const signalR = require("@microsoft/signalr");
import HelloWorld from './components/HelloWorld.vue'

export default {
  name: 'App',
  components: {
    HelloWorld
  },
  data() {
    return {
      connection: null,
    };
  },
  created() {
    this.init();
    //this.sendMsg();
  },
  methods: {
    init() {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7175/chathub", {})
        .configureLogging(signalR.LogLevel.Information)
        .build();
      this.connection.on("ReceiveMessage", (user, message) => {
        console.log(user, message);
      });
      this.connection.start();
    },
    sendMsg() {

      this.connection.invoke("SendMessage", "params", "2");
    }
  }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
