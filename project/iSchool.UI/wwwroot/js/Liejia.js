(function (window, $, LiejiaJS) {
    if (!('HuLyegaJS' in window)) LiejiaJS = window.LiejiaJS = window.HuLyegaJS = window.LyegaJS = {};
    else return;

    /// LiejiaJS.win
    (function () {
        function render(opt, id) {
            var width = opt.width ? ('' + opt.width) : '';
            width = /\d$/g.test(width) ? width + 'px' : width;
            var height = opt.height ? ('' + opt.height) : '';
            height = /\d$/g.test(height) ? height + 'px' : height;
            var str = '<div id="' + id + '" class="modal fade" role="dialog" style="display:none;" ' + (('backdrop' in opt) ? 'data-backdrop="' + opt.backdrop + '"' : '') + '>'
                + '<div class="modal-dialog modal-xl" role="document" '
                + (width ? 'style="width:' + width + ';max-width:' + width + ';"' : '') + '>' //set style width and max-width
                + '<div class="modal-content" ' + (height ? 'style="height:' + height + ';"' : '') + '>';  //set style height
            if (!opt.noheader) {
                str += '<div class="modal-header">'
                    + '     <h3 class="modal-title">' + (opt.title || '') + '</h3>'
                    + '     <button type="button" class="close" data-dismiss="modal">&times;</button>'
                    + '</div>';
            }
            var body = opt.body || opt.content || '';
            if ($.isFunction(body)) body = body(id);
            str += '<div class="modal-body">' + (body) + '</div>';
            if (!opt.nofooter) {
                str += '<div class="modal-footer">'
                    + '    <button type="button" class="btn btn-secondary btn_yes">确定</button>'
                    + '    <button type="button" class="btn btn-secondary btn_no" data-dismiss="modal">关闭</button>'
                    + '</div>';
            }
            str += '</div>  </div></div>';
            return str;
        }
        /**
        // default opt : 
        {
            width: '',            //eg: '500px' 500 
            height: '',           //eg: '500px' 500
            backdrop: '',
            noheader: false,
            nofooter: false,
            title: '',                  //use when noheader==false
            autoShow: true, 
            autoDispose: false,
            body: '',                   //same as `content`
            content: '',                //same as `body`
            on[iI]nit: undefined,          //function the 'this' is the win-obj
            on[sS]how: undefined,          //function the 'this' is the win-obj
            on[sS]hown: undefined,         //function the 'this' is the win-obj
            on[cC]losed: undefined,        //function the 'this' is the win-obj
            on[dD]ispose: undefined,       //function the 'this' is the win-obj
        }
        //*/
        LiejiaJS.win = function (opt) {
            var id = 'liejiaWin_' + (+(new Date())), ele, o, isDisposed;

            var str = render(opt, id);
            ele = $(str).appendTo('body');

            function init() {
                var med = opt.oninit || opt.onInit;
                $.isFunction(med) && med.call(o);
            }
            ele.on('show.bs.modal', function () {
                var med = opt.onshow || opt.onShow;
                $.isFunction(med) && med.call(o);
            });
            ele.on('shown.bs.modal', function () {
                var med = opt.onshown || opt.onShown;
                $.isFunction(med) && med.call(o);
            });
            ele.on('hidden.bs.modal', function () {
                var med = opt.onclosed || opt.onClosed;
                $.isFunction(med) && med.call(o);
                if ((!isDisposed && !opt.autoDispose) || !ele) return;
                ondispose();
            });
            function ondispose() {
                var med = opt.ondispose || opt.onDispose;
                $.isFunction(med) && med.call(o);
                ele.remove(), ele = o.ele = undefined;
            }

            o = {
                id: id,
                ele: ele,
                show: function () {
                    if (isDisposed) return false;
                    return this.ele.modal('show'), true;
                },
                close: function () {
                    if (isDisposed) return false;
                    return this.ele.modal('hide'), true;
                },
                dispose: function () {
                    if (isDisposed) return;
                    isDisposed = true;
                    if (ele.css('display') == 'none') ondispose();
                    else this.ele.modal('hide');
                },
            };

            init();
            opt.autoShow !== false && ele.modal('show');
            return o;
        };
    })();

    /// LiejiaJS.event
    (function () {
        function bus() {
            // 'name': [{c:[], n:[]}, ...]
            this.$dict = {};
        }
        bus.prototype.fire = function (name) {
            var dict = this.$dict, items = dict[name], o = undefined;
            if (!items) return o;
            var c = items.c = items.n;
            if (!c) return o;
            var args = Array.prototype.slice.call(arguments, 0);
            args[0] = { cancel: !1 };
            for (var i = 0, len = c.length; i < len; i++) {
                o = c[i] && c[i].apply(this, args);
                if (args[0].handled || args.cancel) break;
            }
            return o;
        };
        bus.prototype.emit = bus.prototype.trigger = bus.prototype.raise = bus.prototype.fire;
        bus.prototype.on = function (name, handler) {
            if (typeof handler !== 'function') return;
            var dict = this.$dict;
            var items = dict[name];
            if (!items) dict[name] = items = { c: null, n: null };
            add_or_remove(items, handler, true);
            return handler;
        };
        bus.prototype.off = function (name, handler) {
            var dict = this.$dict;
            if (!handler) return delete dict[name];
            if (typeof handler !== 'function') return;
            var items = dict[name];
            if (!items || !items.n) return;
            add_or_remove(items, handler, false);
        };
        function add_or_remove(items, handler, isAdd) {
            if (items.c === items.n) {
                items.n = !items.c ? [] : items.c.slice(0);
                items.c = null;
            }
            if (isAdd) items.n.push(handler);
            else {
                for (var i = 0, len = items.n.length; i < len; i++) {
                    if (handler === items.n[i]) {
                        items.n.splice(i, 1);
                        --i, len--;
                    }
                }
            }
        }

        LiejiaJS.event = bus;
    })();

    /// LiejiaJS.slimPager
    (function () {
        // onpageChanged(pi, reinit)
        LiejiaJS.slimPager = function (ele, onpageChanged) {
            var f1, f2, e1, e2, ii, ele = $(ele);
            function init() {
                e1 = ele.find('a[data-dt-idx]').on('click', f1 = function () {
                    var a = $(this), i = a.attr('data-dt-idx');
                    if (ii == i) return;
                    load(i);
                });
                e2 = ele.find('.btn-skip').on('click', f2 = function () {
                    var pi = parseInt(ele.find('.span-skip input').val(), 10), maxi = parseInt(ele.find('.span-skip').attr('data-totalPageCount'), 10);
                    pi = pi < 1 ? 1 : pi > maxi ? maxi : pi;
                    if (ii == pi) return;
                    load(pi);
                });
            }
            function load(pi) {
                ii = pi;
                onpageChanged(pi, function () {
                    e1.off('click', f1), e1 = f1 = undefined;
                    e2.off('click', f2), e2 = f2 = undefined;
                    init();
                });
            }  
            init();
        };
    })();

})(window, window.jQuery);

