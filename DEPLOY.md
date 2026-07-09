# Despliegue — Chipo Backend (Render · gratis, sin tarjeta)

## 1. Crear la base de datos primero
1. Entrá a https://render.com e iniciá sesión con GitHub.
2. **New + → PostgreSQL**.
   - **Name:** `chipo-db`
   - **Region:** elegí una y **anotala** (el backend tiene que ir en la MISMA región).
   - **Plan:** **Free**.
3. **Create Database.** Esperá a que quede "Available".
4. En la página de la base, buscá **Internal Database URL** y copiala (empieza con `postgresql://…`).

> La base gratis dura 90 días. Antes de que venza te aviso cómo migrarla sin perder nada.

## 2. Crear el backend (Web Service)
1. **New + → Web Service** → conectá el repo `chipo-tienda-backend`.
2. Render detecta el **Dockerfile** solo:
   - **Runtime:** Docker
   - **Region:** la MISMA que la base.
   - **Plan:** **Free**.
3. Antes de crear, abrí **Advanced / Environment Variables** y agregá las de abajo.

## 3. Variables de entorno del backend
| Variable | Valor | Para qué |
|---|---|---|
| `DATABASE_URL` | *(la Internal Database URL del paso 1)* | Conexión a PostgreSQL |
| `Jwt__Secret` | *(texto aleatorio de 40+ caracteres)* | Firma de tokens. **Generá uno propio.** |
| `ADMIN_EMAIL` | `tu-email@chipo.ar` | Usuario admin inicial |
| `ADMIN_PASSWORD` | *(una contraseña fuerte)* | Contraseña admin inicial |
| `AllowedOrigins__0` | *(la URL de Vercel — se completa después)* | CORS |

> `PORT` y `ASPNETCORE_ENVIRONMENT` los maneja Render/el Dockerfile solos.
> Para el `Jwt__Secret`: cualquier texto largo aleatorio. Ej: en Render podés poner
> algo como `chipo_prod_9f3k2mZ...` (mientras sea largo e impredecible).

4. **Create Web Service.** Render construye la imagen (tarda unos minutos la primera vez).

## 4. Qué pasa al desplegar
- El backend **crea todas las tablas** solo (migraciones automáticas).
- Crea el **admin** con `ADMIN_EMAIL` / `ADMIN_PASSWORD`.
- Render te da una URL tipo `https://chipo-tienda-backend.onrender.com`.

## 5. Verificar
- Abrí `TU-URL.onrender.com/api/products` en el navegador → debe responder JSON.
  *(La primera vez puede tardar ~30-50s: el plan free "despierta" el servicio.)*

## Nota sobre el plan free
El servicio se **duerme tras 15 min sin uso**; la siguiente visita lo despierta (~30-50s).
Cuando la tienda tenga movimiento, pasás a un plan pago (7 USD/mes) y deja de dormirse —
sin tocar nada del código.
