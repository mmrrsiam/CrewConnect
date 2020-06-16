//@ sourceURL=RequestService.js

$(document).ready(function () {
    
    var picker = new Lightpick({
        field: document.getElementById('fromDate'),
        secondField: document.getElementById('toDate'),
        singleDate: false,
        onSelect: function (start, end) {
            
        }
    });
    CallService("GetServiceProviderInfo", { "ServiceProviderID": GetQueryString()["ServiceProviderID"] }, function (e) {
        $('#ProviderName').html(e.Name);
        $('#ProviderLogo').attr('src', e.LogoURL);
        try { $('#terms').html(e.ServiceProvider_TermsAndConditions[0].Terms); } catch (e) { }
    });
});
function validate() { }

function SubmitRequest() {
    if (validate()) {
        $('#requestBtn').html('submitting...');
        CallService("RequestService", { "ServiceProviderID": GetQueryString()["ServiceProviderID"], "FromDate": '', "ToDate": '', "Details": $('#details').val() }, function (e) {
            bootbox.alert('Your request has been submited, you will be notified by email.');
        });
    }
}