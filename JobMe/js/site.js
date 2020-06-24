//@ sourceURL=site.js

var UnsavedData = false;
var LoginOpen = false;

$(document).ready(function () {
    UserCheck();
    setInterval(UserCheck, 10000);
    window.onpopstate = function (e) {
        $('#Main_Content').hide();
        Pageinit();
    };
    PageInit();

});
function GetQueryString() {
    var vars = [], hash;
    if (window.location.href.indexOf('#') == -1) return [];
    var hashes = window.location.href.slice(window.location.href.indexOf('#') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}
function PageInit() {
    $.ajax({
        url: 'Views/' + GetQueryString()[0] + ".html",
        type: "GET",
        cache: false,
        success: function (html) {
            $("#Main_Content").html(html);
            AddScript('js/' + GetQueryString()[0] + ".js");

            $('#Main_Content').fadeIn();
        }
    });
}
function AddScript(ScriptName) {
    var s = document.createElement("script");
    s.type = "text/javascript";
    s.src = ScriptName + '?v=' + Math.random();
    $("head").append(s);
}
function LoadPage(PageName) {

    $('#Main_Content').hide();
    window.history.pushState(null, "", "main.html#" + PageName);
    PageInit();

}
function UserCheck() {
    if (!LoginOpen)
        $.ajax({
            type: "POST",
            cache: false,
            contentType: "application/json; charset=utf-8",
            url: 'JobMeService.svc/GetUser',
            success: function (res) {
                if (res.d != "Login") {
                    data = $.parseJSON(res.d);
                    user = data;
                } else
                    user = null;

                if ((!user) && (!LoginOpen)) {
                    LoginOpen = true;
                    $('#BackDrop').fadeIn();
                    LoadPage("Login");
                }
            },
            error: function () {

            }
        });
}
function CallService(fn, params, callback) {
    $.ajax({
        type: "POST",
        cache: false,
        contentType: "application/json; charset=utf-8",
        url: 'JobMeService.svc/'+fn,
        data: JSON.stringify(params),
        success: function (res) {
            var ret = res.d;
            try {
                ret = JSON.parse(res.d);
            } catch (r) { }
            if (callback)
                callback(ret);
        }
    });
}