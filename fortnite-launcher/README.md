# Fortnite Launcher Ligero

Launcher liviano para PC de bajos recursos. No reemplaza a Epic Games Launcher
(Fortnite lo necesita para el anti-cheat), sino que lo usa por detras via su
protocolo (`com.epicgames.launcher://`) para abrir Fortnite directo, sin pasar
por la pantalla principal de la tienda.

## Interfaz

Tema oscuro con barra lateral de navegación, al estilo Epic Games Launcher
(fondo negro, boton "JUGAR" blanco solido). Se logra solo con estilos de
WinForms (colores, botones planos); no usa ni redistribuye ningun asset
grafico de Epic/Fortnite. La barra de titulo de la ventana la sigue dibujando
Windows (no se puede re-temear sin una ventana sin bordes hecha a medida).

## Que hace

- Detecta si Epic Games Launcher y Fortnite estan instalados (lee los
  manifiestos en `C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests`).
  No hace falta tenerlos instalados de antes:
  - Si falta **Epic Games Launcher**, el boton principal cambia a "Instalar
    Epic Games Launcher": descarga el instalador oficial (mismo archivo que
    entrega epicgames.com) y lo abre para que lo instales vos mismo, viendo
    sus propios terminos.
  - Si falta **Fortnite** (pero Epic Games Launcher si esta), el boton pasa a
    "Instalar Fortnite": abre Epic Games Launcher directo en la pagina de
    Fortnite para que le des a instalar ahi. No se puede saltar este paso:
    Fortnite usa anti-cheat (Easy Anti-Cheat) y controles de cuenta que solo
    Epic puede instalar/verificar, asi que la descarga del juego en si
    siempre la maneja Epic.
  - Si los dos ya estan, el boton es "JUGAR" y abre Fortnite directo.
- Panel de mandos conectados: detecta mandos Xbox/XInput y cualquier otro
  mando USB/Bluetooth reconocido por Windows (PlayStation, Switch Pro,
  genericos). Si un mando no es XInput, avisa que puede necesitar
  **DS4Windows** (PS4/PS5) o **BetterJoy** (Switch/Switch Pro) para que
  Fortnite lo reconozca como si fuera un mando Xbox.
- Pestaña "Promociones": muestra los juegos gratis actuales y el proximo de
  Epic Games Store (mismo endpoint publico que usa la tienda oficial), con
  boton para abrir la ficha del juego en el navegador. Se actualiza al abrir
  el launcher y con el boton "Actualizar". No descarga imagenes para
  mantenerse liviano.
- **Calidad grafica (Rendimiento / Calidad / No cambiar)**: antes de abrir el
  juego, ajusta las mismas claves de configuracion (`sg.ViewDistanceQuality`,
  `sg.ShadowQuality`, etc.) que Fortnite escribe en su propio
  `GameUserSettings.ini` cuando cambiás la calidad desde el menu de video del
  juego. "Rendimiento" pone todo al minimo para PCs debiles, "Calidad" al
  maximo. Si Fortnite ya esta abierto al hacer clic en Jugar, el cambio no se
  aplica (avisa en pantalla) porque el juego sobreescribe ese archivo al
  cerrarse.

### Herramientas de rendimiento (pestaña "Rendimiento")

Todas vienen **activadas por defecto** para maximizar rendimiento en equipos
de bajos recursos; se pueden desactivar individualmente si alguna no hace
falta.

- **Subir prioridad del proceso de Fortnite**: apenas arranca el juego, le
  sube la prioridad a `High` para que el sistema le de mas CPU. (No se usa
  `Realtime`: en equipos de bajos recursos puede colgar el mouse/teclado, asi
  que se evita a proposito).
- **Plan de energia Maximo rendimiento**: intenta activar el plan oculto de
  Windows **"Ultimate Performance"** (mas agresivo que "Alto rendimiento",
  evita que Windows reduzca la frecuencia del CPU para ahorrar energia). Si
  el equipo no lo tiene disponible, usa "Alto rendimiento" como respaldo.
  Al cerrar Fortnite, vuelve a poner el plan que tenias antes.
- **Liberar RAM de otros procesos**: vacia la memoria en espera de los demas
  procesos del usuario antes de lanzar el juego (similar a un "RAM cleaner").
- **Cerrar apps pesadas en segundo plano**: detecta si hay apps conocidas
  corriendo (Chrome, Edge, Discord, Spotify, Teams, Slack, OneDrive, etc.),
  las lista para que elijas cuales cerrar, pide confirmacion y les pide que
  cierren su ventana (como si apretaras la X, para que puedan guardar
  cambios antes de salir).
- **Modo Juego de Windows**: boton para activar/desactivar la configuracion
  de Windows `AutoGameModeEnabled` (ajuste general del sistema, no se aplica
  ni revierte por partida).
- **Grabacion en segundo plano (Game DVR)**: boton para desactivar la
  grabacion de fondo de Xbox Game Bar, que consume CPU/RAM aunque no estes
  grabando nada. Tambien es un ajuste general del sistema, persistente.

Nota: si cerrás el launcher mientras Fortnite sigue abierto, el plan de
energia no se restaura automaticamente (la app que lo hace ya se cerro).
En ese caso se puede volver al plan anterior a mano con
`powercfg /setactive <GUID>` o desde la Configuracion de Windows.

## Requisitos para compilar

1. Instalar el .NET SDK 8 (no alcanza con el runtime):
   ```
   winget install Microsoft.DotNet.SDK.8
   ```
   o descargarlo de https://dotnet.microsoft.com/download/dotnet/8.0
2. Verificar la instalacion:
   ```
   dotnet --list-sdks
   ```

## Compilar y ejecutar

Desde esta carpeta (`fortnite-launcher`):

```
dotnet run
```

## Publicar un .exe unico y liviano

Para generar un ejecutable que corra sin necesidad de tener el SDK/runtime
instalado en la PC de destino:

```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:EnableCompressionInSingleFile=true
```

El .exe queda en `bin\Release\net8.0-windows\win-x64\publish\`.

Si en la PC de destino ya hay un .NET runtime instalado (o no importa que
dependa de el), se puede publicar sin `--self-contained` para un archivo mucho
mas chico:

```
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

## Notas

- Requiere Windows. Epic Games Launcher y Fortnite pueden no estar instalados
  todavia: el launcher te guia para instalarlos (ver arriba), pero la
  instalacion en si siempre corre a traves de las herramientas oficiales de
  Epic, con sus propios terminos y elevacion de Windows (UAC).
- El perfil de rendimiento solo cambia la prioridad del proceso; no modifica
  archivos del juego ni el anti-cheat.
