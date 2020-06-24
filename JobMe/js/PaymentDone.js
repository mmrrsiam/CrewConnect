$(document).ready(function () {

    CallService("PaymentSuccessful", { 'PaymentID': GetQueryString()["PaymentID"] }, function () {

    });
});