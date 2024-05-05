(function($, JSON) {
	if (!$.CropAvatarImgWin) throw 'must ref "CropAvatarImgWin.js" first';

	var $option = {
		el: undefined,
		// html attr data-XXX
		data: undefined, //[]	
		
		videoWidth: 250,
		videoHeight: 150,
		videoFileMaxSize: 41943040, // -1 = 不限制大小
		maxItemCount: 10, //or undefined
		title: undefined,		
		onLoaded: undefined, //function(data, el) {}
		onItemAdded: undefined, //function(item, uiItem) {}
		onItemDel: undefined, //function(item, uiItem) {} //返回false取消删除操作	
		
		//function(item, uiItem, field, value) {} //for one
		//function(item, uiItem) {} //for more
		onItemChanged: undefined, 

		canAdd: true,
		canDel: true,
		videoPlaceholder: '只允许上传Mp4格式的视频!视频大小不能超过40M',
		uploadVideoUrl: undefined,		
		fmtResAfterVideoUploaded: function(items) {
			return $.map(items, function(res) {
				return {
					id: res.key,
					videoUrl: res.value,
					cover: res.message,
					videoDesc: res.desc,
				};
			});
		},
		
		cover: undefined, // for win '#avatar-content'
	};
	
	function getId(prex) {
		return (prex || '') + new Date().getTime();
	}
	function getHtml_body(option) {
		var html = '<div vdo class="row"></div>';
		if (option.canAdd) {
			html += '<div item-add class="row">';
			html += '	<div class="col-md-4">';
			html += '		<input type="file" name="principalvideo" multiple accept="video/mp4,audio/mp4" title="' + (option.videoPlaceholder || '') + '" />';		
			html += '	</div>';
			html += '	<div class="col-md-2">';
			html += '		<input type="button" class="uploadvideo-btn btn btn-info btn-block" data-video="principalvideo" data-input="principalvideo" value="上传" />';		
			html += '	</div>';
			html += '	<div class="col-md-6">';
			html += '		<a href="javascript:;" data-toggle="tooltip" data-placement="bottom" title="' + (option.videoPlaceholder || '') + '">';		
			html += '			<i class="fa fa-question-circle"></i>视频格式';
			html += '		</a>';
			html += '	</div>';
			html += '</div>';
		}
		return html;
	}
	function getHtml_item(id, option, item) {
		var html = '<div item-id=' + id + ' class="col-md-4">';
		html += '	<div class="row ppvideo"> <div class="col-md-4">';
		html += '       <video class="vdo_vdo" style="object-fit:cover;" width="' + (option.videoWidth || 250) + '" height="' + (option.videoHeight || 150) + '" controls data-id="' + (item.id || '') + '" poster="' + (item.cover || '') + '" > ';
		html += '       	<source src="' + (item.videoUrl || '') + '" type="video/mp4" />';
		html += '       </video>';
		html += '   </div></div>';
		html += '   <div class="row VideoDesc"><div class="col-md-4">';
		html += '       <input value="' + (item.videoDesc || '') + '" style="width:' + (option.videoWidth || 250) + 'px;" type="text" name="VideoDescs" placeholder="' + (option.cover.placeholder || '') + '" maxlength="' + (option.cover.maxImgDescLength) + '" />';
		html += '   </div></div>';
		if (option.canAdd) html += '   <a style="width:100px;margin-right:30%;color:#00adff;cursor:pointer;" class="container_vedio font-size" data-id="' + (item.id || '') + '">上传视频封面</a>';
		if (option.canDel) html += '   <a style="cursor:pointer;" class="delrankbtn fa font-size deletebutten text-danger" data-id="' + (item.id || '') + '">删除视频</a>  ';
		html += '</div>';
		return html;
	}
	
    function __batchUploadVideo(option) {
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
		
		var win = $.CropAvatarImgWin($.extend({}, option.cover, { 						
			title: option.title,
			onAfterUploaded: function(o) { return on_videoCover_uploaded(option, o) },			
		}));
		
		el.html('').html(getHtml_body(option));
		
		//上传video
        (function (b) {
            el.find('.uploadvideo-btn').on('click', function () {
                var k = $(this).attr('data-video');
                var fileUpload = el.find('[name=' + k + ']').get(0);
                var files = fileUpload.files;
                if (files.length <= 0) {
                    return false;
				}
				if (option.videoFileMaxSize != -1 && files[0].size > option.videoFileMaxSize) {
					ShowAlert('视频大小不能超过' + str_getsize(option.videoFileMaxSize), -1);
					return false;
				}
                if (b) return false;
                b = true, Loading('视频上传中...');
                var data = new FormData();
                for (var i = 0; i < files.length; i++) {
                    data.append(k, files[i]);
                }
                $.ajax({
                    type: "POST",
                    url: option.uploadVideoUrl, //"/school/upload?type=3&id=@ViewBag.eid",
                    contentType: false,
                    processData: false,
                    cache: false,
                    data: data
                }).done(function (res) {
                    b = false, CloseLoading();
                    if (res.state == 200) {                        
                        fileUpload.value = '';
						var rr = $.type(option.fmtResAfterVideoUploaded) === 'function' 
							? option.fmtResAfterVideoUploaded(res.result) : $option.fmtResAfterVideoUploaded(res.result);
                        var divs = renderHtml_videos(option, el, rr, win);                        
						$.type(option.onItemAdded) === 'function' && option.onItemAdded(rr, divs.filter('[item-id]'));
                    } else {
                        ShowAlert(res.message);
                    }
                }).fail(function (res) {
                    b = false, CloseLoading();
                    ShowAlert(res.message);
                });
            });
        })();
		//删除视频
        el.on('click', 'a.fa', function () {       
			var div = $(this).parents('div[item-id]').eq(0);			
			var b = $.type(option.onItemDel) === 'function' ? option.onItemDel(__getData_item(option, div), div) : undefined;
			if (b !== false) div.remove();
        });
		// 视频封面图, 封面图只有一张, 更换直接替换就可以了.
		el.find('div[vdo]').on('click', '.container_vedio', function () { 
			win.show($(this).parents('[item-id]').eq(0));
		});
		el.find('div[vdo]').on('change', 'input[name=VideoDescs]', function () {
			var div = $(this).parents('[item-id]').eq(0);
            $.type(option.onItemChanged) === 'function' && option.onItemChanged(__getData_item(option, div), div, 'videoDesc', this.value);
        });
		
		// init videos
		if (option.data && option.data.length) {		
			renderHtml_videos(option, el, option.data, win);            
		}		

		$.type(option.onLoaded) === 'function' && option.onLoaded(option.data, el);
		
		return {
			getData: function() { return __getData(el, option) },
		};
    }
	function renderHtml_videos(option, el, items, win) {
		var htmlStr = '';
		$.each(items, function(_, item) {
			var id = getId('batchUploadVideo_');
			htmlStr += getHtml_item(id, option, item);		
		});		
		var divs = jQuery(htmlStr).appendTo(el.find('div[vdo]'));				
		//只播放一个视频
        divs.find('video').on('playing', function() {
            var self = this;
            jQuery('video').each(function () {
                self !== this && this.pause();
            });
        });
		return divs;
	}
	function on_videoCover_uploaded(option, o) {
		var div = o.args;
		div.find("video").attr("poster", o.res.url_s);
		div.find("input[name=VideoDescs]").val(o.res.desc);		
		$.type(option.onItemChanged) === 'function' && option.onItemChanged(__getData_item(option, div), div);
	}
    function __getData(el, option) {
		return $.map(el.find('div[item-id]'), function(_x) { return __getData_item(option, $(_x), {}) });
    }
    function __getData_item(option, x, obj) {
		obj = obj || {};
		obj.cover = x.find("video").attr("poster");
		obj.videoDesc = x.find("input[name=VideoDescs]").val();
		obj.id = x.find('video').attr('data-id');
		obj.videoUrl = x.find('video source')[0].src;
		return obj;
    }
    
    $.batchUploadVideo = __batchUploadVideo;
    $.batchUploadVideo.configDefaultOption = function(option) { return $option = $.extend(true, $option, option) };
	$.batchUploadVideo.getDefaultOption = function () { return $option };
    $.batchUploadVideo.getData = function(el, option) { return __getData($(el), $.extend({}, $option, option)) };    
    
    function find_kv(obj, k) {
		if (k === undefined || k === null) return;
		k = k.toLowerCase();
		for (var n in obj) {
			if (n.toLowerCase() == k)
				return { k: n, v: obj[n] };
		}
	}
	function str_getsize(val, fixed) {
		var s = '', fixed = fixed || 0;
		if (val == null || val == 0) s = '0';
		else if (val < 1024) s = val.toFixed(fixed) + 'B';
		else if (val / 1024.0 < 1024) s = (val / 1024.0).toFixed(fixed) + 'K';
		else s = (val / (1024.0 * 1024.0)).toFixed(fixed) + 'M';
		return s;
	}
})(jQuery, JSON);