//import 'json2';

(function($, JSON) {
    if (typeof $.postJSON === 'undefined') {
        if (!JSON || !JSON.stringify) throw "jquery.postJSON need 'JSON.stringify' api to serialize data"; 
        $.extend({
            postJSON: function (url, data, callback, type) {
                if ($.isFunction(data)) {
                    type = type || callback;
                    callback = data;
                    data = undefined;
                }
                if ($.type(data) == 'string') data = data;
                else data = JSON.stringify(data);
                var o = {
                    type: 'POST',
                    url: url,
                    data: data,
                    dataType: type, //'json',
                    contentType: 'application/json'
                };
                return callback && (o.success = callback), $.ajax(o);
            }
        });
    }
})(jQuery, JSON);
