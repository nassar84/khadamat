const CACHE_NAME = 'khadamat-cache-v2';
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
    // For API calls, ALWAYS bypass the service worker cache and go to network
    if (event.request.url.includes('/api/') || 
        event.request.url.includes('/v1/') ||
        event.request.url.includes('/connect/') ||
        event.request.url.includes('/.well-known/')) {
        return; 
    }

    event.respondWith(
        caches.match(event.request)
            .then((response) => response || fetch(event.request))
    );
});
