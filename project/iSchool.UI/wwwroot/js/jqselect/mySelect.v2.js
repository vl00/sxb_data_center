(function ($, window, document, undefined) { 
    /**
     *   需要引用 '~/css/jsselect/select.css'
     *   使用 $('xxx').mySelect({...});
     */ 

    // 默认参数
    var defaults = {       
        msg: "请选择",         // 显示内容(水印)
        separator: ',',        // 选中项的text的分隔符
        searchable: true,      // 能否查询     

        // function(txt, cb) {}
        //     cb: function([{ text, value, checked }, ...]) {}
        onSearch: undefined,   // 自定义(异步)查询
    };

    $.fn.mySelect = function (options) {
        var options = $.extend(defaults, options);    //将传入参数和默认参数合并
        //window.console.log(options);
        var $this = $(this).css('position', 'relative');      //当然响应事件对象        
        var multiple = $this.attr('multiple'); //复选标识
        var hidden = multiple ? '' : 'hidden'; //单选隐藏选项前面的框框
        var isReadonly = $this.attr('readonly') ? true : undefined;                            

        // data
        var checkedArr = [];      //text
        var checkedArrValue = []; //value 

        // init|re-init render html
        if ($this.find("select").length < 1) {
            $this.append('<select class="hidden"></select>');
        }
        function reset_selectOptionItems($this, arr) {
            var html = '';
            $.each(arr, function (_, a) {
                html += '<option ' + (a.value === undefined || a.value === null ? '' : ('value="' + a.value + '" '))
                    + (a.checked ? 'checked="checked"' : '') + ' >' + (a.text || '') + '</option>';
            });
            $this.find('select').html('').append(html);
        }
        function render_html(options, $this, checkedArr, checkedArrValue) {
            var html = '';
            html += '<div class="select-picker-search">';
            html += '<div class="select-picker-search-checked">' + (options.msg || '') + '</div>';
            html += '<input type="hidden" class="select-picker-search-hidden-input"/>';
            html += '</div>';
            html += '<div class="select-picker-options-wrp">';
            html += '<div class="select-picker-options-serch">';
            if (options.searchable) {
                html += '<input type="text" placeholder="" class="select-picker-input-search" style="width:90%;"/>';
            }
            html += '</div>';
            html += '<div class="select-picker-options-list">'; //下拉选择项集合
            $this.find("select option").each(function () {
                var _this = $(this);
                var checked = _this.attr("checked");
                html += '<div class="select-picker-options-list-item" id="' + _this.val() + '">';
                if (checked) {
                    html += '<b class="duihao duihao-checked" ' + hidden + ' value="' + _this.val() + '"></b>';
                    checkedArr.push($.trim(_this.text()));
                    checkedArrValue.push($.trim(_this.val()));
                } else {
                    html += '<b class="duihao duihao-nocheck" ' + hidden + ' value="' + _this.val() + '"></b>';
                }
                html += '<span>' + _this.text() + '</span>';
                html += '</div>';
            });
            html += '</div>';
            html += '</div>';
            $this.append(html);
            if (checkedArr.length > 0) {
                $this.find('.select-picker-search-checked').text(checkedArr.join(options.separator));
                $this.find('.select-picker-search-hidden-input').val(checkedArrValue.join(options.separator));
            }
        }
        render_html(options, $this, checkedArr, checkedArrValue);        

        if (isReadonly == true) {
            $this.addClass('readonly');
        } else {
            $this.removeClass('readonly');

            // 下拉显示隐藏
            $this.on('click', ".select-picker-search", function (e) { 
                $(this).next('.select-picker-options-wrp').toggle();
                $(this).next('.select-picker-options-wrp').show().find('.select-picker-options-serch input').focus();
            });

            // 点击选中或不选
            $this.on('click', ".select-picker-options-list-item", function () {
                var _this = $(this);

                if (_this.find('.duihao').hasClass("duihao-nocheck")) {
                    _this.find('.duihao').removeClass('duihao-nocheck').addClass('duihao-checked');
                } else {
                    _this.find('.duihao').removeClass('duihao-checked').addClass('duihao-nocheck');
                }

                checkedArr = [];
                checkedArrValue = [];
                // 复选 --下拉框不关闭
                if (multiple) { 
                    // 循环遍历options中选中的项添加到选项栏中
                    $this.find(".select-picker-options-list-item").each(function () {
                        var _this = $(this);
                        if (_this.find('.duihao-checked').length > 0) {
                            checkedArr.push($.trim(_this.text()));
                            checkedArrValue.push($.trim(_this.attr("id")));
                        }
                    });
                    if (checkedArr.length > 0) {
                        $this.find('.select-picker-search-checked').text(checkedArr.join(options.separator)); //.css('color', '#fff');
                        $this.find('.select-picker-search-hidden-input').val(checkedArrValue.join(options.separator));
                    } else {
                        $this.find('.select-picker-search-checked').text('请选择').css('color', '#757575');
                        $this.find('.select-picker-search-hidden-input').val('');
                    }
                }
                // 单选--下拉框关闭
                else { 
                    checkedArr.push($.trim(_this.text()));
                    checkedArrValue.push($.trim(_this.attr("id")));
                    _this.parents('.select-picker-options-wrp').hide();
                    $this.find('.select-picker-search-checked').text(_this.text());
                    $this.find('.select-picker-search-hidden-input').val(_this.attr('id'));
                }
            });

            // search 
            $this.on('keyup', ".select-picker-options-serch input", function () {
                var text = $(this).val();
                function render_items() {
                    var html = '';
                    $this.find("select option").each(function () {
                        var _this = $(this);
                        if (_this.text().indexOf(text) != -1) {
                            // (复选下)搜索后判断是否需要选中
                            var clss = checkedArrValue && checkedArrValue.indexOf(_this.val()) > -1 ? 'duihao-checked' : 'duihao-nocheck';
                            // html string for render
                            html += '<div class="select-picker-options-list-item" id="' + _this.val() + '">';
                            html += '<b class="duihao ' + clss + '" ' + hidden + '></b>';
                            html += '<span>' + _this.text() + '</span>';
                            html += '</div>';
                        }
                    });
                    if (html === '') {
                        html += '<p style="text-align:center;">没有相关内容</p>';
                    }
                    $this.find(".select-picker-options-list").html('').append(html);
                }
                if (options.onSearch) {
                    options.onSearch(text, function (arr) {
                        reset_selectOptionItems($this, arr);
                        render_items();
                    });
                } else {
                    // 前端实现下拉搜索
                    render_items();
                }
            });
        }

        return {
            ele: $this,
            getSelectedValues: function () {
                return checkedArrValue.slice(0);
            },
            getSelectedItems: function () {
                return $.map(checkedArrValue, function (v, i) {
                    return { text: checkedArr[i], value: v };
                });
            },
            /**  arr like: 
             *       [{ text: '', value: 1, checked: true }, ...]
             */
            reset: function (arr) {
                checkedArr = [];
                checkedArrValue = [];
                reset_selectOptionItems($this, arr);
                $this.find('.select-picker-search').remove();
                $this.find('.select-picker-options-wrp').remove();
                render_html(options, $this, checkedArr, checkedArrValue);
                return this;
            },
            showList: function () {
                $this.find('.select-picker-options-wrp').show();
                return this;
            },
            hideList: function () {
                $this.find('.select-picker-options-wrp').hide();
                return this;
            }
        };
    };

    // 点击document任意地方 下拉消失
    $(document).on('click', function (event) {       
        var _con = $('.select-picker-options-wrp'); // 设置目标区域
        var _con2 = $('.select-picker-search-checked'); // 设置目标区域
        if (!_con2.is(event.target) && !_con.is(event.target) && _con.has(event.target).length === 0) { // Mark 1 
            $('.select-picker-options-wrp').hide(); //淡出消失
        }
    });
})(jQuery, window, document); 