/**
 * 能限制输入正小数几位的输入文本框, 使用方法:
 *  HuLyegaJS.decimalInput({
 *      ele: 'input标签',
 *      decimalPlaces: 2,
 *  });
 *
 * @param {object} opt //设置
 *      ele // html input element
 *      decimalPlaces //最多小数位数
 *      onblur // function(isOk){} //this指向input标签
 */
(function (window, $, HuLyegaJS) {
    HuLyegaJS.decimalInput = function (opt) {
        opt = opt || {};
        var ele = $(opt.ele || '');        
        ele.css("ime-mode", "disabled");
        //ele.bind
        ele.on("keypress", function (e) {
            if (e.charCode === 0) return true;  //非字符键 for firefox
            var code = (e.keyCode ? e.keyCode : e.which);  //兼容火狐 IE
            if (code >= 48 && code <= 57) {
                var pos = getCurPosition(this);
                var selText = getSelectedText(this);
                var dotPos = this.value.indexOf(".");
                if (dotPos > 0 && pos > dotPos) {
                    if (pos > dotPos + opt.decimalPlaces) return false;
                    if (selText.length > 0 || this.value.substr(dotPos + 1).length < opt.decimalPlaces)
                        return true;
                    else
                        return false;
                }
                return true;
            }
            //输入"."
            if (code == 46) {
                var selText = getSelectedText(this);
                if (selText.indexOf(".") > 0) return true; //选中文本包含"."
                else if (/^[0-9]+\.$/.test(this.value + String.fromCharCode(code)))
                    return true;
            }
            return false;
        });
        ele.bind("blur", function () {
            var v0 = this.value;
            if (this.value.lastIndexOf(".") == (this.value.length - 1)) {
                this.value = this.value.substr(0, this.value.length - 1);
            } else if (isNaN(this.value)) {
                this.value = "";
            }
            if (this.value) {
                this.value = parseFloat(this.value); //parseFloat(this.value).toFixed(opt.decimalPlaces);
            }
            $(this).trigger("input");
            var value = this.value;
            var reg = getRegex(opt.decimalPlaces);
            if (reg.test(value)) {
                $(this).val(value);
                opt.onblur && opt.onblur.apply(this, [true, value]);
            } else if (v0 !== '') {
                opt.onblur && opt.onblur.apply(this, [false, v0]);
            }
        });
        ele.bind("paste", function () {
            if (window.clipboardData) {
                var s = clipboardData.getData('text');
                if (!isNaN(s)) {
                    value = parseFloat(s);
                    return true;
                }
            }
            return false;
        });

        ele.bind("dragenter", function () { return false; });
        ele.bind("keyup", function () { });

        ele.bind("propertychange", function (e) {
            if (isNaN(this.value))
                this.value = this.value.replace(/[^0-9\.]/g, "");
        });
        ele.bind("input", function (e) {
            if (isNaN(this.value))
                this.value = this.value.replace(/[^0-9\.]/g, "");
        });

        return ele;
    };
    //获取当前光标在文本框的位置
    function getCurPosition(domObj) {
        var position = 0;
        if (domObj.selectionStart || domObj.selectionStart == '0') {
            position = domObj.selectionStart;
        }
        else if (document.selection) { //for IE
            domObj.focus();
            var currentRange = document.selection.createRange();
            var workRange = currentRange.duplicate();
            domObj.select();
            var allRange = document.selection.createRange();
            while (workRange.compareEndPoints("StartToStart", allRange) > 0) {
                workRange.moveStart("character", -1);
                position++;
            }
            currentRange.select();
        }
        return position;
    }
    //获取当前文本框选中的文本
    function getSelectedText(domObj) {
        if (domObj.selectionStart || domObj.selectionStart == '0') {
            return domObj.value.substring(domObj.selectionStart, domObj.selectionEnd);
        }
        else if (document.selection) { //for IE
            domObj.focus();
            var sel = document.selection.createRange();
            return sel.text;
        }
        else return '';
    }
    // get regex
    function getRegex(reqPointX) {
        return new RegExp('^(' + "\\d+"
            + '(\\.' + (reqPointX === undefined || reqPointX === null ? '\\d+' : reqPointX > 0 ? '(\\d{1,' + reqPointX + '})(0*)' : '(0*)') + '){0,1}'
            + ')$');
    }
})(window, window.jQuery, window.HuLyegaJS);

