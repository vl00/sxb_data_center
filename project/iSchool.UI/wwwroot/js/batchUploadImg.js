(function($, JSON) {
	if (!$.CropAvatarImgWin) throw 'must ref "CropAvatarImgWin.js" first';

	var $option = {
		el: undefined,
		// html attr data-XXX
		data: undefined, //[]		
		maxItemCount: 10, //or undefined
		title: undefined,		
		onLoaded: undefined, //function(data, el) {}
		onItemAdded: undefined, //function (item, uiItem) {}
		onItemDel: undefined, //function (item, uiItem) {} //返回false取消删除操作		
		placeholder: '请输入图片信息，最多可输入14个字符。',
		maxImgDescLength: 14,					
		imgWidth: 12,
		imgHeight: 12,
		url: undefined,	
		canAdd: true,
		canDel: true,
		elAvatarWin: '#avatar-content',
		fd_imgid: 'id',
		fd_imgsrc: 'url',
		fd_imgsrc_s: 'url_s',
		fd_imgdesc: 'desc',
		fmtResAfterUploaded: undefined,
	};
	
	function getId(prex) {
		return (prex || '') + new Date().getTime();
	}
	function getHtml_itemAdd(option) {
		var html = '<div item-add class="container crop-avatar col-3" data-title="' + (option.title || '') + '" data-url="' + (option.url || '') + '" style="display:inline-block;margin:0;">';
		html += '	<div class="card avatar-view">';
		html += '		<div class="card-body nav flex-column justify-content-center bg-light text-center" style="height:12rem;width:100%;">';
		html += '			<span class="text-primary">上传图片</span>';
		html += '		</div>';
		html += '	</div>';
		html += '</div>';
		return html;
	}
    function getHtml_item(id, option) {
		var html = '<div item-id="' + id + '" class="col-3 mb-3" style="display:inline-block;padding:0 15px;">';
		if (option.canDel) html += '	<i class="show-del fa fa-trash-o float-right" style="right:0;"></i>';
		html += '	<img src="" origin-src="" class="rounded" style="height:12rem;width:12rem;" alt="上传图片" />';		
		html += '	<input type="text" class="form-control mt-3 imgtitle c_ignore" value="" style="' + (option.canDel ? '' : 'width:12rem;') + '" ';
		html += '		placeholder="' + (option.placeholder || '') + '" maxlength="' + option.maxImgDescLength + '" />';		
		html += '</div>';
		return html;
    }
        
    function __batchUploadImg(option) {
		option = $.extend({}, $option, option);
		var el = $(option.el);
		var	data = el.data();
		for (var n in data) {
			var x = find_kv(option, n);
			if (x) option[x.k] = data[n];
			else option[n] = data[n];
		}
		var pre = el.children('pre[data-data]');
		var str_json = pre.length ? $.trim(pre.html()) : '';
		if (str_json != '') {
			option.data = JSON.parse(str_json);
			if (option.maxItemCount != -1 && option.data && option.data.length && option.data.length > option.maxItemCount)
				throw '超出最大item数目';
		}
		
		var win = $.CropAvatarImgWin($.extend({}, option, { 
			el: option.elAvatarWin,
			elAvatarWin: null, 
			data: null,
			onItemAdded: null, 
			onItemDel: null,
			onAfterUploaded: function(o) {
				var id = getId('batchUploadImg_');				
				el.find('[item-add]').before(getHtml_item(id, option));
				var uiItem = el.find('[item-id=' + id + ']');
				uiItem.find('img.rounded')[0].src = o.res[option.fd_imgsrc_s];
				uiItem.find('img.rounded').attr('origin-src', o.res[option.fd_imgsrc])
				//uiItem.find('img.rounded').attr('img-id', o.res[option.fd_imgid]);
				uiItem.find('input.imgtitle').val(o.res[option.fd_imgdesc]);
				if (option.maxItemCount != -1 && el.find('[item-id]').length >= option.maxItemCount) el.find('[item-add]').css('display', 'none');
				$.type(option.onItemAdded) === 'function' && option.onItemAdded(o.res, uiItem);
			}
		}));
		
		el.html('');
		el.on('click', '[item-add]', function () {
			if (option.maxItemCount != -1 && el.find('[item-id]').length >= option.maxItemCount) {
				console.log('将会超出最大item数目'); //ShowAlert('将会超出最大item数目');
				return false;
			}			
			win.show();
		});
		el.on('click', '.show-del', function () {
			var uiItem = $(this).parents('div[item-id]').eq(0);
			var item = __getData_item(option, uiItem);
			var b = $.type(option.onItemDel) === 'function' ? option.onItemDel(item, uiItem) : undefined;
			if (b !== false) {
				uiItem.remove();
				if (option.maxItemCount == -1 || el.find('[item-id]').length < option.maxItemCount) el.find('[item-add]').css('display', 'inline-block');
			}
		});
		var i = 0, len = ((option.data && option.data.length) || 0);
		for (; i < len; i++) {
			var htmlStr = getHtml_item(getId('batchUploadImg_'), option);
			var uiItem = $(htmlStr).appendTo(el);
			uiItem.find('img.rounded')[0].src = option.data[i][option.fd_imgsrc_s];		
			uiItem.find('img.rounded').attr('origin-src', option.data[i][option.fd_imgsrc])
			uiItem.find('img.rounded').attr('img-id', option.data[i][option.fd_imgid]);	
			uiItem.find('input.imgtitle').val(option.data[i][option.fd_imgdesc]);
		}
		option.canAdd && $(getHtml_itemAdd(option)).appendTo(el).css('display', (option.maxItemCount == -1 || len < option.maxItemCount) ? 'inline-block' : 'none');		
		
		$.type(option.onLoaded) === 'function' && option.onLoaded(option.data, el);
		
		return {
			getData: function() { return __getData(el, option) },
		};
    }
    function __getData(el, option) {
		return $.map(el.find('div[item-id]'), function (_x) { return __getData_item(option, $(_x), {}) });
    }
    function __getData_item(option, x, obj) {
		obj = obj || {};
		obj[option.fd_imgid] = x.find('img.rounded').attr('img-id') || null;
		obj[option.fd_imgsrc_s] = x.find('img.rounded')[0].src;
		obj[option.fd_imgsrc] = x.find('img.rounded').attr('origin-src');
		obj[option.fd_imgdesc] = x.find('input.imgtitle').val();
		return obj;
    }
    
    $.batchUploadImg = __batchUploadImg;
    $.batchUploadImg.configDefaultOption = function (option) { return $option = $.extend($option, option) };
	$.batchUploadImg.getDefaultOption = function () { return $option };
    $.batchUploadImg.getData = function(el, option) { return __getData($(el), $.extend({}, $option, option)) };    
    
    function find_kv(obj, k) {
		if (k === undefined || k === null) return;
		k = k.toLowerCase();
		for (var n in obj) {
			if (n.toLowerCase() == k)
				return { k: n, v: obj[n] };
		}
    }    
})(jQuery, JSON);