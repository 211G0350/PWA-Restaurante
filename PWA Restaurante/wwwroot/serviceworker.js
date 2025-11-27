

const cacheName = "pwa-restaurante-v2";
const apiCacheName = "pwa-restaurante-api-v1";

const urlsAlCache = [
    //vistas
    "/", 
    "/Admin/panelAdmin",
    "/Admin/adminProducts",
    "/Admin/aggProductos",
    "/Admin/usuariosadmin",
    "/Mesero/panelMesero",
    "/Mesero/crearNuevaOr",
    "/Mesero/detalleordenDato",
    "/Mesero/EditarPendiente",
    "/Mesero/EliminarPeniente",
    "/Cociner/panelCocina",
    //todo lo css
    "/css/Admin.css",
    "/css/AdminUsuarios.css",
    "/css/Admincss/AdminProductos.css",
    "/css/Cocinacss/detallesmoal.css",
    "/css/Cocinero.css",
    "/css/Home.css",
    "/css/mes.css",
    "/css/Mesero.css",
    "/css/PanelCocina.css",
    "/css/Shared.css",
    // imagenes que ocupamos
    "/Img/chef.png",
    "/Img/default.jpg",
    "/Img/opcion-de-cerrar-sesion.png",
    "/Img/renderindex.png",
    "/Img/restaurante.png",
    "/Img/sincronizar.png",
    "/Img/usuario.png",
    "/Img/usuarios.png"
];


// instalar service worker
self.addEventListener("install", function (event) {
    event.waitUntil(
        caches.open(cacheName).then(function (cache) {
            return cache.addAll(urlsAlCache).catch(function (error) {
                console.error("Error al cachear recursos:", error);
            });
        }).then(function () {
            return self.skipWaiting();
        })
    );
});

self.addEventListener("activate", function (event) {
    event.waitUntil(
        caches.keys().then(function (todoCache) {
            return Promise.all(
                todoCache.map(function (nomCache) {
                    // Mantener solo los caches actuales, eliminar los antiguos
                    if (nomCache !== cacheName && nomCache !== apiCacheName) {
                        return caches.delete(nomCache);
                    }
                })
            );
        }).then(function () {
            return self.clients.claim();
        })
    );
});

async function networkFirst(pedido) {
    try {
        const RespuestaNet = await fetch(pedido);
        
        if (RespuestaNet.ok) {
            const cache = await caches.open(cacheName);
            cache.put(pedido, RespuestaNet.clone());
        }
        
        return RespuestaNet;
    } catch (error) {
        console.warn("Red no disponible, usando caché:", error);
        const cache = await caches.open(cacheName);
        const cacheRespuesta = await cache.match(pedido);
        
        if (cacheRespuesta) {
            return cacheRespuesta;
        }
        
        return new Response("Recurso no disponible", { 
            status: 503 
        });
    }
}

// caché separado para respuestas get de la api
async function networkFirstAPI(pedido) {
    try {
        const RespuestaNet = await fetch(pedido);
        
        if (RespuestaNet.ok) {
            const apiCache = await caches.open(apiCacheName);
            apiCache.put(pedido, RespuestaNet.clone());
        }
        
        return RespuestaNet;
    } catch (error) {
        console.warn("Red no disponible para API, usando caché:", error);
        const apiCache = await caches.open(apiCacheName);
        const cacheRespuesta = await apiCache.match(pedido);
        
        if (cacheRespuesta) {
            return cacheRespuesta;
        }
        
        return new Response(JSON.stringify({ 
            error: "Sin conexión y sin datos en caché",
            message: "No se pudo obtener el recurso y no hay versión en caché"
        }), { 
            status: 503,
            headers: { 'Content-Type': 'application/json' }
        });
    }
}

async function cacheFirst(pedido) {
    try {
        const cache = await caches.open(cacheName);
        const cacheRespuesta = await cache.match(pedido);
        
        if (cacheRespuesta) {
            return cacheRespuesta;
        }
        
        const RespuestaNet = await fetch(pedido);
        
        if (RespuestaNet.ok) {
            cache.put(pedido, RespuestaNet.clone());
        }
        
        return RespuestaNet;
    } catch (error) {
        console.error("Error en cacheFirst:", error);
        return new Response("Error al obtener el recurso: " + pedido.url, { 
            status: 500 
        });
    }
}

self.addEventListener("fetch", function (event) {
    const url = new URL(event.request.url);
    const pathname = url.pathname;
    const method = event.request.method;
    
    if (pathname.startsWith("/api/") && method === "GET") {
        event.respondWith(networkFirstAPI(event.request));
        return;
    }
    
    // cuando es fetch post/put/delete aun no cachear, quiero sincronziar checar despues
    if (pathname.startsWith("/api/")) {
        return;
    }
    
    if (pathname.startsWith("/css/") && pathname.endsWith(".css")) {
        event.respondWith(networkFirst(event.request));
        return;
    }
    
    if (pathname === "/" || 
        pathname.startsWith("/Admin/") || 
        pathname.startsWith("/Mesero/") || 
        pathname.startsWith("/Cociner/")) {
        event.respondWith(networkFirst(event.request));
        return;
    }
    
    if (pathname.startsWith("/Img/")) {
        const img = pathname.split("/").pop();
        const imgSistema = [
            "chef.png", 
            "default.jpg", 
            "opcion-de-cerrar-sesion.png", 
            "renderindex.png", 
            "restaurante.png", 
            "sincronizar.png", 
            "usuario.png", 
            "usuarios.png"
        ];
        if (imgSistema.includes(img)) {
            event.respondWith(cacheFirst(event.request));
            return;
        }
    }
        if (urlsAlCache.includes(pathname)) {
        event.respondWith(cacheFirst(event.request));
    }
});
