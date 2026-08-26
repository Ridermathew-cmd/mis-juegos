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

## Requisitos para compilar

Igual que el launcher de Fortnite: .NET SDK 8 (`winget install
Microsoft.DotNet.SDK.8`).

## Compilar y ejecutar

```
dotnet run
```

## Publicar un .exe para distribuir

```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

El resultado queda en `bin\Release\net8.0-windows\win-x64\publish\`. Igual
que con Fortnite, hay que llevarse toda la carpeta (no solo el .exe) porque
WinForms necesita algunas DLLs nativas al lado.
