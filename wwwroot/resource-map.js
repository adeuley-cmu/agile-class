window.resourceMap = (() => {
    let map;
    let markerLayer;

    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    function render(elementId, markers) {
        const element = document.getElementById(elementId);
        if (!element || typeof L === "undefined") return;

        if (!map || map.getContainer() !== element) {
            map = L.map(element, { zoomControl: true }).setView([39.8283, -98.5795], 4);
            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                maxZoom: 19,
                attribution: "&copy; OpenStreetMap contributors"
            }).addTo(map);
            markerLayer = L.layerGroup().addTo(map);
        }

        markerLayer.clearLayers();
        const bounds = [];
        for (const marker of markers ?? []) {
            const position = [marker.latitude, marker.longitude];
            bounds.push(position);
            L.marker(position).bindPopup(`<strong>${escapeHtml(marker.name)}</strong><br>${escapeHtml(marker.category)}<br>${escapeHtml(marker.address)}`).addTo(markerLayer);
        }

        if (bounds.length > 0) map.fitBounds(bounds, { padding: [24, 24], maxZoom: 13 });
        setTimeout(() => map.invalidateSize(), 0);
    }

    return { render };
})();
