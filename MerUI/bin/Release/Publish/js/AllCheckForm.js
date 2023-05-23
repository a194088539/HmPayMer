Validator = {
    Require: /.+/,
    Email: /^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/,
    Phone: /^((\(\d{2,3}\))|(\d{3}\-))?(\(0\d{2,3}\)|0\d{2,3}-)?[1-9]\d{6,7}(\-\d{1,4})?$/,
    Mobile: /^0?(13[0-9]|15[012356789]|17[678]|18[0123456789]|14[57])[0-9]{8}$/,
    Url: /^http:\/\/[A-Za-z0-9]+\.[A-Za-z0-9]+[\/=\?%\-&_~`@[\]\':+!]*([^<>\"\"])*$/,
    IdCard: /^\d{15}(\d{2}[A-Za-z0-9])?$/,
    Currency: /^\d+(\.\d+)?$/,
    Number: /^\d+$/,
    Zip: /^[1-9]\d{5}$/,
    QQ: /^[1-9]\d{4,11}$/,
    Integer: /^[-\+]?\d+$/,
		//身份证号
	Sfz:/^[0-9]{17}[0-9|xX]{1}$/,
    Double: /^[-\+]?\d+(\.\d+)?$/,
    English: /^[A-Za-z]+$/,
    Chinese: /^[\u0391-\uFFE5]+$/,
    UserName: /^[a-zA-Z0-9_]{1,}$/,
    NewPass: /^[A-Za-z0-9]+$/,
    UnSafe: /^(([A-Z]*|[a-z]*|\d*|[-_\~!@#\$%\^&\*\.\(\)\[\]\{\}<>\?\\\/\'\"]*)|.{0,5})$|\s/,
    IsSafe: function(str) { return !this.UnSafe.test(str); },
    SafeString: "this.IsSafe(value)",
    Filter: "this.DoFilter(value, getAttribute('accept'))",
    Limit: "this.limit(value.length,getAttribute('min'),  getAttribute('max'))",
    LimitB: "this.limit(this.LenB(value), getAttribute('min'), getAttribute('max'))",
    Date: "this.IsDate(value, getAttribute('min'), getAttribute('format'))",
    Repeat: "value == document.getElementsByName(getAttribute('to'))[0].value",
    Range: "getAttribute('min') <= (value|0) && (value|0) <= getAttribute('max')",
    Compare: "this.compare(value,getAttribute('operator'),getAttribute('to'))",
    Custom: "this.Exec(value, getAttribute('regexp'))",
    Group: "this.MustChecked(getAttribute('name'), getAttribute('min'), getAttribute('max'))",
    QQorQQEmail: "this.IsQQorQQEmail(value)",
    PhoneOrMobile: "this.IsPhoneOrMobile(value)",
    ErrorItem: [document.forms[0]],
    ErrorMessage: ["以下原因导致提交失败：\t\t\t\t"],
    Validate: function(theForm, mode) {
        var obj = theForm || event.srcElement;
        var count = obj.elements.length;
        this.ErrorMessage.length = 1;
        this.ErrorItem.length = 1;
        this.ErrorItem[0] = obj;
        for (var i = 0; i < count; i++) {
            with (obj.elements[i]) {
                var _dataType = getAttribute("dataType");
                if (typeof (_dataType) == "object" || typeof (this[_dataType]) == "undefined") continue;
                //this.ClearState(obj.elements[i]);
                //如果当前不正确就清空
                var lastNode = obj.elements[i].parentNode.childNodes[obj.elements[i].parentNode.childNodes.length - 1];
                if (lastNode.className != "tipinfot") {
                    this.ClearState(obj.elements[i]);
                }
                if (getAttribute("require") == "false" && value == "") continue;
                switch (_dataType) {
                    case "Date":
                    case "Repeat":
                    case "Range":
                    case "Compare":
                    case "Custom":
                    case "Group":
                    case "Limit":
                    case "LimitB":
                    case "SafeString":
                    case "QQorQQEmail":
                    case "PhoneOrMobile":
                    case "Filter":
                        if (!eval(this[_dataType])) {
                            this.AddError(i, getAttribute("msg"));
                        }
                        break;
                    default:
                        if (!this[_dataType].test(value)) {
                            this.AddError(i, getAttribute("msg"));
                        }
                        break;
                }
            }
        }
        if (this.ErrorMessage.length > 1) {
            mode = mode || 1;
            var errCount = this.ErrorItem.length;
            switch (mode) {
                case 2:
                    for (var i = 1; i < errCount; i++)
                        this.ErrorItem[i].style.color = "red";
                case 1:
                    alert(this.ErrorMessage.join("\n"));
                    this.ErrorItem[1].focus();
                    break;
                case 3:
                    for (var i = 1; i < errCount; i++) {
                        try {
                            this.ErrorItem[i].style.border = "1px solid #FF0000";
                            var span = document.createElement("SPAN");
                            span.id = "__ErrorMessagePanel";
                            span.className = "tipinfof";
                            this.ErrorItem[i].parentNode.appendChild(span);
                            span.innerHTML = this.ErrorMessage[i].replace(/\d+:/, "");
                        }
                        catch (e) { alert(e.description); }
                    }
                    //20110805
                    this.ErrorItem[1].focus();
                    if (this.ErrorItem[1].style.display == "none") {
                        var teshuobj = this.ErrorItem[1];
                        var _detail = "";
                        with (teshuobj) {
                            _detail = getAttribute("detail");
                        }
                        this.ClearState(teshuobj);
                        teshuobj.style.border = "1px solid #FF0000";
                        if (_detail != "") {
                            var span = document.createElement("SPAN");
                            span.id = "__ErrorMessagePanel";
                            span.className = "tipinfof";
                            span.style.color = "#FF0000";
                            teshuobj.parentNode.appendChild(span);
                            span.innerHTML = _detail;
                        }
                    }
                    else {
                        this.ShowDetail(this.ErrorItem[1]);
                    }
                    break;
                default:
                    alert(this.ErrorMessage.join("\n"));
                    break;
            }
            return false;
        }
        return true;
    },
    CheckField: function(obj) {
        ErrorStr = "";
        with (obj) {
            var _dataType = getAttribute("dataType");
            this.ClearState(obj);
            if (!(getAttribute("require") == "false" && value == "")) {
                switch (_dataType) {
                    case "Date":
                    case "Repeat":
                    case "Range":
                    case "Compare":
                    case "Custom":
                    case "Group":
                    case "Limit":
                    case "LimitB":
                    case "SafeString":
                    case "QQorQQEmail":
                    case "PhoneOrMobile":
                    case "Filter":
                        if (!eval(this[_dataType])) {
                            ErrorStr = getAttribute("msg");
                        }
                        break;
                    default:
                        if (!this[_dataType].test(value)) {
                            ErrorStr = getAttribute("msg");
                        }
                        break;
                }
            }
        }
        if (ErrorStr != "") {
            obj.style.border = "1px solid #989898";/*更改input边框颜色*/
            var span = document.createElement("SPAN");
            span.id = "__ErrorMessagePanel";
            span.className = "tipinfof";
            obj.parentNode.appendChild(span);
            span.innerHTML = ErrorStr;
        }
        else {
            obj.style.border = "1px solid #ADADAD";
            var span = document.createElement("SPAN");
            span.id = "__ErrorMessagePanel";
            span.className = "tipinfot";
            obj.parentNode.appendChild(span);
            span.innerHTML = "&nbsp;";
        }
    },
    ShowDetail: function(obj) {
        var _detail = "";
        with (obj) {
            _detail = getAttribute("detail");
        }
        this.ClearState(obj);
        obj.style.border = "1px solid #ADADAD";
        if (_detail != "") {
            var span = document.createElement("SPAN");
            span.id = "__ErrorMessagePanel";
            span.className = "tipinfozs";
            span.style.color = "#666666";
            obj.parentNode.appendChild(span);
            span.innerHTML = _detail;
        }
    },
    limit: function(len, min, max) {
        min = min || 0;
        max = max || Number.MAX_VALUE;
        return min <= len && len <= max;
    },
    LenB: function(str) {
        return str.replace(/[^\x00-\xff]/g, "**").length;
    },
    ClearState: function(elem) {
        with (elem) {
            if (style.color == "red")
                style.color = "";
            var lastNode = parentNode.childNodes[parentNode.childNodes.length - 1];
            if (lastNode.id == "__ErrorMessagePanel")
                parentNode.removeChild(lastNode);
        }
    },
    AddError: function(index, str) {
        this.ErrorItem[this.ErrorItem.length] = this.ErrorItem[0].elements[index];
        this.ErrorMessage[this.ErrorMessage.length] = this.ErrorMessage.length + ":" + str;
    },
    Exec: function(op, reg) {
        return new RegExp(reg, "g").test(op);
    },
    compare: function(op1, operator, op2) {
        if (op1 == null || op1 == "") return false;
        op2 = document.getElementById(op2).value; //获取要比较的字段的值
        switch (operator) {
            case "NotEqual":
                return (op1 != op2);
            case "GreaterThan":
                return (op1 > op2);
            case "GreaterThanEqual":
                return (op1 >= op2);
            case "LessThan":
                return (op1 < op2);
            case "LessThanEqual":
                return (op1 <= op2);
            default:
                return (op1 == op2);
        }
    },
    MustChecked: function(name, min, max) {
        var groups = document.getElementsByName(name);
        var hasChecked = 0;
        min = min || 1;
        max = max || groups.length;
        for (var i = groups.length - 1; i >= 0; i--)
            if (groups[i].checked) hasChecked++;
        return min <= hasChecked && hasChecked <= max;
    },
    DoFilter: function(input, filter) {
        return new RegExp("^.+\.(?=EXT)(EXT)$".replace(/EXT/g, filter.split(/\s*,\s*/).join("|")), "gi").test(input);
    },
    IsQQorQQEmail: function(objvalue) {
        //20111215
        var zz = /^[1-9]\d{4,11}$/;
        if (objvalue.indexOf("@qq.com") > 0 || zz.test(objvalue))
            return true;
        return false;
    },
    IsPhoneOrMobile: function(objvalue) {
        //20111215
        objvalue = objvalue.replace(" ", "");
        var isMobile = false;
        if (this["Mobile"].test(objvalue) && objvalue.length == 11)
            isMobile = true;
        var isPhone = false; //座机：0开头10-12位数字
        if (objvalue.substr(0, 1) == "0" && (objvalue.length >= 10 && objvalue.length <= 12))
            isPhone = true;
        if (isMobile == true || isPhone == true)
            return true;
        return false;
    },
    IsDate: function(op, formatString) {
        formatString = formatString || "ymd";
        var m, year, month, day;
        switch (formatString) {
            case "ymd":
                m = op.match(new RegExp("^((\\d{4})|(\\d{2}))([-./])(\\d{1,2})\\4(\\d{1,2})$"));
                if (m == null) return false;
                day = m[6];
                month = m[5] * 1;
                year = (m[2].length == 4) ? m[2] : GetFullYear(parseInt(m[3], 10));
                break;
            case "dmy":
                m = op.match(new RegExp("^(\\d{1,2})([-./])(\\d{1,2})\\2((\\d{4})|(\\d{2}))$"));
                if (m == null) return false;
                day = m[1];
                month = m[3] * 1;
                year = (m[5].length == 4) ? m[5] : GetFullYear(parseInt(m[6], 10));
                break;
            default:
                break;
        }
        if (!parseInt(month)) return false;
        month = month == 0 ? 12 : month;
        var date = new Date(year, month - 1, day);
        return (typeof (date) == "object" && year == date.getFullYear() && month == (date.getMonth() + 1) && day == date.getDate());
        function GetFullYear(y) { return ((y < 30 ? "20" : "19") + y) | 0; }
    }
}