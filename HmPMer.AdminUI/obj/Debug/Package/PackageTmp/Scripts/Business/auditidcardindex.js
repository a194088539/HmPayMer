
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

    var tiltle = $("#UserType").val() == 1 ? "商户号" : "代理Id";
    //新闻列表
    var tableIns = table.render({
        elem: '#AuditidcardIndexList',
        url: '/Business/LoadUserIdCardList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 60,
        page: true,
        even: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "AuditidcardIndexList",
        cols: [[
            { field: 'UserId', title: tiltle, width: 90, align: "center" },
            { field: 'CompanyName', title: "企业名", width: 90, align: "center" },
            { field: 'FactName', title: "法人/个人真实姓名", width: 90, align: "center" },
            { field: 'IdCard', title: "身份证号码", width: 200, align: "center" },
            { field: 'CustTel', title: "联系电话", width: 90, align: "center" },
            {
                field: 'addr', title: "联系地址", width: 300, align: "center", templet: function (d) {
                    return d.CompanyProName + d.CompanyCityName + d.CompanyDicName + d.Address;
                }
            },

            {
                field: 'LicenseId', title: "营业执照", width: 300, align: "center"
            },

            {
                field: 'IdCardStatus', title: '审核状态', width: 60, align: "center",
                templet: function (d) {
                    return GetWithdrawStatus(d.IdCardStatus);

                }
            },
            {
                field: 'IdCardTime', title: '审核时间', align: 'center', width: 120,
                templet: function (d) {
                    return hm.changeDateFormat(d.WithdrawTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            { title: '操作', width: 120, templet: '#AuditidcardIndexListBar', fixed: "right", align: "center" }
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
        table.reload("AuditidcardIndexList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch"))
        })
    }

    //列表操作
    table.on('tool(AuditidcardIndexList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;

        //审核 
        if (layEvent === 'btn_audit') {
            var index = layer.open({
                title: '[审核 <b style="color: red">' + data.UserId + "</b> 结算在账户]",
                type: 2,
                area: ["530px", "550px"],
                content: "/Business/AuditIdCard?UserId=" + data.UserId
            })
        }

    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})