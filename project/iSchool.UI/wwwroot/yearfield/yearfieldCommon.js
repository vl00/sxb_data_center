//0：删除；1：已编辑；2：初始值(没变更)；3：新增
var act_delete = 0, act_update = 1, act_select = 2, act_add = 3;

//获取某个字段所有年份按钮，obj当前点击的按钮
function getFieldButtons(obj)
{
    return jQuery(obj).parent().parent().parent().find('button');
}

//给Json数组添加元素 field--字段名,key--当前年份btnId,year--当前年份,act--当前年份btn需变更的状态(0:删除；1:已编辑),arrValue--内容数组,json--     
//多输入项，删除输入项的delIndex
function AppentItemToJson(field, key, year, act, arrValue, json, inputitemcount) {
    //因为年龄段由两个年份字段控制，所有需要同时加入
    if (field == "Age" || field == "MaxAge") {

        var Age_MaxAgeobj = {
            key: key,
            value: [
                {
                    year: year,
                    content: arrValue[0],
                    field: "Age",
                    act: act
                },
                {
                    year: year,
                    content: arrValue[1],
                    field: "MaxAge",
                    act: act
                }
            ]
        };
        json.push(Age_MaxAgeobj);
    }   
    else if (field == "Point" || field == "Date" || field == "Otherfee") {        
        var content = [];
        for (var i = 0; i < inputitemcount; i++) {
            var keyname = "Year_" + field + "_Key_" + i;
            var valuename = "Year_" + field + "_Value_" + i;
            var item = {
                Key: jQuery("[name='" + keyname + "']").val(),
                Value: jQuery("[name='" + valuename + "']").val()
            };
            content.push(item);           
        }
        
        var item = {
            key: key,
            value: [
                {
                    year: year,
                    content: content,
                    field: field,
                    act: act
                }
            ]
        };
        json.push(item);
    }
    else if (field == "Counterpart") {
        var content = [];
        for (var i = 0; i < inputitemcount; i++) {
            var selectObj = jQuery('[name=Year_' + field + '_' + i + ']').find("option:selected");
            var item = {
                Key: selectObj.text(),
                Value: selectObj.val()
            };
            content.push(item);        
        }

        var item = {
            key: key,
            value: [
                {
                    year: year,
                    content: content,
                    field: field,
                    act: act
                }
            ]
        };
        json.push(item);
    }
    else {
        var item = {
            key: key,
            value: [
                {
                    year: year,
                    content: arrValue[2],
                    field: field,
                    act: act
                }
            ]
        };
        json.push(item);
    }
}

//输入框不允许为空，校验
function CheckIsNull(formId) {
    
    var listinput = jQuery("#" + formId +" .notnull:visible input[checkednull],.notnull:visible textarea[checkednull]");
    var isnull = false;
    
    jQuery.each(listinput, function (index, item) {
        
        if (isnull) return;
        if (item.value == "" || item.value == undefined || item.value == null) {
            ShowAlert("输入框不允许为空，请重新输入", -1);
            jQuery(item).focus();
            isnull = true;
            return;
        }
    });
    return isnull;
}

//校验输入范围
function checkIntRange(n, min, max) {
    if (n < min || n > max) {
        ShowAlert("请输入 " + min + " 至 " + max + " 之间的正整数",-1);
        return;
    }
}

//检查是否是一个正整数
function IsIntNumber(num) {
    
    var value = num;
    var isOK = null;
    if (value) {
        var r = /^\d*\.?\d+$/;
        if (r.test(value)) {
            isOK = true
        }
        else isOK = false;
    }
    return isOK;
}

//格式化字符串，并更新隐藏Json文本框的value
function FormatJsonString(field, json) {
    jQuery("#jsonhid_Year_" + field + "").val(JSON.stringify(json));
}

//【首次维护调用】多输入项的单个项状态维护--新增
function MoreInputState_First(inputitemcount, currentIndex, currentBtnId,act) {
    
    var arrState = [];
    var count = parseInt(inputitemcount);
    for (var i = 0; i < count; i++) {
        if (currentIndex == i.toString()) arrState.push(act);
        else arrState.push(act_select);
    }   
    jQuery("#" + currentBtnId + "").attr('more-input-state', JSON.stringify(arrState))
}

