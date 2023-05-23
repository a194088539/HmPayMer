
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
        elem: '#AccountschemedetailList',
        url: '/Withdraw/GetAccountSchemeDetailList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "AccountSchemeList",
        cols: [[
            { field: 'AccountSchemeId', title: '方案ID', width: 130, align: "center" },
            { field: 'StartimeStr', title: '开始时间', width: 150, align: "center" },
            { field: 'EndTimeStr', title: '结束时间', width: 150, align: "center" },
            { field: 'SchemeType1', title: '入账类型', width: 150, align: "center" },
            { field: 'TDay1', title: '到账时间', width: 150, align: "center" },
            { field: 'AmtSingle1', title: '到账比例', width: 150, align: "center" },
            { field: 'SchemeType2', title: '入账类型', width: 150, align: "center" },
            { field: 'TDay2', title: '到账时间', width: 150, align: "center" },
            { field: 'AmtSingle2', title: '到账比例', width: 150, align: "center" },
            { title: '操作', width: 180, templet: '#AccountschemedetailListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $(".addAdmin_btn").click(function () {
        var index = layer.open({
            title: "新增方案明细",
            type: 2,
            area: ["1000px", "300px"],
            content: "/Withdraw/AddSchemeDetail?Id=" + $("#AccountSchemeId").val()
        })
    });

    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });


    function Search() {
        table.reload("AccountschemedetailList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch")),
        })
    }

    //回车事件
    document.onkeydown = function (event) {
        var e = event || window.event || arguments.callee.caller.arguments[0];

        if (e && e.keyCode == 13) { // enter 键
            //要做的事情
            Search();
        }
    };

    //列表操作
    table.on('tool(AccountschemedetailList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;

        if (layEvent === 'edit') {
            var index = layer.open({
                title: "编辑",
                type: 2,
                area: ["1000px", "300px"],
                content: "/Withdraw/UpdateAccountSchemeDetail?id=" + data.Id
            })
        }

        //删除
        if (layEvent === 'delete') { 
            layer.confirm("确定删除该方案明细？", { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Withdraw/DelAccountSchemeDetail", {
                    data: {
                        Id: data.Id
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        layer.close(index);
                        if (result.Success) {
                            layer.msg("操作成功！");
                            window.location.href = window.location.href;
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