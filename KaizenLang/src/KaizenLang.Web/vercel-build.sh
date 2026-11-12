#!/usr/bin/env bash
# Build del playground en Vercel (ver vercel.json en la raíz del repositorio).
# La imagen de build de Vercel no trae .NET: se instala el SDK 9 y se publica el proyecto Blazor WebAssembly.
# El resultado estático queda en dist/wwwroot, que es lo que Vercel sirve.
set -euo pipefail

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
# La imagen de Vercel no incluye ICU; el SDK funciona en modo de globalización invariante.
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

DOTNET_DIR="${HOME}/.dotnet"
if [ ! -x "${DOTNET_DIR}/dotnet" ]; then
  curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 9.0 --install-dir "${DOTNET_DIR}"
fi

"${DOTNET_DIR}/dotnet" --info | head -n 8
"${DOTNET_DIR}/dotnet" publish KaizenLang/src/KaizenLang.Web/KaizenLang.Web.csproj -c Release -o dist
