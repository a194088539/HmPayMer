
function GetWithdrawStatus(val) {
    switch (val) {
        case 1: return "<span style='color:green;'>通过</span>";
        case 2: return "<span style='color:red;'>不通过</span>";
        case 3: return "待审核";
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

    var tiltle = $("#AccountType").val() == 1 ? "商户号" : "代理Id";
    //新闻列表
    var tableIns = table.render({
        elem: '#AuditWithdrawAccountList',
        url: '/Business/LoadUserDetailList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 60,
        page: true,
        even: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "AuditWithdrawAccountList",
        cols: [[
            { field: 'UserId', title: tiltle, width: 90, align: "center" },
            { field: 'WithdrawBank', title: "结算银行", width: 90, align: "center" },
            { field: 'WithdrawFactName', title: "开户名", width: 90, align: "center" },
            { field: 'WithdrawBankCode', title: "账户", width: 200, align: "center" },
            {
                field: 'WithdrawAccountType', title: "账户类型", width: 90, align: "center",
                templet: function (d) {
                    if (d.WithdrawAccountType == 0) {
                        return "个人";
                    }
                    if (d.WithdrawAccountType == 1) {
                        return "企业";
                    }
                }
            },

            {
                field: 'WithdrawBankBranch', title: "开户行信息", width: 300, align: "center"
            },

            { field: 'WithdrawReservedPhone', title: "预留手机", width: 90, align: "center" },


            //{
            //    field: 'img', title: "资质图片", width: 200, align: "center",
            //    templet: function (d) {
                   
            //    }
            //},

            {
                field: 'WithdrawStatus', title: '审核状态', width: 60, align: "center",
                templet: function (d) {
                    return GetWithdrawStatus(d.WithdrawStatus);

                }
            },

            {
                field: 'WithdrawTime', title: '审核时间', align: 'center', width: 120,
                templet: function (d) {
                    return hm.changeDateFormat(d.WithdrawTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            { title: '操作', width: 120, templet: '#AuditWithdrawAccountListBar', fixed: "right", align: "center" }
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


    function Search() {
        table.reload("AuditWithdrawAccountList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch"))
        })
    }

    //列表操作
    table.on('tool(AuditWithdrawAccountList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;

        //审核 
        if (layEvent === 'btn_audit') {
            var index = layer.open({
                title: '[审核 <b style="color: red">' + data.UserId + "</b> 结算在账户]",
                type: 2,
                area: ["530px", "500px"],
                content: "/Business/AuditWithdraw?UserId=" + data.UserId
            })
        }

    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})