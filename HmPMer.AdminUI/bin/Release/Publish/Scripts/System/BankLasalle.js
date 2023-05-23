
//$(function () {
//    //alert(1);
//    //$("#ProvinceId").change(function () {
//    //    alert(1);
//    //    var pid = $.trim(this.value);
//    //    if (pid == '') {
//    //        $("#CityId").empty().append('<option value="">请选择</option>');
//    //        return;
//    //    }
//    //    loadCity(pid);
//    //    var _html = '<option value="">请选择</option>';
//    //});


//    $("ProvinceId").bind("change", function () {
//        alert(1);
//        var pid = $.trim(this.value);
//        if (pid == '') {
//            $("#CityId").empty().append('<option value="">请选择</option>');
//            return;
//        }
//        loadCity(pid);
//        var _html = '<option value="">请选择</option>';
//    })

//});

layui.use(['hm', 'form', 'layer', 'laydate', 'table', 'laytpl'], function () {
    var form = layui.form,
        layer = layui.layer,
        $ = layui.jquery,
        laydate = layui.laydate,
        laytpl = layui.laytpl,
        table = layui.table,
        hm = layui.hm;


    var tableIns = table.render({
        elem: '#BankLasalleList',
        url: '/System/GetBankLasalleList',
        cellMinWidth: 95,
        page: true,
        height: "full-125",
        limit: 15,
        limits: [10, 15, 20, 25],
        id: "BankLasalleList",
        cols: [[
            { type: "checkbox", fixed: "left", width: 50 },
            { field: 'BankLasalleCode', title: '银联号', width: 250, align: "center" },
            { field: 'BankLasalleName', title: '支行名称', width: 450, align: "center" },
            { field: 'ProName', title: '省', width: 150, align: "center" },
            { field: 'CityName', title: '市', width: 150, align: "center" }

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
        table.reload("BankLasalleList", {
            page: {
                curr: 1 //重新从第 1 页开始
            },
            where: hm.serializeObject($("#form_seartch"))
        })
    }

    form.on('select(ProvinceId)', function (data) {
        var pid = $.trim(data.value);
        if (pid == '') {
            $("#CityId").empty().append('<option value="">请选择</option>');
            form.render('select');
            return;
        }
        loadCity(pid);

    });

    //批量审核
    $("#btn_edit_all").click(function () {
        var checkStatus = table.checkStatus('BankLasalleList'),
            data = checkStatus.data,
            idstr = [];

        if (data.length > 0) {
            for (let i in data) {
                idstr.push("'" + data[i].BankLasalleCode + "'");
            }

            var index = layer.open({
                title: "编辑支行省市",
                type: 2,
                area: ["400px", "250px"],
                content: "/System/BankLasalleUpdate?BankLasalleCode=" + idstr.join(',')
            })

        } else {
            layer.msg("请选择要编辑的数据！");
        }
    })

    function loadCity(pid) {
        hm.ajax("/System/loadcitylist", {
            data: { id: pid },
            type: "POST",
            dataType: 'json',
            success: function (d) {
                if (d.length == 0) {
                    $("#CityId").empty().append('<option value="">请选择</option>');
                    form.render('select');
                    return;
                }
                var _html = '';
                _html += '<option value="">请选择</option>';
                $.each(d.data, function (i, v) {
                    _html += '<option value="' + v.Id + '">' + v.Name + '</option>';
                });
                $("#CityId").empty().append(_html);
                form.render('select');
            }
        });
    }

    //定义给子页面
    window.layer = layer;
    window.tableIns = tableIns;

})

