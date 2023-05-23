

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
        elem: '#interfaceBusinessList',
        url: '/Interface/LoadInterfaceBusiness',
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "interfaceBusinessList",
        cols: [[
            { field: 'Code', title: '编号', width: 130, align: "center" },
            { field: 'Name', title: '接口商名称', width: 150, align: "center" },

            { field: 'SubMitUrl', title: '提交地址', width: 300, align: "left" },

            {
                field: 'IsEnable', title: '是否启用', align: 'center', width: 100, templet: function (d) {
                    if (d.IsEnabled == 1) {
                        return '<input type="checkbox" checked name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Code + '" >';
                    } else {
                        return '<input type="checkbox" name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Code + '" >';
                    }
                }
            },

            {
                field: 'AgentPay', title: '代付功能', align: 'center', width: 100, templet: function (d) {
                    if (d.AgentPay == 1) {
                        return '<input type="checkbox" checked name="AgentPay" lay-filter="AgentPay" lay-skin="switch" lay-text="开启|关闭"  value="' + d.Code + '" >';
                    } else {
                        return '<input type="checkbox" name="AgentPay" lay-filter="AgentPay" lay-skin="switch" lay-text="开启|关闭"  value="' + d.Code + '" >';
                    }
                }
            },

            { field: 'Account', title: '接口商商户号', width: 300, align: "center" },
            {
                title: '费率', width: 70, templet: function (d) {
                    return '<a class="layui-btn layui-btn-xs" lay-event="payreateedit">费率</a>';
                }
            },
            {
                title: '风控', width: 120, align: "center", templet: function (d) {
                  
                    if (d.RiskSchemeId)
                        return '<a class="layui-btn layui-btn-xs" lay-event="payriskedit">' + d.RiskSchemeName + '</a>';
                    return '<a class="layui-btn layui-btn-xs" lay-event="payriskedit">设置风控</a>';
                }
            },


            {
                title: '操作', width: 300, fixed: "right", align: "center",
                templet: function (d) {
                    var returnhtml = "";
                    if (Number(d.AccType) == 1) {
                        returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_child">子账户</a>';
                    }
                    returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_paytype">支付类型</a>';
                    returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_edit">编辑</a>';
                    returnhtml += '<a class="layui-btn layui-btn-xs layui-btn-danger" lay-event="deldata">删除</a>';
                    returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_detail">查看</a>';
                    return returnhtml;
                }
            }

        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $(".add_btn").click(function () {
        var index = layer.open({
            title: "新增接口商",
            type: 2,
            area: ["830px", "600px"],
            content: "/Interface/InterfaceBusinessAdd"
        })
    });


    //是否启用
    form.on('switch(IsEnabled)', function (data) {
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        setTimeout(function () {
            var IsEnabled = data.elem.checked ? 1 : 0;
            hm.ajax("/Interface/UpInterfaceBusinessEnabled", {
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

    form.on('switch(AgentPay)', function (data) {
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        setTimeout(function () {
            var AgentPay = data.elem.checked ? 1 : 0;
            hm.ajax("/Interface/UpInterfaceBusinessAgentPay", {
                data: {
                    AgentPay: AgentPay,
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



    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });

    function Search() {
        table.reload("interfaceBusinessList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                Name: $(".searchVal").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    }


    //列表操作
    table.on('tool(interfaceBusinessList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        //编辑
        if (layEvent === 'btn_edit') {
            var index = layer.open({
                title: "[编辑接口商 " + data.Name + "]",
                type: 2,
                area: ["830px", "600px"],
                content: "/Interface/InterfaceBusinessUpdate?Code=" + data.Code
            })
        }

        //修改费率
        if (layEvent === 'payreateedit') {
            var index = layer.open({
                title: "[修改 <b style='color:red'>" + data.Name + "</b> 费率]",
                type: 2,
                area: ["800px", "385px"],
                content: "/Rate/Index?Type=1&UserId=" + data.Code
            })
        }

        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该接口商？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Interface/DelInterface", {
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

        //风控方案
        if (layEvent === 'payriskedit') {
            var index = layer.open({
                title: "[修改 <b style='color:red'>" + data.Name + "</b> 风控方案]",
                type: 2,
                area: ["800px", "385px"],
                content: "/Risk/RiskSettingView?RiskType=1&TargetId=" + data.Code
            })
        }

        //支付类型 
        if (layEvent === 'btn_paytype') {
            var index = layer.open({
                title: '[设置 ' + data.Name + " 支付类型]",
                type: 2,
                area: ["630px", "350px"],
                content: "/Pay/InterfaceType?type=1&Code=" + data.Code
            })
        }

        //子账号
        if (layEvent == 'btn_child') {
            window.location.href = "/Interface/InterfaceAccountIndex?Code=" + data.Code;
        }

        //查看
        if (layEvent === 'btn_detail') {
            var index = layer.open({
                title: '[' + data.Name + "查看]",
                type: 2,
                area: ["630px", "550px"],
                content: "/Interface/InterfaceBusinessDetail?Code=" + data.Code
            })
        }

    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})