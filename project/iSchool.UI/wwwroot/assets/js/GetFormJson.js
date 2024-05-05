(function ($) {
    $.fn.getFormJSON = function (options) {
        var defaultSetting = $.extend({ //设置默认值
            "isRepeat": true //是否开启多个值的输入控件，例如复选框、多选的select
        }, options);
        var jsonObj = {};

        if (defaultSetting.isRepeat) {
            var $form = this;
            $.each(this.serializeArray(), function () {
                var $name = this.name;
                
                var isJsonArray = jQuery(jQuery($form).find("[name='" + this.name + "']")[0]).hasClass("JsonArray");
                if (!jsonObj[$name] && isJsonArray) { jsonObj[$name] = [this.value]; }
                else if (jsonObj[$name]) {//判断jsonObj里面是否有数据
                    if ($.isArray(jsonObj[$name])) {//判断jsonObj的属性是否是数组
                        jsonObj[$name].push(this.value);
                    } else {
                        jsonObj[$name] = [jsonObj[$name], this.value];
                    }
                }
                else {
                    jsonObj[$name] = this.value;
                }
            });
        } else {
            $.each(this.serializeArray(), function () {
                var $name = this.name;
                if (!(jsonObj[$name])) {//判断jsonObj内是否有this.name属性 ---只保留第一个值
                    jsonObj[$name] = this.value;
                }
            });
        }
        return jsonObj;
    };
})(jQuery);
