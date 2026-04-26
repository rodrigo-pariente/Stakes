#!/usr/bin/env bash

if ! command -v dotnet >/dev/null 2>&1; then 
  echo -e "\e[31mThe build requires the .NET SDK installed on your system\e[0m"
fi

# Build
dotnet publish ../src/      \
  -o ../build/              \
  -c Release                \
  --self-contained          \
  /p:PublishSingleFile=true \
  /p:PublishReadyToRun=true
  # /p:PublishAot=true # Breaking Microsoft.Data.Sqlite