//同年份多输入项的change事件
function MoreInputChange(currentController) {
   
    //先获取隐藏json
    var currentInput = jQuery(currentController);
    var currentValue = currentInput.val();

    //新增输入项，编辑也算新增状态
    var inputstate = act_update;
    if (currentInput.attr('inputstate')) inputstate = act_add;

    //全部年份字段都不允许保存空值
    if (currentValue == null || currentValue == "" || currentValue == undefined) {
        return;
    }
    if (currentInput.attr("type") == "number") {

        if (!ISPositiveInteger(currentValue)) {
            ShowAlert("数字框的值为正数", -1);
            currentInput.focus();
            return;
        }
    }

    var isNew = true;
    //分隔输入框的name属性
    var arrStr = currentInput.attr("name").split('_');//示例：多输入框name结构Year_Point_Key_Index  arrStr[3]为输入项的序号
    var dbField = arrStr[1];
    var field = dbField;
    var year = jQuery("select[name='" + field + "']").val();
    var key = field + "_" + year;

    //增删改查状态维护
    ////更新按钮状态 已编辑1 act_update
    jQuery("#" + key + "").attr('data-input', act_update)
    //多输入项，单个输入项的状态维护more-input-state,值是数组，按照输入项顺序存储状态[];分新增和编辑两种情况


    var hidJson = jQuery("#jsonhid_Year_" + field + "").val();
    var json = JSON.parse(hidJson);
    jQuery.each(json, function (index, data) {
        if (data.key == key) {
            isNew = false;
            //更新json值
            jQuery.each(data.value, function (i, value) {
                if (value.field == dbField) {
                    if (arrStr[2] == "Key") {                       
                        try {
                            this.content[arrStr[3]].Key = currentValue;
                        }
                        catch{
                            var v = jQuery("[name='" + arrStr[0] + "_" + arrStr[1] + "_Value_" + arrStr[3] + "']").val();
                            v = v == undefined ? "" : v;
                            var item = {
                                Key: currentValue,
                                Value: v
                            };
                            this.content.push(item);
                        }      
                    }
                    else {                        
                        try { this.content[arrStr[3]].Value = currentValue; }
                        catch {
                            var v = jQuery("[name='" + arrStr[0] + "_" + arrStr[1] + "Key" + arrStr[3] + "']").val();
                            v = v == undefined ? "" : v;
                            var item = {
                                Key: v ,
                                Value: currentValue
                            };
                            this.content.push(item);
                        }                        
                    }
                    this.act = act_update;
                    //维护输入项状态                   
                    var arrState = JSON.parse(jQuery("#" + key + "").attr('more-input-state'));
                    var arrIndex = parseInt(arrStr[3]);
                    if (arrState[arrIndex] != act_add) {
                        arrState[arrIndex] =inputstate;
                    } 
                    
                    jQuery("#" + key + "").attr('more-input-state', JSON.stringify(arrState));
                }
            });
        }
    });

    //给json追加一条记录
    if (isNew) {
        //当前年份按钮增加维护输入项的状态  inputitemcount 输入项的项数                
        var contentcount = parseInt(jQuery("#" + key + "").attr('inputitemcount'));
        MoreInputState_First(contentcount, arrStr[3], key, inputstate);

        var arrValue = [jQuery("input[name='Year_Age']").val(), jQuery("input[name='Year_MaxAge']").val(), currentValue];
        AppentItemToJson(dbField, key, year, act_update, arrValue, json, contentcount);
    }

    //更新隐藏json控件的value
    FormatJsonString(field, json);     
}
//数据库只要一个字段，界面展示多个控件
function OneFieldShowMore(currentBtnId, field, dataValue, isDB) {

    if (field == "Point" || field == "Date" || field == "Otherfee" || field == "Counterpart") {
        jQuery("#" + field + "_Content").html('');
        jQuery(".updateaddinputitem").remove();
        var content = null;
        if (isDB) { try { content = JSON.parse(dataValue); } catch{ content = dataValue; } }
        else content = dataValue;

        var year = currentBtnId.split('_')[1];
        var inputItemCount = 0;
        var html = '';
        jQuery.each(content, function (index, item) {
            ++inputItemCount;           
            html += GetInputItemHtmlByField(currentBtnId, index, field, item);                
        });     
        //动态渲染输入项
        jQuery("#" + field + "_Content").append(html);        
        if (field == "Date") {
            jQuery("#Date_Content .datetimepicker4").datetimepicker({
                format: 'YYYY-MM-DD',
                locale: moment.locale('zh-cn'),
                useCurrent: false
            }).on("dp.change", function (e) {
            
                MoreInputChange(e.currentTarget);
            }); 

            jQuery("#Date_Content .datetimepicker4").each(function (i, op) {                
                jQuery(op).data("DateTimePicker").minDate('' + year + '-01-01').maxDate('' + year + '-12-31');
            });
        }
        if (field == "Counterpart") {
            InitSelect2();     
            SelectOnChange();
        }
        
        //编辑模式的添加按钮，每个年份按钮对应一个
        var addhtml = '<button name="addinputBtn_' + currentBtnId + '"  id="' + currentBtnId+'_item"  type = "button" inputitemcount=' + inputItemCount + ' currentBtnId="' + currentBtnId + '" field="' + field + '" isfirst="true"  onclick = "Update_AddInputItem(this)" style = "margin-left:5px;margin-top:15px;width:25%;border:1px solid #ced4da;border-radius:0.25rem;" class="btn btn-outline-secondary btn-lg btn-block updateaddinputitem" > <i class="fa fa-plus"></i></button >';
        jQuery("#" + field + "_Content").parents("#show_input_item_" + field+"").after(addhtml);
        
        //当前年份按钮记录输入项的项数
        jQuery("#" + currentBtnId).attr('inputitemcount', inputItemCount);
    }
}

