# MojiGotchi

MojiGotchi is a charming console-based Tamagotchi-style pet simulation game built with C#. Dive into a nostalgic experience where you adopt and care for your very own digital companion, right within your terminal!

## Disclaimer

- The game is currently localized in **Dutch** and **English**.
- This project was created with the assistance and tutoring of AI tools.

---

## Features

- **Console-Based Graphics:** Experience a retro aesthetic with ASCII art and ANSI color graphics.
- **Pet Care Simulation:** Keep your MojiGotchi happy and healthy by managing its core stats:
  - **Saturation:** Keep it fed!
  - **Energy:** Ensure it gets enough rest and play.
  - **Mood:** A happy pet is a healthy pet!
  - **Sleepiness:** Manage its sleep cycle.
- **Interactive Actions:** Feed, play with, pet, and wake up your MojiGotchi through a simple menu system.
- **Minigame** Collect all the coins to give your pet a better mood.
- **Dynamic Environment:** Your pet wanders around a customizable level, interacting with its surroundings.
- **Life Cycle:** Watch your MojiGotchi grow, but be careful – neglect can lead to unfortunate outcomes!
- **High Scores:** Track your best pet-parenting achievements.
- **Save & Load:** Your MojiGotchi's progress is saved, allowing you to pick up where you left off.
- **Randomization:** Each new MojiGotchi gets a unique name and color scheme.
- **Help & Information:** Access in-game help and view high scores through dedicated modal windows.

## How to Play

Navigate the game using the `Up Arrow` and `Down Arrow` keys to select menu options. Press `Enter` to confirm your selection and perform an action. If a modal window (like Help or High Scores) is open, press `Escape` to close it. Keep an eye on your MojiGotchi's stats and respond to its needs to ensure a long and happy life.

## Build Instructions (tested on Windows only)

This game is developed on Windows. The provided PowerShell scripts allow you to build optimized, self-contained versions for multiple platforms.

**Note:** This project is provided "as-is" without warranty of any kind.

### Windows (x64)

1.  **Build:** Run the Windows build script in PowerShell:
    ```powershell
    ./build-win-x64.ps1
    ```
2.  **Run:** Navigate to the `publish/windows/` directory and run `MojiGotchi.exe`.

### macOS (Apple Silicon / osx-arm64)

1.  **Build:** Run the macOS build script in PowerShell:
    ```powershell
    ./build-mac.ps1
    ```
2.  **Run:** This creates a `MojiGotchi.app` bundle in the `publish/` folder.
    - To bypass "Unidentified Developer" warnings, right-click the app and select **Open**.
    - The app uses a `play.command` script to launch the terminal environment correctly.

### Linux (x64)

1.  **Build:** Run the Linux build script in PowerShell:
    ```powershell
    ./build-linux-x64.ps1
    ```
2.  **Extract:** Extract the generated archive on your Linux machine:
    ```bash
    tar -xzf publish/MojiGotchi-linux.tar.gz -C /path/to/directory
    ```
3.  **Run:** Execute the launcher script:
    ```bash
    ./start.sh
    ```
    Ensure the script has permissions: `chmod +x start.sh`.

Enjoy caring for your MojiGotchi!
