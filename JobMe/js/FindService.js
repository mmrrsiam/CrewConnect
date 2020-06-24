//@ sourceURL=FindService.js

var map;
var ServiceTypes;
var markers=[];

$(document).ready(function () {
    CallService("GetServiceTree", "", function (e) {
        ServiceTypes = e;
        $("#SkillSearch").autocomplete({
            source: ServiceTypes.map(x => x.Name)
        });

    });
    $('#SkillSearch').keypress(function (event) {
        if (event.which == 13) {
            FindService();
            $('#SkillSearch').autocomplete('close');
        } 
    });
});
function initMap() {
    map = new google.maps.Map(document.getElementById("map"), {
        center: { lat: -25.7478676, lng: 28.2292712 },
        zoom: 10
    });
    var geocoder = new google.maps.Geocoder();
    geocoder.geocode({ 'address': "South Africa Johannesburg" }, function (results, status) {
        if (status === 'OK') {
            map.setCenter(results[0].geometry.location);
        } else {
            //alert('Geocode was not successful for the following reason: ' + status);
        }
    });

    
}
function FindService() {
    var lat0 = map.getBounds().getNorthEast().lat();
    var lng0 = map.getBounds().getNorthEast().lng();
    var lat1 = map.getBounds().getSouthWest().lat(); 
    var lng1 = map.getBounds().getSouthWest().lng();
    var icon = {
        url: '../assets/images/Service.png',
        scaledSize: new google.maps.Size(50, 50), 
        origin: new google.maps.Point(0, 0), 
        anchor: new google.maps.Point(25, 50) 
    };

    CallService('FindSkillsByRegion', { 'Skill': $('#SkillSearch').val(), 'LatA': lat0, 'LatB': lat1, 'LonA': lng0, 'LonB': lng1 }, function (e) {
        for (var m in markers)
            markers[m].setMap(null);
        markers = [];

        $(e).each(function (p) {
            var marker = new google.maps.Marker({
                position: { lat: this.Latitude, lng: this.Longitude },
                map: map,
                icon: icon,
                title: this.ServiceProviderName
            });
            marker.info = new google.maps.InfoWindow({
                backgroundColor: '#4b334b',
                content: '<img style="max-width: 120px; margin-left: 15%;" src="'+this.Logo+'"/><h4 style="#ff7600"><b>' + this.ServiceProviderName + '<b></h4><div style="color:#b9b8b9">' + this.ServiceType + '</div> <br/>Rate: ' + ('<i style="color:#ff7600;" class="fa fa-star" aria-hidden="true"></i>').repeat(parseInt(this.Rank)) + '<br/>Fees:' + this.CalloutFee +
                    '<button onclick="LoadPage(\'RequestService&ServiceProviderID=' + this.ServiceProviderId+'\')" style="    background-color: purple;    color: white;    border: none;    border-radius: 4px;    width: 100%;    margin-top: 20px;    height: 35px;">Request Service</button>'
            });
            google.maps.event.addListener(marker, 'click', function () {
                marker.info.open(map, marker);
            });
            markers.push(marker);

        });
            
    });
}
