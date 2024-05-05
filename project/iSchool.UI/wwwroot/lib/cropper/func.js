(function (factory) {
    if (typeof define === 'function' && define.amd) { define(['jquery'], factory); } else if (typeof exports === 'object') { factory(require('jquery')); } else { factory(jQuery); }
})
'use strict';
var $ = jQuery;
var console = window.console ||
    { log: function () { } };
function CropAvatar($element, callback) {
    console.log($element);
    this.$callback = callback;
    this.$container = $element; this.$avatarView = this.$container.find('.avatar-view'); this.$avatar = this.$avatarView.find('img');
    this.$avatarModal = this.$container.find('.avatar-modal');
    this.$loading = this.$container.find('.loading');
    this.$avatarForm = this.$avatarModal.find('.avatar-form');
    this.$avatarUpload = this.$avatarForm.find('.avatar-upload');
    this.$avatarSrc = this.$avatarForm.find('.avatar-src');
    this.$avatarData = this.$avatarForm.find('.avatar-data');
    this.$avatarInput = this.$avatarForm.find('.avatar-input');
    this.$avatarSave = this.$avatarForm.find('.avatar-save');
    this.$avatarBtns = this.$avatarForm.find('.avatar-btns');
    this.$avatarWrapper = this.$avatarModal.find('.avatar-wrapper');
    this.$avatarPreview = this.$avatarModal.find('.avatar-preview');
    this.$imgtitle = this.$container.find(".imgtitle");
    this.$imgwidth = this.$container.attr("imgwidth");
    this.$imgheight = this.$container.attr("imgheight");
    this.init();
    console.log(this);
}
CropAvatar.prototype = {
    constructor: CropAvatar, support: { fileList: !!$('<input type="file">').prop('files'), blobURLs: !!window.URL && URL.createObjectURL, formData: !!window.FormData }, init: function () {
        this.support.datauri = this.support.fileList && this.support.blobURLs; if (!this.support.formData) { this.initIframe(); }
        this.initTooltip(); this.initModal(); this.addListener();
    }, addListener: function () {
        this.$avatarView.on('click', $.proxy(this.click, this));
        this.$avatarInput.on('change', $.proxy(this.change, this));
        this.$avatarForm.on('submit', $.proxy(this.submit, this)); this.$avatarBtns.on('click', $.proxy(this.rotate, this));
    }, initTooltip: function () { this.$avatarView.tooltip({ placement: 'bottom' }); }, initModal: function () { this.$avatarModal.modal({ show: false }); }, initPreview: function () {
        var url = this.$avatar.attr('src');
        if (url) {
            this.$avatarPreview.empty().html('<img src="' + url + '">');
        }
    }, initIframe: function () {
        var target = 'upload-iframe-' + (new Date()).getTime(), $iframe = $('<iframe>').attr({ name: target, src: '' }), _this = this; $iframe.one('load', function () {
            $iframe.on('load', function () {
                var data; try { data = $(this).contents().find('body').text(); } catch (e) { console.log(e.message); }
                if (data) {
                    try { data = $.parseJSON(data); } catch (e) { console.log(e.message); }
                    _this.submitDone(data);
                } else { _this.submitFail('Image upload failed!'); }
                _this.submitEnd();
            });
        }); this.$iframe = $iframe; this.$avatarForm.attr('target', target).after($iframe.hide());
    }, change: function () {
        var files, file; if (this.support.datauri) {
            files = this.$avatarInput.prop('files');
            if (files.length > 0) {
                file = files[0]; if (this.isImageFile(file)) {
                    if (this.url) { URL.revokeObjectURL(this.url); }
                    this.url = URL.createObjectURL(file); this.startCropper();
                }
            }
        } else { file = this.$avatarInput.val(); if (this.isImageFile(file)) { this.syncUpload(); } }
    }, submit: function () {
        if (!this.$avatarSrc.val() && !this.$avatarInput.val()) { return false; }
        if (this.support.formData) { this.ajaxUpload(); return false; }
    }, rotate: function (e) {
        var data;
        if (this.active) {
            data = $(e.target).data();
            if (data.method) { this.$img.cropper(data.method, data.option); }
        }
    }, isImageFile: function (file) {
        if (file.type) { return /^image\/\w+$/.test(file.type); }
        else { return /\.(jpg|jpeg|png|gif)$/.test(file); }
    }, startCropper: function () {
        var _this = this;
        if (this.active) { this.$img.cropper('replace', this.url); }
        else {
            this.$img = $('<img src="' + this.url + '">');
            this.$avatarWrapper.empty().html(this.$img);
            this.$img.cropper({
                preview: this.$avatarPreview, cropBoxResizable: false, zoomable: false, dragMode: "move", data: {
                    width: parseInt(this.$imgwidth), height: parseInt(this.$imgheight)
                },
                strict: false, crop: function (data) {
                    var json = ['{"x":' + data.detail.x, '"y":' + data.detail.y, '"height":' + data.detail.height, '"width":' + data.detail.width, '"rotate":' + data.detail.rotate + '}'].join();
                    _this.$avatarData.val(json);
                }
            }); this.active = true;
        }
    }, stopCropper: function () {
        if (this.active) {
            this.$img.cropper('destroy');
            this.$img.remove();
            this.active = false;
        }
    }, ajaxUpload: function () {
        var url = this.$avatarForm.attr('action'),
            _this = this;
        this.$img.cropper("getCroppedCanvas", this.$img.cropper('getData', {})).toBlob(function (blob) {
            var xy, data = new FormData();
            data.set('avatar_file', _this.$avatarInput.prop('files')[0]);
            data.set("avatar_data", JSON.stringify(JSON.parse(_this.$avatarData.val())));
            //xy = JSON.parse(data.get('avatar_data')), (xy.x = xy.y = 0), data.set('avatar_data', JSON.stringify(xy));
            $.ajax(url, {
                type: 'post', data: data, dataType: 'json', processData: false, contentType: false,
                beforeSend: function () { _this.submitStart(); },
                success: function (data) { _this.submitDone(data); },
                error: function (XMLHttpRequest, textStatus, errorThrown) { _this.submitFail(textStatus || errorThrown); },
                complete: function () { _this.submitEnd(); }
            });
        }, 'image/png');
    }, syncUpload: function () { this.$avatarSave.click(); }, submitStart: function () { this.$loading.fadeIn(); },
    submitDone: function (data) {
        if ($.isPlainObject(data) && data.state === 200) {
            if (data.result) {
                this.url = data.result;
                if (this.support.datauri || this.uploaded) { this.uploaded = false; this.cropDone(); }
                else { this.uploaded = true; this.$avatarSrc.val(this.url); this.startCropper(); }
                this.$avatarInput.val('');
                //赋值
                //jQuery(".card-body input[name='Schoollogo']").val(data.result);
                this.$callback(data.result, this.$container.attr("imagename"), $(this.$imgtitle[0]).val());
                $(this.$imgtitle[0]).val("");
            }
            else if (data.message) {
                this.alert(data.message);
            }
        } else { this.alert('上传失败！'); }
    }, submitFail: function (msg) { this.alert(msg); }, submitEnd: function () { this.$loading.fadeOut(); }, cropDone: function () {

        this.$avatar.attr('src', this.url); this.stopCropper(); this.$avatarModal.modal('hide');
    }, alert: function (msg) { var $alert = ['<div class="alert alert-danger avater-alert">', '<button type="button" class="close" data-dismiss="alert">&times;</button>', msg, '</div>'].join(''); this.$avatarUpload.after($alert); }
};

