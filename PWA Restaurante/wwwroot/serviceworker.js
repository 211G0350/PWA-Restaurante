

const cacheName = "pwa-restaurante-v1";

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
                    if (nomCache !== cacheName) {
                        return caches.delete(nomCache);
                    }
                })
            );
        }).then(function () {
            return self.clients.claim();
        })
    );
});

// cache first todo lo estatico
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
    
    // aun no pasar a cache los fetch api, pasar directamente a la red
    if (pathname.startsWith("/api/")) {
        return;
    }
    
    let irCache = false;  
    if (urlsAlCache.includes(pathname)) {
        irCache = true;
    }
    else if (pathname === "/" || 
             pathname.startsWith("/Admin/") || 
             pathname.startsWith("/Mesero/") || 
             pathname.startsWith("/Cociner/")) {
        irCache = true;
    }
    else if (pathname.startsWith("/css/") && pathname.endsWith(".css")) {
        irCache = true;
    }
    else if (pathname.startsWith("/Img/")) {
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
            irCache = true;
        }
    }

    if (irCache) {
        event.respondWith(cacheFirst(event.request));
    }
});
