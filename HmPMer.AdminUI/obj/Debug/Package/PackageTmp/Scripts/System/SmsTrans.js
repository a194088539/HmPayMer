

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
        elem: '#SmsTransList',
        url: '/System/GetSmsTransPageList',
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "SmsTransList",
        cols: [[
            { field: 'Mobile', title: '手机', width: 150, align: "center" },

            { field: 'Content', title: '短信内容', width: 350, align: "center" },

            {
                field: 'AddTime', title: '添加时间', align: 'center', width: 200,
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


    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });

    function Search() {
        table.reload("SmsTransList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                Mobile: $(".searchVal").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    }


    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})