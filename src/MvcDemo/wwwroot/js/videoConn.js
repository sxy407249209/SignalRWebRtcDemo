"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

document.getElementById("loginButton").disabled = true;

inintCon();

var configuration = {
    "iceServers": [{ "url": "stun:stun2.1.google.com:19302" }]
};

var yourConn = new RTCPeerConnection(configuration);

var stream;

connection.start().then(function () {
    document.getElementById("loginButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});


//登录
document.getElementById("loginButton").addEventListener("click", function (event) {
    var  userName = document.getElementById("userName").value;
    connection.invoke("Login", userName).catch(function (err) {
        return console.error(err.toString());
    });
});

//监听上线事件，加入列表
connection.on("FriendsOnline", function (userNameListStr) {
    var userList = JSON.parse(userNameListStr);
    var ul = document.getElementById("messagesList");
    ul.innerHTML = "";
    for (var i = 0; i < userList.length; i++) {
        var li = document.createElement("li");
        ul.appendChild(li);

        if (userList[i].UserName != document.getElementById("userName").value) {
            li.innerHTML = `${userList[i].UserName} <button onclick="connVideo('${userList[i].UserName}')">连接</button>`;
        } else {
            li.innerHTML = userList[i].UserName;
        }

        
    }
});


//初始化本地视频
function inintCon() {
    const setting = {
        video: true,
        audio: true
    };
    navigator.mediaDevices.getUserMedia(setting).then(myStream => {
        stream = myStream;
        $('#localVideo').get(0).srcObject = stream;
        yourConn.addStream(stream);
    }).catch(error => console.log(error));

}

//webrtc监听远程视频发送
yourConn.addEventListener('addstream', (event) => {
    $('#remoteVideo').get(0).srcObject = event.stream;
})

// 监听ice候选 并向对方发送ice
yourConn.onicecandidate = function (event) {
    var userName = document.getElementById("userName").value;
    var userA = localStorage.getItem("aName");
    var userB = localStorage.getItem("bName");
    var other = userName == userA ? userB : userA;

    console.log(other);

    if (event.candidate) {
        connection.invoke("SendCandidate", other, JSON.stringify(event.candidate)).catch(function (err) {
            return console.log(err.toString());
        });
    }
};

//请求连接
function connVideo(bName) {
    var aName = document.getElementById("userName").value;
    localStorage.setItem("aName", aName);
    localStorage.setItem("bName", bName);
    connection.invoke("VideoCall", aName, bName).catch(function (err) {
        return console.error(err.toString());
    });
}

//收到连接请求 并出处理
connection.on("ReceiveVideoCall", function (msg, aName, bName) {

    console.log(aName)
    console.log(bName)
    localStorage.setItem("aName", aName);
    localStorage.setItem("bName", bName); 
    if (window.confirm(msg)) {
        //发给A同意
        connection.invoke("IsAgreeVideo", aName,"是").catch(function (err) {
            return console.error(err.toString());
        });
    } else {
        //发给A不同意
        connection.invoke("IsAgreeVideo", aName,"否").catch(function (err) {
            return console.error(err.toString());
        });
    }

});

//监听对面是否同意视频
connection.on("ReceiveIsAgreeVideo", function (isAgree) {
    if (isAgree != "是") {
        alert("对方不同意视频")
    } else {
        var bName = localStorage.getItem("bName");
        call(bName);
    }
});

//A 发送 B WebRtc请求 SendOffer
function call(bName) {
    if (bName.length > 0) {
        yourConn.createOffer({
            offerToReceiveVideo: 1,
            offerToReceiveAudio: 1
        }).then(offer => {
            //设置视频源触发icecandidate事件
            yourConn.setLocalDescription(offer);
            connection.invoke("SendOffer",bName, JSON.stringify(offer)).catch(function (err) {
                return console.log(err.toString());
            });
        });
    } else {
        alert("获取对方信息错误，请重新进入！");
    }
}

//B接收Offer并向A回复Answer
connection.on("ReceiveOffer", function (offerStr) {
    var offer = JSON.parse(offerStr);
    yourConn.setRemoteDescription(new RTCSessionDescription(offer));
    ///收到后应答请求
    yourConn.createAnswer().then(answer => {
        ///更改本地描述，抛出icecandidate事件
        yourConn.setLocalDescription(answer).then(() => {
            var aName = localStorage.getItem("aName");
            connection.invoke("WasCallAnswer", aName, JSON.stringify(answer)).catch(function (err) {
                return console.error(err.toString());
            });
        });
    });
});

//A接收BAnswer 修改远程描述
connection.on("ReceiveWasCallAnswer", function (answerStr) {
    var answer = JSON.parse(answerStr);
    //更改远程描述
    yourConn.setRemoteDescription(new RTCSessionDescription(answer));

});

//处理Candidate事件
connection.on("ReceiveCandidate", function (candidateStr) {
    var candidate = JSON.parse(candidateStr);
    //更改远程描述
    yourConn.addIceCandidate(new RTCIceCandidate(candidate));

});












