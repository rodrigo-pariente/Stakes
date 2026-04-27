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
bin_path="$data_path"


# Install
echo "Creating stakes directory..."
mkdir -p "$data_path"

echo "Moving application to $bin_path..."
mv -- "$build_path"/* "$bin_path/"

echo "Moving application data files..."
mv -- ../data/* "$data_path" 2>/dev/null || true

echo "Fixing ownership..."
chown -R "$REAL_USER:$REAL_USER" "$data_path"
chown -R "$REAL_USER:$REAL_USER" "$usr_bin_path"

echo "Linking executable..."
ln -sf -- "$bin_path/Stakes" "$usr_bin_path/stakes"

echo -e "\e[32mInstallation finished 🙂\e[0m"
