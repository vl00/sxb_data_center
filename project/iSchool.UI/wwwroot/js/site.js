if (typeof Function.prototype.bind !== 'function') {
    Function.prototype.bind = function (scope) {
        var self = this;
        return function () { return self.apply(scope, arguments); }
    };
}

/// enable/disable element
(function ($) {
    if ($.type($.fn.enable) === 'undefined') $.fn.enable = function () { return $(this).removeAttr('disabled') };
    if ($.type($.fn.disable) === 'undefined') $.fn.disable = function () { return $(this).attr('disabled', 'disabled') };
    if ($.type($.fn.isEnabled) === 'undefined') $.fn.isEnabled = function () { return $(this).attr('disabled') !== 'disabled' };
    if ($.type($.fn.isDisabled) === 'undefined') $.fn.isDisabled = function () { return $(this).attr('disabled') === 'disabled' };
})(jQuery);

/// jQuery.app
if (jQuery.type(jQuery.app) === 'undefined')
    jQuery.app = {};

/// nextTick
(function (app) {
    app.nextTick = function (f) {
        ("object" == typeof process && process.nextTick ? process.nextTick : function (task) { setTimeout(task, 0); })(f);
    };
})(jQuery.app);

/// 全局ajax拦截
(function (cb) {
    cb(window, document, jQuery, jQuery.app);
})(function (window, document, $, app) {
    if (app.globalAjax)
        return;

    var noNeedHandleError, ajax = $.ajax;

    $.ajax = function (a1, a2) {
        var jqxhr = ajax(a1, a2);
        jqxhr.ignoreGlobal = function () {
            return $.globalAjax.ignoreIfError(this), this;
        };
        return jqxhr;
    };

    $(document).ajaxComplete(function (event, jqxhr, settings, err) {
        if (noNeedHandleError) return;
        if (jqxhr['__ignoreIfError__']) return;

        function oncomplete() {
            if (jqxhr['__ignoreIfError__']) return;
            
            console.log({ jqxhr, settings, err });
             
            if (jqxhr.status == 401) {
                return jqxhr.responseText ? window.location.replace(jqxhr.responseText) : window.location.reload();
            }

            var j = jqxhr.responseJSON;
            if (jqxhr.status == 400 && j && !j.isOk && (j.errCode || j.code) && (j.msg !== undefined)) {
                console.log(j.msg + (j.stackTrace ? ('\n' + j.stackTrace) : ''));
                j.msg && (window.ShowAlert || alert)(j.msg);
                window.CloseLoading && window.CloseLoading();
                return;
            }
        }

        // ensure oncomplete raise after 'jqxhr.then(...)'
        if ('success' in settings || 'error' in settings || 'complete' in settings) oncomplete();
        else app.nextTick(oncomplete);
    });

    $.globalAjax = app.globalAjax = {
        noNeedHandleError: function (b) { noNeedHandleError = b },

        ignoreIfError: function (jqxhr) {
            if (jqxhr && jqxhr.then && ('readyState' in jqxhr)) {
                jqxhr['__ignoreIfError__'] = true;
            }
            return jqxhr;
        },
    };
});

/// 用于移除ue文本style
function rmStyle(str) {
    var m, str = str.replace(/(style|class)=\"[^\"]*\"/gi, '');
    /** 
    while (m = /(?<h>\<(?<end>\/*)(?<ele>h\d|em|b|strong|small|s)((?<nbsp>\s+)|>))/gi.exec(str)) {
        var has_nbsp = !!m.groups['nbsp'], has_end = !!m.groups['end'];
        var h = m.groups['h'], ele = m.groups['ele'];
        // js regex named-groups 貌似只有谷歌核的浏览器才支持！！！
     */
    while (m = /(\<(\/*)(h\d|em|b|strong|small|s)((\s+)|>))/gi.exec(str)) {
        var has_nbsp = !!m[5], has_end = !!m[2];
        var h = m[1], ele = m[3];
        str = (!m.index ? '' : str.substring(0, m.index))
            + (has_end ? '</' : '<') + (ele[0] == 'h' || ele[0] == 'H' ? 'p' : 'span') + (has_nbsp ? ' ' : '>')
            + str.substring(m.index + h.length);
    }
    return str;
}  

/// 修正ue图片：禁止上传.webp图片, 去掉ueditor目录的图片(可能是截图粘贴上传或粘贴word等本地粘贴板上传造成的...)
function resolveUeditorContentImgs(str) {
    if (!str) return str;
    var div = jQuery('#_div_resolveUecontentImgs');
    if (div.length < 1) {
        div = jQuery('<div id="_div_resolveUecontentImgs" style="display:none;width:0;height:0;"></div>').appendTo('body');
    }
    div.html(str), str = div.html();
    var imgs = jQuery.map(div.find('img'), function (_) { return _ });
    for (var i = 0, len = imgs.length; i < len; i++) {
        var img = imgs[i];
        if (!img.src) {
            str = str.replace(img.outerHTML, '');
        }
        else if (/\.webp$/gi.test(img.src)) {
            throw new Error('含有.webp图片');
        }
        else if (/^\/ueditor\//gi.test(img.src.replace(/^(http|https):\/\/[^\/]+/i, ''))) {
            str = str.replace(img.outerHTML, '');
        }
    }
    div.html('');
    return jQuery("<textarea/>").html(str).text();
}

/**
 * 格式化为n位小数。成功时返回数值, 失败返回undefined
 * 
 * @param {string} value     
 * @param {bool?} reqPosnum  //true=要求正数; false=要求负数; null=不要求正负
 * @param {uint?} reqPointX   //null=不要求最多几位小数; 非null=要求最多几位小数; 0=要求为整数
 */
function regexParseToDouble2(value, reqPosnum, reqPointX) {
    var gs = new RegExp('^(' + "([\\+\\-]{0,1})" + "\\d+"
        + '(\\.' + (reqPointX === undefined || reqPointX === null ? '\\d+' : reqPointX > 0 ? '(\\d{1,' + reqPointX + '})(0*)' : '(0*)') + '){0,1}'
        + ')$');
    var a = gs.exec(value);
    a = a ? parseFloat(a[0], 10) : undefined;
    if (a === undefined) return;
    if (reqPosnum == true && a < 0) return;
    if (reqPosnum == false && a >= 0) return;
    return a;
}