var app = new Vue({
    el: '#app',
    data: {
        imageIndex: 0,
        imageArry: [],
        initPreviewUrl: "/images/schooldefaultlogo.png",
        cropper: [],
    },
    methods: {
        //上传图片方法
        uplooad: function (name) {
            this.cropper[name].submit();
        },
        //删除图片方法
        removeimg: function (index, name) {
            var thisImgArray = this.imageArry.filter(function (x) { return x.Name == name; });
            thisImgArray[0].Data.splice(index, 1);

            jQuery("#isonlyjump").val('false');
        },
        //根据name属性值 取图片数组方法
        getImgarraybyname: function (name) {
            var thisImgArray = this.imageArry.filter(function (x) { return x.Name == name; });
            if (thisImgArray.length > 0)
                return thisImgArray[0].Data;
            return [];
        },
        isShowAddImage: function (name,maxcount) {
            var thisImgArray = this.imageArry.filter(function (x) { return x.Name == name; });
            if (thisImgArray.length > 0) {
                return maxcount > thisImgArray[0].Data.length;
            }
            return true;
        },
        //取消事件
        quit: function (name) {
            this.cropper[name].$avatarModal.modal('hide');
        },
        //计算完成度
        comoletion: function () {
            //一个图片控件算一个 需要排除不计算完成度的控件
            //所有的图片控件数量
            var imgAllArray = jQuery(".vueimgcompletion").length;
            //已有上传控件的数量
            var imgOkArray = app.imageArry.filter(function (x) {
                if (x.Data.length > 0)
                    return x;
            }).length;
            return [imgAllArray, imgOkArray];
        },
        setData: function (data) {
            this.imageArry = data;
        },
        getData: function (name) {
            return this.imageArry.filter(function (x) {
                if (x.Name == name) {
                    return x;
                }
            })[0];
        },
        showModal: function (name) {
            this.cropper[name].$avatarModal.modal('show')
        }
    },
    //页面加载完成以后
    mounted: function () {
        debugger
        var that = this;
        var crop = jQuery('.crop-avatar');
        for (var i = 0; i < crop.length; i++) {
            //上传图片
            that.cropper[jQuery(crop[i]).attr("imagename")] = new CropAvatar(jQuery(crop[i]), function (res, name, title) {
                var thisImgArray = that.imageArry.filter(function (x) { return x.Name == name; });
                if (thisImgArray.length > 0) {
                    thisImgArray[0].Data.push({ Url: { Url: res.url, CompressUrl: res.compressUrl }, ShowDel: false, Title: title });
                } else {
                    that.imageArry.push({ Name: name, Data: [{ Url: { Url: res.url, CompressUrl: res.compressUrl }, ShowDel: false, Title: title }] });
                }

                jQuery("#isonlyjump").val('false');
            });
        }
    }
})