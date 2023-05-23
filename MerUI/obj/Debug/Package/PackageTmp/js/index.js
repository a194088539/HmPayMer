/**
 * Created by Hajay on 2017/2/7.
 */
$(function(){
    var banner_slide_on = 0,
        banner_slide_max = 3,
        banner_slide_time = 500,
        timer = null;
    function move(id, cls, index, banner_on){
        $(cls + " .con").eq(index).css({"opacity": 1, "display": "block"});
        $(cls + " .con").eq(banner_on).animate({"opacity": 0}, banner_slide_time);
        $(id + " i").eq(banner_on).removeClass("on");
        $(id + " i").eq(banner_on).animate({"width": "10px"}, banner_slide_time);
        $(id + " i").eq(index).addClass("on").animate({"width": "20px"}, banner_slide_time);
        setTimeout(function(){
            $(cls + " .con").eq(banner_on).css({"z-index": 10});
            $(cls + " .con").eq(index).css({"z-index": 11});
            //banner_on = index;
        }, banner_slide_time);
    }

    //banner slide
    $("#banner_slide i").each(function(index){
        $(this).click(function(){
            if(index != banner_slide_on){
                move("#banner_slide", ".banner", index, banner_slide_on);
                setTimeout(function(){
                    banner_slide_on = index;
                }, banner_slide_time);
            }
        })
    });
    auto_move("#banner_slide", ".banner");
    $(".banner").hover(function(){
        clearInterval(timer);
    },function(){
        auto_move("#banner_slide", ".banner");
    });

    function auto_move(id, cls){
        timer = setInterval(function(){
            var curr_banner = banner_slide_on;
            if((banner_slide_on+1) == banner_slide_max){
                curr_banner = 0;
            }else{
                curr_banner = banner_slide_on+1;
            }
            //console.log(curr_banner);
            move(id, cls, curr_banner, banner_slide_on);
            setTimeout(function(){
                banner_slide_on = curr_banner;
            }, banner_slide_time);
        }, 5000);
    }

    //user banner slide  user_banner_slide
    var user_timer = null,
        user_slide_max = 4,
        user_banner_slide_on = 0;
    $("#user_banner_slide i").each(function(index){
        $(this).click(function(){
            if(index != user_banner_slide_on){
                move("#user_banner_slide", ".banner_bottom", index, user_banner_slide_on);
                setTimeout(function(){
                    user_banner_slide_on = index;
                }, banner_slide_time);
            }
        })
    });
    user_auto_move("#user_banner_slide", ".banner_bottom");
    $(".banner_bottom").hover(function(){
        clearInterval(user_timer);
    },function(){
        user_auto_move("#user_banner_slide", ".banner_bottom");
    });

    function user_auto_move(id, cls){
        user_timer = setInterval(function(){
            var curr_banner = user_banner_slide_on;
            if((user_banner_slide_on+1) == user_slide_max){
                curr_banner = 0;
            }else{
                curr_banner = user_banner_slide_on+1;
            }
            //console.log(curr_banner);
            move(id, cls, curr_banner, user_banner_slide_on);
            setTimeout(function(){
                user_banner_slide_on = curr_banner;
            }, banner_slide_time);
        }, 5000);
    }

    //notice close
    $(".notice i").click(function(){
        $(".notice_bg").hide();
    });

    //SaaS交易管理系统
    var saas_time = 1000;
    var saas_text = [
        {
            title: '技术研发',
            con: '去聚合拥有一支行业一流的技术团队为您提供便捷、稳定和安全的技术服务。去聚合计费自主研发的聚合支付系统可以提供专业的sdk、api数据服务，从而打造一站式的接口管理、行程简单稳定的聚合支付云服务。'
        },{
            title: '运营管理',
            con: '管理平台可以查阅各个渠道的所有订单明细，手机金额和状态一目了然，实时查看交易明细，财务管理模块统筹详尽的财务信息，集中分析数据。无论是技术、运营还是财务人员，均可拥有高效、精准、体验绝佳的一站式管理平台。'
        },{
            title: '渠道安全',
            con: '平台采用银行级别的资金安全管理机制，为您提供安全可靠的支付环境，保障您在平台上的资金安全。去聚合计费与各大银行及实力第三方支付公司有着密切的合作，确保您的支付渠道和资金的安全'
        },{
            title: '开发调试',
            con: '在线实时查看开发中的日志数据，帮助检测开发过程中遇见的问题。'
        },{
            title: '服务流程',
            con: '支持个性化定制和私有化部署，为你提供7*24小时全流程的优质服务，您的满意就是对我们最大的奖励；去聚合计费为您提供从前期入网申请、接口联调、测试上线到后期系统运维、管理平台使用等各方向全面的行业一流技术服务。'
        }
    ];
    $(".saas ul li:not(:nth-child(1))").each(function(index){
        $(this).click(function(){
            $(".saas ul li").removeClass("on");
            $(this).addClass("on");
            $(".img_list").css({"opacity": 0.1,"margin-top": "-13px"});
            $(".img_list img").attr("src","Public/Home/common/img/index/saas_img" + (index+1) + ".png");
            $(".img_list").animate({"opacity": 1,"margin-top": "7px"}, saas_time);
            $(".con_r dl").css({"margin-left": "-20px","opacity": 0.1});
            $(".con_r dl").animate({"margin-left": 0,"opacity": 1}, saas_time);
            $(".con_r dl dt").text(saas_text[index].title);
            $(".con_r dl dd").text(saas_text[index].con);
            $(".con_r dl dt").removeAttr("class").addClass("dt"+(index + 1));
        });
    });

    $(".float_bar li:nth-child(5)").click(function(){
        $('body,html').animate({scrollTop: 0}, 500);
    });

    //渠道动画
    var channel_switch_on = 0,//从第一个渠道开始
        channel_switch_max = 5,//5个渠道
        channel_switch_speed = 4000,//4s轮换一个渠道
        channel_timer = null;
    function auto_channel_switch(){
        channel_timer = setInterval(function(){
            if((channel_switch_on) == channel_switch_max){
                channel_switch_on = 0;
            }
            $(".channel-main .channel-con").removeClass("current");
            $(".channel-main .channel-con").eq(channel_switch_on).addClass("current");
            channel_switch_on++;
        }, channel_switch_speed);
    }
    $(".channel-main .channel-con").hover(function(){
        $(".channel-main .channel-con").removeClass("current");
        var _index = $(this).index();
        clearInterval(channel_timer);
        channel_switch_on = _index + 1;
    },function(){
        auto_channel_switch();
    });
    auto_channel_switch();
});
