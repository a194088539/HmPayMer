

layui.use(['hm', 'form', 'layer', 'laydate', 'table', 'laytpl'], function () {
    var form = layui.form,
        layer = layui.layer,
        $ = layui.jquery,
        laydate = layui.laydate,
        laytpl = layui.laytpl,
        table = layui.table,
        hm = layui.hm;

    //通道管理
    var tableIns = table.render({
        elem: '#paychannelList',
        url: '/Pay/LoadRechargePayChannel',
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "paychannelList",
        cols: [[
            { field: 'Code', title: '编号', width: 130, align: "center" },
            { field: 'ChannelName', title: '通道名称', width: 150, align: "center" },
            { field: 'PayName', title: '支付类型', width: 150, align: "center" },
            { field: 'InterfaceName', title: '默认接口商', width: 150, align: "center" },
            {
                field: 'IsEnabled', title: '是否启用', align: 'center', width: 150, templet: function (d) {
                    if (d.IsEnable == 1) {
                        return '<input type="checkbox" checked name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Code + '" >';
                    } else {
                        return '<input type="checkbox" name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Code + '" >';
                    }
                }
            },

            { field: 'ChannelSort', title: '排序号', width: 130, align: "center" },
            { title: '操作', width: 180, templet: '#paychannelListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $(".addAdmin_btn").click(function () {
        var index = layer.open({
            title: "新增通道",
            type: 2,
            area: ["430px", "400px"],
            content: "/Pay/PayChannelAdd"
        })
    });


    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });

    function Search() {
        table.reload("paychannelList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                ChannelName: $(".searchVal").val(),  //搜索的关键字 layer.msg("请输入搜索的内容");
                PayCode: $("#PayCode").val()
            }
        })
    }

    //是否启用
    form.on('switch(IsEnabled)', function (data) {
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        setTimeout(function () {
            var IsEnabled = data.elem.checked ? 1 : 0;
            hm.ajax("/Pay/UpPayChannelEnabled", {
                data: {
                    IsEnabled: IsEnabled,
                    Code: data.elem.value
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
    table.on('tool(paychannelList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        if (layEvent === 'edit') {
            var index = layer.open({
                title: "编辑通道",
                type: 2,
                area: ["430px", "400px"],
                content: "/Pay/PayChannelUpdate?Code=" + data.Code
            })
        }

        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该通道？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Pay/DelChannel", {
                    data: { Code: data.Code },
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