//根据Field获取动态加载的html
function GetInputItemHtmlByField(currentBtnId, index, field, item) {
    var html = '';
    switch (field) {
        case "Point":
            {
                html = '<div class="form-group" style="margin:5px; margin-bottom:50px;" id="show_input_item_' + field + '_' + index + '"><div class="col-md-5"  style="margin-top:7px;"><input onchange="MoreInputChange(this)" class="form-control c_ignore" placeholder="请输入分数线类型" name="Year_' + field + '_Key_' + index + '" data-update="Y" required data-placement="bottom" value="' + item.Key + '" /></div><div class="col-md-4"  style="margin-top:7px;">  <input onchange="MoreInputChange(this)"  type="number" placeholder="请输入分数"  name="Year_' + field + '_Value_' + index + '" data-update="Y" required data-placement="bottom" min="0" class="form-control c_ignore" value="' + item.Value + '"></div><div class="col-md-3"  style="margin-top:7px;"><a href="javascript:void(0)" name="a_' + currentBtnId + '"  currentbtnid="' + currentBtnId + '" index="' + index + '"   onclick="DelInputItem(this)"><i class="fa fa-times-circle text-danger"></i></a></div></div>';
            }
            break;
        case "Date":
            {
                html = '<div class="form-group" style="margin:5px; margin-bottom:50px;" id="show_input_item_' + field + '_' + index + '"><div class="col-md-5"  style="margin-top:7px;"><input onchange="MoreInputChange(this)" class="form-control c_ignore" placeholder="请输入名称" name="Year_' + field + '_Key_' + index + '" data-update="Y" required data-placement="bottom" value="' + item.Key + '" /></div><div class="col-md-4"  style="margin-top:7px;">  <input onchange="MoreInputChange(this)"  type="text" placeholder="请输入时间"  name="Year_' + field + '_Value_' + index + '" data-update="Y" required data-placement="bottom"  class="form-control c_ignore datetimepicker4" value="' + item.Value + '"  ></div><div class="col-md-3"  style="margin-top:7px;"><a href="javascript:void(0)" name="a_' + currentBtnId + '"  currentbtnid="' + currentBtnId + '" index="' + index + '"   onclick="DelInputItem(this)"><i class="fa fa-times-circle text-danger"></i></a></div></div>';
            }
            break;
        case "Otherfee":
            {
                html = '<div class="form-group" style="margin:5px; margin-bottom:50px;margin-left:-15px; " id="show_input_item_' + field + '_' + index + '"><div class="col-md-5"  style="margin-top:7px;"><input onchange="MoreInputChange(this)" class="form-control c_ignore" placeholder="请输入其他费用项" name="Year_' + field + '_Key_' + index + '" data-update="Y" required data-placement="bottom" value="' + item.Key + '" /></div><div class="col-md-4"  style="margin-top:7px;">  <input onchange="MoreInputChange(this)"  type="number" placeholder="请输入金额"  name="Year_' + field + '_Value_' + index + '" data-update="Y" required data-placement="bottom" min="0" class="form-control c_ignore" value="' + item.Value + '"></div><div class="col-md-1" style="margin-top:13px;margin-left:-20px;"><label class="pr-1  form-control-label">元</label></div><div class="col-md-2"  style="margin-top:13px;"><a href="javascript:void(0)" name="a_' + currentBtnId + '"  currentbtnid="' + currentBtnId + '" index="' + index + '"   onclick="DelInputItem(this)"><i class="fa fa-times-circle text-danger"></i></a></div></div>';
            }
            break;
        case "Counterpart":
            {
                var select = '<select  data-placeholder="选择一个学校" name="Year_' + field + '_' + index + '" class="counterschool selonchange" tabindex="1"><option value = "' + item.Value + '" selected>' + item.Key + '</option ></select >';
                html = '<div class="form-group" style="margin:5px; margin-bottom:50px;margin-left:-15px; " id="show_input_item_' + field + '_' + index + '"><div class="col-md-6"  style="margin-top:7px;">' + select + '</div><div class="col-md-2" style="margin-top:7px;"></div><div class="col-md-4"  style="margin-top:7px;"><a href="javascript:void(0)" name="a_' + currentBtnId + '"  currentbtnid="' + currentBtnId + '" index="' + index + '"   onclick="DelInputItem(this)"><i class="fa fa-times-circle text-danger"></i></a></div></div>';

            }
            break;
        default:
            break;
    }   

    var str_inputItemState = jQuery("#" + currentBtnId).attr('more-input-state');

    if (str_inputItemState) {
        var arr_inputItemState = JSON.parse(str_inputItemState);
        //已删除的输入项，则不显示
        if (arr_inputItemState[index] == act_delete.toString()) {
            html = '';  
        }
        //已编辑 
        if (arr_inputItemState[index] == act_update.toString()) html = html.replace("</i></a></div></div>", "</i></a><span>已编辑</span></div></div>");
        
    }
    return html;
}

