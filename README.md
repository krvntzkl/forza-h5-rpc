![Forza Horizon 5 RPC Banner](assets/forza-h5-rpc-banner.png)

[![Discord][discord-shield]][discord-url]
[![Stars][stars-shield]][stars-url]
[![Releases][releases-shield]][releases-url]
[![Language][language-shield]][language-url]
[![License][license-shield]][license-url]

> This repository is an **independently maintained fork** of [jaaiden/FH5RP](https://github.com/jaaiden/FH5RP), with additional ideas from [Artprozew/FH5RP](https://github.com/Artprozew/FH5RP) and various telemetry resources.  
> It focuses on a more feature‑rich and user‑friendly Discord Rich Presence for **Forza Horizon 5**.

## Shortcut

- [About](#about)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Support](#support)
- [Credits](#credits)
- [Disclaimer](#disclaimer)


## About

Forza H5 RPC lets you show **detailed in‑game information** from Forza Horizon 5 directly in your Discord profile:

- current car (name, class, PI, drivetrain),
- live speed,
- and a smart status that switches between **“in race”** and **free roam** based on telemetry.

It is built on top of the original FH5RP project, but modernized and tuned for a nicer experience (multi‑language, better state detection, safer SignalR handling, etc.).


## Features

- **Localized rich presence**
  - Selectable language via `appsettings.json` (`PresenceLanguage`).
  - Currently supported:
    - `"fr"` → French texts (`Conduit …`, `En course`, `Explore le Mexique`, etc.)
    - `"en"` → English texts (`Driving …`, `In race`, `Exploring México`, etc.).

- **Smart vehicle information**
  - Uses the official‑style ID list from [ForzaMods/fh5idlist](https://github.com/ForzaMods/fh5idlist) to resolve the **exact car name** (e.g. `2018 McLaren Senna`).
  - If a car ID is unknown, falls back to a localized description such as:
    - French: `une voiture 8 cylindres` / `une voiture`
    - English: `a 8-cylinder car` / `a car`
  - Shows **class, PI and drivetrain** in the small image tooltip  
    (e.g. `S1 | 800 (AWD)`).

- **Improved race / free‑roam detection**
  - Uses a combination of telemetry fields (`InRace`, `LapNumber`, `RacePosition`, `TotalRaceTime`) instead of a single flag.
  - Only considers you **“in race”** when the data looks realistic:
    - `1 ≤ LapNumber ≤ 200`
    - `1 ≤ RacePosition ≤ 24`
  - If telemetry sends absurd values (very high lap/position), the presence safely falls back to **free roam** (`Explore le Mexique` / `Exploring México`).

- **Stable session timer**
  - The green timer in Discord uses a **stable session start timestamp**:
    - based on the ForzaHorizon5 process start time when possible,
    - or `UtcNow` as a fallback.
  - Presence updates **never reset the timer** when your speed or state changes.

- **More robust web UI connection**
  - Blazor pages (`Index`, debug telemetry page) connect to `http://localhost:5000/datahub` with:
    - automatic reconnect,
    - try/catch around `StartAsync` to avoid unhandled exceptions like  
      `WebSockets failed: A task was canceled`.
  - If the hub fails, the app logs the error instead of crashing the circuit.


## Installation

- Download the latest **Forza H5 RPC** release from:  
  [`https://github.com/krvntzkl/forza-h5-rpc/releases`](https://github.com/krvntzkl/forza-h5-rpc/releases)
- Extract the `.zip` to a folder of your choice.
- Run `FH5RP.exe`.


## Usage

- **Step 1 – Enable FH5 “Data Out”**
  - In Forza Horizon 5, go to **Settings → HUD & Gameplay → Data Out**.
  - Enable data out and set:
    - **IP address**: `127.0.0.1`
    - **Port**: `9909`
  - Confirm and restart the race/free roam if needed.
  - You can use the following in‑game settings as a reference:  
    ![Data out settings](/wwwroot/img/dataoutsettings.png?raw=true)

- **Step 2 – Start the RPC**
  - Launch `FH5RP.exe`.
  - A console window should show that:
    - the **listen server** is running on `127.0.0.1:9909`,
    - the **web app** is listening on `http://localhost:5000`,
    - Discord RPC is connected.

- **Step 3 – Play**
  - Start Forza Horizon 5 (if not already running).
  - Once the game sends telemetry, Discord should update:
    - car name / description,
    - speed in MPH and KPH,
    - race vs free roam status.

- (Optional) Open a browser at [`http://localhost:5000`](http://localhost:5000) to see live telemetry cards (speed, PI, position, etc.).


## Configuration

- **Language**

  - Open `appsettings.json` in the same folder as `FH5RP.exe` (or in the project root if you build from source).
  - Set:

    ```json
    "PresenceLanguage": "fr"
    ```

    or

    ```json
    "PresenceLanguage": "en"
    ```

  - Restart `FH5RP.exe` to apply changes.

- **Building from source**

  - Requirements:
    - .NET 6 SDK
  - From the project root:

    ```bash
    dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true -o "./FH5RP-Win64"
    ```

  - Run `FH5RP.exe` from the `FH5RP-Win64` folder.


## Support

Either open an issue on this repository or join the Discord:

[![Discord Banner 2][discord-banner]][discord-url]


## Credits

- **Original FH5RP**:  
  [jaaiden/FH5RP](https://github.com/jaaiden/FH5RP) – initial ASP.NET + Discord RPC implementation.
- **Additional ideas / experiments**:  
  [Artprozew/FH5RP](https://github.com/Artprozew/FH5RP)
- **Car ID list**:  
  [ForzaMods/fh5idlist](https://github.com/ForzaMods/fh5idlist)
- **CSS framework**:  
  Bulma (used by the original FH5RP UI).


## Disclaimer

This project is not affiliated with Playground Games, Turn 10 Studios, Xbox Game Studios or any of their employees and therefore does not reflect the views of said parties.

Forza Horizon and Forza Motorsport, and all associated properties are trademarks or registered trademarks of Microsoft Corporation and/or its affiliates.  
They do not endorse or sponsor this project.

---

[discord-shield]: https://img.shields.io/discord/938509236906917982?color=7289da&label=Support&logo=discord&logoColor=7289da&style=for-the-badge
[discord-url]: https://discord.gg/8SRNkCGDjk
[discord-banner]: https://discordapp.com/api/guilds/938509236906917982/widget.png?style=banner2

[license-shield]: https://img.shields.io/github/license/krvntzkl/forza-h5-rpc?style=for-the-badge
[license-url]: https://github.com/krvntzkl/forza-h5-rpc/blob/main/LICENSE

[stars-shield]: https://img.shields.io/github/stars/krvntzkl/forza-h5-rpc?logo=github&style=for-the-badge
[stars-url]: https://github.com/krvntzkl/forza-h5-rpc/stargazers

[releases-shield]: https://img.shields.io/github/downloads/krvntzkl/forza-h5-rpc/total?style=for-the-badge
[releases-url]: https://github.com/krvntzkl/forza-h5-rpc/releases

[language-shield]: https://img.shields.io/github/languages/top/krvntzkl/forza-h5-rpc?logo=dotnet&logoColor=white&style=for-the-badge
[language-url]: https://dotnet.microsoft.com/
