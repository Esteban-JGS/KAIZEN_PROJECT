#!/usr/bin/env bash
# Build del playground en Vercel (ver vercel.json en la raíz del repositorio).
# La imagen de build de Vercel no trae .NET: se instala el SDK fijado en KaizenLang/global.json y se
# publica el proyecto Blazor WebAssembly. El resultado estático queda en dist/wwwroot, que es lo que Vercel sirve.
set -euo pipefail

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
# La imagen de Vercel no incluye ICU; el SDK funciona en modo de globalización invariante.
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

REPO_ROOT="$(pwd)"
GLOBAL_JSON="${REPO_ROOT}/KaizenLang/global.json"
DOTNET_DIR="${HOME}/.dotnet"

# La descarga del SDK (~200 MB) puede cortarse por la red del build ("curl: (56) Send failure:
# Broken pipe" hizo fallar un deploy de producción). Se reintenta con espera antes de rendirse.
install_dotnet() {
  curl -sSL --retry 5 --retry-delay 3 --retry-all-errors https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  for attempt in 1 2 3; do
    if bash /tmp/dotnet-install.sh --jsonfile "${GLOBAL_JSON}" --install-dir "${DOTNET_DIR}"; then
      return 0
    fi
    echo "Falló la instalación del SDK de .NET (intento ${attempt} de 3); reintentando en $((attempt * 10)) s..."
    rm -rf "${DOTNET_DIR}"
    sleep $((attempt * 10))
  done
  echo "No se pudo instalar el SDK de .NET después de 3 intentos." >&2
  return 1
}

if [ ! -x "${DOTNET_DIR}/dotnet" ]; then
  install_dotnet
fi

# dotnet busca global.json desde el directorio actual hacia arriba: se compila desde KaizenLang/.
cd "${REPO_ROOT}/KaizenLang"
echo "SDK de .NET: $("${DOTNET_DIR}/dotnet" --version)"
"${DOTNET_DIR}/dotnet" publish src/KaizenLang.Web/KaizenLang.Web.csproj -c Release -o "${REPO_ROOT}/dist"
