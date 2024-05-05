(function($, JSON) {
	var $option = {
		el: undefined,
		// html attr data-XXX
		data: undefined, //[]		
		maxItemCount: 10, //or undefined
		title: undefined,
		imgWidth: 12,
		imgHeight: 12,

		onLoaded: undefined, //function(data, el) {}
		onItemAdded: undefined, //function (item, uiItem) {}
		onItemDel: undefined, //function (item, uiItem) {} //返回false取消删除操作						
		
		canAdd: true,
		canDel: true,
		canDownload: true,

		fd_imgid: 'id',
		fd_imgsrc: 'url',
		fd_imgsrc_s: 'url_s',

		accept: 'jpg,png',

		// function (files, cb) {}
		//     cb: function({ url, url_s }) {}
		onImgUpload: undefined,  
	};
	
	function getId(prex) {
		return (prex || '') + new Date().getTime();
	}
	function getHtml_itemAdd(option) {
		var html = '<div item-add class="container col-md-3 mb-3" data-title="' + (option.title || '') + '" style="display:inline-block;margin:0;padding:0 3em;">';
		html += '	<div class="card avatar-view">';
		html += '		<div class="card-body nav flex-column justify-content-center bg-light text-center" style="height:12rem;width:100%;">';
		html += '			<span class="text-primary">上传图片</span>';
		html += '		</div>';
		html += '	</div>';
		html += '</div>';
		html += '<input type="file" hidden="hidden" class="_upload_file" name="files" multiple="" accept="' + (option.accept || '') + '" />';
		return html;
	}
    function getHtml_item(id, option) {
		var html = '<div item-id="' + id + '" class="col-md-3 mb-3" style="display:inline-block;padding:0 15px;">';
		if (option.canDel) html += '<i class="show-del fa fa-trash-o float-right" style="right:0;"></i>';
		html += '<img src="" origin-src="" class="rounded" style="height:12rem;width:12rem;" alt="上传图片" />';
		if (option.canDownload) html += '<div style="text-align:center;margin:0 auto;"><a down="1" class="text-info" href="javascript:;">下载</a></div>';
		html += '</div>';
		return html;
    }
        
    function __batchUploadImg2(option) {
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
		
		el.html('');
		// 添加
		el.on('click', '[item-add]', function () {
			if (option.maxItemCount != -1 && el.find('[item-id]').length >= option.maxItemCount) {
				console.log('将会超出最大item数目'); //ShowAlert('将会超出最大item数目');
				return false;
			}
			return el.find('._upload_file').click();
		});
		// 上传
		el.on('change', '._upload_file', function () {
			var $this = this;
			if ($this.files.length == 0) {
				return;
			}
			option.onImgUpload($this.files, function (data) {
				$this.value = '';
				var id = getId('batchUploadImg_');
				el.find('[item-add]').before(getHtml_item(id, option));
				var uiItem = el.find('[item-id=' + id + ']');
				uiItem.find('img.rounded')[0].src = data[option.fd_imgsrc_s] || data[option.fd_imgsrc];
				uiItem.find('img.rounded').attr('origin-src', data[option.fd_imgsrc]);
				uiItem.find('img.rounded').attr('img-id', data[option.fd_imgid]);
				if (option.maxItemCount != -1 && el.find('[item-id]').length >= option.maxItemCount) el.find('[item-add]').css('display', 'none');
				$.type(option.onItemAdded) === 'function' && option.onItemAdded(data, uiItem);
			});
		});
		// 删除
		el.on('click', '.show-del', function () {
			var uiItem = $(this).parents('div[item-id]').eq(0);
			var item = __getData_item(option, uiItem);
			var b = $.type(option.onItemDel) === 'function' ? option.onItemDel(item, uiItem) : undefined;
			if (b !== false) {
				uiItem.remove();
				if (option.maxItemCount == -1 || el.find('[item-id]').length < option.maxItemCount) el.find('[item-add]').css('display', 'inline-block');
			}
		});
		// 下载
		el.on('click', 'a[down]', function () {
			var url = $(this).parents('div[item-id]').eq(0).find('img').attr('origin-src');
			forceDownload(url);
		});

		var i = 0, len = ((option.data && option.data.length) || 0);
		for (; i < len; i++) {
			var htmlStr = getHtml_item(getId('batchUploadImg2_'), option);
			var uiItem = $(htmlStr).appendTo(el);
			uiItem.find('img.rounded')[0].src = option.data[i][option.fd_imgsrc_s];		
			uiItem.find('img.rounded').attr('origin-src', option.data[i][option.fd_imgsrc]);
			uiItem.find('img.rounded').attr('img-id', option.data[i][option.fd_imgid]);	
		}
		option.canAdd && $(getHtml_itemAdd(option)).appendTo(el).css('display', (option.maxItemCount == -1 || len < option.maxItemCount) ? 'inline-block' : 'none');		
		
		$.type(option.onLoaded) === 'function' && option.onLoaded(option.data, el);
		
		return {
			getData: function () {
				return __getData(el, option);
			},
			setMaxItemCount: function (count) {
				option.maxItemCount = count;
				if (option.maxItemCount == -1 || el.find('[item-id]').length < option.maxItemCount) el.find('[item-add]').css('display', 'inline-block');
				else el.find('[item-add]').css('display', 'none');
			},
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
		return obj;
    }
    
    $.batchUploadImg2 = __batchUploadImg2;
    $.batchUploadImg2.configDefaultOption = function (option) { return $option = $.extend($option, option) };
	$.batchUploadImg2.getDefaultOption = function () { return $option };
    $.batchUploadImg2.getData = function(el, option) { return __getData($(el), $.extend({}, $option, option)) };    
    
    function find_kv(obj, k) {
		if (k === undefined || k === null) return;
		k = k.toLowerCase();
		for (var n in obj) {
			if (n.toLowerCase() == k)
				return { k: n, v: obj[n] };
		}
	}

	function forceDownload(url) {
		var fileName = url.split(/(\\|\/)/g).pop();
		var xhr = new XMLHttpRequest();
		xhr.open("GET", url, true);
		xhr.responseType = "blob";
		xhr.onload = function () {
			var urlCreator = window.URL || window.webkitURL;
			var imageUrl = urlCreator.createObjectURL(this.response);
			var tag = document.createElement('a');
			tag.href = imageUrl;
			tag.download = fileName;
			document.body.appendChild(tag);
			tag.click();
			document.body.removeChild(tag);
		}
		xhr.send();
	}
})(jQuery, JSON);