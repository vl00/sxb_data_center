(function (factory) {
    if (typeof define === 'function' && define.amd) { define(['jquery'], factory); }
    else if (typeof exports === 'object') { factory(require('jquery')); }
    else { factory(jQuery); }
})
'use strict';
var $ = jQuery;
var console = window.console ||
    { log: function () { } };
function CropAvatar($element, $clkelement, imgtitle, imgwidth, imgheight, url, callback, isdefault = false) {
    console.log($element);
    this.$callback = callback;
    this.$container = $element;
    this.$click = $clkelement;
    this.$avatarView = $('#avatar-view');
    this.$avatar = this.$avatarView.find('img');
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
    this.$imgtitle = imgtitle;
    this.$imgwidth = imgwidth;
    this.$imgheight = imgheight;
    this.$url = url;
    this.$default = isdefault;
    this.init();
    console.log(this);
}
CropAvatar.prototype = {
    constructor: CropAvatar,
    support: {
        fileList: !!$('<input type="file">').prop('files'),
        blobURLs: !!window.URL && URL.createObjectURL, formData: !!window.FormData
    },
    init: function () {
        this.$avatarForm.find(".modal-title").html(this.$imgtitle);
        this.support.datauri = this.support.fileList && this.support.blobURLs;
        if (!this.support.formData) { this.initIframe(); }
        this.initTooltip();
        this.initModal();
        this.removeListener();
        this.addListener();
    },
    addListener: function () {
        this.$avatarView.on('click', $.proxy(this.click, this));
        this.$avatarInput.on('change', $.proxy(this.change, this));
        this.$avatarForm.on('submit', $.proxy(this.submit, this));
        this.$avatarBtns.on('click', $.proxy(this.rotate, this));
    },
    removeListener: function () {
        this.$avatarView.unbind('click');
        this.$avatarInput.unbind('change');
        this.$avatarForm.unbind('submit');
        this.$avatarBtns.unbind('click');
    },
    initTooltip: function () { this.$avatarView.tooltip({ placement: 'bottom' }); },
    initModal: function () { this.$avatarModal.modal({ show: false }); },
    initPreview: function () {
        var url = this.$avatar.attr('src');
        if (url) {
            this.$avatarPreview.empty().html('<img src="' + url + '">');
        }
    },
    initIframe: function () {
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
    },
    change: function () {
        var files, file;
        if (this.support.datauri) {
            files = this.$avatarInput.prop('files');
            if (files.length > 0) {
                file = files[0]; if (this.isImageFile(file)) {
                    if (this.url) { URL.revokeObjectURL(this.url); }
                    this.url = URL.createObjectURL(file); this.startCropper();
                }
            }
        } else { file = this.$avatarInput.val(); if (this.isImageFile(file)) { this.syncUpload(); } }
    },
    submit: function () {
        //debugger;
        if (!this.$avatarSrc.val() && !this.$avatarInput.val()) { return false; }
        if (this.support.formData) { this.ajaxUpload(); return false; }
    },
    rotate: function (e) {
        var data;
        if (this.active) {
            data = $(e.target).data();
            if (data.method) { this.$img.cropper(data.method, data.option); }
        }
    },
    isImageFile: function (file) {
        if (file.type) { return /^image\/\w+$/.test(file.type); }
        else { return /\.(jpg|jpeg|png|gif)$/.test(file); }
    },
    startCropper: function () {
        var _this = this;
        if (this.active) { this.$img.cropper('replace', this.url); }
        else {
            this.$img = $('<img src="' + this.url + '">');
            this.$avatarWrapper.empty().html(this.$img);
            if (!this.$imgheight || !this.$imgwidth || this.$imgwidth == 0 || this.$imgheight == 0) {
                this.$img.cropper({
                    aspectRatio: 1,
                    preview: this.$avatarPreview,
                    strict: false,
                    crop: function (data) {
                        var json = ['{"x":' + data.detail.x, '"y":' + data.detail.y, '"height":' + data.detail.height, '"width":' + data.detail.width, '"rotate":' + data.detail.rotate + '}'].join();
                        _this.$avatarData.val(json);
                    }
                });
            } else {
                this.$img.cropper({
                    preview: this.$avatarPreview,
                    toggleDragModeOnDblclick: false,
                    cropBoxResizable: false,
                    zoomable: true,
                    dragMode: "move",
                    data: {
                        width: parseInt(this.$imgwidth), height: parseInt(this.$imgheight)
                    },
                    strict: false,
                    crop: function (data) {
                        var json = ['{"x":' + data.detail.x, '"y":' + data.detail.y, '"height":' + data.detail.height, '"width":' + data.detail.width, '"rotate":' + data.detail.rotate + '}'].join();
                        _this.$avatarData.val(json);
                    }
                });
            }
            this.active = true;
        }
    },
    stopCropper: function () {
        if (this.active) {
            this.$img.cropper('destroy');
            this.$img.remove();
            this.active = false;
        }
    },
    ajaxUpload: function () {
        var url = this.$url,
            _this = this;
        if (this.$default) {
            //上传logo
            this.$img.cropper("getCroppedCanvas", this.$img.cropper('getData', {})).toBlob(function (blob) {
                var xy, data = new FormData();
                data.set('avatar_file', blob);
                xy = JSON.parse(data.get('avatar_data')),
                    (xy.x = xy.y = 0),
                    data.set('avatar_data', JSON.stringify(xy));
                $.ajax(url, {
                    type: 'post', data: data, dataType: 'json', processData: false, contentType: false,
                    beforeSend: function () { _this.submitStart(); },
                    success: function (data) { _this.submitDone(data); },
                    error: function (XMLHttpRequest, textStatus, errorThrown) { _this.submitFail(textStatus || errorThrown); },
                    complete: function () { _this.submitEnd(); }
                });
            }, 'image/png');
        } else {
            //裁剪
            var data = new FormData();
            data.set('avatar_file', _this.$avatarInput.prop('files')[0]);
            data.set("avatar_data", JSON.stringify(JSON.parse(_this.$avatarData.val())));
            $.ajax(url, {
                type: 'post', data: data, dataType: 'json', processData: false, contentType: false,
                beforeSend: function () { _this.submitStart(); },
                success: function (data) { _this.submitDone(data); },
                error: function (XMLHttpRequest, textStatus, errorThrown) { _this.submitFail(textStatus || errorThrown); },
                complete: function () { _this.submitEnd(); }
            });
        }
    },
    syncUpload: function () { this.$avatarSave.click(); },
    submitStart: function () { this.$loading.fadeIn(); },
    submitDone: function (data) {
        if ($.isPlainObject(data) && data.state === 200) {
            if (data.result) {
                this.url = data.result;
                if (this.support.datauri || this.uploaded) { this.uploaded = false; this.cropDone(); }
                else { this.uploaded = true; this.$avatarSrc.val(this.url); this.startCropper(); }
                //赋值
                //jQuery(".card-body input[name='Schoollogo']").val(data.result);
                console.log(data);
                console.log(this.$avatarForm.serializeArray());
                this.$callback(data, this.$avatarForm.serializeArray(), this.$click);
                console.log("重置form表单");
                this.$avatarForm[0].reset();
                this.$container.modal("hide");
            }
            else if (data.message) {
                this.alert(data.message);
            }
        } else { this.alert('上传失败！'); }
    },
    submitFail: function (msg) { this.alert(msg); },
    submitEnd: function () {
        this.$loading.fadeOut();
    },
    cropDone: function () {

        this.$avatar.attr('src', this.url); this.stopCropper(); this.$avatarModal.modal('hide');
    },
    alert: function (msg) {
        var $alert = ['<div class="alert alert-danger avater-alert">', '<button type="button" class="close" data-dismiss="alert">&times;</button>', msg, '</div>'].join('');
        this.$avatarUpload.after($alert);
    }
};