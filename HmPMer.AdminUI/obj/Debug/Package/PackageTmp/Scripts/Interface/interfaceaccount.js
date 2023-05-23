

layui.use(['hm', 'form', 'layer', 'laydate', 'table', 'laytpl', 'upload'], function () {
    var form = layui.form,
        layer = layui.layer,
        $ = layui.jquery,
        laydate = layui.laydate,
        laytpl = layui.laytpl,
        table = layui.table,
        upload = layui.upload,
        hm = layui.hm;

    var tableIns = table.render({
        elem: '#interfaceAccountList',
        url: '/Interface/LoadInterfaceAccount?Code=' + $("#Code").val(),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "interfaceAccountList",
        cols: [[
            {
                field: 'Id', type: 'checkbox', width: 30, align: "center"
            },
            { field: 'Code', title: '接口商编号', width: 110, align: "center" },
            {
                field: 'IsEnable', title: '是否启用', align: 'center', width: 150, templet: function (d) {
                    if (d.IsEnabled == 1) {
                        return '<input type="checkbox" checked name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Id + '" >';
                    } else {
                        return '<input type="checkbox" name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Id + '" >';
                    }
                }
            },
            { field: 'Account', title: '商户号', width: 300, align: "center" },
            { field: 'ChildAccount', title: '子商户号', width: 200, align: "center" },
            { field: 'AppId', title: 'AppId', width: 200, align: "center" },
            { field: 'OpenId', title: 'OpenId', width: 200, align: "center" },
            { field: 'BindDomain', title: '绑定域名', width: 200, align: "left" },
            { field: 'SubDomain', title: '提交域名', width: 200, align: "left" },
            { field: 'OrderNo', title: '排序号', width: 130, align: "center" },
            { title: '操作', width: 300, templet: '#interfaceAccountListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $(".add_btn").click(function () {
        var index = layer.open({
            title: "新增账户",
            type: 2,
            area: ["830px", "600px"],
            content: "/Interface/InterfaceAccountAdd?Code=" + $("#Code").val()
        })
    });

    $(".add_delbat").click(function () {
        var checkStatus = table.checkStatus('interfaceAccountList'),
            data = checkStatus.data,
            ids = [];
        if (data.length > 0) {
            for (var i in data) {
                ids.push(data[i].Id);
            }
            layer.confirm('是否批量删除这些账号，删除之后不可恢复！', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Interface/DelInterfaceAccountBat", {
                    data: {
                        idlist: ids
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.IsSuccess) {
                            layer.msg("操作成功！");
                            tableIns.reload();
                        } else {
                            layer.msg(result.Message);
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });
            })
        } else {
            layer.msg("请选择需要删除的账号");
        }
    });

    $(".add_cancel").click(function () {
        window.location.href = "/Interface/InterfaceBusinessIndex";
    });

    // 导入接口账户
    var uploadInst = upload.render({
        elem: '.add_import' //绑定元素
        , url: '/Interface/InterfaceAccountImport' //上传接口
        , method: "post"
        , accept: "file"
        , exts: "xlsx"
        , field: "file"
        , number: 1
        , data: { 'code': $("#Code").val() }
        , done: function (res, index, upload) {
            if (res.IsSuccess) {
                layer.msg("上传成功！", { icon: 1 });
                tableIns.reload();
            }
            else {
                layer.alert("上传失败！", { icon: 0 });
            }
        }
        , error: function () {
            layer.alert("上传文件出现异常！", { icon: 0 });
        }
    });

    //是否启用
    form.on('switch(IsEnabled)', function (data) {
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        setTimeout(function () {
            var IsEnabled = data.elem.checked ? 1 : 0;
            hm.ajax("/Interface/UpInterfaceAccountEnabled", {
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

    //搜索
    $(".search_btn").on("click", function () {
        table.reload("interfaceAccountList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                Account: $(".searchVal").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    });


    //列表操作
    table.on('tool(interfaceAccountList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        //编辑
        if (layEvent === 'btn_edit') {
            var index = layer.open({
                title: "编辑账户",
                type: 2,
                area: ["830px", "600px"],
                content: "/Interface/InterfaceAccountUpdate?Id=" + data.Id
            })
        }

        //查看
        if (layEvent === 'btn_deail') {
            var index = layer.open({
                title: "查看",
                type: 2,
                area: ["630px", "500px"],
                content: "/Interface/LoadInterfaceAccountDetail?Id=" + data.Id
            })
        }

        //删除
        if (layEvent === 'btn_del') {
            layer.confirm('是否批量删除此账号，删除之后不可恢复！', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Interface/DelInterfaceAccountBat", {
                    data: {
                        idlist: [data.Id]
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.IsSuccess) {
                            layer.msg("操作成功！");
                            tableIns.reload();
                        } else {
                            layer.msg(result.Message);
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });
            })
        }

        //风控设置
        if (layEvent === 'btn_risksetting') {
            var index = layer.open({
                title: "[修改 <b style='color:red'>" + data.Account + "</b> 风控方案]",
                type: 2,
                area: ["800px", "385px"],
                content: "/Risk/RiskSettingView?RiskType=3&TargetId=" + data.Id
            });
        }

    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})