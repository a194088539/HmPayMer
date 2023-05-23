function GetWithdrawAccountType(val) {
    switch (val) {
        case 0: return "个人";
        case 1: return "企业";
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

   
    var tableIns = table.render({
        elem: '#BusinessList',
        url: '/Withdraw/LoadWithdrawUser',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 60,
        page: true,
        even: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "BusinessList",
        cols: [[

            { type: "checkbox", fixed: "left", width: 50 },
            { field: 'UserId', title: '商户号', width: 90, align: "center" },
            { field: 'MerName', title: '商户名称', width: 160, align: "center" },

            {
                field: 'Balance', title: '余额(元)', width: 100, align: "center",
                templet: function (d) {
                    return (d.Balance / 100).toFixed(2);
                }
            },

            {
                field: 'UnBalance', title: '不可用金额(元)', width: 100, align: "center",
                templet: function (d) {
                    return (d.UnBalance / 100).toFixed(2);
                }
            },

            {
                field: 'Freeze', title: '冻结金额(元)', width: 100, align: "center",
                templet: function (d) {
                    return (d.Freeze / 100).toFixed(2);
                }
            },

            {
                field: 'OrderAmt', title: '总交易量(元)', width: 100, align: "center",
                templet: function (d) {
                    return (d.OrderAmt / 100).toFixed(2);
                }
            },

            {
                field: 'WithdrawAccountType', title: '账户类型', width: 60, align: "center",
                templet: function (d) {
                    return GetWithdrawAccountType(d.WithdrawAccountType);
                }
            },

            { field: 'WithdrawBank', title: '开户行', width: 160, align: "center" },
            { field: 'WithdrawFactName', title: '开户姓名', width: 160, align: "center" },
            { field: 'WithdrawBankCode', title: '银行卡号', width: 200, align: "center" },
            { field: 'WithdrawBankBranch', title: '开户支行', width: 250, align: "center" },
            { title: '操作', width: 40, templet: '#BusinessListBar', fixed: "right", align: "center" }
        ]],
        response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });



    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });


    //批量审核
    $("#btn_audit_all").click(function () {
        var checkStatus = table.checkStatus('BusinessList'),
            data = checkStatus.data,
            idstr = [];

        if (data.length > 0) {
            for (let i in data) {
                idstr.push(data[i].UserId);
            }

            layer.confirm("确定审核清算所选商户？", { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Withdraw/SettleWithdraw", {
                    data: {
                        idstr: idstr.join(',')
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        layer.close(index);
                        if (result.Success) {
                            layer.msg("操作成功！");
                            window.location.href = window.location.href;
                        } else {
                            layer.msg(result.Message);
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });

            });

        } else {
            layer.msg("请选择要审核的数据！");
        }
    })

    //回车事件
    document.onkeydown = function (event) {
        var e = event || window.event || arguments.callee.caller.arguments[0];

        if (e && e.keyCode == 13) { // enter 键
            //要做的事情
            Search();
        }
    };



    function Search() {
        table.reload("BusinessList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch"))
        })
    }

    //列表操作
    table.on('tool(BusinessList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        //console.info(data.UserId); return;
        //审核 
        if (layEvent === 'btn_audit') {

            layer.confirm("确定审核清算所选商户？", { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/Withdraw/SettleWithdraw", {
                    data: {
                        idstr: data.UserId
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        layer.close(index);
                        if (result.Success) {
                            layer.msg("操作成功！");
                            window.location.href = window.location.href;
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