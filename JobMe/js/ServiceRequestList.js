//@ sourceURL=ServiceRequestList.js

$(document).ready(function () {
    CallService('GetRequestList', '', function (e) {
        for (var i in e) {
            var item = e[i];
            var trTemp = $('#temp').html();
            trTemp = trTemp.replace('<tbody>','');
            trTemp = trTemp.replace('</tbody>', '');
            trTemp = trTemp.replace('{Requester}', item.AspNetUser.FirstName + " " + item.AspNetUser.Surname );
            trTemp = trTemp.replace('{Date}', item.DateCreated);
            trTemp = trTemp.replace('{Detail}', item.Details );
            $('#list').append(trTemp);

        }
    });
});