# ECMS — Engineering Equipment Control & Monitoring System
 
A desktop application for real-time monitoring and control of engineering equipment. Diploma project, Software Engineering.
 
![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Framework](https://img.shields.io/badge/.NET_Framework-4.7%2B-purple)
![Language](https://img.shields.io/badge/language-C%23-239120)
![Architecture](https://img.shields.io/badge/architecture-MVVM-orange)
 
---
 
## What it does
 
ECMS lets you add motors and pumps, start and stop them, track the temperature and state of each device in real time, and configure cooling links between motors and pumps.
 
Key scenarios:
 
- A motor heats up while running (proportional to speed). When temperature reaches the **Warning** threshold (180 °C) the event is logged. At the **Alarm** threshold (200 °C) the motor stops automatically and transitions to the **Fault** state.
- Linked pumps cool the motor — each additional pump is less effective (damping factor 0.5).
- Changing `MaxSpeed` while the motor is running transitions it to **Needs Restart**, after which it restarts automatically with the new settings.
- All events (Warning, Alarm, start/stop) are recorded in the event log with a timestamp.
---
 
## Tech stack
 
| Layer | Technology |
|---|---|
| Language | C# (.NET Framework 4.7+) |
| UI | WPF (XAML) |
| Architecture | MVVM |
| Async | async/await, CancellationToken |
| UI timer | DispatcherTimer (500 ms tick) |
| Serialization | System.Text.Json |
 
---
 
## Architecture & GoF design patterns
 
The project implements 7 GoF design patterns:
 
**State** — motor states (Stopped → Starting → Running → Stopping → Fault / Needs Restart) are extracted into separate classes under `Folder_States/`. Each state decides itself what to do on `Start()` / `Stop()`.
 
**Factory Method** — `DeviceFactory` creates devices via the `IDeviceFactory` interface, making it easy to add new device types.
 
**Builder** — `MotorBuilder` constructs a `MotorConfig` step by step (name → MaxSpeed → linked pumps).
 
**Memento** — `DeviceSnapshot` + `SnapshotManager` save and restore device state.
 
**Observer** — `MotorDevice` raises `StateChanged`, `DataChanged`, `WarningTriggered`, `AlarmTriggered` events; the ViewModel subscribes and updates the UI.
 
**Singleton** — `EventLogger.Instance` is the single point of entry for all event logging.
 
**Command** — `RelayCommand` implements `ICommand` to bind UI buttons to ViewModel methods without code-behind.
 
---
 
## Project structure
 
```
ECMS/
├── ECMS.sln
└── ECMS/
    ├── Folder_Core/              # Business logic (no WPF dependency)
    │   ├── Folder_Factories/     # DeviceFactory, IDeviceFactory
    │   ├── Folder_Interfaces/    # IDevice, IDialogService
    │   ├── Folder_States/        # 6 motor states + IDeviceStates
    │   ├── DeviceController.cs   # DispatcherTimer, device update loop
    │   ├── EventLogger.cs        # Singleton event logger
    │   ├── MotorDevice.cs        # Motor: speed, temperature, state machine
    │   ├── MotorBuilder.cs       # Builder for MotorConfig
    │   ├── MotorConfig.cs        # Motor configuration DTO
    │   ├── PumpDevice.cs         # Pump: motor cooling logic
    │   ├── DeviceSnapshot.cs     # Memento: device state snapshot
    │   └── SnapshotManager.cs    # Save / restore snapshots
    └── Folder_UI/
        ├── Folder_Commands/      # RelayCommand (ICommand)
        ├── Folder_Services/      # DialogService (IDialogService)
        ├── Folder_ViewModels/    # MainViewModel, MotorViewModel, PumpViewModel...
        └── Folder_Views/         # MainWindow, SettingWindow, LinkPumpWindow (XAML)
```
 
---
 
## Getting started
 
**Requirements:** Visual Studio 2022 (or 2019), .NET Framework 4.7+, Windows.
 
1. Clone the repository:
```bash
   git clone https://github.com/YOUR_USERNAME/ecms-wpf.git
```
2. Open `ECMS/ECMS.sln` in Visual Studio.
3. Press **F5** — NuGet packages will restore automatically.
> The application is Windows-only due to its dependency on WPF.
 
---
 
## Screenshots
 
<!-- Add screenshots after first launch: Win+Shift+S, save to docs/screenshots/ -->
_Screenshots coming soon._
 
---
 
## Author
 
**Roman Mishchenko** — Junior .NET / C# Developer  
Diploma project, Kyiv College of Computer Technologies and Economics, NAU, 2026
