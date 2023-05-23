
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
        elem: '#UserGradeList',
        url: '/Grade/LoadUserGrade',
        where: { UserType: $("#UserType").val() },
        cellMinWidth: 95,
        id: "UserGradeList",
        cols: [[
            { field: 'Id', title: 'Id', width: 130, align: "center" },
            { field: 'GradeName', title: '等级名称', width: 180, align: "center" },
            {
                title: '费率', width: 70, templet: function (d) {
                    return '<a class="layui-btn layui-btn-xs" lay-event="payreateedit">费率</a>';
                }
            },
            { title: '操作', width: 350, templet: '#UserGradeListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $("#btn_add").click(function () {

        var index = layer.open({
            title: "[新增等级]",
            type: 2,
            area: ["400px", "205px"],
            content: "/Grade/UserGradeAdd?UserType=" + $("#UserType").val()
        })

    });

    //列表操作
    table.on('tool(UserGradeList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;

        //修改费率
        if (layEvent === 'btn_edit') {
            var index = layer.open({
                title: "[修改等级]",
                type: 2,
                area: ["400px", "205px"],
                content: "/Grade/UserGradeUpdate?Id=" + data.Id
            })
        }

        //修改费率
        if (layEvent === 'payreateedit') {
            var index = layer.open({
                title: "[修改 <b style='color:red'>" + data.GradeName + "</b> 费率]",
                type: 2,
                area: ["800px", "385px"],
                content: "/Rate/Index?Type=3&UserId=" + data.Id
            })
        }


        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该等级？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Grade/DelUserGrade", {
                    data: { Id: data.Id },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.Success) {
                            layer.msg("删除成功！");
                            tableIns.reload();

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