//编辑状态，通过“+”添加输入项
function Update_AddInputItem(obj)
{   
    var currentTag = jQuery(obj);
    var currentBtnId = currentTag.attr('currentBtnId'), field = currentTag.attr('field');
    var index = currentTag.attr('inputitemcount');//编辑总项数，也是新增的开始下标;
    var inputitemtatolcount = currentTag.attr('inputitemcount');//编辑总项数，新增项也累计到一起
    var year = currentBtnId.split('_')[1];
    var html = '';
    switch (field) {
        case "Point":
            {
                html = '<div class="form-group" style="margin:5px;margin-bottom:50px;margin-top:50px;" id="show_input_item_' + field + '_' + index + '"><div class="col-md-5" style="margin-top:7px;"><input onchange="MoreInputChange(this)" inputstate="' + act_add + '" class="form-control c_ignore" placeholder="请输入分数线类型" name="Year_' + field + '_Key_' + index + '" data-update="Y" required data-placement="bottom" value="" /></div><div class="col-md-4"  style="margin-top:7px;">  <input onchange="MoreInputChange(this)"  inputstate="' + act_add + '"  type="number" placeholder="请输入分数"  name="Year_' + field + '_Value_' + index + '" data-update="Y" required data-placement="bottom"  class="form-control c_ignore" value=""></div><div class="col-md-3"  style="margin-top:7px;"><a href="javascript:void(0)" name="a_' + currentBtnId + '"  currentbtnid="' + currentBtnId + '" index="' + index + '"   onclick="DelInputItem(this)"><i class="fa fa-times-circle text-danger"></i></a></div></div>';
                jQuery("#" + field + "_Content").append(html);
            }
            break;
        case "Date":
            {
                html = '<div class="form-group" style="margin:5px; margin-bottom:50px;margin-top:50px;" id="show_input_item_' + field + '_' + index + '"><div class="col-md-5" style="margin-top:7px;"><input onchange="MoreInputChange(this)"  inputstate="' + act_add + '" class="form-control c_ignore" placeholder="请输入名称" name="Year_' + field + '_Key_' + index + '" data-update="Y" required data-placement="bottom" value="" /></div><div class="col-md-4"  style="margin-top:7px;">  <input onchange="MoreInputChange(this)"   inputstate="' + act_add + '" type="text" placeholder="请输入时间"  name="Year_' + field + '_Value_' + index + '" data-update="Y" required data-placement="bottom"  class="form-control c_ignore datetimepicker4" value=""  ></div><div class="col-md-3"  style="margin-top:7px;"><a href="javascript:void(0)" name="a_' + currentBtnId + '"  currentbtnid="' + currentBtnId + '" index="' + index + '"   onclick="DelInputItem(this)"><i class="fa fa-times-circle text-danger"></i></a></div></div>';
                jQuery("#" + field + "_Content").append(html);
                jQuery("#Date_Content .datetimepicker4").last().val('' + year + '-01-01')
                jQuery("#Date_Content .datetimepicker4").last().datetimepicker({
                    format: 'YYYY-MM-DD',
                    locale: moment.locale('zh-cn')
                }).on("dp.change", function (e) {   
                    MoreInputChange(e.currentTarget);
                }).data("DateTimePicker").minDate('' + year + '-01-01').maxDate('' + year + '-12-31');       
 
            }
            break;
        case "Otherfee":
            {
                html = '<div class="form-group" style="margin:5px;margin-bottom:50px;margin-top:50px;margin-left:-15px; " id="show_input_item_' + field + '_' + index + '"><div class="col-md-5" style="margin-top:7px;"><input onchange="MoreInputChange(this)"  inputstate="' + act_add + '" class="form-control c_ignore" placeholder="输入费用项" name="Year_' + field + '_Key_' + index + '" data-update="Y" required data-placement="bottom" value="" /></div><div class="col-md-4"  style="margin-top:7px;">  <input onchange="MoreInputChange(this)"   inputstate="' + act_add + '" type="number" placeholder="请输入金额"  name="Year_' + field + '_Value_' + index + '" data-update="Y" required data-placement="bottom"  class="form-control c_ignore" value=""></div><div class="col-md-1" style="margin-top:13px;margin-left:-20px;"><label class="pr-1  form-control-label">元</label></div><div class="col-md-2"  style="margin-top:13px;"><a href="javascript:void(0)" name="a_' + currentBtnId + '"  currentbtnid="' + currentBtnId + '" index="' + index + '"   onclick="DelInputItem(this)"><i class="fa fa-times-circle text-danger"></i></a></div></div>';
                jQuery("#" + field + "_Content").append(html);
            }
            break;
        case "Counterpart":
            {
                html = '<div class="form-group" style="margin:5px; margin-top:15px;margin-left:0px; " id="show_input_item_' + field + '_' + index + '"><div><select style="margin-top:7px;"  data-placeholder="选择一个学校" name="Year_' + field + '_' + index + '" class="counterschool selonchange" tabindex="1"><option value = "" selected></option ></select >&nbsp;&nbsp;&nbsp;<a style="margin-top:7px;" href="javascript:void(0)" name="a_' + currentBtnId + '"  currentbtnid="' + currentBtnId + '" index="' + index + '"   onclick="DelInputItem(this)"><i class="fa fa-times-circle text-danger"></i></a></div></div>';
                jQuery("#" + field + "_Content").append(html);    
                InitSelect2();
                SelectOnChange();
            }
            break;
        default:
    }

    currentTag.attr('isfirst', false);
    //维护年份按钮的相关属性
    var currentBtn = jQuery("#" + currentBtnId);   
    ++inputitemtatolcount;
    currentBtn.attr('inputitemcount', inputitemtatolcount);
    currentTag.attr('inputitemcount', inputitemtatolcount);
    //状态属性为空，则为首次维护状态
    var state = currentBtn.attr('more-input-state');
    if (state) {
        var newState = JSON.parse(state);
        newState.push(act_add);
        currentBtn.attr('more-input-state', JSON.stringify(newState));        
    }
    else {
        MoreInputState_First(inputitemtatolcount, index, currentBtnId, act_add);
    }
}

