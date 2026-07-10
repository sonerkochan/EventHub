
$(document).on('shown.bs.modal', '.modal', function () {
    const mapContainer = document.getElementById('locationMap');
    const latInput = document.getElementById('latInput');
    const lngInput = document.getElementById('lngInput');
    const cityInput = document.getElementById('cityInput');
    const countryInput = document.getElementById('countryInput');
    const postalInput = document.getElementById('postalInput');

    if (!mapContainer || !latInput || !lngInput) return;

    if (window.activeModalMap) {
        try { window.activeModalMap.remove(); } catch (e) { }
    }

    const hasValidCoords = latInput.value && lngInput.value &&
        parseFloat(latInput.value) !== 0 &&
        parseFloat(lngInput.value) !== 0;

    const defaultLat = hasValidCoords ? parseFloat(latInput.value) : 42.6977;
    const defaultLng = hasValidCoords ? parseFloat(lngInput.value) : 23.3219;

    const map = L.map('locationMap').setView([defaultLat, defaultLng], hasValidCoords ? 14 : 7);
    window.activeModalMap = map;

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    let marker = null;

    function attachMarkerEvents(m) {
        m.on('dragend', function (e) {
            const pos = m.getLatLng();
            latInput.value = pos.lat.toFixed(6);
            lngInput.value = pos.lng.toFixed(6);
            updateAddressFromCoordinates(pos.lat, pos.lng);
        });
    }

    if (hasValidCoords) {
        marker = L.marker([defaultLat, defaultLng], { draggable: true }).addTo(map);
        attachMarkerEvents(marker);
    }

    function updateAddressFromCoordinates(lat, lng) {
        const url = `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lng}&accept-language=bg,en`;

        fetch(url, { headers: { 'User-Agent': 'EventHubApplication' } })
            .then(response => response.json())
            .then(data => {
                if (data && data.address) {
                    const addr = data.address;
                    const city = addr.city || addr.town || addr.village || addr.municipality || "";
                    if (cityInput) cityInput.value = city;

                    const country = addr.country || "";
                    if (countryInput) countryInput.value = country;

                    const postal = addr.postcode || "";
                    if (postalInput) postalInput.value = postal;
                }
            })
            .catch(error => console.error('Грешка при reverse geocoding:', error));
    }

    function syncMapFromTextInputs() {
        const city = cityInput ? cityInput.value.trim() : "";
        const country = countryInput ? countryInput.value.trim() : "";
        const postal = postalInput ? postalInput.value.trim() : "";

        if (city === "") return;

        const searchQuery = `${postal} ${city} ${country}`.trim();
        const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(searchQuery)}&limit=1`;

        fetch(url, { headers: { 'User-Agent': 'EventHubApplication' } })
            .then(response => response.json())
            .then(data => {
                if (data && data.length > 0) {
                    const lat = parseFloat(data[0].lat);
                    const lon = parseFloat(data[0].lon);

                    map.setView([lat, lon], 13);
                    latInput.value = lat.toFixed(6);
                    lngInput.value = lon.toFixed(6);

                    if (marker) {
                        marker.setLatLng([lat, lon]);
                    } else {
                        marker = L.marker([lat, lon], { draggable: true }).addTo(map);
                        attachMarkerEvents(marker);
                    }
                }
            })
            .catch(error => console.error('Грешка при търсене по адрес:', error));
    }

    map.on('click', (e) => {
        const { lat, lng } = e.latlng;
        latInput.value = lat.toFixed(6);
        lngInput.value = lng.toFixed(6);

        if (marker) {
            marker.setLatLng(e.latlng);
        } else {
            marker = L.marker(e.latlng, { draggable: true }).addTo(map);
            attachMarkerEvents(marker);
        }

        updateAddressFromCoordinates(lat, lng);
    });

    function syncMapFromCoords() {
        const lat = parseFloat(latInput.value);
        const lng = parseFloat(lngInput.value);
        if (isNaN(lat) || isNaN(lng)) return;
        const latlng = [lat, lng];
        if (marker) {
            marker.setLatLng(latlng);
        } else {
            marker = L.marker(latlng, { draggable: true }).addTo(map);
            attachMarkerEvents(marker);
        }
        map.panTo(latlng);
    }

    latInput.addEventListener('input', syncMapFromCoords);
    lngInput.addEventListener('input', syncMapFromCoords);

    let debounceTimer;
    function handleTextInput() {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(syncMapFromTextInputs, 600);
    }

    if (cityInput) cityInput.addEventListener('input', handleTextInput);
    if (countryInput) countryInput.addEventListener('input', handleTextInput);
    if (postalInput) postalInput.addEventListener('input', handleTextInput);

    if (!hasValidCoords && cityInput && cityInput.value) {
        syncMapFromTextInputs();
    }

    setTimeout(() => {
        map.invalidateSize();
        if (marker) {
            map.setView(marker.getLatLng(), 14);
        }
    }, 300);
});

$(document).on('hidden.bs.modal', '.modal', function () {
    if (window.activeModalMap) {
        try {
            window.activeModalMap.remove();
            window.activeModalMap = null;
        } catch (e) { }
    }
});