#!/usr/bin/env bash

# Check if script is run with sudo
if [ "$EUID" -ne 0 ]; then
    echo -e "\e[31mThis script requires root privileges.\e[0m"
    exit 1
fi


# Paths
# Resolve real user
REAL_USER="${SUDO_USER:-$USER}"
REAL_HOME=$(eval echo "~$REAL_USER")

# Path of the freshly build Stakes
build_path="../build"

# Path to store the database and configuration
data_path="$REAL_HOME/.Stakes"

# User binaries path
usr_bin_path="$REAL_HOME/.local/bin"

# Path to store the binary
bin_path="$usr_bin_path"


# Install
echo "Initializing application folder..."
./"$build_path/Stakes" >/dev/null 2>&1 || true

echo "Moving application to $bin_path..."
mv -- "$build_path"/* "$bin_path/"

if sed -i -- "s|HOME_PATH|$data_path|g" ../data/config.json; then
  echo "Updated config.json with real data path"
else
  echo "Couldn't update config.json with real data path"
  echo "Please, update your configuration manually at"
  echo -- "$data_path/config.json"
fi

echo "Moving application data files to $data_path..."
[[ ! -d "$data_path" ]] && mkdir -- "$data_path" || true
mv -- ../data/* "$data_path"

echo "Symbolic linking application data files to application folder"
ln -sf -- "$data_path/config.json" "$bin_path"
ln -sf -- "$data_path/stakes.db" "$bin_path"

echo "Fixing ownership..."
chown -R "$REAL_USER:$REAL_USER" "$data_path"
chown -R "$REAL_USER:$REAL_USER" "$usr_bin_path"

if [[ -d "$usr_bin_path" ]]; then
  echo -e "Symbolic linking \e[34mStakes\e[0m into $usr_bin_path..."
  echo
  ln -sf -- "$bin_path/Stakes" "$usr_bin_path/stakes"
fi

echo -e "\e[32mInstallation finished 🙂\e[0m"
