 //0、待交易 1、交易成功 2、失败 3、付款中
function GetStatus(val) {
    switch (val) { 
        case 0: return '<span class="layui-red">待交易</span>';
        case 1: return '<span class="layui-green">成功</span>';
        case 2: return '<span class="layui-blue">失败</span>';
        case 3: return '<span class="layui-blue">付款中</span>';
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
        elem: '#WithdrawOrderTarnIndexList',
        url: '/Withdraw/GetWithdrawOrderTarnPageList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "WithdrawOrderTarnIndexList",
        cols: [[
            { field: 'UserId', title: '商户号', width: 80, align: "center" },
            { field: 'OrderId', title: '订单ID', width: 150, align: "center" },
            { field: 'ChannelOrderNo', title: '通道订单ID', width: 150, align: "center" },
            
            { field: 'InterfaceName', title: '支付接口', width: 100, align: "center" },
            
            { field: 'FactName', title: '姓名', width: 130, align: "center" },
            { field: 'BankCode', title: '银行账号', width: 130, align: "center" },
            { field: 'BankAddress', title: '支行地址', width: 180, align: "center" },
            {
                field: 'Amount', title: '交易金额', width: 100, align: "center", templet: function (d) {
                    return (parseFloat(d.Amount) / 100).toFixed(2);
                }
            },
            {
                field: 'Handing', title: '手续费', width: 100, align: "center", templet: function (d) {
                    return (parseFloat(d.Handing) / 100).toFixed(2);
                }
            },
            {
                field: 'TarnState', title: '状态', width: 100, align: "center",
                templet: function (d) {
                    return GetStatus(d.TarnState);
                }
            },

            { field: 'TarnRemark', title: '备注', width: 130, align: "center" },


            
            {
                field: 'AddTime', title: '提交时间', align: 'center', width: 210,
                templet: function (d) {
                    return hm.changeDateFormat(d.AddTime, "yyyy-MM-dd HH:mm:ss");
                }
            }

        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });

    $("#span_orderimprotexcel").click(function () {
        layer.msg("正在导出订单......", { icon: 16, time: 1500 });
        var url = '/Withdraw/OrderTarnImprotExcel?ts=' + (new Date().getTime());
        var param = hm.serializeObject($("#form_seartch"));
        for (var key in param) {
            url += '&' + key + '=' + param[key];
        }
        console.info(url);
        $("#iframeOut").attr('src', url);
    });

    //搜索
    $(".search_btn").on("click", function () {
        table.reload("WithdrawOrderTarnIndexList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch")) //搜索的关键字 layer.msg("请输入搜索的内容");

        })
    });

    //$("#btn_excel").on("click", function () {
    //    // window.location.href = "/Withdraw/ImprotExcel?model=" + hm.serializeObject($("#form_seartch"));
    //    alert(1);
    //    hm.ajax("/Withdraw/ImprotExcel", {
    //        data: hm.serializeObject($("#form_seartch")),
    //        type: "get",
    //        dataType: 'Excel',
    //        success: function (result) {
    //            layer.close(index);
    //            if (result.Success) {
    //                layer.msg("操作成功！");
    //            } else {
    //                layer.msg(result.Message);
    //            }
    //        },
    //        error: function (x, t, e) {
    //            layer.closeAll();
    //        }
    //    });

    //});

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})