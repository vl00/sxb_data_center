
(function ($, window, document, undefined) { //用一个自调用匿名函数把插架代码包裹起来，防止代码污染
    var $ = jQuery;
    $.fn.mySelect = function (options) {

        var defaults = {          //defaults 使我们设置的默认参数。
            Event: "click",      //触发响应事件
            msg: "Holle word!"   //显示内容
        };
        var options = $.extend(defaults, options);    //将传入参数和默认参数合并
        console.log(options);
        var $this = $(this);      //当然响应事件对象
        var $inputvalueId = $(this).attr("id") + '_hidden';//隐藏input的id,用于存储选中的value
        var $inputvalueClass = $(this).attr("data-select") + '_hidden';//隐藏input的class,用于存储选中的value
        var multiple = $this.attr("multiple");//复选标识
        var hidden = "hidden"; if (multiple) { hidden = ""; };//单选隐藏选项前面的框框
        var isReadonly = undefined;
        if ($this.attr("readonly")) {
            isReadonly = true;
        }//是否只读
        // $this.live(options.Event, function (e) {   //功能代码部分，绑定事件
        //     alert(options.msg);
        // });

        //展示库中值
        var oldCheckedArr = [];//显示
        var oldCheckedArrValue = [];//value

        //生成option-item并追加展示
        var html = '';
        html += '<div class="select-picker-search">';
        html += '<div class="select-picker-search-checked">请选择</div>';
        html += '<input type="hidden" id="' + $inputvalueId + '"  class="' + $inputvalueClass + '" >';
        html += '</div>';
        html += '<div class="select-picker-options-wrp">';
        html += '<div class="select-picker-options-serch">';
        html += '<input type="text" placeholder="" class="select-picker-input-search" style="width:90%;">';
        html += '</div>';
        html += '<div class="select-picker-options-list">';//下拉选择项集合
        $this.find("option").each(function () {
            let _this = $(this);
            var checked = _this.attr("data-id");
            html += '<div class="select-picker-options-list-item" id="' + _this.val() + '">';
            if (checked) {
                html += '<b class="duihao duihao-checked" ' + hidden + ' value="' + _this.val() + '"></b>';
                oldCheckedArr.push($.trim(_this.text()));
                oldCheckedArrValue.push($.trim(_this.val()));
            } else {
                html += '<b class="duihao duihao-nocheck" ' + hidden + '  value="' + _this.val() + '"></b>';
            }
            html += '<span>' + _this.text() + '</span>';
            html += '</div>';
        })
        html += '</div>';
        html += '</div>';
        $this.append(html);

        //展示库中值
        if (oldCheckedArr.length > 0) {
            $this.find('.select-picker-search-checked').text(oldCheckedArr.join(','));
            jQuery("#" + $inputvalueId).val(oldCheckedArrValue.join(','));
        }

        if (isReadonly == true) {
            $this.addClass('readonly');
        } else {
            $this.removeClass('readonly');
            // 下拉显示隐藏
            $this.on('click', ".select-picker-search", function (e) {   //功能代码部分，绑定事件

                $(this).next('.select-picker-options-wrp').toggle();
                $(this).next('.select-picker-options-wrp').show().find('.select-picker-options-serch input').focus();
            });

            // 点击选中或不选
            $this.on('click', ".select-picker-options-list-item", function () {

                //var multiple = $this.attr("multiple");//复选标识
                let _this = $(this);

                if (_this.find('.duihao').hasClass("duihao-nocheck")) {
                    _this.find('.duihao').removeClass('duihao-nocheck').addClass('duihao-checked');
                } else {
                    _this.find('.duihao').removeClass('duihao-checked').addClass('duihao-nocheck');
                }
                /*if (_this.find('.duihao-nocheck').length > 0) {//选中
                    if (multiple) {//复选
 
                    } else {//单选--选中则把全部都改为不选
                        $this.find('.duihao').addClass('duihao-nocheck').removeClass('duihao-checked');
                    }
                    //设置选中项
                    _this.find('.duihao').removeClass('duihao-nocheck').addClass('duihao-checked');
 
                } else {//不选
                    _this.find('.duihao').addClass('duihao-nocheck').removeClass('duihao-checked');
                }*/

                //是否关闭下拉框并回显值
                if (multiple) { //复选 --下拉框不关闭
                    // 循环遍历options中选中的项添加到选项栏中
                    var checkedArr = [];
                    var checkedArrValue = [];
                    $this.find(".select-picker-options-list-item").each(function () {
                        let _this = $(this);
                        if (_this.find('.duihao-checked').length > 0) {
                            checkedArr.push($.trim(_this.text()))
                            checkedArrValue.push($.trim(_this.attr("id")))
                        }
                    })
                    if (checkedArr.length > 0) {
                        $this.find('.select-picker-search-checked').text(checkedArr.join(','));
                        jQuery("#" + $inputvalueId).val(checkedArrValue.join(','));
                        // $this.find('.select-picker-search-checked').text(checkedArr.join(',')).css('color', '#fff');
                    }
                    else {
                        $this.find('.select-picker-search-checked').text('请选择').css('color', '#757575');
                        jQuery("#" + $inputvalueId).val("");
                    }
                } else { //单选--下拉框关闭
                    _this.parents('.select-picker-options-wrp').hide();
                    $this.find('.select-picker-search-checked').text(_this.text());
                    jQuery("#" + $inputvalueId).val(_this.attr("id"));
                }
            })

            // 前端实现下拉搜索 
            $this.on('keyup', ".select-picker-options-serch input", function () {

                //只读则不执行改方法
                var text = $(this).val();
                var html = '';
                $this.find("option").each(function () {
                    let _this = $(this);
                    if (_this.text().indexOf(text) != -1) {
                        html += '<div class="select-picker-options-list-item" id="' + _this.val() + '">';
                        html += '<b class="duihao duihao-nocheck" ' + hidden + ' ></b>';
                        html += '<span>' + _this.text() + '</span>';
                        html += '</div>';
                    }
                })
                if (html == '') {
                    html += '<p style="text-align:center;">没有相关内容</p>';
                }
                $this.find(".select-picker-options-list").html('').append(html);
            })
        }

    }
    // 点击document任意地方 下拉消失
    $(document).click(function (event) {
        var _con = $('.select-picker-options-wrp'); // 设置目标区域
        var _con2 = $('.select-picker-search-checked'); // 设置目标区域
        if (!_con2.is(event.target) && !_con.is(event.target) && _con.has(event.target).length === 0) { // Mark 1 
            $('.select-picker-options-wrp').hide(); //淡出消失
        }
    });
    //将下拉框重新设置值
    $.fn.setMySelectData = function (options) {
        console.log(options);
        var _this = $(this);
        var html = "";
        if (options.data != null && options.data.length > 0) {
            options.data.forEach(function (data) {
                if (data.code == options.code) {
                    html += '<option  data-id="duihao-checked" value="' + data.code + '">' + data.name + '</option>';
                } else {
                    html += '<option value="' + data.code + '">' + data.name + '</option>';
                }
            });
        }
        var selectDiv = $(_this.find("select")[0]);
        selectDiv.siblings().remove();
        selectDiv.html(html);
        _this.mySelect();
    }
    $.fn.mySelectClear = function (options) {
        var _this = $(this);
        var selectDiv = $(_this.find("select")[0]);
        selectDiv.siblings().remove();
        selectDiv.html("");
        _this.mySelect();
    }
})(jQuery, window, document);





