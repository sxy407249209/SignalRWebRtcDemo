import { defineAsyncComponent } from "vue";
let extension = {
    components: { //动态扩充组件或组件路径
        //表单header、content、footer对应位置扩充的组件
        gridHeader: defineAsyncComponent(() =>
            import("./Sys_User/Sys_UserGridHeader.vue")),
        gridBody: defineAsyncComponent(() =>
            import("./Video")),
        gridFooter: '',
        //弹出框(修改、编辑、查看)header、content、footer对应位置扩充的组件
        modelHeader: '',
        modelBody: '',
        modelFooter: ''
    },
    text: "只能看到当前角色下的所有帐号",
    buttons: [], //扩展的按钮
    methods: { //事件扩展
        onInit() {
            var that = this;
            this.signaR.SR.on('FriendsOnline', function (username) {
                that.$message.success(username + '上线了');
                that.search();
            });

        
            this.signaR.SR.on("ReceiveVideoCall", function (msg, aName, bName) {
           
                that.$refs.gridBody.aName = aName;
                that.$refs.gridBody.bName = bName;
                if (window.confirm(msg)) {
                    that.$refs.gridBody.open(aName,bName,"",false);
                    //发给A同意
                    that.signaR.SR.invoke("IsAgreeVideo",  that.$refs.gridBody.aName, "是").catch(function (err) {
                        return console.error(err.toString());
                    });

                } else {
                    //发给A不同意
                    that.signaR.SR.invoke("IsAgreeVideo", that.$refs.gridBody.bName.aName, "否").catch(function (err) {
                        return console.error(err.toString());
                    });
                }
            });
            var myuserinfo ;

            this.http.post("/api/sys_user/GetCurrentUserInfo",{},true).then(reslut=>{
                console.log(reslut);
                 myuserinfo = reslut.data;
            })
            


            this.boxOptions.height = 530;
            this.columns.push({
                title: '操作',
                hidden: false,
                align: "center",
                fixed: 'right',
                width: 120,
                render: (h, { row, column, index }) => {
                    return h(
                        "div", { style: { 'font-size': '13px', 'cursor': 'pointer', 'color': '#409eff' } }, [
                        h(
                            "a", {
                            style: { 'margin-right': '15px' },
                            onClick: (e) => {
                                e.stopPropagation()
                                this.$refs.gridHeader.open(row);
                            }
                        }, "修改密码"
                        ),
                        h(
                            "a", {
                            style: { 'margin-right': '15px' },
                            onClick: (e) => {
                                e.stopPropagation()
                                this.edit(row);
                            }
                        },
                            "编辑"
                        ),
                        h(
                            "a", {
                            style: { 'margin-right': '15px' },
                            onClick: (e) => {

                                
                                var myuserid = myuserinfo.user_Id
                                var muysername = myuserinfo.userName
                                var touserid = row.User_Id;
                               
                               
                                if (myuserid == touserid) {
                                    that.$Message.error("不能和自己连接");
                                } else {
                                    this.$refs.gridBody.open(myuserid,touserid,muysername,true);
                                }


                            }
                        },
                            "视频"
                        ),
                    ])
                }
            })
        },
        onInited() { },
        addAfter(result) { //用户新建后，显示随机生成的密码
            if (!result.status) {
                return true;
            }
            //显示新建用户的密码
            //2020.08.28优化新建成后提示方式
            this.$confirm(result.message, '新建用户成功', {
                confirmButtonText: '确定',
                type: 'success',
                center: true
            }).then(() => { })

            this.boxModel = false;
            this.refresh();
            return false;
        },
        modelOpenAfter() {
            //点击弹出框后，如果是编辑状态，禁止编辑用户名，如果新建状态，将用户名字段设置为可编辑
            let isEDIT = this.currentAction == this.const.EDIT;
            this.editFormOptions.forEach(item => {
                item.forEach(x => {
                    if (x.field == "UserName") {
                        x.disabled = isEDIT;
                    }
                })
                //不是新建，性别默认值设置为男
                if (!isEDIT) {
                    this.editFormFields.Gender = "0";
                }
            })
        },
        searchAfter(rows, result) {
            rows.push(...result.extra);
            return true;
        }

    }
};
export default extension;