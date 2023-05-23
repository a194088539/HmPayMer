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
        elem: '#OrderAccountIndexList',
        url: '/Withdraw/GetOrderAccountList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "OrderAccountIndexList",
        cols: [[
            { field: 'UserId', title: '商户号', width: 80, align: "center" },
            { field: 'OrderId', title: '订单ID', width: 150, align: "center" },
            {
                title: '订单金额', width: 150, align: "center", templet: function (d) {
                    return hm.amountToFixed(d.OrderAmt);
                }
            },
            {
                title: '入账方案', width: 300, align: "center", templet: function (d) {
                    var str = '【' + d.AccountrRnge + '】';
                    str += ' [' + d.SchemeType == 0 ? 'D' : 'T';
                    str += '+' + d.TDay;
                    str += '] 到账比例' + d.AmtSingle + "%";
                    return str;
                }
            },            
            {
                field: 'Amt', title: '入账金额', width: 100, align: "center", templet: function (d) {
                    return hm.amountToFixed(d.Amt);
                }
            },  
            {
                field: 'AccountState', title: '入账状态', align: 'center', width: 100,
                templet: function (d) {
                    if (d.AccountState==0) return '待入账';
                    return '已入账';
                }
            },
            {
                field: 'AddTime', title: '提交时间', align: 'center', width: 210,
                templet: function (d) {
                    return hm.changeDateFormat(d.AddTime, "yyyy-MM-dd HH:mm:ss");
                }
            },
            {
                field: 'AccountTime', title: '待到账时间', align: 'center', width: 210,
                templet: function (d) {
                    return hm.changeDateFormat(d.AccountTime, "yyyy-MM-dd HH:mm:ss");
                }
            }, {
                field: 'EndTime', title: '已到账时间', align: 'center', width: 210,
                templet: function (d) {
                    if (!d.EndTime) return '';
                    return hm.changeDateFormat(d.EndTime, "yyyy-MM-dd HH:mm:ss");
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
        table.reload("OrderAccountIndexList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch")) //搜索的关键字 layer.msg("请输入搜索的内容");
        });
    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})