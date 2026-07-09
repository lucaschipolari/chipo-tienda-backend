# Despliegue — Chipo Backend (Railway)

## 1. Crear el proyecto en Railway
1. Entrá a https://railway.app e iniciá sesión con GitHub.
2. **New Project → Deploy from GitHub repo** → elegí `chipo-tienda-backend`.
3. Railway detecta el `Dockerfile` y construye la API automáticamente.

## 2. Agregar la base de datos
1. En el proyecto: **New → Database → PostgreSQL**.
2. Railway crea la base y expone la variable `DATABASE_URL`.
3. En el servicio del backend → pestaña **Variables** → **Add Reference** → elegí `DATABASE_URL` de la base.
   (Así el backend recibe la conexión. El código la convierte solo al formato .NET.)

## 3. Variables de entorno del backend
En el servicio backend → **Variables**, agregá:

| Variable | Valor | Para qué |
|---|---|---|
| `DATABASE_URL` | *(referencia a la base)* | Conexión a PostgreSQL |
| `Jwt__Secret` | *(texto aleatorio de 40+ caracteres)* | Firma de los tokens. **Generá uno propio.** |
| `AllowedOrigins__0` | `https://TU-FRONTEND.vercel.app` | CORS: dominio del frontend |
| `ADMIN_EMAIL` | `tu-email@chipo.ar` | Usuario admin inicial |
| `ADMIN_PASSWORD` | *(una contraseña fuerte)* | Contraseña admin inicial |
| `ASPNETCORE_ENVIRONMENT` | `Production` | (ya viene en el Dockerfile) |

> Para generar el `Jwt__Secret`: cualquier texto largo y aleatorio sirve. Ej. en la terminal:
> `openssl rand -base64 48`

## 4. Al desplegar
- El backend aplica las **migraciones** solo (crea todas las tablas en la base vacía).
- Crea el **usuario admin** con `ADMIN_EMAIL` / `ADMIN_PASSWORD`.
- Railway te da una URL pública tipo `https://chipo-tienda-backend-production.up.railway.app`.

## 5. Después del deploy
1. Copiá la URL pública del backend → la vas a necesitar para el frontend (Vercel).
2. Entrá a `TU-BACKEND-URL/api/products` en el navegador → debería responder JSON (`200`).
3. Iniciá sesión en el admin y **cambiá la contraseña** si usaste una temporal.
