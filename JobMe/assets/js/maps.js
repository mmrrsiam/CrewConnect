var map;
var markers = [];
var image;
var markerClusterer;
var myPos;
var dotnetHelper = null;

function initMap(componentRef) {
    dotnetHelper = componentRef;

    map = new google.maps.Map(
        document.getElementById('searchMap'),
        { center: new google.maps.LatLng(-25.747110, 28.232243), zoom: 12 });
    //{ center: new google.maps.LatLng(-28.38, 25.3), zoom: 6 });

    image = {
        url: 'global_assets/images/ui/map_marker.png',

        // This marker is 20 pixels wide by 32 pixels tall.
        size: new google.maps.Size(20, 32),

        // The origin for this image is 0,0.
        origin: new google.maps.Point(0, 0),

        // The anchor for this image is the base of the flagpole at 0,32.
        anchor: new google.maps.Point(0, 32)
    };


    navigator.geolocation.getCurrentPosition((position) => {
        //console.log(position);

        var latLng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
        map.setCenter(latLng);

        myPos = new google.maps.Marker({
            position: { lat: position.coords.latitude, lng: position.coords.longitude },
            title: 'Me',
            icon: image,
            map: map
        });

    }, (error) => {
        console.log(error);
    });


}

function addMyLocation() {
    markers.push(myPos);
}

function clearMarkers() {
    for (var i = 0; i < markers.length; i++) {
        markers[i].setMap(null);
    }

    markers = [];
}

function addMapMarkers(foundProviders) {
    clearMarkers();

    var counter = 0;
    foundProviders.serviceProviders.map(item => {
        markers.push(buildMarker(item.latitude, item.longitude, item.name, item.locationId, counter < 5));
        counter++;
    });

    markerClusterer = new MarkerClusterer(map, markers,
        { imagePath: 'lib/google/markerclustererplus/images/m' });
}

function buildMarker(latitude, longitude, name, locationId, animate) {
    var infoWindow = new google.maps.InfoWindow({
        maxWidth: 200,
        content: '<div class="mt-2 text-center"><label><b>' + name + '</b></label><br/><button type="button" class="btn btn-sm bg-teal-400" onclick="javascript:markerClick(' + locationId + ');"><i class="icon-info22 mr-1" style="font-size:10px"></i>View</button></div>'
    });

    var animationType;

    if (animate)
        animationType = google.maps.Animation.DROP;

    var marker = new google.maps.Marker({
        position: { lat: latitude, lng: longitude },
        title: name,
        //icon: image
        animation: animationType
    });

    //infoWindow.open(map, marker);

    marker.addListener('click', function () {
        infoWindow.open(map, marker);
    });

    return marker;
}

function markerClick(locationId) {
    return dotnetHelper.invokeMethodAsync('DisplayServiceProviderInfo', locationId);
}

function displayModal(modalId, modalBackdropId, display) {
    var modal = $('#' + modalId);
    var modalBackdrop = $('#' + modalBackdropId);

    setTimeout(function () {
        if (display) {
            modal.addClass('show');

            modalBackdrop
                .addClass('modal-backdrop')
                .addClass('fade')
                .addClass('show');
        }
        else
            modal.removeClass('show');

        return true;
    }, 1);
}
