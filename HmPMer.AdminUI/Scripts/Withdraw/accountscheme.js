
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
        elem: '#AccountSchemeList',
        url: '/Withdraw/GetAccountSchemeList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "AccountSchemeList",
        cols: [[
            { field: 'Id', title: '方案ID', width: 130, align: "center" },
            { field: 'name', title: '方案名称', width: 150, align: "center" },
            { title: '操作', width: 180, templet: '#AccountSchemeListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $(".addAdmin_btn").click(function () {
        var index = layer.open({
            title: "新增方案",
            type: 2,
            area: ["430px", "200px"],
            content: "/Withdraw/AddAccountScheme"
        })
    });

    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });


    function Search() {
        table.reload("AccountSchemeList", {
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
    table.on('tool(AccountSchemeList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        if (layEvent === 'edit') { //分配权限
            var index = layer.open({
                title: "编辑",
                type: 2,
                area: ["430px", "200px"],
                content: "/Withdraw/AddAccountScheme?id=" + data.Id
            })
        }

        if (layEvent === 'setdeail') { 
            var index = layer.open({
                title: "设置方案明细",
                type: 2,
                content: "/Withdraw/AccountSchemeDetailList?Id=" + data.Id
            })
            layer.full(index);
        }

        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该方案？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Withdraw/DelAccountScheme", {
                    data: { Id: data.Id },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.Success) {
                            layer.msg("删除成功！");
                            Search();

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