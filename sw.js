var APP_PREFIX = '梓'
var VERSION = '20220818'
var VERSION_AZUSA_PATCH_USE = '20220315'
var AZUSA_PATCH_SKIP_LIST = [
    './manifest.json',
    './icon.jpg',
    './index.html',
    './Build/BuildWebRelease.loader.js'
]
var CACHE_NAME = APP_PREFIX + VERSION
var AZUSA_CACHE = APP_PREFIX + VERSION_AZUSA_PATCH_USE
var URLS = [
    './',
]
self.addEventListener('fetch', event => {
    if (event.request.url.indexOf("unity3d")>0) {
        event.respondWith(new Response("Fuck You Unity Analytics"));
        return true;
    }
    if (event.request.method == "GET" && (event.request.url.indexOf("http") == 0) && (event.request.url.indexOf("bililive.qinlili.workers.dev") == -1)) {
        event.respondWith(
            caches.open(CACHE_NAME).then(async cache => {
                return cache.match(event.request).then(response => {
                    return response || fetch(event.request).then(response => {
                        if (response.status < 300) {
                            cache.put(event.request, response.clone());
                            console.log('file cached : ' + event.request.url)
                        }
                        return response;
                    });
                });
            })

        );
    } else {
        event.respondWith(fetch(event.request))
    }
});
self.addEventListener('install', e => {
    e.waitUntil(
        caches.open(CACHE_NAME).then(async cache => {
            console.log('installing cache : ' + CACHE_NAME)
            if ((await caches.has(AZUSA_CACHE))) {
                console.log("Found Old Cache! Azusa Patch Working...");
                caches.open(AZUSA_CACHE).then(oldCache => {
                    AZUSA_PATCH_SKIP_LIST.forEach(async url => {
                        let tempResponse = await oldCache.match(url);
                        if (tempResponse) {
                            console.log("Azusa Success Transfer Old Cache : " + url)
                            cache.put(url, tempResponse);
                        }
                    })
                })
            }
            return cache.addAll(URLS)
        })
    )
})
self.addEventListener('activate', e => {
    e.waitUntil(
        caches.keys().then((keyList) => {
            var cacheWhitelist = keyList.filter(key => {
                return key.indexOf(APP_PREFIX)
            })
            cacheWhitelist.push(CACHE_NAME)

            return Promise.all(keyList.map((key, i) => {
                if (cacheWhitelist.indexOf(key) === -1) {
                    console.log('deleting cache : ' + keyList[i])
                    return caches.delete(keyList[i])
                }
            }))
        })
    )
})
