
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
        elem: '#OrderAccountTarnIndexList',
        url: '/Withdraw/GetOrderAccountTarnList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "OrderAccountTarnIndexList",
        cols: [[
            { field: 'UserId', title: '商户号', width: 80, align: "center" },
            { field: 'MerName', title: '商户名称', width: 150, align: "center" },
            { field: 'Balance', title: '账户余额', width: 150, align: "center" },
            { field: 'UnBalance', title: '在途资金', width: 150, align: "center" },
            { field: 'Amt', title: '待入账金额', width: 150, align: "center" },
            { field: 'AccountCount', title: '待入账笔数', width: 150, align: "center" },   
            { title: '操作', width: 200, templet: '#BusinessListBar', fixed: "right", align: "center" }           
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });

    console.log(tableIns);

    //搜索
    $(".search_btn").on("click", function () {
        tableIns.reload("OrderAccountTarnIndexList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch")) //搜索的关键字 layer.msg("请输入搜索的内容");

        })
    });
    
    //列表操作
    table.on('tool(OrderAccountTarnIndexList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        
        //审核 
        if (layEvent === 'btn_confirm') {
            layer.confirm("是否确认入账？", { icon: 3, title: '提示信息' }, function (index) {
                var ddd = [];
                ddd.push({ UserId: data.UserId });                
                hm.ajax("Withdraw/ConfirmOrderAccountTarnBat", {
                    data: { list: ddd},
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        layer.close(index);
                        if (result.Success) {
                            layer.msg("操作成功！");
                            setTimeout(function () {
                                $(".search_btn").click();
                            }, 1200);
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
    });
    
    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

    


})