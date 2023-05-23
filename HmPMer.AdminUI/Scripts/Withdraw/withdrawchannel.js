
layui.use(['hm', 'form', 'layer', 'laydate', 'table', 'laytpl'], function () {
    var form = layui.form,
        layer = layui.layer,
        $ = layui.jquery,
        laydate = layui.laydate,
        laytpl = layui.laytpl,
        table = layui.table,
        hm = layui.hm;

    //列表
    var tableIns = table.render({
        elem: '#WithdrawChannelList',
        url: '/Withdraw/GetWithdrawChannelPageList',
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "WithdrawChannelList",
        cols: [[
            { field: 'Code', title: '编码', width: 130, align: "center" },
            { field: 'Name', title: '名称', width: 130, align: "center" },
            //{ field: 'InterfaceName', title: '接口商名称', width: 130, align: "center" },
            {
                field: 'IsEnabled', title: '是否启用', align: 'center', width: 130, templet: function (d) {
                    if (d.IsEnabled == 1) {
                        return '<input type="checkbox" checked name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Id + '" >';
                    } else {
                        return '<input type="checkbox" name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Id + '" >';
                    }
                }
            },
            { field: 'Sort', title: '排序', width: 130, align: "center" }
            //{ title: '操作', width: 180, templet: '#WithdrawChannelListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $("#btn_add").click(function () {
        var index = layer.open({
            title: "新增结算通道",
            type: 2,
            area: ["430px", "300px"],
            content: "/Withdraw/WithdrawChannelAddUc"
        })
    })

    //搜索
    $(".search_btn").on("click", function () {
        table.reload("WithdrawChannelList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                Name: $("#Name").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    });


    //是否启用
    form.on('switch(IsEnabled)', function (data) {
       
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        setTimeout(function () {
            var IsEnabled = data.elem.checked ? 1 : 0;
            hm.ajax("/Withdraw/UpWCIsEnabled", {
                data: {
                    IsEnabled: IsEnabled,
                    Id: data.elem.value
                },
                type: "POST",
                dataType: 'json',
                success: function (result) {
                    layer.close(index);
                    if (result.Success) {
                        layer.msg("操作成功！");
                    } else {
                        layer.msg(result.Message);
                    }
                },
                error: function (x, t, e) {
                    layer.closeAll();
                }
            });

        }, 500);
    });

    //列表操作
    table.on('tool(WithdrawChannelList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        if (layEvent === 'edit') { //分配权限
            var index = layer.open({
                title: "[修改<span style='color:red'>" + data.Name + "</span>]",
                type: 2,
                area: ["430px", "300px"],
                content: "/Withdraw/WithdrawChannelUpdateUc?Id=" + data.Id
            })
        }
    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})