;/**
 * Name:hm.js
 * Author:ZhqngQingWen
 * E-mail:zhangqingweng@foxmail.com
 * Website:http://www.hamanhm.com/
 * LICENSE:MIT
 */
layui.define(["jquery"], function (exports) {
    var $ = layui.jquery;
    
    var hm = {
        host: window.location.protocol + "//" + window.location.host + "/",
        ajax: function (url, options) {
            url = hm.host + url;
            var settings = $.extend({
                'data': {},
                'type': 'POST',
                'dataType': 'json',
                'success': function () {
                }
            }, options);

            $.ajax(url, {
                'data': settings.data,
                type: settings.type,
                dataType: settings.dataType,
                beforeSend: function () {
                    layer.msg("正在请求中请稍后...", { icon: 16, time: 0 });
                },
                success: function (d) {
                    layer.closeAll();
                    if (d.code == 301) {
                        layer.alert(d.message, { icon: 2 });
                        setTimeout(function () {
                            if (top)
                                top.window.location = window.location.protocol + "//" + window.location.host + d.data;
                            else
                                window.location = window.location.protocol + "//" + window.location.host + d.data;
                        }, 2000);                        
                        return;
                    }
                    if (d.code == 401) {
                        layer.msg(d.message, { icon: 2 });
                        return;
                    }
                    if (settings.success) {
                        settings.success(d);
                    }
                },
                error: function (x, t, e) {
                    layer.closeAll();
                }
            });

        },

        //时间格式转换 [ jsondate 时间,formart:格式 yyyy-MM-dd ) ]
        changeDateFormat: function (jsondate, formart) {
            if (!jsondate)
            {
                return '';
            }
            if (jsondate && jsondate != '') {
                jsondate = jsondate.replace("/Date(", "").replace(")/", "");
            } else {
                return '';
            }
            if (jsondate.indexOf("+") > 0) {
                jsondate = jsondate.substring(0, jsondate.indexOf("+"));
            } else if (jsondate.indexOf("-") > 0) {
                jsondate = jsondate.substring(0, jsondate.indexOf("-"));
            }
            var date = new Date(parseInt(jsondate, 10));
            if (isNaN(date.getMonth())) {
                return '';
            }
            if (!formart || formart == '') {
                formart = "yyyy-MM-dd HH:mm:ss";
            }
            var o = {
                "M+": date.getMonth() + 1,
                "d+": date.getDate(),
                "H+": date.getHours(),
                "m+": date.getMinutes(),
                "s+": date.getSeconds(),
                "q+": Math.floor((date.getMonth() + 3) / 3),
                "S": date.getMilliseconds()
            };
            if (/(y+)/.test(formart)) {
                formart = formart.replace(RegExp.$1, (date.getFullYear() + "").substr(4 - RegExp.$1.legth));
            }
            for (var k in o) {
                if (new RegExp("(" + k + ")").test(formart)) {
                    formart = formart.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
                }
            }
            return formart;
        },

        //表单转Json
        serializeObject: function (from) {
            var o = {};
            var a = $(from).serializeArray();
            
            $.each(a, function () {
                if (o[this.name]) {
                    if (!o[this.name].push) {
                        o[this.name] = [o[this.name]];
                    }
                    o[this.name].push($.trim(this.value) || '');
                } else {
                    o[this.name] = $.trim(this.value) || '';
                }
            });
            return o;
        },

        /*
         格式化链接参数.
         @url 序列为键值对。 不填写 默认为当前地址
         return 键值对
         */
        serializeUrlKeyValue: function (url) {
            url = url || window.location.search;
            url = url.split("?"),
                url = url[url.length - 1];
            var parameter = url.split("&");
            var result = [];
            for (var i = 0; i < parameter.length; i++) {
                var val = parameter[i].split("=");
                result.push({ key: val[0].toLowerCase(), value: $.trim(val[1]) });
            }
            return result;
        },
        /*
        格式化链接参数.
        @url 序列为Json对象。 不填写 默认为当前地址
        return Json对象
        */
        serializeUrlJson: function (url) {
            var result = {};
            var arr = hmExtends.serializeUrlKeyValue(url);
            for (var i = 0; i < arr.length; i++) {
                var t = arr[i];
                result[t.key] = t.value;
            }
            return result;
        },
        /*
         json对象转字符串.
         @value json对象
         return Json字符串
         */
        jsonStr: function (value) {
            function f(n) { return n < 10 ? '0' + n : n; }
            Date.prototype.toJSON = function (key) {
                return isFinite(this.valueOf()) ?
                    this.getUTCFullYear() + '-' +
                    f(this.getUTCMonth() + 1) + '-' +
                    f(this.getUTCDate()) + 'T' +
                    f(this.getUTCHours()) + ':' +
                    f(this.getUTCMinutes()) + ':' +
                    f(this.getUTCSeconds()) + 'Z' : null;
            };

            String.prototype.toJSON = Number.prototype.toJSON = Boolean.prototype.toJSON = function (key) { return this.valueOf(); };
            var cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
                escapable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
                gap,
                indent,
                meta = { '\b': '\\b', '\t': '\\t', '\n': '\\n', '\f': '\\f', '\r': '\\r', '"': '\\"', '\\': '\\\\' },
                rep;

            function quote(string) {
                escapable.lastIndex = 0;
                return escapable.test(string) ?
                    '"' + string.replace(escapable, function (a) {
                        var c = meta[a];
                        return typeof c === 'string' ? c :
                            '\\u' + ('0000' + a.charCodeAt(0).toString(16)).slice(-4);
                    }) + '"' :
                    '"' + string + '"';
            }

            function str(key, holder) {

                var i,          // The loop counter.
                    k,          // The member key.
                    v,          // The member value.
                    length,
                    mind = gap,
                    partial,
                    value = holder[key];

                if (value && typeof value === 'object' &&
                    typeof value.toJSON === 'function') {
                    value = value.toJSON(key);
                }

                if (typeof rep === 'function') {
                    value = rep.call(holder, key, value);
                }

                switch (typeof value) {
                    case 'string':
                        return quote(value);
                    case 'number':
                        return isFinite(value) ? String(value) : 'null';
                    case 'boolean':
                    case 'null':
                        return String(value);
                    case 'object':
                        if (!value) {
                            return 'null';
                        }
                        gap += indent;
                        partial = [];
                        if (_isArray(value)) {
                            length = value.length;
                            for (i = 0; i < length; i += 1) {
                                partial[i] = str(i, value) || 'null';
                            }
                            v = partial.length === 0 ? '[]' :
                                gap ? '[\n' + gap +
                                    partial.join(',\n' + gap) + '\n' +
                                    mind + ']' :
                                    '[' + partial.join(',') + ']';
                            gap = mind;
                            return v;
                        }

                        if (rep && typeof rep === 'object') {
                            length = rep.length;
                            for (i = 0; i < length; i += 1) {
                                k = rep[i];
                                if (typeof k === 'string') {
                                    v = str(k, value);
                                    if (v) {
                                        partial.push(quote(k) + (gap ? ': ' : ':') + v);
                                    }
                                }
                            }
                        } else {
                            for (k in value) {
                                if (Object.hasOwnProperty.call(value, k)) {
                                    v = str(k, value);
                                    if (v) {
                                        partial.push(quote(k) + (gap ? ': ' : ':') + v);
                                    }
                                }
                            }
                        }
                        v = partial.length === 0 ? '{}' :
                            gap ? '{\n' + gap + partial.join(',\n' + gap) + '\n' +
                                mind + '}' : '{' + partial.join(',') + '}';
                        gap = mind;
                        return v;
                }
            }

            return (function (value, replacer, space) {
                var i;
                gap = '';
                indent = '';

                if (typeof space === 'number') {
                    for (i = 0; i < space; i += 1) {
                        indent += ' ';
                    }
                } else if (typeof space === 'string') {
                    indent = space;
                }

                rep = replacer;
                if (replacer && typeof replacer !== 'function' &&
                    (typeof replacer !== 'object' ||
                        typeof replacer.length !== 'number')) {
                    throw new Error('JSON2Str');
                }
                return str('', { '': value });
            })(value);
        },
        /*
         判断是否是正确的手机号码.
         return true|false;
         */
        isMobile: function (mobile) {
            var r = '^1[34578][0-9]{9}$',
                RE = new RegExp(r);
            return RE.test(mobile);
        },
        /*
          判断是否是正确的座机号码.
           return true|false;
        */
        isPhone: function (phone) {
            var re = /^0\d{2,3}-?\d{7,8}$/;
            return re.test(phone);
        },
        /*
           判断是否是数字
        */
        isNumber: function (num) {
            var re = /^[0-9]+.?[0-9]*$/;
            return re.test(num);
        },
        isNumLetterLine: function (val) {//只能输入数字、字母、下划线
            var patten = /^[a-zA-Z0-9_]{1,}$/;
            return patten.test(val);
        },
        /*
           简介：字节转KB
           @bit 字节长度
           return kb计量单位
           实例： hm.bitToKb(139915) return 136kb
        */
        bitToKb: function (bitLength) {
            if (bitLength < 1024) return bitLength + '字节';
            var kb = Math.ceil(bitLength / 1024);
            kb += 'kb';
            return kb;
        },
        /*
            简介：判断文件是否属于图片
            return 是图片 true 否则 false
            实例： hm.IsImage('aaa.png') return true; ***************************
        */
        isImage: function (fileName) {
            if (!fileName || fileName.length <= 3) return false;
            fileName = $.trim(fileName);
            var es = fileName.lastIndexOf('.');
            var ex = fileName.substring(es, fileName.length).toUpperCase();
            var arr = ['.BMP', '.PNG', '.GIF', '.JPG', '.JPEG'];
            for (var i = 0; i < arr.length; i++) {
                if (ex == arr[i]) return true;
            }
            return false;
        },
        toAmount: function (v) {
            return v / 100;
        },
        amountToFixed: function (v, p) {
            var str = (v / 100).toFixed(p) + '';
            if (str.indexOf('.') == -1) return str + '.00';

            var arr = str.split('.');

            var s = arr[0] + '.' + arr[1];
            var fixed = arr[1].length;
            for (var i = fixed; i < p; i++) {
                s += '0';
            }
            return s;
        }
    };

    /*
      简介：回车事件
            return 当前com对象;
    
      实例：$('选择器').enter(function () {});
    */
    $.fn.enter = function (fun) {
        this.keydown(function (e) {
            if ((e.keyCode || e.which) == 13) {
                if ($.isFunction(fun)) {
                    fun.call($(this));
                }
            }
        });
        return this;
    };
    //输出hm接口
    exports('hm', hm);
});