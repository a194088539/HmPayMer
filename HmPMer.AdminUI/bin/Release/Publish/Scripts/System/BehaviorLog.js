function GetBlType(val) {
    switch (val) {
        case 1: return '<font style="color:#2c7;">新增</font>';
        case 2: return '<font style="color:blue;">修改</font>';
        case 3: return '<font style="color:#f00;">删除</font>';
        case 4: return '<font style="color:#FF6100;">查询</font>';
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
        elem: '#BehaviorLogList',
        url: '/DataManage/GetBehaviorLogList',
        where: hm.serializeObject($("#form_seartch")),
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "BehaviorLogList",
        cols: [[
            { field: 'BlName', title: '操作名称', width: 150, align: "center" },
            {
                field: 'BlType', title: '操作类型', width: 150, align: "center",
                templet: function (d) {
                    return GetBlType(d.BlType);
                } },
            { field: 'parm', title: '请求参数', width: 750, align: "center" },
            { field: 'createUser', title: '操作人', width: 150, align: "center" },
            {
                field: 'addTime', title: '添加时间', align: 'center', width: 200,
                templet: function (d) {
                    return hm.changeDateFormat(d.addTime, "yyyy-MM-dd HH:mm:ss");
                }
            }
        ]]
        , response: {
            countName: 'totalCount',
            dataName: 'Item'
        },
    });


    //搜索
    $(".search_btn").on("click", function () {
        Search();
    });

    function Search() {
        table.reload("BehaviorLogList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch"))
        })
    }
    
    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})