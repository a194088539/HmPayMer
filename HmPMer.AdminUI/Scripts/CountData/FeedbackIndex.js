
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
        elem: '#FeedbackIndexList',
        url: '/CountData/LoadFeedbackPage',
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "FeedbackIndexList",
        cols: [[
            { field: 'UserId', title: '商户ID', width: 250, align: "center" },
            { field: 'Title', title: '标题', width: 250, align: "center" },
            { field: 'Content', title: '内容', width: 400, align: "center" },
            {
                field: 'AddTime', title: '添加时间', align: 'center', width: 200,
                templet: function (d) {
                    return hm.changeDateFormat(d.AddTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            {
                title: '操作', width: 150, fixed: "right", align: "center",
                templet: function (d) {
                    var returnhtml =   '<a class="layui-btn layui-btn-xs layui-btn-danger" lay-event="btn_edit">回复</a>';
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

    function Search() {
        table.reload("FeedbackIndexList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                UserId: $("#UserId").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    }


    //列表操作
    table.on('tool(FeedbackIndexList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        if (layEvent === 'btn_edit') { //回复
            var index = layer.open({
                title: "回复",
                type: 2,
                area: ["800px", "550px"],
                content: "/CountData/FeedbackReply?id=" + data.Id
            })
        }
    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})