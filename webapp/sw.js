const cacheName = 'cache-v1';
const precacheResources = [
  './',
  'index.html',
  'default.css',
  'api.js',
  'https://cdnjs.cloudflare.com/ajax/libs/pulltorefreshjs/0.1.16/pulltorefresh.min.js',
  './icons/flash_off-24px.svg',
  './icons/flash_on-24px.svg',
  './icons/wifi_off-24px.svg',
  './icons/wifi_on-24px.svg',
  './slider/bootstrap-slider.min.css',
  './slider/bootstrap-slider.min.js'
];

self.addEventListener('install', event => {
  console.log('Service worker install event!');
  event.waitUntil(
    caches.open(cacheName)
      .then(cache => {
        return cache.addAll(precacheResources);
      })
  );
});

self.addEventListener('activate', event => {
  console.log('Service worker activate event!');
});

self.addEventListener('fetch', event => {
  console.log('Fetch intercepted for:', event.request.url);
  event.respondWith(caches.match(event.request)
    .then(cachedResponse => {
        if (cachedResponse) {
          return cachedResponse;
        }
        return fetch(event.request);
      })
    );
});