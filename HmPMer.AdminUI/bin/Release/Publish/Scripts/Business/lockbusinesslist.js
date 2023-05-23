function GetUserType(val) {
    switch (val) {
        case 0: return "个人";
        case 1: return "企业";
    }
    return "";
}

function GetIsEnabled(val) {
    switch (val) {
        case 0: return "未审核";
        case 1: return "正常";
        case 2: return "冻结";
    }
    return "";
}

//认证
function GetIsCertification(val) {
    if (val == '1') {
        return '<i class="layui-icon" style="font-size: 30px; color: green">&#xe618;</i> ';
    } else {
        return '<i class="layui-icon" style="font-size: 30px; color: red;">&#x1006;</i> ';
    }
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
        elem: '#BusinessList',
        url: '/Business/LoadUserPage?Type=1',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "BusinessList",
        cols: [[
            {
                field: 'UserId', title: '商户号', width: 130, align: "center"
            },
            { field: 'MerName', title: '商户名称', width: 180, align: "center" },
            { field: 'OrderAmt', title: '总交易量(元)', width: 150, sort: true, align: "center" },
            {
                field: 'UserType', title: '签约类型', width: 100, align: "center",
                templet: function (d) {
                    return GetUserType(d.UserType);
                }
            },

            {
                field: 'IdCardStatus', title: '资质', width: 30, align: "center",
                templet: function (d) {
                    return GetIsCertification(d.IdCardStatus);
                }
            },

            {
                field: 'IsMobilePhone', title: '手机', width: 60, align: "center",
                templet: function (d) {
                    return GetIsCertification(d.IsMobilePhone);
                }
            },

            {
                field: 'IsEmail', title: '邮箱', width: 60, align: "center",
                templet: function (d) {
                    return GetIsCertification(d.IsEmail);
                }
            },
            { field: 'MobilePhone', title: '商户手机', width: 120, align: "center" },

            //  {
            //      field: 'IsEnabled', title: '是否启用', align: 'center', width: 90, templet: function (d) {
            //          if (d.IsEnabled == 1) {
            //              return '<input type="checkbox" checked name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.UserId + '" >';
            //          } else {
            //              return '<input type="checkbox" name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.UserId + '" >';
            //          }
            //      }
            //  },



            {
                title: '单独费率', width: 100, templet: function (d) {
                    return d.AloneRate == 1 ? "<span style='color:red;'>开启</span>" : "未开启";
                }
            },


            {
                field: 'IsEnabled', title: '状态', width: 80, align: "center",
                templet: function (d) {
                    return GetIsEnabled(d.IsEnabled);

                }
            },


            {
                field: 'RegTime', title: '注册时间', align: 'center', width: 180,
                templet: function (d) {
                    return hm.changeDateFormat(d.RegTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            {
                field: 'LastLoginTime', title: '最后登录', align: 'center', width: 180,
                templet: function (d) {
                    return hm.changeDateFormat(d.LastLoginTime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            {
                field: 'LastLoginIp', title: '最后登录ip', align: 'center', width: 110
            },


            { title: '操作', width: 100, templet: '#BusinessListBar', fixed: "right", align: "center" }
        ]], response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });

    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });


    function Search() {
        table.reload("BusinessList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch"))
        })
    }

    function SetIsEnabled(IsEnabled, userId) {
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        layer.confirm(IsEnabled == 1 ? "确定解冻该商户？" : "确定冻结该商户？", { icon: 3, title: '提示信息' }, function (index) {
            setTimeout(function () {
                // var IsEnabled = data.elem.checked ? 1 : 0;
                hm.ajax("/Business/UpIsEnabled", {
                    data: {
                        IsEnabled: IsEnabled,
                        userId: userId
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        layer.close(index);
                        if (result.Success) {
                            layer.msg("操作成功！");
                            Search();
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
    }

    //列表操作
    table.on('tool(BusinessList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;

        //解冻
        if (layEvent === 'btn_defreeze') {
            SetIsEnabled(1, data.UserId);

        }

    });



})