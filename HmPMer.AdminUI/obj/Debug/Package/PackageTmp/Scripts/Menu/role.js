function GetUserType(val) {
    switch (val) {
        case 0: return "普通商户";
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
        elem: '#roleList',
        url: '/MenuRole/LoadRechargeRole',
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "roleList",
        cols: [[
            { field: 'roleName', title: '角色名称', width: 130, align: "center" },
            { field: 'describe', title: '描述', width: 380, align: "center" },
            {
                field: 'createTime', title: '添加时间', align: 'center', minWidth: 110,
                templet: function (d) {
                    return hm.changeDateFormat(d.createTime, "yyyy-MM-dd");
                }
            },
            { field: 'createUser', title: '添加人', width: 380, align: "center" }, 
            {
                field: 'modifyTime', title: '最后修改时间', align: 'center', minWidth: 110,
                templet: function (d) {
                    return hm.changeDateFormat(d.modifyTime, "yyyy-MM-dd");
                }
            },
            { field: 'modifyUser', title: '修改人', width: 380, align: "center" },
            { title: '操作', width: 180, templet: '#roleListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $(".addAdmin_btn").click(function () {
        var index = layer.open({
            title: "新增用户",
            type: 2,
            area: ["430px", "250px"],
            content: "/MenuRole/AddRole"
        })
    })

    //搜索
    $(".search_btn").on("click", function () {
        table.reload("roleList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                roleName: $(".searchVal").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    });

    function Search() {
        table.reload("roleList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                roleName: $(".searchVal").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    }



    //列表操作
    table.on('tool(roleList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        if (layEvent === 'edit') { //分配权限
            var index = layer.open({
                title: "[分配<span style='color:red'>" + data.roleName + "</span>权限]",
                type: 2,
                area: ["950px", "485px"],
                content: "/MenuRole/SetRoleFlag?RoleId=" + data.Id
            })
        }   

        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该角色？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/MenuRole/DelRole", {
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