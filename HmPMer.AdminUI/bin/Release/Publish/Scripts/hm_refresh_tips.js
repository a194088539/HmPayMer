; (function ($) {
    if (!$) return;
     $.extend({
        /*
            使用示例
            var _index;
            var refresh= $.hm_refresh_tips({
                 // 提交地址
                 url:'/Order/GetNewOrder'
                 // 提交参数,可以是方法
                ,contentData:{ PayState:1,time: (new Date().getTime()}
                ,time: 30,//间隔时间吗，单位S
                ,mp3:'/static/mp3/1.mp3'//你要播放的mp3
                 //执行之前
                ,beforeSend: function () {
                    _index=layer.msg("加载中......", { icon: 16, time: 0 });
                },
                // obj:当前操作
                // data:接口返回的值
                callback:function(obj, data){                    
                    // 判断是否是新消息
                    obj.play();//播放mp3
                    //如果你需要根据值关闭轮询
                    obj.end();
                }
            });
            refresh.begin();//开始刷新
            refresh.end();//结束刷新
         
         */
        hm_refresh_tips: function (options) {
            var opts = {};
            var defaults = {
                url: '',//提交地址
                contentData: { time: (new Date().getTime()) },//提交参数
                type: 'GET',
                dataType: 'json',
                beforeSend: function () {
                },
                time: 30,//间隔时间吗，单位S                
                mp3: '',//声音提醒MP3
                //回调方法
                callback: function (obj, data) {

                }
            };
            opts = $.extend({}, defaults, options);

            var refreshState = false;
            var refreshData = null;

            var playSound = function (mp3) {
                var borswer = window.navigator.userAgent.toLowerCase();
                if (borswer.indexOf("ie") >= 0) {
                    //IE内核浏览器  
                    var strEmbed = '<embed name="embedPlay" src="' + mp3 + '" autostart="true" hidden="true" loop="false"></embed>';
                    if ($("body").find("embed").length <= 0)
                        $("body").append(strEmbed);
                    var embed = document.embedPlay;

                    //浏览器不支持 audion，则使用 embed 播放  
                    embed.volume = 100;
                    embed.play();
                } else {
                    //非IE内核浏览器  
                    var strAudio = "<audio id='audioPlay' src='" + mp3 + "' hidden='true'>";
                    if ($("body").find("audio").length <= 0)
                        $("body").append(strAudio);
                    var audio = document.getElementById("audioPlay");
                    //浏览器支持 audion  
                    audio.play();
                }
            };

            var getContent = function (_obj, opts) {
                if (!refreshState) return;
                $.ajax(opts.url, {
                    data: $.isFunction(opts.contentData) ? opts.contentData() : opts.contentData,
                    type: opts.type,
                    dataType: opts.dataType,
                    beforeSend: opts.beforeSend,
                    success: function (d) {
                        if (d.code == 301) {
                            window.location = window.location.protocol + "//" + window.location.host + d.data;
                            return;
                        }
                        clearTimeout(refreshData);

                        // 执行回调
                        if (opts.callback) { opts.callback(_obj, d); }

                        var time = opts.time;
                        refreshData = setTimeout(function () {
                            getContent(_obj, opts);
                        }, time * 1000);
                    },
                    error: function (x, t, e) {
                        clearTimeout(refreshData);
                        var time = opts.time;
                        refreshData = setTimeout(function () {
                            getContent(_obj, opts);
                        }, time * 1000);
                    }
                });
            };

            var obj = {
                begin: function () {
                    if (refreshState === true) return;
                    refreshState = true;
                    getContent(this, opts);
                },
                end: function () {
                    refreshState = false;
                    if (refreshData) clearTimeout(refreshData);
                },
                play: function () {
                    if (opts.mp3 && opts.mp3.length > 0)
                        playSound(opts.mp3)
                }
            };
            return obj;
         }
         , bbb: 'dsdd'
    });
})(jQuery);
