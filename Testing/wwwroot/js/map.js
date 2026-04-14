// Global variable to keep track of our map instance
window.mapInstance = null;
window.mapMarker = null; // for single marker tracking
window.mapMarkers = []; // for multiple markers

window.initializeMap = (containerId, initialLat, initialLng, zoomLevel, dotnetHelper) => {
    // If map already exists, remove it first (for SPA navigation)
    if (window.mapInstance !== null) {
        window.mapInstance.remove();
    }

    // Create the map and set starting view
    window.mapInstance = L.map(containerId).setView([initialLat, initialLng], zoomLevel);

    // Add OpenStreetMap tiles (the actual map images)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap contributors'
    }).addTo(window.mapInstance);

    // Add click event to the map
    window.mapInstance.on('click', function(e) {
        var lat = e.latlng.lat;
        var lng = e.latlng.lng;
        
        // When clicked, let's call the C# method back to inform it of the coordinates!
        if (dotnetHelper) {
            dotnetHelper.invokeMethodAsync('OnMapClicked', lat, lng);
        }
    });
};

window.addMarker = (lat, lng, title) => {
    if (window.mapInstance) {
        var marker = L.marker([lat, lng]).addTo(window.mapInstance);
        if (title) {
            marker.bindPopup(title);
        }
        window.mapMarkers.push(marker);
    }
};

window.clearAllMarkers = () => {
    if (window.mapInstance) {
        window.mapMarkers.forEach(m => window.mapInstance.removeLayer(m));
        window.mapMarkers = [];
    }
};

window.flyToLocation = (lat, lng, zoomLevel = 15) => {
    if (window.mapInstance) {
        window.mapInstance.flyTo([lat, lng], zoomLevel, {
            animate: true,
            duration: 1.5
        });
    }
};
