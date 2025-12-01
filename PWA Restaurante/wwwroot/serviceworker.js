
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


const apisPublicas = [
    '/api/Productos/Todos',
    '/api/Productos/Categorias'
];
async function precargarAPIsPublicas() {
    const apiCache = await caches.open(apiCacheName);
    
    for (const apiUrl of apisPublicas) {
        try {
            const Url = new URL(apiUrl, self.location.origin).href;
            const response = await fetch(Url);
            if (response.ok) {
                await apiCache.put(Url, response.clone());
                console.log('API precargada:', apiUrl);
            }
        } catch (error) {
            console.warn('No se pudo precargar API:', apiUrl, error);
        }
    }
}

// instalar service worker
self.addEventListener("install", function (event) {
    event.waitUntil(
        Promise.all([
            // Cachear recursos estáticos
            caches.open(cacheName).then(function (cache) {
                return cache.addAll(urlsAlCache).catch(function (error) {
                    console.error("Error al cachear recursos:", error);
                });
            }),
            precargarAPIsPublicas()
        ]).then(function () {
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

async function cacheFirstAPI(pedido) {
    try {
        const apiCache = await caches.open(apiCacheName);       
        const cacheRespuesta = await apiCache.match(pedido, { ignoreSearch: false, ignoreMethod: false, ignoreVary: true });
        
        if (cacheRespuesta) {
            fetch(pedido).then(respuestaNet => {
                if (respuestaNet.ok) {
                    apiCache.put(pedido, respuestaNet.clone());
                }
            }).catch(() => {
            });
            return cacheRespuesta;
        }
        const RespuestaNet = await fetch(pedido);
        
        if (RespuestaNet.ok) {
            apiCache.put(pedido, RespuestaNet.clone());
        }
        
        return RespuestaNet;
    } catch (error) {
        console.warn("Red no disponible para API, usando caché:", error);
        const apiCache = await caches.open(apiCacheName);     
        const cacheRespuesta = await apiCache.match(pedido, { ignoreSearch: false, ignoreMethod: false, ignoreVary: true });
        
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
            event.respondWith(cacheFirstAPI(event.request));
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
        const response = await fetch(request);
        if (response.ok) {
            return response;
        }       
        return response;
    } catch (error) {
        console.warn('Sin conexión, guardando en IndexedDB:', request.method, request.url);
        
        let objeto = {
            method: request.method,
            url: request.url,
            headers: Array.from(request.headers.entries())
        };

        if (request.method === "POST" || request.method === "PUT") {
            try {
                let datos = await clon.text();
                objeto.body = datos;
            } catch (bodyError) {
                console.warn('Error al leer body:', bodyError);
                objeto.body = null;
            }
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

    if (pendientes.length === 0) {
        return;
    }
    console.log(`Sincronizando ${pendientes.length} acción(es) pendiente(s)...`);

    for (let p of pendientes) {
        try {
            let headersObj = {};
            if (p.headers && Array.isArray(p.headers)) {
                p.headers.forEach(([key, value]) => {
                    headersObj[key] = value;
                });
            }

            let fetchOptions = {
                method: p.method,
                headers: headersObj
            };

            if (p.method !== "DELETE" && p.body !== null && p.body !== undefined) {
                fetchOptions.body = p.body;
            }
            let response = await fetch(p.url, fetchOptions);

            if (response.ok) {
                await eliminarObjeto("pendientes", p.id);
                console.log('Acción sincronizada exitosamente:', p.method, p.url);
            } else {
                if (response.status >= 400 && response.status < 500) {
                    await eliminarObjeto("pendientes", p.id);
                    console.warn('Error del servidor al sincronizar (eliminado de cola):', p.method, p.url, response.status);
                } else {
                    console.warn('Error al sincronizar (se reintentará):', p.method, p.url, response.status);
                    break;
                }
            }
        } catch (error) {
            console.error('Error de red al sincronizar (se reintentará):', error);
            break; // Si hay error de red, se reintentará proximo intento
        }
    }
}

self.addEventListener("sync", function (event) {
    if (event.tag === "pwa-restaurante-sync") {
        event.waitUntil(enviarAlReconectar());
    }
});

self.addEventListener("message", function (event) {
    if (event.data && event.data.type === "PRECARGAR_APIS") {
        const token = event.data.token;
        if (token) {
            precargarAPIsAutenticadas(token);
        }
    }
});

async function precargarAPIsAutenticadas(token) {
    const apiCache = await caches.open(apiCacheName);
    const baseUrl = self.location.origin;
    const apisParaPrecargar = [
        '/api/Productos/TodosAdmin',
        '/api/Usuarios/ObtenerTodos',
        '/api/Pedidos/Pendientes',
        '/api/Pedidos/Enviados',
        '/api/Pedidos/EnPreparacion',
        '/api/Pedidos/Listo'
    ];
    
    const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };
    
    for (const apiUrl of apisParaPrecargar) {
        try {
            const Url = new URL(apiUrl, baseUrl).href;
            const response = await fetch(Url, { headers });
            if (response.ok) {
                const peticionCache = new Request(Url, { headers });
                await apiCache.put(peticionCache, response.clone());
                console.log('API autenticada precargada:', apiUrl);
            } else {
                console.log('API no disponible para este rol o error:', apiUrl, response.status);
            }
        } catch (error) {
            console.warn('No se pudo precargar API autenticada:', apiUrl, error);
        }
    }
}
