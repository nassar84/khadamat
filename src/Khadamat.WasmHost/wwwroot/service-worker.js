const CACHE_NAME = 'khadamat-cache-v1';
const ASSETS = [
    '/',
    '/index.html',
    '/manifest.json',
    '/favicon.png',
    '/_framework/blazor.webassembly.js',
    '/css/khadamat.css'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => cache.addAll(ASSETS))
    );
});

self.addEventListener('fetch', (event) => {
    // For API calls, try network first, then cache
    if (event.request.url.includes('/api/')) {
        event.respondWith(
            fetch(event.request)
                .catch(() => caches.match(event.request))
        );
        return;
    }

    event.respondWith(
        caches.match(event.request)
            .then((response) => response || fetch(event.request))
    );
});
