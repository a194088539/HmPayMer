//订单状态 0：待处理、1：处理完成、2：锁定中、3：操作失败、4：支付过期 
function GetOrderState(val) {
    switch (val) {
        case 0: return '<font style="color: #0ae;">处理中</font>';
        case 1: return '<font style="color:#2c7;">成功</font>';
        case 2: return '<font style="color:#f00;">失败</font>';
    }
    return "";
}
//支付状态，0：待支付、1：支付成功、2：支付失败、3：支付过期 
function GetPayState(val) {
    switch (val) {
        case 0: return '<font style="color: #0ae;">处理中</font>';
        case 1: return '<font style="color:#2c7;">成功</font>';
        case 2: return '<font style="color:#f00;">失败</font>';
        case 3: return '<font style="color:#FF6100;">已过期</font>';
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


    //订单列表
    var tableIns = table.render({
        elem: '#OrderList',
        url: '/Order/QueryOrderList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-200",
        even: true,
        limit: 15,
        limits: [10, 15, 20, 25,30,50],
        id: "OrderList",
        cols: [[
            {
                field: 'UserId', title: '商户Id', width: 130, align: "center", sort: true,
                templet: function (d) {
                    return '<a class="layui-btn-xs layui-btn-mycss" lay-event="btn_userid">' + d.UserId + '</a>';
                }
            },
            { field: 'MerOrderNo', title: '商户订单号', width: 180, align: "center" },
            {
                field: 'OrderId', title: '系统订单号', width: 200, align: "center",
                templet: function (d) {
                    return '<a class="layui-btn-xs layui-btn-mycss" lay-event="btn_detail">' + d.OrderId + '</a>';
                }
            },
            { field: 'ChannelOrderNo', title: '接口商订单号', width: 120, align: "center" },

            { field: 'InterfaceName', title: '接口商', width: 120, align: "center" },
            {
                field: 'PayName', title: '支付类型', align: 'center', width: 90
            },
            {
                field: 'ChannelName', title: '通道', align: 'center', width: 150
            },
            {
                field: 'OrderAmt', title: '订单金额', width: 120, align: "center", 
                templet: function (d) {
                    return (d.OrderAmt / 100).toFixed(2);
                }
            },

            {
                field: 'MerAmt', title: '商户金额', width: 120, align: "center", 
                templet: function (d) {
                    return (d.MerAmt / 100).toFixed(2);
                }
            },

            {
                field: 'Profits', title: '利润', width: 80, align: "center",
                templet: function (d) {
                    return (d.Profits / 100).toFixed(2);
                }
            },

            {
                field: 'AgentAmt', title: '代理提成', width: 80, align: "center",
                templet: function (d) {
                    return (d.AgentAmt / 100).toFixed(2);
                }
            },

            {
                field: 'PromAmt', title: '业务员提成', width: 80, align: "center",
                templet: function (d) {
                    return (d.PromAmt / 100).toFixed(2);
                }
            },

            {
                field: 'OrderTime', title: '下单时间', align: 'center', width: 125, 
                templet: function (d) {
                    return hm.changeDateFormat(d.OrderTime, "MM-dd HH:mm:ss");
                }
            },
            

            {
                field: 'PayState', title: '支付状态', align: 'center', width: 90,
                templet: function (d) {
                    return GetPayState(d.PayState);
                }
            },

            {
                field: 'OrderState', title: '通知状态', align: 'center', width: 90,
                templet: function (d) {
                    return GetOrderState(d.OrderState);
                }
            },

            {
                title: '操作', width: 120, fixed: "right", align: "center",
                templet: function (d) {
                    var returnhtml = "";
                    if (Number(d.PayState) == 1) {
                        returnhtml += '<a class="layui-btn-xs layui-btn-mycss" lay-event="btn_sent">补发</a>';
                    }
                    if (Number(d.PayState) != 1) {
                        returnhtml += '<a class="layui-btn-xs layui-btn-mycss" lay-event="btn_budan">补单</a>';
                    }                    

                    return returnhtml;
                }
            }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
        done: function (res, curr, count) {
            var data = res.Data;
            console.info(data);
            $("#TotalOrderAmt").text(hm.amountToFixed(data.TotalOrderAmt, 2));
            $("#TotalCostAmt").text(hm.amountToFixed(data.TotalCostAmt, 2));
            $("#TotalPromAmt").text(hm.amountToFixed(data.TotalPromAmt, 2));
            $("#TotalAgentAmt").text(hm.amountToFixed(data.TotalAgentAmt, 2));
            $("#TotalProfits").text(hm.amountToFixed(data.TotalProfits, 2));
        }
    });


    var OrderState;
    var PayState;
    var ChannelId;

    form.on('select(OrderState)', function (data) {
        OrderState = data.value;

    });
    form.on('select(PayState)', function (data) {
        PayState = data.value;
    });
    form.on('select(ChannelId)', function (data) {
        ChannelId = data.value;
    });

    $("#span_orderimprotexcel").click(function () {
        layer.msg("正在导出订单......", { icon: 16, time: 1500 });
        var url = '/Order/OrderImprotExcel?ts=' + (new Date().getTime());
        var param = hm.serializeObject($("#form_seartch"));
        for (var key in param) {
            url += '&' + key + '=' + param[key];
        }
        console.info(url);
        $("#iframeOut").attr('src', url);
    });

    //列表操作
    table.on('tool(OrderList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        //补发
        if (layEvent === 'btn_sent') {
            setTimeout(function () {
                hm.ajax("/Order/Reissue", {
                    data: {
                        OrderId: data.OrderId
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.IsSuccess) {
                            layer.msg(result.message, { icon: 1 }, function () {
                                Search();
                            });

                        }
                        else {
                            layer.msg(result.message, { icon: 0 });
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });
            }, 500);
        } else if (layEvent === 'btn_budan') {
            setTimeout(function () {
                hm.ajax("/Order/MakeUpOrder", {
                    data: {
                        OrderId: data.OrderId
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.IsSuccess) {
                            layer.msg(result.message, { icon: 1 }, function () {
                                Search();
                            });
                        }
                        else {
                            layer.msg(result.message, { icon: 0 });
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });
            }, 500);
        }
        else if (layEvent === 'btn_detail') {
            layer.open({
                title: "订单详情",
                type: 2,
                area: ["1000px", "700px"],
                content: "/Order/OrderInfo?OrderId=" + data.OrderId
            });
        }
        else if (layEvent === 'btn_userid') {
            $("#UserId").val(data.UserId);
            $(".search_btn").click();
        }
    });


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
        table.reload("OrderList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch"))
        })
    }

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})