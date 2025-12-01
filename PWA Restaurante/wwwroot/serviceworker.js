
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
    
    if (method === "GET") {
        if (pathname.startsWith("/api/")) {
            event.respondWith(networkFirstAPI(event.request));
            return;
        }
        else {
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
                return;
            }
        }
    } else {
        if (pathname.startsWith("/api/")) {
            event.respondWith(manejarModificaciones(event.request));
            return;
        }
    }
});

// todo lo que irá aindexedDB desde aqui
async function openDatabase() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('pwa-restaurante-sync', 1);
        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(request.result);

        request.onupgradeneeded = (event) => {
            const db = event.target.result;
            if (!db.objectStoreNames.contains('pendientes')) {
                const store = db.createObjectStore('pendientes', {
                    keyPath: 'id',
                    autoIncrement: true
                });
            }
        };
    });
}

async function agregarObjeto(storeName, objeto) {
    const db = await openDatabase();
    const transaction = db.transaction(storeName, 'readwrite');
    const store = transaction.objectStore(storeName);
    const request = store.add(objeto);
    return new Promise((resolve, reject) => {
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function obtenerTodos(storeName) {
    const db = await openDatabase();
    const transaction = db.transaction(storeName, 'readonly');
    const store = transaction.objectStore(storeName);
    const request = store.getAll();
    return new Promise((resolve, reject) => {
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function eliminarObjeto(storeName, id) {
    const db = await openDatabase();
    const transaction = db.transaction(storeName, 'readwrite');
    const store = transaction.objectStore(storeName);
    const request = store.delete(id);
    return new Promise((resolve, reject) => {
        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
    });
}

// que el plan que si se hacen cambios offline se queden guardadon
//y se vayan a poner cuando regrese la conexion
async function manejarModificaciones(request) {
    let clon = request.clone();
    try {
        return await fetch(request);
    } catch (error) {
        let objeto = {
            method: request.method,
            url: request.url,
            headers: Array.from(request.headers.entries())
        };

        if (request.method === "POST" || request.method === "PUT") {
            let datos = await clon.text();
            objeto.body = datos;
        } else {
            objeto.body = null;
        }

        await agregarObjeto("pendientes", objeto);



        if ('sync' in self.registration) {
            try {
                await self.registration.sync.register('pwa-restaurante-sync');
            } catch (syncError) {
                console.warn('No se pudo registrar sync:', syncError);
            }
        }

        return new Response(null, { status: 200 });
    }
}

async function enviarAlReconectar() {
    let pendientes = await obtenerTodos("pendientes");

    for (let p of pendientes) {
        try {
            let headersObj = {};
            p.headers.forEach(([key, value]) => {
                headersObj[key] = value;
            });

            let response = await fetch(p.url, {
                method: p.method,
                headers: headersObj,
                body: p.method === "DELETE" ? null : p.body
            });

            if (response.ok) {
                await eliminarObjeto("pendientes", p.id);
                console.log('Acción sincronizada exitosamente:', p.method, p.url);
            } else {
                console.warn('Error al sincronizar acción:', p.method, p.url, response.status);
                break;
            }
        } catch (error) {
            console.error('Error al sincronizar:', error);
            break; 
        }
    }
}

self.addEventListener("sync", function (event) {
    if (event.tag === "pwa-restaurante-sync") {
        event.waitUntil(enviarAlReconectar());
    }
});
