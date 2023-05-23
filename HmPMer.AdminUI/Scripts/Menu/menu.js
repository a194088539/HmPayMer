
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
        elem: '#menuList',
        url: '/MenuRole/GetMenuPageList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "menuList",
        cols: [[
            { field: 'Id', title: '菜单ID', width: 130, align: "center" },
            { field: 'menuName', title: '菜单名称', width: 150, align: "center" },
            { field: 'menuUrl', title: '菜单地址', width: 150, align: "center" },
            { field: 'menuLeval', title: '级别', width: 150, align: "center" }, 
            { field: 'parentID', title: '父级ID', width: 130, align: "center" },
            { field: 'FlagStr', title: '权限值', width: 130, align: "center" },
            { field: 'orderNo', title: '排序号', width: 130, align: "center" },
            {
                field: 'IsEnabled', title: '是否启用', align: 'center', width: 150, templet: function (d) {
                    if (d.IsEnabled == 1) {
                        return '<input type="checkbox" checked name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Id + '" >';
                    } else {
                        return '<input type="checkbox" name="IsEnabled" lay-filter="IsEnabled" lay-skin="switch" lay-text="是|否"  value="' + d.Id + '" >';
                    }
                }
            },

            {
                field: 'icon', title: '图标', width: 130, align: "center", templet: function (d) {
                    return '<i class="layui-icon">' + d.icon+'</i> ';
                }
            },
            {
                field: 'createTime', title: '添加时间', align: 'center', width: 110,
                templet: function (d) {
                    return hm.changeDateFormat(d.createTime, "yyyy-MM-dd");
                }
            },
           
            { title: '操作', width: 180, templet: '#menuListBar', fixed: "right", align: "center" }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    $(".addAdmin_btn").click(function () {
        var index = layer.open({
            title: "新增菜单",
            type: 2,
            area: ["430px", "400px"],
            content: "/MenuRole/AddMenu"
        })
    });

    //是否启用
    form.on('switch(IsEnabled)', function (data) {
  
        var index = layer.msg('修改中，请稍候', { icon: 16, time: false, shade: 0.8 });
        setTimeout(function () {
            var IsEnabled = data.elem.checked ? 1 : 0;
            hm.ajax("/MenuRole/UpIsEnabled", {
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
        Search();
    });


    function Search() {
        table.reload("menuList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch")),
        })
    }

    //回车事件
    document.onkeydown = function (event) {
        var e = event || window.event || arguments.callee.caller.arguments[0];

        if (e && e.keyCode == 13) { // enter 键
            //要做的事情
            Search();
        }
    }; 

    //列表操作
    table.on('tool(menuList)', function (obj) {
        var layEvent = obj.event,
            data = obj.data;
        if (layEvent === 'edit') { //分配权限
            var index = layer.open({
                title: "编辑菜单",
                type: 2,
                area: ["430px", "480px"],
                content: "/MenuRole/UpdateMenu?id=" + data.Id
            })
        }
    });

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})