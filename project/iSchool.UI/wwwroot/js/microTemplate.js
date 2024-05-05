/*(no with data) http://www.cnblogs.com/phpgcs/p/3577026.html 
 *(with data)    http://ejohn.org/blog/javascript-micro-templating/#postcomment
 */

window.microTemplate = (function () {
    return function (str, data) {
        var fn = new Function("data",
					    "var __p__=[];" +
					    //"with(data){" +
					    "__p__.push('" +
					    str.replace(/[\r\t\n]/g, " ")
					      .split("<%").join("\t")
					      .replace(new RegExp("((^|%>)[^\t]*)'", 'g'), "$1\r")
					      .replace(new RegExp("\t=(.*?)%>", 'g'), "',$1,'")
					      .split("\t").join("');")
					      .split("%>").join("__p__.push('")
					      .split("\r").join("\\'")
				      + "');"
				      //+ "}"
				      + "return __p__.join('');"
            );

        return data ? fn(data) : fn;
    };
})();