/**
 *  能用于某个html element里的loading
 *  用法:
 *      HuLyegaJS.openLoading('#xx', 'text');
 *      HuLyegaJS.openLoading('#xx', { text: 'text', color: '' });
 *      HuLyegaJS.closeLoading('#xx');
 */
(function (document, $, HuLyegaJS) {
    var defaultOpts = {
        zindex: 1000,
        img: '/images/loading.gif',
        color: 'black',
        text: undefined, // ''
        mode: 'inner' // parent|inner
    };    
    HuLyegaJS.openLoading = function (ele, option) {
        var text = $.type(option) == 'string' ? option : undefined;
        var opt = $.type(option) == 'object' ? $.extend({}, defaultOpts, option) : $.extend({}, defaultOpts);
        text = text || opt.text;  
        var ele = $(ele);
        var isbody = ele[0] == document.body;

        // close prev loading
        HuLyegaJS.closeLoading(ele);

        // open a new one
        var id = '_hushushu_loading' + (new Date().getTime());
        var html = render(id, text, opt, isbody);
        ele.attr('hushushu-loading-id', id);
        if (isbody) ele.append(html);
        else {
            if (opt.mode == 'parent') ele.parent().append(html);
            else ele.append(html);
        }
        $('#' + id).show();
    };
    HuLyegaJS.closeLoading = function (ele) {
        var ele = $(ele);
        var id = ele.attr('hushushu-loading-id');
        if (id == undefined || id == '') return false;
        $('#' + id).hide();
        ele.removeAttr('hushushu-loading-id');
        $('#' + id).remove();
        return true;
    };
    function render(id, text, opt, isbody) {
        var html = '';
        html += '<div class="hushushu-loading" id=' + id + ' style="display:none;">';
        html += '<div class="hushushu-loading-over" style="position:' + (isbody ? 'fixed' : 'absolute') + ';top:0;left:0;width:100%;height:100%;background-color:'
            + opt.color + ';opacity:0.5;z-index:' + opt.zindex + ';"></div>';
        html += '<div class="hushushu-loading-layout" style="position:' + (isbody ? 'fixed' : 'absolute') + ';top:40%;left:40%;width:20%;height:20%;z-index:' + opt.zindex + ';text-align:center;">';
        html += '<span class="text-white hushushu-loading-text">' + (text || '') + '</span><br/><img src="/images/loading.gif"/>';
        html += '</div>';
        html += '</div>';
        return html;
    }
})(document, window.jQuery, window.HuLyegaJS);
