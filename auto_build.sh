#!/bin/bash

# Configuration
# Path to your Unity executable. Adjust version as needed.
UNITY_PATH="/Applications/Unity/Hub/Editor/6000.3.2f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(pwd)"

echo "----------------------------------------------------------------"
echo "Starting Hyperloop Dash Automation"
echo "Project Path: $PROJECT_PATH"
echo "----------------------------------------------------------------"

# Check if Unity exists
if [ ! -f "$UNITY_PATH" ]; then
    echo "ERROR: Unity executable not found at $UNITY_PATH"
    echo "Please edit auto_build.sh and set the correct UNITY_PATH."
    # Try generic find (Slow) or assume user knows
    exit 1
fi

# 1. Generate Content (Run HeadlessBuilder.GenerateGameContent)
echo "[1/2] Generating Game Content (Scenes, Prefabs, Materials)..."
"$UNITY_PATH" -quit -batchmode -projectPath "$PROJECT_PATH" -executeMethod HeadlessBuilder.GenerateGameContent -logFile unity_gen.log

if [ $? -eq 0 ]; then
    echo "Content Generation Complete."
else
    echo "Content Generation Failed! Check unity_gen.log"
    exit 1
fi

# 2. Build APK (Run HeadlessBuilder.PerformAndroidBuild)
echo "[2/2] Building Android APK..."
"$UNITY_PATH" -quit -batchmode -projectPath "$PROJECT_PATH" -executeMethod HeadlessBuilder.PerformAndroidBuild -logFile unity_build.log

if [ $? -eq 0 ]; then
    echo "----------------------------------------------------------------"
    echo "SUCCESS! "
    echo "APK should be at: $PROJECT_PATH/Build/HyperloopDash.apk"
    echo "----------------------------------------------------------------"
else
    echo "Build Failed! Check unity_build.log"
    exit 1
fi
