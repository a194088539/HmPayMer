
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
        elem: '#WithdrawSchemeList',
        url: '/Withdraw/GetWithdrawSchemePageList',
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "WithdrawSchemeList",
        cols: [[
            { field: 'SchemeName', title: '方案名称', width: 130, align: "center" },
            {
                field: 'SchemeType', title: '方案类别', width: 100, align: "center", templet: function (d) {
                    return d.SchemeType == 1 ? "T" : "D";
                }
            },
            {
                field: 'UserType', title: '使用类别', width: 100, align: "center", templet: function (d) {
                    return d.UserType == 1 ? "代理" : "商户";
                }
            },
            {
                field: 'MinAmtSingle', title: '最低提现金额', width: 120, align: "center", templet: function (d) {
                    return parseFloat(d.MinAmtSingle) / 100;
                }
            },
            {
                field: 'MaxAmtSingle', title: '最高提现金额', width: 120, align: "center", templet: function (d) {
                    return parseFloat(d.MaxAmtSingle) / 100;
                }
            },
            { field: 'MaxtDay', title: '最高提现次数', width: 120, align: "center" },
            {
                field: 'LimitAmtDay', title: '提现限额', width: 130, align: "center", templet: function (d) {
                    return parseFloat(d.LimitAmtDay) / 100;
                }
            },
            {
                field: 'IsMinHandingSingle', title: '最低手续费限制', width: 130, align: "center", templet: function (d) {
                    return d.IsMinHandingSingle == 1 ? "<b style='color:red'>是</b>" : "否";
                }
            },
            {
                field: 'MinHandingSingle', title: '最低手续费', width: 130, align: "center", templet: function (d) {
                    return parseFloat(d.MinHandingSingle) / 100;
                }
            },
            {
                field: 'IsMaxHandingSingle', title: '最高手续费限制', width: 130, align: "center", templet: function (d) {
                    return d.IsMaxHandingSingle == 1 ? "<b style='color:red'>是</b>" : "否";
                }
            },
            {
                field: 'MaxHandingSingle', title: '最高手续费', width: 130, align: "center", templet: function (d) {
                    return parseFloat(d.MaxHandingSingle) / 100;
                }
            },
            {
                field: 'IsInterface', title: '是否走接口', width: 130, align: "center", templet: function (d) {
                    return d.IsInterface == 1 ? "<b style='color:red'>是</b>" : "否";
                }
            },

            { field: 'InterfaceName', title: '默认结算接口', width: 130, align: "center" },

            { field: 'Sort', title: '排序', width: 130, align: "center" },
            { title: '操作', width: 180, templet: '#WithdrawSchemeListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $("#btn_add").click(function () {
        var index = layer.open({
            title: "新增结算方案",
            type: 2,
            area: ["880px", "550px"],
            content: "/Withdraw/WithdrawSchemeAddUc"
        })
    })

    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });

    function Search() {
        table.reload("WithdrawSchemeList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                roleName: $(".searchVal").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    }


    //列表操作
    table.on('tool(WithdrawSchemeList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        if (layEvent === 'edit') { 
            var index = layer.open({
                title: "[修改<span style='color:red'>" + data.SchemeName + "</span>]",
                type: 2,
                area: ["880px", "550px"],
                content: "/Withdraw/WithdrawSchemeUpdateUc?Id=" + data.Id
            })
        }

        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该方案？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Withdraw/DelWithdrawScheme", {
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