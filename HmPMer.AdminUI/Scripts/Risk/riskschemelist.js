layui.use(['hm', 'form', 'layer', 'table', 'laytpl'], function () {
    var form = layui.form,
        layer = layui.layer,
        $ = layui.jquery,
        laytpl = layui.laytpl,
        table = layui.table,
        hm = layui.hm;

    //列表
    var tableIns = table.render({
        elem: '#RiskSchemeList',
        url: '/Risk/GetRiskSchemeList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "RiskSchemeList",
        cols: [[
            { field: 'SchemeName', title: '方案名称', width: 130, align: "center" },
            {
                field: 'RiskSchemeTaype', title: '方案类型', width: 130, align: "center", templet: function (d) {
                    switch (d.RiskSchemeTaype) {
                        case 1: return "接口商";
                        case 2: return "商户";
                    }
                    return "";
                }
            },
            { field: 'UserId', title: '所属商户', width: 130, align: "center" },
            {
                field: 'SingleMinAmt', title: '单笔最低限额', width: 130, align: "center", templet: function (d) {
                    return d.SingleMinAmt / 100;
                }
            },
            {
                field: 'SingleMaxAmt', title: '单笔最高限额', width: 130, align: "center", templet: function (d) {
                    return d.SingleMaxAmt / 100;
                }
            },
            {
                field: 'DayCount', title: '日交易次数', width: 130, align: "center", templet: function (d) {
                    return d.IsDayCount == 1 ? d.DayCount : "<b style='color:#f00;'>不限制</b>";
                }
            },
            {
                field: 'DayAmt', title: '日交易限额', width: 130, align: "center", templet: function (d) {
                    return d.IsDayAmt == 1 ? d.DayAmt/100 : "<b style='color:#f00;'>不限制</b>";
                }
            },
            {
                field: 'MonthCount', title: '月交易次数', width: 130, align: "center", templet: function (d) {
                    return d.IsMonthCount == 1 ? d.MonthCount : "<b style='color:#f00;'>不限制</b>";
                }
            },
            {
                field: 'MonthAmt', title: '月交易限额', width: 130, align: "center", templet: function (d) {
                    return d.IsMonthAmt == 1 ? d.MonthAmt/100 : "<b style='color:#f00;'>不限制</b>";
                }
            },
            { field: 'Sort', title: '排序', width: 130, align: "center" },
            { title: '操作', templet: '#RiskSchemeListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $("#btn_add").click(function () {
        var index = layer.open({
            title: "新增风控方案",
            type: 2,
            area: ["600px", "430px"],
            content: "/Risk/RiskSchemeAddView"
        })
    })

    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });

    function Search() {
        table.reload("RiskSchemeList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch"))
        })
    }


    //列表操作
    table.on('tool(RiskSchemeList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;

        if (layEvent === 'btn_edit') { //编辑
            var index = layer.open({
                title: "[修改<span style='color:red'>" + data.SchemeName + "</span>]",
                type: 2,
                area: ["600px", "430px"],
                content: "/Risk/RiskSchemeUpdateView?id=" + data.RiskSchemeId
            })
        }

        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该风控方案？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Risk/DelRiskScheme", {
                    data: { RiskSchemeId: data.RiskSchemeId },
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