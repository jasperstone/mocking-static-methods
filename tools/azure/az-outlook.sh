#!/usr/bin/env bash
set -euo pipefail
export AZURE_CONFIG_DIR="${AZURE_CONFIG_DIR:-$HOME/.azure-outlook}"
az account set --subscription "9490eefa-f2af-4485-983f-63397bfb5386" >/dev/null
az "$@"
