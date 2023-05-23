/*!
 * jQuery showMessage plugin
 * Version 0.01
 * @requires jQuery v1.2.3 or later
 * 
 *Copyright (c) 2010 全冠清
 */
$(function () {
    $.messageBox = function (opts) {
        var defaults = {
            title: '提醒',
            msg: '您有未处理的提现订单！'
        }
        var setting = $.extend({}, defaults, opts)
        $.messageBox.dom = $('<div class="window" style="position: absolute;display: none; width: 308px; cursor: default; right: 0px; z-index: 9000;"></div>')
        var panel_header = $('<div class="panel-header panel-header-noborder window-header" style="width: 308px;"></div>')
        $('<div class="panel-title"></div>').html(setting.title).appendTo(panel_header)
        $('<div class="panel-tool"><div class="panel-tool-close"></div></div>').click(function () {
            $.closeMessageBox()
        }).appendTo(panel_header)
        $.messageBox.dom.append(panel_header)
        $('<div class="messager-body panel-body panel-body-noborder window-body" title="" style="width: 286px; height: 135px;"></div>').html(setting.msg).appendTo($.messageBox.dom)
        var h = (document.documentElement.scrollTop ? document.documentElement.scrollTop : document.body.scrollTop) + document.documentElement.clientHeight - $.messageBox.dom.height() - 12;
        if (h < document.body.offsetHeight) {
            $.messageBox.dom.css('top', h + 'px')
        }
        else {
            // $.messageBox.dom.css('top', document.body.offsetHeight + 'px')
            $.messageBox.dom.css('top', document.documentElement.clientHeight - 195 + 'px')
            console.info(document.documentElement.clientHeight)
        }
        //    $.messageBox.interval = setInterval(function () {
        //                var h = (document.documentElement.scrollTop ? document.documentElement.scrollTop : document.body.scrollTop) + document.documentElement.clientHeight - $.messageBox.dom.height() - 12;                  


        //                if (h < document.body.offsetHeight) {                        
        //                 $.messageBox.dom.css('top', h + 'px')
        //             }
        //             else {
        //                   // $.messageBox.dom.css('top', document.body.offsetHeight + 'px')
        //                    $.messageBox.dom.css('top', document.documentElement.clientHeight - 195 + 'px')
        //                    console.info(document.documentElement.clientHeight)
        //             }
        //},20)

        $.messageBox.dom.appendTo('body')
        $.messageBox.dom.slideDown('slow')
        if (setting.timeout) {
            setTimeout(function () { $.closeMessageBox() }, setting.timeout)
        }
    }
    $.closeMessageBox = function () {
        $.messageBox.dom.slideUp('slow')
        setTimeout(function () {
            clearInterval($.messageBox.interval)
            $.messageBox.dom.remove()
        }, 1000)
    }
})
