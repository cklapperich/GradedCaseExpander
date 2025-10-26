#!/bin/bash

# Store the script's directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Get current user dynamically
CURRENT_USER="$(whoami)"

# Define paths relative to script directory
SOURCE_DLL="$SCRIPT_DIR/bin/Debug/netstandard2.1/GradedCaseExpander.dll"
BEPINEX_PLUGINS="$SCRIPT_DIR/BepInEx/plugins/GradedCaseExpander/GradedCaseExpander.dll"
STEAM_PLUGINS="/home/$CURRENT_USER/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/BepInEx/plugins/GradedCaseExpander/GradedCaseExpander.dll"
EXAMPLE_ZIP="$SCRIPT_DIR/GradedCaseExpander-example.zip"
FOR_USERS_ZIP="$SCRIPT_DIR/GradedCaseExpander-for-users.zip"
FOR_USERS_DIR="$SCRIPT_DIR/for_users"
MOD_FOLDER="$SCRIPT_DIR/BepInEx"

# Echo paths for verification
echo "[INFO] Current directory: $(pwd)"
echo "[INFO] Script directory: $SCRIPT_DIR"
echo "[INFO] Current user: $CURRENT_USER"
echo "[INFO] Source DLL path: $SOURCE_DLL"
echo "[INFO] BepInEx plugins path: $BEPINEX_PLUGINS"
echo "[INFO] Steam plugins path: $STEAM_PLUGINS"

# Build the project
echo "[BUILD] Building project..."
dotnet build GradedCaseExpander.csproj
if [ $? -ne 0 ]; then
    echo "[ERROR] Build failed!"
    exit 1
fi

# Verify source file exists
if [ ! -f "$SOURCE_DLL" ]; then
    echo "[ERROR] Source DLL not found at: $SOURCE_DLL"
    exit 1
fi

# Create directories if they don't exist
echo "[INFO] Creating directories..."
mkdir -p "$(dirname "$BEPINEX_PLUGINS")"
if [ $? -ne 0 ]; then
    echo "[ERROR] Failed to create directory: $(dirname "$BEPINEX_PLUGINS")"
    exit 1
fi

mkdir -p "$(dirname "$STEAM_PLUGINS")"
if [ $? -ne 0 ]; then
    echo "[ERROR] Failed to create directory: $(dirname "$STEAM_PLUGINS")"
    exit 1
fi

# Copy DLL to local BepInEx folder
echo "[INFO] Copying DLL to local BepInEx folder..."
cp "$SOURCE_DLL" "$BEPINEX_PLUGINS"
if [ $? -ne 0 ]; then
    echo "[ERROR] Local BepInEx folder copy failed!"
    echo "[DEBUG] Source: $SOURCE_DLL"
    echo "[DEBUG] Destination: $BEPINEX_PLUGINS"
    exit 1
fi

# Copy entire BepInEx folder to Steam directory
echo "[INFO] Checking Steam directory..."
STEAM_GAME_DIR="/home/$CURRENT_USER/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator"
STEAM_BEPINEX_DIR="$STEAM_GAME_DIR/BepInEx"

if [ ! -d "$STEAM_GAME_DIR" ]; then
    echo "[WARNING] Steam game directory not found. Is the game installed?"
    read -p "Do you want to continue without copying to Steam directory? (y/N): " choice
    case "$choice" in
        y|Y ) echo "[INFO] Continuing without Steam copy...";;
        * ) exit 1;;
    esac
else
    # Copy entire BepInEx folder to Steam with enhanced error checking
    echo "[INFO] Copying BepInEx folder to Steam..."

    # Test write permissions first
    if ! touch "$STEAM_GAME_DIR/.test" 2>/dev/null; then
        echo "[ERROR] No write permission to Steam directory. Try running with sudo or check permissions."
        read -p "Do you want to continue without copying to Steam directory? (y/N): " choice
        case "$choice" in
            y|Y ) echo "[INFO] Continuing without Steam copy...";;
            * ) exit 1;;
        esac
    else
        rm -f "$STEAM_GAME_DIR/.test"

        # Create BepInEx directory structure in Steam if it doesn't exist
        mkdir -p "$STEAM_BEPINEX_DIR"

        # Copy the entire BepInEx folder contents
        cp -r "$MOD_FOLDER"/* "$STEAM_BEPINEX_DIR/"
        if [ $? -ne 0 ]; then
            echo "[ERROR] Failed to copy BepInEx folder to Steam directory!"
            exit 1
        else
            echo "[INFO] Successfully copied BepInEx folder to Steam directory"
        fi
    fi
fi

# Create example zip (full package with BepInEx folder structure)
echo "[INFO] Creating example zip file..."
if [ -f "$EXAMPLE_ZIP" ]; then
    rm "$EXAMPLE_ZIP"
fi

if command -v zip >/dev/null 2>&1; then
    cd "$SCRIPT_DIR"
    zip -r "$EXAMPLE_ZIP" "BepInEx/"
    if [ $? -ne 0 ]; then
        echo "[ERROR] Example zip creation failed!"
        echo "[DEBUG] Source: $MOD_FOLDER"
        echo "[DEBUG] Destination: $EXAMPLE_ZIP"
        exit 1
    fi
    echo "[SUCCESS] Created $EXAMPLE_ZIP"
else
    echo "[ERROR] 'zip' command not found. Please install zip package."
    exit 1
fi

# Create for-users zip (just the DLL)
echo "[INFO] Creating for-users zip file..."
if [ -f "$FOR_USERS_ZIP" ]; then
    rm "$FOR_USERS_ZIP"
fi

# Create temporary directory structure for for-users zip
mkdir -p "$FOR_USERS_DIR/BepInEx/plugins/GradedCaseExpander"
cp "$SOURCE_DLL" "$FOR_USERS_DIR/BepInEx/plugins/GradedCaseExpander/"
if [ $? -ne 0 ]; then
    echo "[ERROR] Failed to copy DLL to for_users directory!"
    exit 1
fi

cd "$FOR_USERS_DIR"
zip -r "$FOR_USERS_ZIP" "BepInEx/"
if [ $? -ne 0 ]; then
    echo "[ERROR] For-users zip creation failed!"
    echo "[DEBUG] Destination: $FOR_USERS_ZIP"
    exit 1
fi

# Clean up temporary structure
rm -rf "$FOR_USERS_DIR/BepInEx"
cd "$SCRIPT_DIR"
echo "[SUCCESS] Created $FOR_USERS_ZIP"

# Move log file if it exists
echo "[INFO] Checking for BepInEx log file..."
BEPINEX_LOG="$STEAM_GAME_DIR/BepInEx/LogOutput.log"
LOCAL_LOG="$SCRIPT_DIR/LogOutput.log"

if [ -f "$BEPINEX_LOG" ]; then
    echo "[INFO] Found BepInEx log file, moving to local directory..."
    mv "$BEPINEX_LOG" "$LOCAL_LOG"
    if [ $? -eq 0 ]; then
        echo "[INFO] Log file moved to: $LOCAL_LOG"
    else
        echo "[WARNING] Failed to move log file"
    fi
else
    echo "[INFO] No BepInEx log file found at: $BEPINEX_LOG"
fi

echo "[SUCCESS] Build complete!"