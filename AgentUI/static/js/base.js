; (function (window) {
    var com = function (selector) {
        return new com.fn.init(selector);
    },
        class2type = [],
        toString = Object.prototype.toString,
        ct = "Boolean Number String Function Array Date RegExp Object Error".split(' ');

    for (var __name in ct) {
        __name.toLowerCase() !== 'indexof' && (class2type["[object " + ct[__name] + "]"] = ct[__name].toLowerCase());
    }
    com.fn = com.prototype = {
        init: function (selector) {
            selector && (this.selector = selector);
        },
        selector: ''
    };

    com.fn.init.prototype = com.fn;
    com.extend = com.fn.extend = function () {
        var src, copyIsArray, copy, name, options, clone,
            target = arguments[0] || {},
            i = 1,
            length = arguments.length,
            deep = false;

        if (typeof target === 'boolean') {
            deep = target;
            target = arguments[0] || {};
            i = 2;
        }

        if (typeof target !== 'object' && !com.isFunction(target)) {
            target = {};
        }
        if (length === i) {
            target = this;
            --i;
        }
        for (; i < length; i++) {
            if ((options = arguments[i]) != null) {
                for (name in options) {
                    src = target[name];
                    copy = options[name];
                    if (target === copy) {
                        continue;
                    }
                    if (deep && copy && (com.isPlainObject(copy) || (copyIsArray = com.isArray(copy)))) {
                        if (copyIsArray) {
                            copyIsArray = false;
                            clone = src && com.isArray(src) ? src : [];
                        } else {
                            clone = src && com.isPlainObject(src) ? src : {};
                        }

                        target[name] = com.extend(deep, clone, copy);

                    } else if (copy !== undefined) {
                        target[name] = copy;
                    }
                }
            }
        }
        return target;
    }

    var _type = function (obj) {
        if (obj == null) {
            return String(obj);
        }
        return typeof obj === 'object' || typeof obj === 'function' ?
            class2type[toString.call(obj)] || "object" :
            typeof obj;
    },
        _isWindow = function (obj) {
            return obj != null && obj == obj.window;
        },
        _isArray = Array.isArray || function (obj) {
            return _type(obj) === 'array';
        },
        _SerializeURL2KeyVal = function (url) {
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
        ForIdCardValid = {
            /*身份证验证 包括出生 年月  和  性别*/
            Wi: [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2, 1],//加权因子
            ValidCode: [1, 0, 10, 9, 8, 7, 6, 5, 4, 3, 2],//身份中验证位数，10代表X
            IdCardValidate: function (idCard) {
                idCard = this.trim(idCard.replace(/ /g, ''));//去掉首尾的空格
                if (idCard.length == 15) {
                    return this.isValidBrithBy15IdCard(idCard);//进行15位身份证的验证
                } else if (idCard.length == 18) {
                    var a_idcard = idCard.split("");//得到身份证数组
                    if (this.isValidBrithBy18IdCard(idCard) && this.isTrueValidateCodeBy18IdCard(a_idcard))//进行18位身份证的基本验证和第18位的验证
                    {
                        return true;
                    }
                    else {
                        return false;
                    }
                } else {
                    return false;
                }
            },

            /***
             *判断身份证号码为18位时最后的验证位是否正确
             *@param a_idcard 身份证号码数组
             *return
            ***/
            isTrueValidateCodeBy18IdCard: function (a_idcard) {
                var sum = 0;
                if (a_idcard[17].toLowerCase() == 'x') {
                    a_idcard[17] = 10;
                }
                for (var i = 0; i < 17; i++) {
                    sum += this.Wi[i] * a_idcard[i];
                }
                var valCodePosition = sum % 11;
                if (a_idcard[17] == this.ValidCode[valCodePosition]) {
                    return true;
                } else
                    return false;
            },
            /***
             *验证18位身份证号码中的生日是否是有效生日
             *@param idcard18 18位身份证号码字符串
             *return
            ***/
            isValidBrithBy18IdCard: function (idcard18) {
                var year = idcard18.substring(6, 10);
                var month = idcard18.substring(10, 12);
                var day = idcard18.substring(12, 14);
                var temp_date = new Date(year, parseFloat(month) - 1, parseFloat(day));
                //这里用getFullYear()获取年份，避免千年虫的问题
                if (temp_date.getFullYear() != parseFloat(year) || temp_date.getMonth() != parseFloat(month) - 1 || temp_date.getDate() != parseFloat(day))
                    return false;
                else {
                    // $("#txtBirthday").val(year + "-" + month + "-" + day);
                    $("#txtBirthday").val(month + "月" + day + "日");
                    $("#hiddenBirthday").val(year + "-" + month + "-" + day);
                    return true;
                }
            },
            /***
             *验证15位身份证号码中的生日是否是有效生日
             *@param idcard15 15位身份证号码字符串
             *return
            ***/
            isValidBrithBy15IdCard: function (idcard15) {
                var year = idcard15.substring(6, 8);
                var month = idcard15.substring(8, 10);
                var day = idcard15.substring(10, 12);
                var temp_date = new Date(year, parseFloat(month) - 1, parseFloat(day));
                //对于老身份证中的年龄则不需要考虑千年虫的问题而是用getYear()方法
                if (temp_date.getYear() != parseFloat(year) || temp_date.getMonth() != parseFloat(month) - 1 || temp_date.getDate() != parseFloat(day))
                    return false;
                else {
                    // $("#txtBirthday").val(temp_date.getFullYear() + "-" + month + "-" + day);
                    $("#txtBirthday").val(month + "月" + day + "日");
                    $("#hiddenBirthday").val(temp_date.getFullYear() + "-" + month + "-" + day);
                    return true;
                }
            },
            /***
             *通过身份证判断性别
             *@param idCard 15/18位身份证号码字符串
             *return ‘0’女，‘1’男
            ***/
            maleOrFemaleByIdCard: function (idCard) {
                if (idCard.length == 15) {
                    if (idCard.substring(14, 15) % 2 == 0)
                        return 0;//女
                    else
                        return 1;//男
                } else if (idCard.length == 18) {
                    if (idCard.substring(14, 17) % 2 == 0)
                        return 0;//女
                    else
                        return 1;//男
                }
            },
            trim: function (str) {
                return str.replace(/(^\s*)|(\s*$)/g, '');
            }
        };

    var MD5 = {
        _defaults: {
            hexcase: 0,
            b64pad: "",
            chrsz: 8
        }
        ,
        Hex_MD5: function (s) { return this._binl2hex(this._core_md5(this._str2binl(s), s.length * this._defaults.chrsz)); },
        Base64_MD5: function (s) { return this._binl2b64(this._core_md5(this._str2binl(s), s.length * this._defaults.chrsz)); },
        Hex_Hmac_MD5: function (key, data) { return this._binl2hex(this._core_hmac_md5(key, data)); },
        Base64_Hmac_MD5: function (key, data) { return this._binl2b64(this._core_hmac_md5(key, data)); },
        CalcMD5: function (s) { return this._binl2hex(this._core_md5(this._str2binl(s), s.length * this._defaults.chrsz)); },
        _md5_vm_test: function () { return hex_md5("abc") == "900150983cd24fb0d6963f7d28e17f72"; },


        _core_md5: function (x, len) {
            x[len >> 5] |= 0x80 << ((len) % 32);
            x[(((len + 64) >>> 9) << 4) + 14] = len;

            var a = 1732584193;
            var b = -271733879;
            var c = -1732584194;
            var d = 271733878;

            for (var i = 0; i < x.length; i += 16) {
                var olda = a;
                var oldb = b;
                var oldc = c;
                var oldd = d;
                a = this._md5_ff(a, b, c, d, x[i + 0], 7, -680876936);
                d = this._md5_ff(d, a, b, c, x[i + 1], 12, -389564586);
                c = this._md5_ff(c, d, a, b, x[i + 2], 17, 606105819);
                b = this._md5_ff(b, c, d, a, x[i + 3], 22, -1044525330);
                a = this._md5_ff(a, b, c, d, x[i + 4], 7, -176418897);
                d = this._md5_ff(d, a, b, c, x[i + 5], 12, 1200080426);
                c = this._md5_ff(c, d, a, b, x[i + 6], 17, -1473231341);
                b = this._md5_ff(b, c, d, a, x[i + 7], 22, -45705983);
                a = this._md5_ff(a, b, c, d, x[i + 8], 7, 1770035416);
                d = this._md5_ff(d, a, b, c, x[i + 9], 12, -1958414417);
                c = this._md5_ff(c, d, a, b, x[i + 10], 17, -42063);
                b = this._md5_ff(b, c, d, a, x[i + 11], 22, -1990404162);
                a = this._md5_ff(a, b, c, d, x[i + 12], 7, 1804603682);
                d = this._md5_ff(d, a, b, c, x[i + 13], 12, -40341101);
                c = this._md5_ff(c, d, a, b, x[i + 14], 17, -1502002290);
                b = this._md5_ff(b, c, d, a, x[i + 15], 22, 1236535329);
                a = this._md5_gg(a, b, c, d, x[i + 1], 5, -165796510);
                d = this._md5_gg(d, a, b, c, x[i + 6], 9, -1069501632);
                c = this._md5_gg(c, d, a, b, x[i + 11], 14, 643717713);
                b = this._md5_gg(b, c, d, a, x[i + 0], 20, -373897302);
                a = this._md5_gg(a, b, c, d, x[i + 5], 5, -701558691);
                d = this._md5_gg(d, a, b, c, x[i + 10], 9, 38016083);
                c = this._md5_gg(c, d, a, b, x[i + 15], 14, -660478335);
                b = this._md5_gg(b, c, d, a, x[i + 4], 20, -405537848);
                a = this._md5_gg(a, b, c, d, x[i + 9], 5, 568446438);
                d = this._md5_gg(d, a, b, c, x[i + 14], 9, -1019803690);
                c = this._md5_gg(c, d, a, b, x[i + 3], 14, -187363961);
                b = this._md5_gg(b, c, d, a, x[i + 8], 20, 1163531501);
                a = this._md5_gg(a, b, c, d, x[i + 13], 5, -1444681467);
                d = this._md5_gg(d, a, b, c, x[i + 2], 9, -51403784);
                c = this._md5_gg(c, d, a, b, x[i + 7], 14, 1735328473);
                b = this._md5_gg(b, c, d, a, x[i + 12], 20, -1926607734);
                a = this._md5_hh(a, b, c, d, x[i + 5], 4, -378558);
                d = this._md5_hh(d, a, b, c, x[i + 8], 11, -2022574463);
                c = this._md5_hh(c, d, a, b, x[i + 11], 16, 1839030562);
                b = this._md5_hh(b, c, d, a, x[i + 14], 23, -35309556);
                a = this._md5_hh(a, b, c, d, x[i + 1], 4, -1530992060);
                d = this._md5_hh(d, a, b, c, x[i + 4], 11, 1272893353);
                c = this._md5_hh(c, d, a, b, x[i + 7], 16, -155497632);
                b = this._md5_hh(b, c, d, a, x[i + 10], 23, -1094730640);
                a = this._md5_hh(a, b, c, d, x[i + 13], 4, 681279174);
                d = this._md5_hh(d, a, b, c, x[i + 0], 11, -358537222);
                c = this._md5_hh(c, d, a, b, x[i + 3], 16, -722521979);
                b = this._md5_hh(b, c, d, a, x[i + 6], 23, 76029189);
                a = this._md5_hh(a, b, c, d, x[i + 9], 4, -640364487);
                d = this._md5_hh(d, a, b, c, x[i + 12], 11, -421815835);
                c = this._md5_hh(c, d, a, b, x[i + 15], 16, 530742520);
                b = this._md5_hh(b, c, d, a, x[i + 2], 23, -995338651);
                a = this._md5_ii(a, b, c, d, x[i + 0], 6, -198630844);
                d = this._md5_ii(d, a, b, c, x[i + 7], 10, 1126891415);
                c = this._md5_ii(c, d, a, b, x[i + 14], 15, -1416354905);
                b = this._md5_ii(b, c, d, a, x[i + 5], 21, -57434055);
                a = this._md5_ii(a, b, c, d, x[i + 12], 6, 1700485571);
                d = this._md5_ii(d, a, b, c, x[i + 3], 10, -1894986606);
                c = this._md5_ii(c, d, a, b, x[i + 10], 15, -1051523);
                b = this._md5_ii(b, c, d, a, x[i + 1], 21, -2054922799);
                a = this._md5_ii(a, b, c, d, x[i + 8], 6, 1873313359);
                d = this._md5_ii(d, a, b, c, x[i + 15], 10, -30611744);
                c = this._md5_ii(c, d, a, b, x[i + 6], 15, -1560198380);
                b = this._md5_ii(b, c, d, a, x[i + 13], 21, 1309151649);
                a = this._md5_ii(a, b, c, d, x[i + 4], 6, -145523070);
                d = this._md5_ii(d, a, b, c, x[i + 11], 10, -1120210379);
                c = this._md5_ii(c, d, a, b, x[i + 2], 15, 718787259);
                b = this._md5_ii(b, c, d, a, x[i + 9], 21, -343485551);
                a = this._safe_add(a, olda);
                b = this._safe_add(b, oldb);
                c = this._safe_add(c, oldc);
                d = this._safe_add(d, oldd);
            }
            return Array(a, b, c, d);

        },
        _md5_cmn: function (q, a, b, x, s, t) {
            return this._safe_add(this._bit_rol(this._safe_add(this._safe_add(a, q), this._safe_add(x, t)), s), b);
        },
        _md5_ff: function (a, b, c, d, x, s, t) {
            return this._md5_cmn((b & c) | ((~b) & d), a, b, x, s, t);
        },
        _md5_gg: function (a, b, c, d, x, s, t) {
            return this._md5_cmn((b & d) | (c & (~d)), a, b, x, s, t);
        },
        _md5_hh: function (a, b, c, d, x, s, t) {
            return this._md5_cmn(b ^ c ^ d, a, b, x, s, t);
        },
        _md5_ii: function (a, b, c, d, x, s, t) {
            return this._md5_cmn(c ^ (b | (~d)), a, b, x, s, t);
        },
        _core_hmac_md5: function (key, data) {
            var bkey = this._str2binl(key);
            if (bkey.length > 16) bkey = this._core_md5(bkey, key.length * this._defaults.chrsz);

            var ipad = Array(16), opad = Array(16);
            for (var i = 0; i < 16; i++) {
                ipad[i] = bkey[i] ^ 0x36363636;
                opad[i] = bkey[i] ^ 0x5C5C5C5C;
            }

            var hash = this._core_md5(ipad.concat(this._str2binl(data)), 512 + data.length * this._defaults.chrsz);
            return this._core_md5(opad.concat(hash), 512 + 128);
        },
        _safe_add: function (x, y) {
            var lsw = (x & 0xFFFF) + (y & 0xFFFF);
            var msw = (x >> 16) + (y >> 16) + (lsw >> 16);
            return (msw << 16) | (lsw & 0xFFFF);
        },
        _bit_rol: function (num, cnt) {
            return (num << cnt) | (num >>> (32 - cnt));
        },
        _str2binl: function (str) {
            var bin = Array();
            var mask = (1 << this._defaults.chrsz) - 1;
            for (var i = 0; i < str.length * this._defaults.chrsz; i += this._defaults.chrsz)
                bin[i >> 5] |= (str.charCodeAt(i / this._defaults.chrsz) & mask) << (i % 32);
            return bin;
        },
        _binl2hex: function (binarray) {
            var hex_tab = this._defaults.hexcase ? "0123456789ABCDEF" : "0123456789abcdef";
            var str = "";
            for (var i = 0; i < binarray.length * 4; i++) {
                str += hex_tab.charAt((binarray[i >> 2] >> ((i % 4) * 8 + 4)) & 0xF) +
                    hex_tab.charAt((binarray[i >> 2] >> ((i % 4) * 8)) & 0xF);
            }
            return str;
        },
        _binl2b64: function (binarray) {
            var tab = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            var str = "";
            for (var i = 0; i < binarray.length * 4; i += 3) {
                var triplet = (((binarray[i >> 2] >> 8 * (i % 4)) & 0xFF) << 16)
                    | (((binarray[i + 1 >> 2] >> 8 * ((i + 1) % 4)) & 0xFF) << 8)
                    | ((binarray[i + 2 >> 2] >> 8 * ((i + 2) % 4)) & 0xFF);
                for (var j = 0; j < 4; j++) {
                    if (i * 8 + j * 6 > binarray.length * 32) str += this._defaults.b64pad;
                    else str += tab.charAt((triplet >> 6 * (3 - j)) & 0x3F);
                }
            }
            return str;
        }
    };

    com.extend({
        /*输出调试*/
        log: function () {
            try { window.console && window.console.log && console.log(arguments) } catch (e) { };
        },
        isPlainObject: function (obj) {
            var key, core_hasOwn = class2type.hasOwnProperty;
            if (!obj || _type(obj) !== "object" || obj.nodeType || _isWindow(obj)) {
                return false;
            }
            try {
                if (obj.constructor &&
                    !core_hasOwn.call(obj, 'constructor') &&
                    !core_hasOwn.call(obj.constructor.prototype, 'isPrototypeOf')) {
                    return false;
                }
            } catch (e) {
                return false;
            }
            var a = navigator.userAgent.toLowerCase();
            if (a = /msie (\d)/ig.exec(a) && parseInt(a[1]) < 9) {
                for (key in obj) {
                    return core_hasOwn.call(obj, key);
                }
            }

            for (key in obj) { }
            return key === undefined || core_hasOwn.call(obj, key);

        },
        isWindow: function (obj) {
            return obj != null && obj == obj.window;
        },
        isArray: _isArray,
        isFunction: function (obj) {
            return _type(obj) === 'function';
        },
        isNullorEmpty: function (obj) {
            return !(obj && obj != null && obj.toString().replace(/ /ig, '').length > 0);
        },
        type: _type,
        isIPAddress: function (address) {
            var r = '^(?:(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9][0-9]|[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9][0-9]|[0-9])$',
                RE = new RegExp(r);
            return RE.test(address);
        },
        isMacAddress: function (address) {
            var r = '^([A-Fa-f0-9]{2}-[A-Fa-f0-9]{2}-[A-Fa-f0-9]{2}-[A-Fa-f0-9]{2}-[A-Fa-f0-9]{2}-[A-Fa-f0-9]{2})$',
                RE = new RegExp(r);
            return RE.test(address);
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
        /*
          判断是否是正确的身份证号码
        */
        isIdCardValid: function (idcard) {
            return ForIdCardValid.IdCardValidate(idcard);
        },
        /* 
            http://www.JSON.org/json2.js
            2010-03-20
            Public Domain.
            NO WARRANTY EXPRESSED OR IMPLIED. USE AT YOUR OWN RISK.
            See http://www.JSON.org/js.
            This code should be minified before deployment.
            See http://javascript.crockford.com/jsmin.html
            USE YOUR OWN COPY. IT IS EXTREMELY UNWISE TO LOAD CODE FROM SERVERS YOU DO NOT CONTROL.
        */
        JSON2str: function (value) {
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
         序列化URL 
         例： 
         com.SerializeURL2KeyVal("a.aspx?A=1&B=2")  序列为键值对。 不填写 默认为当前地址
    
         @url 序列为键值对。 不填写 默认为当前地址
        */
        SerializeURL2KeyVal: function (url) {
            return _SerializeURL2KeyVal(url);
        },
        /*         
          简介：序列化URL
              @url 序列为Json对象。 不填写 默认为当前地址 
              return 序列为Json对象

          实例：com.SerializeURL2Json("a.aspx?A=1&B=2");  
       */
        SerializeURL2Json: function (url) {
            var result = {};
            var arr = _SerializeURL2KeyVal(url);
            for (var i = 0; i < arr.length; i++) {
                var t = arr[i];
                result[t.key] = t.value;
            }
            return result;
        },
        /*
            简介：字节转KB
            @bit 字节长度
            return kb计量单位
            实例： com.BitToKb(139915) return 136kb
        */
        BitToKb: function (bitLength) {
            if (bitLength < 1024) return bitLength + '字节';
            var kb = Math.ceil(bitLength / 1024);
            kb += 'kb';
            return kb;
        },
        /*
            简介：得到字符串的base64Md5效验码          
            return base64字符串
            实例： com.Base64_MD5('你好') return ***************************
        */
        Base64_MD5: function (str) {
            return MD5.Base64_MD5(str);
        },
        /*
            简介：判断文件是否属于图片
            return 是图片 true 否则 false
            实例： com.IsImage('aaa.png') return true; ***************************
        */
        IsImage: function (fileName) {
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
        /*
          简介：验证密码【长度为6-20位，只能包含数字字母】
          return 是图片 true 否则 false
          实例： com.validatePassword('a123456') return true; ***************************
      */
        validatePassword: function (strPassword) {
            //!/^.{6,20}$/.test(_Pwd.trim())) {///^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{6,20}/
            return /^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{6,20}$/.test(strPassword.trim());
        }
    });
    window.com = com;
})(window);

com.fn.extend({
    /*       
        格式化时间（支持时间戳）。
            @format 格式：
                        [yy|yyyy]年、M/月、d/日、h/12小时制、H/24小时制、m/分、s/秒、w/星期、q/季度、S/毫秒、n/[上午|下午]
            return '格式化时间'
        实例：
            1、com(new Date()).formatDate('yyyy年MM月dd日 n hh:mm:ss S毫秒 w 第q季度')
            2、com(时间).formatDate('yyyy年MM月dd日')
            3、com('Date(13568954655892)').formatDate('yyyy年MM月dd日')
            4、com('13568954655892').formatDate('yyyy年MM月dd日')
       */
    formatDate: function (format, t) {
        !format && (format = 'yyyy/MM/dd');
        var dt = this.selector, o;
        if (!com.isNullorEmpty(dt) && com.type(dt) !== 'date') {
            if (/Date\([\d\+]+(-\d+)?\)/ig.test(dt)) {       ///Date(13568954655892) 时间戳
                dt.charAt(0) == '/' && (dt = dt.replace(/\//ig, ''));
                dt = Function("return new " + dt)();
            } else if (/^-?(\d)+(.\d{1,})?$/.test(dt)) {//13568954655892 时间戳
                if (t && t == 12) {
                    var s = dt.toString().length;
                    if (s < 12) {
                        s = 12 - s;
                        s = Math.pow(10, s);
                    } else s = 1;
                }
                else {
                    s = 1000;
                }
                dt = new Date(parseInt(dt) * s);
            } else {
                dt = new Date(0);
            }
        }
        else if (com.type(dt) !== 'date') {
            dt = new Date(0);
        }

        o = {
            'M': (dt.getMonth() + 1) < 10 ? '0' + (dt.getMonth() + 1) : dt.getMonth() + 1, //月
            'd': dt.getDate() < 10 ? '0' + dt.getDate() : dt.getDate(),               //日
            'H': dt.getHours() < 10 ? '0' + dt.getHours() : dt.getHours(),            //时24
            'h': dt.getHours() > 12 ? dt.getHours() - 12 : dt.getHours(),             //时12
            'm': dt.getMinutes() < 10 ? '0' + dt.getMinutes() : dt.getMinutes(),      //分
            's': dt.getSeconds() < 10 ? '0' + dt.getSeconds() : dt.getSeconds(),      //秒
            'w': '\u661F\u671F' + '\u65e5\u4e00\u4e8c\u4e09\u56db\u4e94\u516d'.charAt(dt.getDay()),        //星期？
            'q': Math.floor((dt.getMonth() + 3) / 3),     //第几季
            'S': dt.getMilliseconds(),                    //毫秒
            'n': (dt.getHours() < 13 ? '\u4E0A' : '\u4E0B') + '\u5348',
            'yy': (dt.getFullYear() + '').substr(2),      //年
            'yyyy': dt.getFullYear()                      //年
        };

        return format.replace(/(M|d|h|m|s|w|q|S|n|H)+|(yyyy|yy)+/g, function () {
            var arg = arguments;
            if (arg != null) {
                return o[arg[0].charAt(0) == 'y' ? arg[2] : arg[0].charAt(0)];
            }
        });
    }
});


(function ($) {
    if (!$) return;
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

    /*
      简介：限制输入格式
            @type 输入类型
            @copy 是否开启复制功能
            return 当前$对象;
    
      实例：$('选择器').limit();
    */
    $.fn.limit = function (type, copy) {
        var prevent = function (e) {
            e.preventDefault ? e.preventDefault() : e.returnValue = false;
        },
            isCopy = function (e, c) {
                //判断是否是复制粘贴
                if (e.ctrlKey && (c == 67 || c == 69 || c == 86 || c == 118))
                    return true;
                else
                    return false;
            },
            isNumber = function (c) {
                if (
                    (c >= 48 && c <= 57) || (c >= 96 && c <= 105) || c == 8 || c == 46 || (c > 37 && c < 41)
                )
                    return true;
                else
                    return false;
            };

        this.keydown(function (e) {
            var ee = e || window.event;
            var c = e.charCode || e.keyCode;
            if (copy && isCopy(e, c)) {
                return;
            }

            switch (type) {
                case 'num': //只能输入数字
                    if (!isNumber(c) || e.shifKey)
                        prevent(e);
                    break;
            }

        });
        return this;
    };
    /*
      简介：获取控件非默认值
    */
    $.fn.getRealVal = function () {
        var v = $.trim(this.val());
        if (v == this[0].defaultValue) return "";
        else return v;
    },
        /*
            简介：设置input值
                @v 要设置的值，如果为空则不设置            
                return 当前$对象;
            
            实例：$('选择器').limit();
        */
        $.fn.setInputVal = function (v) {
            if (!v) return this;
            v = $.trim(v);
            if (v.length == 0 || v == '') this.val(this[0].defaultValue);
            this.val(v);
        };

    $.extend({
        HM: {
            host: window.location.protocol + "//" + window.location.host + "/",
            ajax: function (url, options) {
                url = $.HM.host + url;
                var settings = $.extend({
                    'data': {},
                    'type': 'POST',
                    'dataType': 'json',
                    'load': true,
                    'success': function () {
                    }
                }, options);

                var _hmindex = 0;
                $.ajax(url, {
                    'data': settings.data,
                    type: settings.type,
                    dataType: settings.dataType,
                    beforeSend: function () {
                        if (settings.load)
                            _hmindex = layer.msg("正在请求中请稍后...", { icon: 16, time: 0 });
                    },
                    success: function (d) {
                        if (settings.load)
                            layer.close(_hmindex);

                        if (d.code == 301) {
                            window.location = window.location.protocol + "//" + window.location.host + d.data;
                            return;
                        }
                        if (settings.success) {
                            settings.success(d);
                        }
                    },
                    error: function (x, t, e) {
                        if (settings.load)
                            layer.close(_hmindex);
                    }
                });

            },
            isIdCard: function (idcard) {
                return com.isIdCardValid(idcard);
            },
            amountToFixed: function (v, p) {
                var rate = p * 10;
                var fixed = parseInt((v / 100) * rate) / rate;
                var s = fixed.toString();
                var rs = s.indexOf('.');
                if (rs < 0) {
                    rs = s.length;
                    s += '.';
                }
                while (s.length <= rs + p) {
                    s += '0';
                }
                return s;
            }
        }
    });
})(jQuery);


//#region 数据验证
var validate = {
    isInteger: function (val) {//验证正整数，包括0
        var patten = /^[1-9]\d*|0$/;
        return patten.test(val);
    },

    isIdcard: function (val) {//验证身份证 
        var patten = /^\d{15}(\d{2}[A-Za-z0-9])?$/;
        return patten.test(val);
    },

    idPhone: function (val) {//验证电话号码 
        var patten = /^((\(\d{2,3}\))|(\d{3}\-))?(\(0\d{2,3}\)|0\d{2,3}-)?[1-9]\d{6,7}(\-\d{1,4})?$/;
        return patten.test(val);

    },

    isMobile: function validateNum(val) {// 验证手机号码 
        var patten = /^(13|15|17|18|14)\d{9}$/;
        return patten.test(val);

    },

    isTelephone: function (val) { //验证手机或电话号
        var patten = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1})|(0[0-9]{2,3}))+\d{7,8})$/;
        return patten.test(val);
    },

    isEmail: function (val) {//验证email账号
        var patten = /^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/;
        return patten.test(val);
    },

    isNum: function (val) {//验证整数
        var patten = /^-?\d+$/;
        return patten.test(val);
    },

    isRealNum: function (val) {//验证实数 
        var patten = /^-?\d+\.?\d*$/;
        return patten.test(val);

    },

    isFloat2: function validateNum(val) {//验证小数，保留2位小数点 
        var patten = /^-?\d+\.?\d{0,2}$/;
        return patten.test(val);

    },

    isFloat: function (val) {//验证小数
        var patten = /^-?(?:\d+|\d{1,3}(?:,\d{3})+)(?:\.\d+)?$/;
        return patten.test(val);
    },

    isNumOrLetter: function (val) {//只能输入数字和字母
        var patten = /^[A-Za-z0-9]+$/;
        return patten.test(val);
    },

    isColor: function (val) {//验证颜色
        var patten = /^#[0-9a-fA-F]{6}$/;
        return patten.test(val);
    },

    isUrl: function (val) { //验证URL
        var patten = /^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$/i;
        return patten.test(val);
    },

    isNull: function (val) {//验证空
        if (!val) {
            return true;
        }
        return val.replace(/\s+/g, "").length == 0;
    },

    isData: function (val) {//验证时间2010-10-10
        var patten = /^\d{4}[\/-]\d{1,2}[\/-]\d{1,2}$/;
        return patten.test(val);
    },

    isNumLetterLine: function (val) {//只能输入数字、字母、下划线
        var patten = /^[a-zA-Z0-9_]{1,}$/;
        return patten.test(val);
    },
    //时间格式化
    changeDateFormat: function (jsondate, formart) {
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
    //表单序列化
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
    }
}
//#endregion 数据验证


//#region 分页控件

//替换指定传入参数的值,paramName为参数,replaceWith为新值 跳转
function repUrlGo(paramName, replaceWith) {
    var oUrl = this.location.href.toString();
    if (oUrl.toLowerCase().indexOf(paramName) > -1) {
        oUrl = repUrl(paramName, replaceWith);
    } else {
        if (oUrl.indexOf("?") > 0) {
            oUrl = oUrl + "&" + paramName + "=" + replaceWith;
        } else {
            oUrl = oUrl + "?" + paramName + "=" + replaceWith;
        }
    }

    this.location = oUrl.repAllNull('#');
}

String.prototype.repAll = function (exp, exp1) { return this.replace(new RegExp(exp, "g"), exp1); }
String.prototype.repAllNull = function (exp) { return this.repAll(exp, ''); }

//替换指定传入参数的值,paramName为参数,replaceWith为新值 不跳转
function repUrl(paramName, replaceWith) {
    var oUrl = this.location.href.toString();
    var re = eval('/(' + paramName + '=)([^&]*)/gi');
    return oUrl.replace(re, paramName + '=' + replaceWith);
}

//分页 DIVid total总数 pagesize分页大小 pageindex当前页
function page(id, total, pagesize, pageindex) {

    if (pagesize >= total) {
        return false;
    }
    $("#" + id).pagination(total,
        {
            callback: function (p) { p = p + 1; repUrlGo("pageindex", p); },
            prev_text: '‹',
            next_text: '›',
            items_per_page: pagesize,
            num_display_entries: 3,
            current_page: pageindex,
            num_edge_entries: 1
        });
}
//脚本异步分页
function pageSarch(id, total, pagesize, pageindex) {
    if (pagesize >= total) {
        return false;
    }
    $("#" + id).pagination(total,
        {
            callback: function (p) { p = p + 1; showPageSearch(p); },
            prev_text: '‹',
            next_text: '›',
            items_per_page: pagesize,
            num_display_entries: 3,
            current_page: pageindex,
            num_edge_entries: 1
        });
}

//#endregion 分页控件


