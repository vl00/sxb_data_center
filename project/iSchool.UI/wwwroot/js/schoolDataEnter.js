/// school录入页面相关js

/**
 * 页面跳转是拦截，提醒数据未保存
 */
function before_unload(e) {
    var message = '页面数据还未保存，是否要离开本页？';
    if (typeof e === 'undefined') {
        e = window.event;
    }
    if (e) {
        e.returnValue = message;
    }
    return message;
}
window.onbeforeunload = before_unload;

jQuery(function () {
    var url = jQuery('#a-step1win').attr('a-href');
    jQuery('#a-step1win').on('click', function () {
        LiejiaJS.win['step1win'] = new LiejiaJS.win({
            title: '基本信息',
            width: 800,
            height: 500,
            backdrop: 'static',
            nofooter: true,
            body: '<iframe src="' + url + '" frameborder="0" style="width:100%;height:400px;border-width:0;"></iframe>',
            onshown: function () {
                //...
            },
            onclosed: function () {
                this.dispose();
            }
        });
        return false;
    });
});

