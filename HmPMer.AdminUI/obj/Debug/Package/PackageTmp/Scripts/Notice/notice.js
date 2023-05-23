
function GetIsRelease(val) {
    switch (val) {
        case 0: return "未发布";
        case 1: return "<span style='color:red;'>已发布</span>";
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
        elem: '#NoticeList',
        url: '/System/GetNoticePageList',
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "NoticeList",
        cols: [[
            { field: 'Title', title: '标题', width: 250, align: "center" },
            {
                field: 'NoticeType', title: '类别', width: 250, align: "center", templet: function (d) {
                    if (d.NoticeType==1)
                    {
                        return "商户";
                    }
                    if (d.NoticeType == 2) {
                        return "代理";
                    }
                    return "";
                }
            },
            //{
            //    field: 'Content', title: '公告内容', width: 350, align: "center", templet: function (d) {
            //        return decodeURI(d.Content);
            //    }
            //},

            {
                field: 'IsRelease', title: '是否发布', width: 150, align: "center", templet: function (d) {
                    return GetIsRelease(d.IsRelease);
                }
            },

            /// {
            ///     field: 'IsEnable', title: '是否启用', align: 'center', width: 100, templet: function (d) {
            ///         if (d.IsEnabled == 1) {
            ///             return '<input type="checkbox" checked name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Code + '" >';
            ///         } else {
            ///             return '<input type="checkbox" name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Code + '" >';
            ///         }
            ///     }
            /// },

            {
                field: 'Addtime', title: '添加时间', align: 'center', width: 200,
                templet: function (d) {
                    return hm.changeDateFormat(d.Addtime, "yyyy-MM-dd HH:mm:ss");
                }
            },

            {
                title: '操作', width: 150, fixed: "right", align: "center",
                templet: function (d) {
                    var returnhtml = "";
                    if (Number(d.IsRelease) == 0) {
                        returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_release">发布</a>';
                        returnhtml += '<a class="layui-btn layui-btn-xs" lay-event="btn_edit">编辑</a>';
                    }
                    returnhtml += '<a class="layui-btn layui-btn-xs layui-btn-danger" lay-event="deldata">删除</a>';
                    return returnhtml;
                }
            }

        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $(".addAdmin_btn").click(function () {
        var index = layer.open({
            title: "新增公告",
            type: 2,
            area: ["830px", "550px"],
            content: "/System/NoticeAdd"
        })
    });


    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });

    function Search() {
        table.reload("NoticeList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: {
                Title: $(".searchVal").val()  //搜索的关键字 layer.msg("请输入搜索的内容");
            }
        })
    }


    //列表操作
    table.on('tool(NoticeList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        if (layEvent === 'btn_edit') { //编辑
            var index = layer.open({
                title: "编辑",
                type: 2,
                area: ["830px", "550px"],
                content: "/System/NoticeUpdate?id=" + data.Id
            })
        }

        if (layEvent === 'btn_release') { //发布
            setTimeout(function () {
                hm.ajax("/System/ReleaseNotice", {
                    data: {
                        Id: data.Id
                    },
                    type: "POST",
                    dataType: 'json',
                    success: function (result) {
                        if (result.Success) {
                            layer.msg(result.Message, { icon: 1 }, function () {
                                Search();
                            });

                        }
                        else {
                            layer.msg(result.Message, { icon: 0 });
                        }
                    },
                    error: function (x, t, e) {
                        layer.closeAll();
                    }
                });
            }, 500);

        } 

        if (layEvent === 'deldata') { //删除
            layer.confirm('确定删除该公告？', { icon: 3, title: '提示信息' }, function (index) {
                hm.ajax("/System/DelNotice", {
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