const CACHE_NAME = 'khadamat-cache-v3';
const ASSETS = [
    '/',
    '/index.html',
    '/manifest.json',
    '/favicon.png',
    '/icon-192.png',
    '/icon-512.png',
    '/apple-touch-icon.png',
    '/_framework/blazor.webassembly.js',
    '/css/khadamat.css'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => cache.addAll(ASSETS))
    );
    self.skipWaiting();
});

// Delete ALL old caches on activation to force icon refresh
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((keys) =>
            Promise.all(
                keys.filter((key) => key !== CACHE_NAME)
                    .map((key) => caches.delete(key))
            )
        ).then(() => self.clients.claim())
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
