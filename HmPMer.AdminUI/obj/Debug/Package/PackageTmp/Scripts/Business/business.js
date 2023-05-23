function GetAccountType(val) {
    switch (val) {
        case 0: return "个人";
        case 1: return "企业";
    }
    return "";
}

function GetIsEnabled(val) {
    switch (val) {
        case 0: return "未审核";
        case 1: return "<span style='color:green;'>正常</span>";
        case 2: return "<span style='color:red;'>冻结</span>";
    }
    return "";
}


//认证
function GetIsCertification(val) {
    if (val == '1') {
        return '<i class="layui-icon" style="font-size: 30px; color: green">&#xe618;</i> ';
    } else {
        return '<i class="layui-icon" style="font-size: 30px; color: red;">&#x1006;</i> ';
    }
}


layui.use(['hm', 'form', 'layer', 'laydate', 'table', 'laytpl'], function () {
    var form = layui.form,
        layer = layui.layer,
        $ = layui.jquery,
        laydate = layui.laydate,
        laytpl = layui.laytpl,
        table = layui.table,
        hm = layui.hm;

    //新闻列表
    var tableIns = table.render({
        elem: '#BusinessList',
        url: '/Business/LoadUserPage',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 60,
        page: true,
        even: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "BusinessList",
        cols: [[
            {
                field: 'UserId', title: '商户号', width: 90, align: "center", templet: function (d) {
                    return '<a class="layui-btn-xs layui-btn-mycss" lay-event="btn_editinfo">' + d.UserId + '</a>';
                }
            },
            { field: 'MerName', title: '商户名称', width: 160, align: "center" },

            {
                field: 'Balance', title: '余额(元)', width: 100, align: "center",
                templet: function (d) {
                    return (d.Balance / 100).toFixed(2);
                }
            },

            {
                field: 'UnBalance', title: '不可用金额(元)', width: 100, align: "center",
                templet: function (d) {
                    return (d.UnBalance / 100).toFixed(2);
                }
            },

            {
                field: 'Freeze', title: '冻结金额(元)', width: 100, align: "center",
                templet: function (d) {
                    return (d.Freeze / 100).toFixed(2);
                }
            },

            {
                field: 'OrderAmt', title: '总交易量(元)', width: 100, align: "center",
                templet: function (d) {
                    return (d.OrderAmt / 100).toFixed(2);
                }
            },

            {
                field: 'AccountType', title: '签约类型', width: 60, align: "center",
                templet: function (d) {
                    return GetAccountType(d.AccountType);
                }
            },

            {
                field: 'IdCardStatus', title: '资质', width: 30, align: "center",
                templet: function (d) {
                    return GetIsCertification(d.IdCardStatus);
                }
            },

            {
                field: 'IsMobilePhone', title: '手机', width: 30, align: "center",
                templet: function (d) {
                    return GetIsCertification(d.IsMobilePhone);
                }
            },

            {
                field: 'IsEmail', title: '邮箱', width: 30, align: "center",
                templet: function (d) {
                    return GetIsCertification(d.IsEmail);
                }
            },

            {
                field: 'AgentPay', title: '代付功能', align: 'center', width: 100, templet: function (d) {
                    if (d.AgentPay == 1) {
                        return '<input type="checkbox" checked name="AgentPay" lay-filter="AgentPay" lay-skin="switch" lay-text="开启|关闭"  value="' + d.UserId + '" >';
                    } else {
                        return '<input type="checkbox" name="AgentPay" lay-filter="AgentPay" lay-skin="switch" lay-text="开启|关闭"  value="' + d.UserId + '" >';
                    }
                }
            },

            {
                title: '单独费率', width: 50, align: "center", templet: function (d) {
                    return d.AloneRate == 1 ? "<span style='color:green;'>开启</span>" : "<span style='color:red;'>关闭</span>";
                }
            },

            {
                title: '费率', width: 40, align: "center", templet: function (d) {
                    return '<a class="layui-btn-xs layui-btn-mycss" lay-event="payreateedit">费率</a>';
                }
            },
            {
                title: '风控方案', width: 70, align: "center", templet: function (d) {
                    if (d.RiskSchemeId)
                        return '<a class="layui-btn-xs layui-btn-mycss" lay-event="riskschemeedit">' + d.RiskSchemeName + '</a>';
                    return '<a class="layui-btn-xs layui-btn-mycss" lay-event="riskschemeedit">设置风控</a>';
                }
            },

            {
                field: 'IsEnabled', title: '状态', width: 60, align: "center",
                templet: function (d) {
                    return GetIsEnabled(d.IsEnabled);

                }
            },


            {
                field: 'RegTime', title: '注册时间', align: 'center', width: 120,
                templet: function (d) {
                    return hm.changeDateFormat(d.RegTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            {
                field: 'LastLoginTime', title: '最后登录', align: 'center', width: 120,
                templet: function (d) {
                    return hm.changeDateFormat(d.LastLoginTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            {
                field: 'LastLoginIp', title: '最后登录IP', align: 'center', width: 90
            },


            { title: '操作', minWidth: 170, templet: '#BusinessListBar', fixed: "right", align: "center" }
        ]],
        response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });
    
    function SetIsEnabled(IsEnabled, userId) {
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        layer.confirm(IsEnabled == 1 ? "确定解冻该商户？" : "确定冻结该商户？", { icon: 3, title: '提示信息' }, function (index) {
            setTimeout(function () {
                // var IsEnabled = data.elem.checked ? 1 : 0;
                hm.ajax("/Business/UpIsEnabled", {
                    data: {
                        IsEnabled: IsEnabled,
                        userId: userId
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        layer.close(index);
                        if (result.Success) {
                            layer.msg("操作成功！");
                            Search();
                        } else {
                            layer.msg(result.Message);
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });

            }, 500);

        });
    }


    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });

    //回车事件
    document.onkeydown = function (event) {
        var e = event || window.event || arguments.callee.caller.arguments[0];

        if (e && e.keyCode == 13) { // enter 键
            //要做的事情
            Search();
        }
    }; 



    function Search() {
        table.reload("BusinessList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch"))
        })
    }

    $("#btn_add").click(function () {
        var index = layer.open({
            title: "添加商户",
            type: 2,
            area: ["1000px", "350px"],
            content: "/Business/AddUc"
        })
    });

    form.on('switch(AgentPay)', function (data) {
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        setTimeout(function () {
            var AgentPay = data.elem.checked ? 1 : 0;
            hm.ajax("/Business/UpBusinessAgentPay", {
                data: {
                    AgentPay: AgentPay,
                    UserId: data.elem.value
                },
                type: "POST",
                dataType: 'json',
                success: function (result) {
                    layer.close(index);
                    if (result.Success) {
                        layer.msg("操作成功！");
                    } else {
                        layer.msg(result.Message);
                    }
                },
                error: function (x, t, e) {
                    layer.closeAll();
                }
            });

        }, 500);
    });

    //列表操作
    table.on('tool(BusinessList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;

        //支付类型 
        if (layEvent === 'btn_paytype') {
            var index = layer.open({
                title: '[设置 <b style="color: red">' + data.MerName + "</b> 支付类型]",
                type: 2,
                area: ["530px", "550px"],
                content: "/Pay/UserInterfaceType?type=2&Code=" + data.UserId
            })
        }

        //修改费率
        if (layEvent === 'payreateedit') {
            var index = layer.open({
                title: "[修改 <b style='color:red'>" + data.MerName + "</b> 费率]",
                type: 2,
                area: ["800px", "385px"],
                content: "/Rate/Index?Type=2&UserId=" + data.UserId
            })
        }

        //风控
        if (layEvent === 'riskschemeedit') {
            var index = layer.open({
                title: "[修改 <b style='color:red'>" + data.MerName + "</b> 风控方案]",
                type: 2,
                area: ["800px", "385px"],
                content: "/Risk/RiskSettingView?RiskType=2&TargetId=" + data.UserId
            })
        }

        //冻结
        if (layEvent === 'btn_freeze') {
            SetIsEnabled(2, data.UserId);

        }


        //解冻
        if (layEvent === 'btn_defreeze') {
            SetIsEnabled(1, data.UserId);

        }

        ////增加余额
        //if (layEvent === 'addflow') {
        //    Addflow(data.UserId);
        //}


        //支付账户
        if (layEvent === 'btn_useraccount') {
            window.location.href = "/Business/PayAccountUserIndex?UserId=" + data.UserId;

            //var index = layer.open({
            //    title: '[设置 <b style="color: red">' + data.MerName + "</b> 支付账户 ]",
            //    type: 2,
            //    area: ["1200px", "830px"],
            //    content: "/Business/PayAccountUserIndex?UserId=" + data.UserId
            //})
        }

        //编辑查看
        if (layEvent === 'btn_editinfo') {
            // window.location.href = "/Business/BusinessUpdate?UserId=" + data.UserId;

            var index = layer.open({
                title: '[编辑商户 <b style="color: red">' + data.MerName + "</b> ]",
                type: 2,
                area: ["1000px", "680px"],
                content: "/Business/BusinessUpdate?UserId=" + data.UserId
            })

        }

        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该商户？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Business/DelBusiness", {
                    data: { UserId: data.UserId },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.Success) {
                            layer.msg("删除成功！");
                            Search();

                        } else {
                            layer.msg(result.Message);
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });

            });
        }


        //重置密码
        //if (layEvent == 'restpwd') {
        //    layer.confirm('确定重置该账号密码为 88888888 ？', { icon: 3, title: '提示信息' }, function (index) {
        //        hm.ajax("/Business/RestPwd", {
        //            data: { UserId: data.UserId },
        //            type: "POST",
        //            dataType: 'json',
        //            success: function (result) {
        //                if (result.Success) {
        //                    layer.msg("重置成功！");
        //                    layer.close(index);
        //                } else {
        //                    layer.msg(result.Message);
        //                }
        //            },
        //            error: function (x, t, e) {
        //                layer.closeAll();
        //            }
        //        });

        //    });
        //}

    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})