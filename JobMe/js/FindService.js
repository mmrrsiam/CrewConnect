//@ sourceURL=FindService.js

var map;
var ServiceTypes;
$(document).ready(function () {
    CallService("GetServiceTree", "", function (e) {
         ServiceTypes=e ;
        const autoCompletejs = new autoComplete({
            data: {
                src: ServiceTypes,
                key: ["id"],
                cache: true
            },
            selector: "#SkillSearch",
            threshold: 0,
            debounce: 0,
            searchEngine: "strict",
            highlight: true,
            maxResults: 5,
        });
    });
    
});
function initMap() {
    map = new google.maps.Map(document.getElementById("map"), {
        center: { lat: 0, lng: 0 },
        zoom: 8
    });
    var geocoder = new google.maps.Geocoder();
    geocoder.geocode({ 'address': "Johannesburg" }, function (results, status) {
        if (status === 'OK') {
            map.setCenter(results[0].geometry.location);
        } else {
            alert('Geocode was not successful for the following reason: ' + status);
        }
    });

    
}
function FindService() {
    var lat0 = map.getBounds().getNorthEast().lat();
    var lng0 = map.getBounds().getNorthEast().lng();
    var lat1 = map.getBounds().getSouthWest().lat(); 
    var lng1 = map.getBounds().getSouthWest().lng();

    CallService('FindSkillsByRegion', { 'Skill': $('#SkillSearch').val(), 'LatA': lat0, 'LatB': lat1, 'LonA': lng0, 'LonB': lng1 }, function (e) {
        $(e).each(function (p) {
            var marker = new google.maps.Marker({
                position: { lat: this.Latitude, lng: this.Longitude },
                map: map,
                title: this.ServiceProviderName
            });
        });
            
    });
}
