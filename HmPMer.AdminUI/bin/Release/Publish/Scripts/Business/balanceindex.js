

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
        elem: '#balanceindexList',
        url: '/DataManage/GetTradeList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-25",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "balanceindexList",
        cols: [[
            { field: 'UserId', title: '商户ID', width: 150, align: "center" },
            { field: 'TradeId', title: '操作流水号', width: 130, align: "center" },
            {
                field: 'Type', title: '操作类型', width: 150, align: "center", templet: function (d) {
                    return d.Type == 1 ? "增加" : "减少";
                }
            },
            {
                field: 'BeforeAmount', title: '原有金额', width: 150, align: "center", templet: function (d) {
                    return (d.BeforeAmount / 100).toFixed(2);
                }
            },
            {
                field: 'Amount', title: '异动金额', width: 150, align: "center", templet: function (d) {
                    return (d.Amount / 100).toFixed(2);
                }
            },

            {
                field: 'Balance', title: '异动后金额', width: 150, align: "center", templet: function (d) {
                    return (d.Balance / 100).toFixed(2);
                }
            },

            {
                field: 'TradeTime', title: '异动时间', align: 'center', width: 180,
                templet: function (d) {
                    return hm.changeDateFormat(d.TradeTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            { field: 'Remark', title: '备注', minWidth: 130, align: "center" },

        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $(".addAdmin_btn").click(function () {
        var index = layer.open({
            title: "增减余额",
            type: 2,
            area: ["430px", "330px"],
            content: "/DataManage/AddBalance"
        })
    });


    //搜索
    $(".search_btn").on("click", function () {
        table.reload("balanceindexList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch")),
        })
    });


    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})