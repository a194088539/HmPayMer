function GetNotityState(val) {
    switch (val) {
        case 0: return "处理中";
        case 1: return "成功";
        case 2: return "过期";
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
        elem: '#ordernOtityIndexLis',
        url: '/Order/GetOrderNotityPageList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "ordernOtityIndexLis",
        cols: [[
            { field: 'UserId', title: '商户Id', width: 250, align: "center" },
            { field: 'OrderId', title: '订单ID', width: 250, align: "center" },
            {
                field: 'NotityState', title: '通知状态', width: 150, align: "center", templet: function (d) {
                    return GetNotityState(d.NotityState);
                }
            },
            { field: 'NotityCount', title: '通知次数', width: 150, align: "center" },
            { field: 'NotityUrl', title: '通知地址', width: 230, align: "center" },
            { field: 'NotityContext', title: '通知结果', width: 130, align: "center" },
            {
                field: 'NotityTime', title: '通知时间', align: 'center', width: 210,
                templet: function (d) {
                    return hm.changeDateFormat(d.NotityTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            { title: '操作', width: 100, templet: '#ordernOtityIndexLisBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
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
        table.reload("ordernOtityIndexLis", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch")) //搜索的关键字 layer.msg("请输入搜索的内容");

        })
    }

    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });


    //列表操作
    table.on('tool(ordernOtityIndexLis)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        if (layEvent === 'bufa') { //补发
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
        }
    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})