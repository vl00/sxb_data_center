function Completion(select, b = 0, c = 0, isMorePage = false) {
    
    //设置默认参数

    var $select = jQuery("#" + select);
    //所有可见的Input
    //var allInput = $select.find("input[type=text]:not('#Name'):visible");
    var allInput = isMorePage ? $select
        .find("input:not('.bootstrap-tagsinput input'):not('.c_ignore'):not('#editor1'):not('.map input'):not('input[type=hidden]')") :
        $select.find("input:not('.bootstrap-tagsinput input'):not('.c_ignore'):not('#editor1'):not('.map input'):visible");

    var containInput = $select.find(".c_contain");
    if (containInput.length > 0) {
        jQuery(containInput).each(function () {
            allInput.push(this);
        });
    }
    //将textarea也包含进去
    var allTextarea = jQuery("textarea:visible");
    if (allTextarea.length > 0) {
        jQuery(allTextarea).each(function () {
            allInput.push(this);
        });
    }



    var allRadio = isMorePage ? $select.find("input[type=radio]") : $select.find("input[type=radio]:visible");
    var allcheckedRadio = isMorePage ? $select.find("input[type=radio]:checked") : $select.find("input[type=radio]:visible:checked");
    //将select2添加进去
    var allSelect = isMorePage ? $select.find("select:not('.select2-hidden-accessible')")
        : $select.find("select:not('.select2-hidden-accessible'):visible");
    $select.find("select.select2-hidden-accessible").each(function () {
        allSelect.push(this);
    });


    //遍历radio
    var radios = [];
    if (allRadio.length > 0) {
        jQuery.each(allRadio, function (index, item) {
            var radioName = jQuery(this).prop("name");
            if (radios.indexOf(radioName) === -1) {
                radios.push(radioName);
            }
        });
    }
    console.log(radios);
    //遍历Input
    var inputs = [];
    var emptyInputs = [];
    if (allInput.length > 0) {
        jQuery.each(allInput, function (index, item) {
            var inputName = jQuery(this).prop("name");
            if (inputs.indexOf(inputName) === -1) {
                inputs.push(inputName);
                var selectInputs = $select.find("[name='" + inputName + "']");
                if (selectInputs.length === 1) {
                    //一个input
                    if (!jQuery.trim(selectInputs.val())) {
                        emptyInputs.push(inputName);
                    }
                } else {
                    //遍历input
                    emptyInputs.push(inputName);
                    jQuery.each(selectInputs, function (index, item) {
                        if (jQuery.trim(jQuery(item).val())) {
                            var indexof = emptyInputs.indexOf(inputName);
                            emptyInputs.splice(indexof, 1);
                            return true;
                        }
                    });
                }
            }
        });
    }
    console.log(inputs);
    console.log(emptyInputs);

    //遍历select
    var selects = [];
    var emptySelects = [];
    jQuery.each(allSelect, function (index, item) {
        var selectName = jQuery(this).prop("name");
        if (selects.indexOf(selectName) === -1) {
            selects.push(selectName);
            var selectInputs = $select.find("select[name='" + selectName + "']");
            if (selectInputs.length === 1) {
                //一个select
                var selectValue = jQuery.trim(selectInputs.val());
                if (!selectValue || selectValue === "0") {
                    emptySelects.push(selectName);
                }
            } else {
                //遍历select
                emptySelects.push(selectName);
                var moreselectValue = jQuery.trim(selectInputs.val());
                if (moreselectValue && moreselectValue !== "0") {
                    var indexof = emptyInputs.indexOf(selectName);
                    emptySelects.splice(indexof, 1);
                    return true;
                }
            }

        }
    });

    console.log(selects);
    console.log(emptySelects);



    var count = radios.length + inputs.length + selects.length;
    var check = allcheckedRadio.length + (inputs.length - emptyInputs.length) + (selects.length - emptySelects.length);

    //已统计的数值完成率
    var completionValue = $select.find(".c_value");
    jQuery.each(completionValue, function (index, item) {
        count += 1;
        var cValue = jQuery.trim(jQuery(this).val());
        if (cValue === undefined || cValue === null || cValue === "0" || cValue === "") {
            cValue = 0;
        } else {
            cValue = parseFloat(cValue);
        }
        check += cValue;
    });

    //隐藏的json数值
    var completionJson = $select.find(".JsonData");
    jQuery.each(completionJson, function (index, item) {
        count += 1;
        var jvalue = jQuery.trim(jQuery(this).val());
        var last = jvalue.indexOf("]");
        if (last > 2) {
            check += 1;
        }
    });

    //如果有VUE写的图片控件获取图片控件的完成度
    if (typeof app != "undefined") {
        var imgArray = app.comoletion();
        b += imgArray[1];
        c += imgArray[0];
    }

    console.log((check + b) / (count + c));
    if ((count + c) === 0) {
        return 1;
    }
    else {
        return (check + b) / (count + c);
    }
}
