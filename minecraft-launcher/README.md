# Minecraft Launcher Ligero

Launcher liviano para PC de bajos recursos, hermano del
[Fortnite Launcher Ligero](../fortnite-launcher/). Detecta y abre Minecraft
**Java Edition** y/o **Bedrock Edition**, y trae las mismas herramientas de
rendimiento y limpieza de Windows.

## Que hace

- Detecta si tenes **Java Edition** instalado (Minecraft Launcher oficial,
  en `Program Files\Minecraft Launcher\` o la version de Microsoft Store) y
  si tenes **Bedrock Edition** (Microsoft Store). Si falta alguna, el boton
  correspondiente te lleva a instalarla por el canal oficial
  (`minecraft.net` para Java, Microsoft Store para Bedrock) — no se instala
  nada por nuestra cuenta.
- Boton "JUGAR" independiente por cada edicion que tengas instalada.
- Herramientas de rendimiento (checkboxes activados por defecto):
  - Subir prioridad del proceso — **solo aplica a Bedrock**
    (`Minecraft.Windows.exe`, nombre de proceso estable). Java Edition corre
    como `javaw.exe`, un nombre generico que comparten otras apps de Java,
    asi que no es seguro identificarlo ni subirle la prioridad sin arriesgar
    tocar el proceso equivocado.
  - Plan de energia Maximo rendimiento (Ultimate Performance con respaldo a
    Alto rendimiento). En Bedrock se revierte solo al cerrar el juego; en
    Java Edition se avisa que hay que revertirlo a mano (por la misma razon
    del proceso generico de arriba).
  - Liberar RAM de otros procesos antes de jugar.
  - Cerrar apps pesadas en segundo plano (con confirmacion).
- Modo Juego de Windows y Game DVR: toggles persistentes del sistema.
- Limpieza de Windows: borra temporales y opcionalmente vacia la papelera de
  reciclaje.
- Pantalla completa (F11 o boton), ventana redimensionable.
- **Fondo tipo "vidrio esmerilado"** (estilo Liquid Glass de iOS 26): una
  imagen propia con paisaje en bloques (cielo, sol, colinas, arboles —
  dibujada a mano en [generate_background.ps1](generate_background.ps1), sin
  usar texturas ni assets reales de Minecraft) con blur y un velo oscuro
  encima para que el texto siga siendo legible. Los titulos de cada seccion
  tienen colores distintos (azul, naranja, violeta, verde) para darle un
  toque multicolor sin depender de un blur real del escritorio.
- Si la app llegara a crashear, escribe el detalle en `crash.log` al lado
  del .exe (util para diagnosticar problemas reportados).

## Requisitos para compilar

Igual que el launcher de Fortnite: .NET SDK 8 (`winget install
Microsoft.DotNet.SDK.8`).

## Compilar y ejecutar

```
dotnet run
```

## Publicar el instalador para distribuir

A diferencia de Fortnite (que se distribuye como .zip), este se empaqueta
como un instalador real con [Inno Setup](https://jrsoftware.org/isinfo.php)
(gratis, `winget install JRSoftware.InnoSetup`), asi que quien lo descarga
recibe un solo `.exe` que instala, crea accesos directos y se puede
desinstalar desde "Agregar o quitar programas" — no una carpeta suelta.

1. Publicar el build:
   ```
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
   ```
   Borrar cualquier `.pdb` o `.zip` viejo que haya quedado en
   `bin\Release\net8.0-windows\win-x64\publish\` antes del siguiente paso
   (Inno Setup empaqueta todo lo que encuentre en esa carpeta).

2. Compilar el instalador:
   ```
   cd installer
   iscc setup.iss
   ```
   (`iscc` es `ISCC.exe` de Inno Setup; si no esta en el PATH, usar la ruta
   completa, ej. `C:\Users\<usuario>\AppData\Local\Programs\Inno Setup 6\ISCC.exe`)

   El resultado queda en `bin\installer\MinecraftLauncherLigero-Setup.exe`.
   Se instala por usuario (sin pedir admin) en
   `%LocalAppData%\Programs\Minecraft Launcher Ligero`.

3. Copiar ese `.exe` a
   `minecraft-launcher-web/downloads/MinecraftLauncherLigero-Setup.exe`.

Si se quiere subir la version, editar `AppVersion` en
[installer/setup.iss](installer/setup.iss).
