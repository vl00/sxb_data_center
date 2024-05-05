; 
(function (jQuery, undefined) {
	jQuery.fn.yearTagSelect = function (isnew) {
		
		var myDate = new Date();
		var startYear = myDate.getFullYear()
		var years = [];
		var year = parseInt(startYear) - 5;
		
		for (var i = 0; i < 15; i++) {
			years.push(year++);
		}
		var options = {
		
			selectitems: years
		};
	
		var jQuerydocument = jQuery(document);
		var count = jQuery(this).length;
		return this.each(function (i) {
			
			var _self = this;
			var jQuerythis = jQuery(this);
		
			if (jQuerythis.data('inittype') == 'select') { //添加新的时候，防止重复
			
			} else {
				jQuerythis.data('inittype', 'select');
				
				options.selectitems.forEach(function (item) {
					jQuerythis.append("<option value=" + item + ">" + item + "</option>");

				});
				//默认值2020,	
				var t = parseInt(startYear);
				jQuerythis.val(t);
			}
			//编辑时赋值			
			if (isnew) {
				var oldVal = jQuerythis.val();
				if (oldVal) { jQuerythis.val(oldVal) }
			}
			else {
				var oldVal = jQuerythis.data("selval");
				if (oldVal) { jQuerythis.val(oldVal) }
            }
		
			//其他费用设置select
			var setdisabled = jQuerythis.data("setdisabled");
			if (setdisabled) { jQuerythis.attr("disabled","disabled"); }
		});
	};







})(jQuery);
function getfieldname(e) {
	switch (e) {
		case "Age":
			return "招生年龄";
		case "MaxAge":
			return "招生年龄";
		case "Count":
			return "招生人数";
		case "Data":
			return "报名所需资料";
		case "Contact":
			return "报名方式";
		case "Scholarship":
			return "奖学金计划";
		case "Target":
			return "招生对象";
		case "Subjects":
			return "考试科目";
		case "Pastexam":
			return "往期考试内容";
		case "Range":
			return "学校划片范围";
		case "Applicationfee":
			return "申请费用";
		case "Tuition":
			return "学费";
		default:
	}
}
