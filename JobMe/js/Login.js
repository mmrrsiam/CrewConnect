//@ sourceURL=Login.js
var ip = "";


$.ajax({
    url: 'https://icanhazip.com', type: "GET", cache: false, success: function (html) {
        ip = html.substring(0, html.length - 1);
    }
});
$("#pass").keyup(function (e) {
    if (e.keyCode == 13) {
        Login();
    }
});

function forget() {
    bootbox.prompt({
        title: "Please Enter your <b>Email Address</b>",
        inputType: 'email',
        callback: function (result) {
            if (result != null)
                $.ajax({
                    type: "POST",
                    headers: {
                        'Cache-Control': 'no-cache, no-store, must-revalidate',
                        'Pragma': 'no-cache',
                        'Expires': '0'
                    },
                    cache: false,
                    contentType: "application/json; charset=utf-8",
                    url: 'JobMeService.svc/PasswordResetByEmail',
                    data: '{"Email": "' + result + '"}',
                    processData: true,
                    dataType: "json",
                    success: function (response) {
                        bootbox.alert(response.d);
                        return;

                    }
                });
        }
    });
}
function Login() {
    if ($("#user").val() == "") {
        $("#user").addClass("InvalidText");
        return;
    }
    else
        $("#user").removeClass("InvalidText");

    if ($("#pass").val() == "") {
        $("#pass").addClass("InvalidText");
        return;
    }
    else
        $("#pass").removeClass("InvalidText");

    $('#LoginButton').html("Validating...");
    $.ajax({
        type: "POST",
        headers: {
            'Cache-Control': 'no-cache, no-store, must-revalidate',
            'Pragma': 'no-cache',
            'Expires': '0'
        },
        cache: false,
        contentType: "application/json; charset=utf-8",
        url: 'JobMeService.svc/Login?_' + Math.round(Math.random() * 10000),
        data: '{"username": "' + $("#username").val() + '", "password":"' + $("#password").val() + '","ip":"' + ip + '"}',
        dataType: "json",
        success: function (response) {
            console.log(response.d);

            if (response.d.indexOf("Maintenance") >= 0) {
                $('#LoginButton').html("Login");
                bootbox.alert(response.d);
                return;
            }
            if (response.d.indexOf("Invalid") >= 0) {
                $("#pass").addClass("InvalidText");
                bootbox.alert(response.d);
                $('#LoginButton').html("Login");
                return;
            }
            if (response.d.indexOf("Inactive") >= 0) {
                $('#LoginButton').html("Login");
                bootbox.alert(response.d);
                return;
            }
            if (response.d.indexOf("Internal Error") >= 0) {
                console.log(response.d);
                $('#LoginButton').html("Login");
                alert("Internal Error. Please try again later.");
                return;
            }
            try {
                user = $.parseJSON(response.d);
                LoginOpen = false;
                $('#BackDrop').fadeOut();
                LoadPage('FindService');
            }
            catch (e) {
                console.log(response.d);
                $('#LoginButton').html("Login");
                alert("Internal Error. Please try again later.");
                return;
            }
        },
        error: function (errormsg) {
         
            $('#LoginButton').html("Login");
            console.log(errormsg);
            //alert(errormsg.responseText);
        }
    });
}
