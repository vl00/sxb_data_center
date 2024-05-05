//1、校验类型【Start】------------------------------------------------------------

//判断是否是一个正整数
function ISPositiveInteger(number) {
    var r = /^[1-9]\d*$/;
    if (!r.test(number)) { return false; }
    return true;
}

function checkIntRange(n, min, max) {
    if (n < min || n > max) {
        alert("请输入 " + min + " 至 " + max + " 之间的正整数");        
        return;
    }
}

//1、校验类型【End】------------------------------------------------------------
