/*

let urls = [
    "Home/index.cshtml",
 //   "api/Admin",
    "/"
];


async function descargarInstalar() {
    let cache = await caches.open("cachePlanes");
    //cache.put("api/carreras");
    await cache.addAll(urls);
    self.skipWaiting();
}

async function obtenerDesdeCache(request) {
    let cache = await caches.open("cachePlanes");
    let response = await cache.match(request);
    if (response) {
        return response;
    }
    else {
        return await fetch(request);
    }
}

self.addEventListener("install", function (event) {
    event.waitUntil(descargarInstalar());
});

//Funcion fetch (recuperar datos)

self.addEventListener("fetch", function (event) {
    event.respondWith(obtenerDesdeCache(event.request));
});*/