//dbdata to page-cache
//pagecacheinputId--页面存储DBdata隐藏input的id
//arrValue：arrValue[0]--Age-content、arrValue[1]--MaxAge-content、arrValue[2]--other single control-content
function DbDataToPageCache(pagecacheinputId, field, year, arrValue) {
    var pagecache = jQuery("#" + pagecacheinputId).val();
    var pagecachejson = JSON.parse(pagecache);
    var btnId = field + "_" + year;
    var isexit = false;
    jQuery.each(pagecachejson, function (index, item) {
        if (item.key == btnId) { isexit = true; return; }
    });
    if (isexit == false) {
        AppentItemToJson(field, btnId, year, act_select, arrValue, pagecachejson, jQuery("#" + btnId).attr('inputitemcount'))
        jQuery("#" + pagecacheinputId).val(JSON.stringify(pagecachejson));
    }
}

//delete  page-cache by btnId
function DelPageCacheByBtnId(btnId, pagecacheinputId) {
    var json = JSON.parse(jQuery("#" + pagecacheinputId).val());
    jQuery.each(json, function (index, item) {       
        if (json[index].key && (json[index].key == btnId))
            json.splice(index, 1);
    });
    jQuery("#" + pagecacheinputId).val(JSON.stringify(json));
}

//select   page-cache by btnId
function QueryPageCacheByBtnId(btnId, pagecacheinputId) {
    var json = JSON.parse(jQuery("#" + pagecacheinputId).val());
    var data = null;
    jQuery.each(json, function (index,item) {
        if (item.key == btnId)
            data = item.value;
    });
    return data
}

