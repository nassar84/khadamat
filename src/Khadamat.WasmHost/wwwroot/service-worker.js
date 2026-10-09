const CACHE_NAME = 'khadamat-cache-v3';
const ASSETS = [
    '/',
    '/index.html',
    '/manifest.json',
    '/favicon.png',
    '/_framework/blazor.webassembly.js',
    '/css/khadamat.css'
];

self.addEventListener('install', (event) => {
    self.skipWaiting();
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => cache.addAll(ASSETS))
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((name) => {
                    if (name !== CACHE_NAME) {
                        console.log('Purging old service worker cache:', name);
                        return caches.delete(name);
                    }
                })
            );
        }).then(() => self.clients.claim())
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

    // Network-First for navigation/HTML and blazor.boot.json so updates are immediately discovered
    if (event.request.mode === 'navigate' ||
        event.request.url.endsWith('/index.html') ||
        event.request.url.endsWith('blazor.boot.json')) {
        event.respondWith(
            fetch(event.request).catch(() => caches.match(event.request))
        );
        return;
    }

    event.respondWith(
        caches.match(event.request)
            .then((response) => response || fetch(event.request))
    );
});
