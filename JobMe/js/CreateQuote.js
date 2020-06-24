//@ sourceURL=CreateQuote.js
var pay = false;
$(document).ready(function () {

    if (GetQueryString()['QuoteID']) {

        try {
            pay = true;
            $('#PayBtn').show();
            $('#CreateBtn').hide();

            CallService("GetPaymentInfo", { 'QuoteID':parseInt( GetQueryString()['QuoteID']) }, function (e) { $('#content').append(e); });
            CallService("GetQuoteInfo", { 'QuoteID': parseInt(GetQueryString()['QuoteID']) }, function (e) {
                $(e.ServiceRequest_Quote_Line).each(function (i, x) {
                    AddLineInfo(x);
                });
                $('#Total').html("R" + e.Total);  
                
            });
        } catch (r) {
        bootbox.alert('Internal error: Missing info.', function () { LoadPage('FindService'); });

        }

    } else if (GetQueryString()['ServiceRequestID']) {
        

        $('#addline').on('click', function () {
            AddLine();
        });
        AddLine();
    }
    else {
        bootbox.alert('Internal error: Missing info.', function () { LoadPage('FindService'); });
    }
});
function AddLineInfo(lineInfo) {
    var l = AddLine();
    $(l).find("#LineHeader").val(lineInfo.Description);
    $(lineInfo.ServiceRequest_Quote_Line_Item).each(function (i, x) {
        AddItem($(l).find('#additem'), x);
    });
    //for (var item in lineInfo.ServiceRequest_Quote_Line_Item) {
    //    var i = lineInfo.ServiceRequest_Quote_Line_Item[item];
        
    //    AddItem($(l).find('#additem'),i);

    //}

}
function AddLine() {
    var f = $('#FullLine').clone();
    $(f).attr('id', '');
    var t = $(f).find('#tbl');
    $(t).DataTable({
        "searching": false,
        "paging": false,
        "sort": false,
        "info": false
    });
    $('#Lines').append(f);
    $(f).fadeIn();
    return f;
}
function AddItem(e,i)
{
    $(e).parent().parent().last().find('tbody').append("<tr id='Line_" + i.Id+"'>" + $('#itemRow').html() + "</tr>");
    if (i) {
        setTimeout(function (ex) {
            $('#'+ex.LineID).find('#des').val(ex.i.Description);
            $('#'+ex.LineID).find('#rate').val(ex.i.Amount);
            $('#' + ex.LineID).find('#qty').val(ex.i.Qty);
            Calculate();
        }, 500, { LineID: "Line_" + i.Id, i: i });
    }
}
function Calculate() {
    $('tr').each(function (e, i) {
        var rate = $(i).find('#rate').val();
        var qty = $(i).find('#qty').val();
        var amt = $(i).find('#amt');
        try {
            if((rate)&&(qty))
            $(amt).val(parseFloat(rate) * parseFloat(qty));
        } catch (r) {}
        
    });
}
function Create() {
    var data = [];
    $('.LineHeader').each(function (ex, l) {
        var items = [];
        $(l).parent().parent().find('tr').each(function (e, i) {

            var rate = $(i).find('#rate').val();
            var des = $(i).find('#des').val();
            var qty = $(i).find('#qty').val();
            var amt = $(i).find('#amt');
            try {
                if ((rate) && (qty)) {
                    $(amt).val(parseFloat(rate) * parseFloat(qty));
                    items.push({
                        'Description': des, 'Amount': rate, 'Qty': qty, 'Id': 0, 'ServiceRequestLineId': 0, 'Vat': 0, 'DateCreated': null, 'CreatedBy': null, 'ServiceRequest_Invoice_Line_Item': [], 'IsDeleted': false, 'ServiceRequest_Quote_Line': null, "DateUpdated": null, "UpdatedBy": null
                    });
                }
            } catch (r) { }

        });
        if ($(l).val()) {
            eval("data.push({'" + $(l).val() + "': items.slice() });");
        }
        items = [];
    });
    var id = parseInt(GetQueryString()["ServiceRequestID"]);
    CallService("CreateQoute", { 'ServiceRequestID': id, 'TotalDiscountPercentage': 0, 'Vat': 15, 'itemsJson': JSON.stringify(data) }, function () {
        bootbox.alert("Quote Created", function () {
            LoadPage("FindService");
        });
    })
}
