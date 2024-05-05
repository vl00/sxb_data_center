(function($, JSON, CropAvatar) {
	var $option = {
		el: '#avatar-content',
		title: undefined,
		placeholder: '请输入图片信息，最多可输入14个字符。',		
		maxImgDescLength: 14,				
		imgWidth: 12,
		imgHeight: 12,
		url: undefined,				
		fd_imgid: 'id',
		fd_imgsrc: 'url',
		fd_imgsrc_s: 'url_s',
		fd_imgdesc: 'desc',
		fmtResAfterUploaded: function (res) { //this === option
			var o = {};
			o[this.fd_imgsrc] = res['result'];
			o[this.fd_imgsrc_s] = res['message'];
			return o;
		},
		onAfterUploaded: undefined, //function ({ args, res: { url, url_s, desc }, form }) {}
	};
	 
    function __CropAvatarImgWin(option) {
		var o = {}, option = $.extend({}, $option, option);		 
		o.show = function(args) {
			var el = $(option.el);
			if (!el.length) {
				console.log("找不到CropAvatarImgWin '" + option.el + "'");
				return false;
			}
			el.modal('show');				
			el.find('[name=desc]')
				.attr('placeholder', option.placeholder || '')
				.attr('maxlength', option.maxImgDescLength);	
			el.find('button[type=submit]').on('click', function(e) {
				var desc = el.find('[name=desc]').val();
				if (!$.trim(desc)) return ShowAlert('请输入图片信息', -1), false;
			});
			new CropAvatar(el, args, option.title, option.imgWidth, option.imgHeight, option.url, function (res, arr, args) {				
				console.log(res), console.log(arr);
				debugger;
				var res1 = $.type(option.fmtResAfterUploaded) === 'function' 
					? option.fmtResAfterUploaded(res) : $option.fmtResAfterUploaded(res);
				var imgDesc = findInFormArr(arr, option.fd_imgdesc);
				if ($.type(option.onAfterUploaded) === 'function') {
					res1[option.fd_imgdesc] = imgDesc;
					option.onAfterUploaded({ args: args, res: res1, form: arr });
				}
			});		
			return true;
		};						
		return o;
    }
    
    $.CropAvatarImgWin = __CropAvatarImgWin;
    $.CropAvatarImgWin.configDefaultOption = function (option) { return $option = $.extend($option, option) };
    $.CropAvatarImgWin.getDefaultOption = function (option) { return $option };
    
    function findInFormArr(arr, name) {
		for (var i = 0, len = arr.length; i < len; i++) {			
			var n = arr[i].name ? arr[i].name.toLowerCase() : '';
			if (n === name.toLowerCase()) return arr[i].value;
		}
    }
})(jQuery, JSON, CropAvatar);