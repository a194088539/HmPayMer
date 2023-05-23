
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
        elem: '#AdminList',
        url: '/Account/AdminList',
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "AdminList",
        cols: [[
            { field: 'AdmUser', title: '账号', width: 130, align: "center" },
            { field: 'NickName', title: '昵称', width: 180, align: "center" },
            {
                field: 'IsEnable', title: '是否启用', align: 'center', templet: function (d) {
                    if (d.IsEnable == 1) {
                        return '<input type="checkbox" checked name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.ID + '" >';
                    } else {
                        return '<input type="checkbox" name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.ID + '" >';
                    }
                }
            },

            {
                field: 'AddTime', title: '注册时间', align: 'center', minWidth: 110,
                templet: function (d) {
                    return hm.changeDateFormat(d.AddTime, "yyyy-MM-dd");
                }
            },

            {
                field: 'Rate', title: '费率（%）', width: 150, align: "center",
                templet: function (d) {
                    return d.Rate*100;
                }
            },

            {
                field: 'LastLoginTime', title: '最后登录时间', align: 'center', minWidth: 110,
                templet: function (d) {
                    return hm.changeDateFormat(d.LastLoginTime, "yyyy-MM-dd HH:mm");
                }
            },

            {
                field: 'LastLoginIp', title: '最后登录Ip', align: 'center', minWidth: 80
            },

            { title: '操作', width: 200, templet: '#AdminListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });

    //是否启用
    form.on('switch(IsEnabled)', function (data) {
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        setTimeout(function () {           
            var IsEnabled = data.elem.checked ? 1 : 0;
            hm.ajax("/Account/UpIsEnabled", {
                data: {
                    IsEnabled: IsEnabled,
                    userId: "'" + data.elem.value + "'"
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

    //搜索
    $(".search_btn").on("click", function () {
        table.reload("AdminList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                AdmUser: $(".searchVal").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    });

    function Search() {
        table.reload("AdminList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                AdmUser: $(".searchVal").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    }

    //新增用户
    $("#addadmin_btn").click(function () {
        var index = layer.open({
            title: "新增用户",
            type: 2,
            area: ["530px", "480px"],
            content: "/Account/Add"
        })
    })

    //重置密码
    function RestPwd(Id) {
        layer.confirm('确定重置该账号密码为 888888 ？', { icon: 3, title: '提示信息' }, function (index) {
            hm.ajax("/Account/RestPwd", {
                data: { Id: Id },
                type: "POST",
                dataType: 'json',
                success: function (result) {
                    layer.close(index);
                    if (result.Success) {
                        layer.msg("重置成功！");
                        layer.close(index);
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


    //重置密码
    function UpdaeRate(Id) {
        var index = layer.open({
            title: "修改",
            type: 2,
            area: ["560px", "360px"],
            content: "/Account/Update?id=" + Id
        })
    }

    //列表操作
    table.on('tool(AdminList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        if (layEvent === 'editpwd') { //重置密码
            RestPwd(data.ID);

        }
        if (layEvent === 'editrate') { //编辑
            UpdaeRate(data.ID);
        }
        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该管理员账号？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Account/deleteHmAdmin", {
                    data: { Id: data.ID },
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