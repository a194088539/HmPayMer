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
        case 1: return "正常";
        case 2: return "冻结";
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
        url: '/Agent/GetAgentList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "BusinessList",
        cols: [[
            {
                field: 'UserId', title: '代理ID', width: 130, align: "center", templet: function (d) {
                    return '<a class="layui-btn layui-btn-xs" lay-event="btn_editinfo">' + d.UserId + '</a>';
                }
            },
            {
                field: 'MerName', title: '代理名称', width: 130, align: "center"
            },

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
                field: 'AccountType', title: '签约类型', width: 60, align: "center",
                templet: function (d) {
                    return GetAccountType(d.AccountType);
                }
            },

            {
                field: 'IdCardStatus', title: '身份证', width: 80, align: "center",
                templet: function (d) {
                    return GetIsCertification(d.IdCardStatus);
                }
            },

            {
                field: 'IsMobilePhone', title: '手机', width: 60, align: "center",
                templet: function (d) {
                    return GetIsCertification(d.IsMobilePhone);
                }
            },

            {
                field: 'IsEmail', title: '邮箱', width: 60, align: "center",
                templet: function (d) {
                    return GetIsCertification(d.IsEmail);
                }
            },

            {
                title: '单独费率', width: 100, templet: function (d) {
                    return d.AloneRate == 1 ? "<span style='color:red;'>开启</span>" : "未开启";
                }
            },
            {
                title: '费率', width: 70, templet: function (d) {
                    return '<a class="layui-btn layui-btn-xs" lay-event="payreateedit">费率</a>';
                }
            },

            {
                field: 'IsEnabled', title: '状态', width: 80, align: "center",
                templet: function (d) {
                    return GetIsEnabled(d.IsEnabled);

                }
            },


            {
                field: 'RegTime', title: '注册时间', align: 'center', width: 180,
                templet: function (d) {
                    return hm.changeDateFormat(d.RegTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            {
                field: 'LastLoginTime', title: '最后登录', align: 'center', width: 180,
                templet: function (d) {
                    return hm.changeDateFormat(d.LastLoginTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            {
                field: 'LastLoginIp', title: '最后登录ip', align: 'center', width: 110
            },


            { title: '操作', width: 280, templet: '#BusinessListBar', fixed: "right", align: "center" }
        ]],
        response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    function SetIsEnabled(IsEnabled, userId) {
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        layer.confirm(IsEnabled == 1 ? "确定解冻该代理？" : "确定冻结该代理？", { icon: 3, title: '提示信息' }, function (index) {
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
            title: "添加代理",
            type: 2,
            area: ["1000px", "350px"],
            content: "/Agent/AddUc"
        })
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
                area: ["530px", "280px"],
                content: "/Pay/InterfaceType?type=4&Code=" + data.UserId
            })
        }

        //修改费率
        if (layEvent === 'payreateedit') {
            var index = layer.open({
                title: "[修改 <b style='color:red'>" + data.MerName + "</b> 费率]",
                type: 2,
                area: ["800px", "385px"],
                content: "/Rate/Index?Type=4&UserId=" + data.UserId
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

        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该代理？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Agent/DelBusiness", {
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

        //编辑查看
        if (layEvent === 'btn_editinfo') {
          var index = layer.open({
              title: '[编辑代理 <b style="color: red">' + data.MerName + "</b> ]",
              type: 2,
              area: ["1000px", "350px"],
              content: "/Agent/UpdateUc?UserId=" + data.UserId
          })

        }

    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})