//删除输入项
function DelInputItem(obj) {
    //删除时，在当前年份按钮中添加维护的属性 添加：delcount--删除数属性；维护：more-input-state--输入项状态数组、inputitemcount--输入项总项数
    //删除时，a标签添加的属性 index--当前输入项的序号、currentbtnid--当前年份按钮Id
    var r = confirm("你确定要删除该项吗?")
    if (r) {
        var currentALable = jQuery(obj);
        var currentBtnId = currentALable.attr("currentbtnid");
        var currentBtn = jQuery("#" + currentBtnId)
        var tatolcount = parseInt(currentBtn.attr('inputitemcount'));
        var arr_fieldandyear = currentBtnId.split('_');
        var field = arr_fieldandyear[0];
        var currentIndex = parseInt(currentALable.attr('index'));//删除的下标

        //1.1、删除该输入项
        currentALable.parent().parent().remove();

        //重洗删除项后面的相关下标和总项数-1
        var needUpdateChildrenNodes = jQuery("#" + field + "_Content").children(".form-group");
        jQuery.each(needUpdateChildrenNodes, function (index) {
            if (index >= currentIndex) {
                var nextOldIndex = index + 1;
                jQuery("#show_input_item_" + field + "_" + nextOldIndex).attr('id', 'show_input_item_' + field + '_' + index);
                jQuery("[name=Year_" + field + "_Key_" + nextOldIndex + "]").attr('name', 'Year_' + field + '_Key_' + index);
                jQuery("[name=Year_" + field + "_Value_" + nextOldIndex + "]").attr('name', 'Year_' + field + '_Value_' + index);
                jQuery("a[name=a_" + currentBtnId + "]").attr('index', index);//a标签--index                        
            }
        });
        //+按钮的--inputitemcount
        jQuery("#" + currentBtnId + "_item").attr('inputitemcount', tatolcount - 1);

        //年份按钮--总项数-1；状态集合移除删除项
        currentBtn.attr('inputitemcount', tatolcount - 1);
        var states = currentBtn.attr('more-input-state');
        if (states) {
            var arr_states = JSON.parse(states);
            //arr_states[currentIndex] = act_delete;
            arr_states.splice(currentIndex, 1);
            currentBtn.attr('more-input-state', JSON.stringify(arr_states));
        }
        else {
            var arrState = [];
            for (var i = 0; i < tatolcount - 1; i++) {
                arrState.push(act_select);
            }
            jQuery("#" + currentBtnId + "").attr('more-input-state', JSON.stringify(arrState))
        }
        
        var act = act_update;
        //总项数=1，即为删除最后一项              
        if (tatolcount == 1) {
            //年份按钮变更：样式变为灰色 不可编辑 状态为0
            currentBtn.attr("disabled", "disabled").attr("class", "btnyearclick btn btn-grey").attr("data-input", act_delete);
            act = act_delete;            
        }
        //然后更新隐藏json的act为0
        var arr_fieldandyear = currentBtnId.split('_');
        var field = arr_fieldandyear[0];
        var hidJson = jQuery("#jsonhid_Year_" + field).val();
        var json = JSON.parse(hidJson);
        var isNew = true;
        jQuery.each(json, function (index, data) {
            if (data.key == currentBtnId) {
                isNew = false;
                jQuery.each(data.value, function (i, item) {
                    item.act = act;
                    item.content.splice(currentIndex, 1);
                });
            }
        });
        if (isNew) {
            AppentItemToJson(field, currentBtnId, arr_fieldandyear[1], act, null, json, tatolcount-1);
        }
        jQuery("#jsonhid_Year_" + field).val(JSON.stringify(json));
       
        DelPageCacheByBtnId(currentBtnId, currentBtn.attr('page-cache-input-id'));        
    }
}

