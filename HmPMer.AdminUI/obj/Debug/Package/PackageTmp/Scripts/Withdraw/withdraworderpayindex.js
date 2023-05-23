
function GetStatus(val) {
    switch (val) {
        case 0: return '<span class="layui-red">待付款</span>';
        case 1: return '<span class="layui-green">成功</span>';
        case 2: return '<span class="layui-blue">待确认</span>';
        case 3: return '<span class="layui-blue">失败</span>';
        case 4: return '<span class="layui-red">处理中</span>';
        case 5: return '<span class="layui-red">系统退回</span>';
    }
    return "";
}

function GetOrderTpe(val) {
    switch (val) {
        case 1: return '结算';
        case 2: return '代付';
        case 3: return '后台';
        case 4: return 'Api';
    }
    return "";
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
        elem: '#WithdrawOrderPayIndexList',
        url: '/Withdraw/GetWithdrawOrderPageList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "WithdrawOrderPayIndexList",
        cols: [[
            { field: 'UserId', title: '商户Id', width: 130, align: "center" },
            { field: 'OrderId', title: '订单Id', width: 220, align: "center" },
            {
                field: 'OrderType', title: '清算来源', width: 150, align: "center", templet: function (d) {
                    return GetOrderTpe(d.OrderType);
                }
            },


            {
                field: 'WithdrawAmt', title: '提现金额', width: 130, align: "center", templet: function (d) {
                    return (parseFloat(d.WithdrawAmt) / 100).toFixed(2);
                }
            },
            {
                field: 'Handing', title: '手续费', width: 130, align: "center", templet: function (d) {
                    return (parseFloat(d.Handing) / 100).toFixed(2);
                }
            },
            {
                field: 'Amt', title: '扣款总额', width: 130, align: "center", templet: function (d) {
                    return (parseFloat(d.Amt) / 100).toFixed(2);
                }
            },

            {
                field: 'PayState', title: '付款状态', width: 100, align: "center",
                templet: function (d) {
                    return GetStatus(d.PayState);
                }
            },
            { field: 'AuditDesc', title: '说明', width: 220, align: "center" },
            {
                field: 'AddTime', title: '申请时间', align: 'center', width: 180,  
                templet: function (d) {
                    return hm.changeDateFormat(d.AddTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            { field: 'ChannelOrderNo', title: '通道订单号', width: 130, align: "center" },
            { field: 'BankName', title: '银行名称', width: 130, align: "center" },
            { field: 'BankCode', title: '银行卡号', width: 200, align: "center" },
            { field: 'FactName', title: '真实姓名', width: 130, align: "center" },
            {
                field: '', title: '支行地址', width: 230, align: "center", templet: function (d) {
                    return d.ProvinceName + d.CityName + d.BankAddress;
                }
            },

            { field: 'InterfaceName', title: '接口商', width: 130, align: "center" },
            { field: 'Attach', title: '备注', width: 130, align: "center" },


            {
                title: '操作', width: 180, templet: '#WithdrawOrderPayIndexListBar', fixed: "right", align: "center",
                templet: function (d) {
                    var returnhtml = "";
                    //待支付
                    if (Number(d.PayState) == 0) {
                        returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_pay">付款</a>';
                        returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_return">退回</a>';
                    }
                    //支付失败
                    if (Number(d.PayState) == 2) {
                        returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_fail">确认失败</a>';
                        returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_pay">付款</a>';
                    }
                    //处理中
                    if (Number(d.PayState) == 4) {
                        returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_search">查询</a>';
                    }

                    return returnhtml;
                }
            }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });



    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });

    $("#span_orderimprotexcel").click(function () {
        layer.msg("正在导出订单......", { icon: 16, time: 1500 });
        var url = '/Withdraw/OrderImprotExcel?ts=' + (new Date().getTime());
        var param = hm.serializeObject($("#form_seartch"));
        for (var key in param) {
            url += '&' + key + '=' + param[key];
        }
        console.info(url);
        $("#iframeOut").attr('src', url);
    });

    //搜索方法
    function Search() {
        console.info(hm.serializeObject($("#form_seartch")));
        table.reload("WithdrawOrderPayIndexList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch"))
        })
    }


    //列表操作
    table.on('tool(WithdrawOrderPayIndexList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;

        if (layEvent == 'btn_pay') {
            var index = layer.open({
                title: '[付款 <b style="color: red">' + data.OrderId + "</b> ]",
                type: 2,
                area: ["400px", "300px"],
                content: "/Withdraw/UpdateWithdrawOrder?OrderId=" + data.OrderId
            })
        }

        //确认失败
        if (layEvent == 'btn_fail') {
            layer.confirm("确认该支付失败？", { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Withdraw/PointFailTrue", {
                    data: {
                        OrderId: data.OrderId
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.Success) {
                            layer.msg("确认成功！", { icon: 1 }, function () {
                                Search();
                            });

                        }
                        else {
                            layer.msg(result.Message, { icon: 0 });
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });
            });
        }  
        //退回
        if (layEvent == 'btn_return') {
            layer.confirm("确认退回该支付？", { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Withdraw/PointFailReturn", {
                    data: {
                        OrderId: data.OrderId
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.Success) {
                            layer.msg("退回成功！", { icon: 1 }, function () {
                                Search();
                            });

                        }
                        else {
                            layer.msg(result.Message, { icon: 0 });
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });
            });
        }

        //查询处理中的订单 
        if (layEvent == 'btn_search') {
            setTimeout(function () {
                hm.ajax("/Withdraw/PayWithdrawSeartch", {
                    data: {
                        OrderId: data.OrderId
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.Success) {
                            layer.msg("查询成功", { icon: 1 }, function () {
                                Search();
                            });

                        }
                        else {
                            layer.msg(result.Message, { icon: 0 });
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });
            }, 500);
        }

    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})