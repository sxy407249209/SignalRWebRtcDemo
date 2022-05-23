import * as signalR from "@microsoft/signalr";
import http from "./http.js";
import store from '../store/index';
export default {
    SR: {},
    //初始化连接
    initSR: function () {
        console.log("初始化")
        let that = this;
        console.log(http.ipAddress)
        var token = store.getters.getToken();
        that.SR = new signalR.HubConnectionBuilder()
            .withUrl(http.ipAddress + "chatHub", {
                accessTokenFactory: () => {
                    return token.replace("Bearer ", "");
                    //return token;
                }
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();
        // 3.携带参数
       
        // 4.启动连接的方法
        async function start() {
            try {
                await that.SR.start();
                console.log("signaR连接成功");
            } catch (err) {
                console.log("err", err);
                setTimeout(start, 5000);
            }
        }
        //5.关闭之后重连
        that.SR.onclose(async () => {
            await start();
        });
        // 6.启动连接
        start();
    },
    // 停止连接（这个方法好像没啥用，先放着吧）
    stopSR: function () {
        let that = this;
        async function stop() {
            try {
                await that.SR.stop();
                console.log("signaR退出成功");
            } catch (err) { }
        }
        stop();
    },
    



};