(function (window, $, YearsField) {
    if (!YearsField) YearsField = window.YearsField = {};
    else return;

    /** 
     *  年份字段变更操作act
     */
    YearsField.ChangeAct = {
        none: 2, //没变化
        add: 3,  //新添加
        update: 1, //修改
        remove: 0, //删除
    };

    /**
     *  获取年份字段变更list
     */
    YearsField.getYearslistChanges = function (yearslist, listfieldyeartags, jqUIhidjson) {
        yearslist = yearslist || [];
        if (listfieldyeartags && listfieldyeartags.length > 0) {
            $.each(listfieldyeartags, function (_, yd) {
                var ct = $.type(yd.content);
                if (ct == 'array' || ct == 'object') ct = JSON.stringify(yd.content);
                else ct = yd.content;
                yearslist.push({ year: yd.year, field: yd.field, content: ct, act: YearsField.ChangeAct.add });
            });
        }
        if (jqUIhidjson && jqUIhidjson.length > 0) {
            jqUIhidjson.each(function () {
                var hval = $(this).val();
                if (!hval) return;
                $.each(JSON.parse(hval), function (_, kv) {
                    kv.value && $.each(kv.value, function (_, yd) {
                        var ct = $.type(yd.content);
                        if (ct == 'array' || ct == 'object') ct = JSON.stringify(yd.content);
                        else ct = yd.content;
                        yearslist.push({ year: yd.year, field: yd.field, content: ct, act: yd.act });
                    });
                });
            });
        }
        return yearslist;
    }
})(window, window.jQuery);
