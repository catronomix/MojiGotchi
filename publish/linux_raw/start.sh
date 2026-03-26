#!/bin/bash
# Get the directory where the script is located
HERE=$(dirname "$(readlink -f "$0")")
cd "$HERE"
# Ensure the game has execution permissions
chmod +x "./MojiGotchi"
# Run the game
"./MojiGotchi"
