# Using Unity ML-Agents
See documentation [here](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Installation.html) for details

## Running a training session
1. Open an agent training scene
2. Start the mlagents in a terminal using:
```bash
mlagents-learn .\AgentConfigs\BasicAgent.yaml --run-id=test_run
```
3. Press play in the editor to start the training session

Afterwards, the resulting `model.onnx` can be put on the Behavior Parameters component of the agent to use the trained model.

Training metrics can be visualized using Tensorboard with the command:
```bash
tensorboard --logdir results
```

You can test in parallel with multiple Unity instances using `--num-envs=x` flag in the `mlagents-learn` command. See docs [here](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Learning-Environment-Create-New.html#optional-training-using-concurrent-unity-instances).

# Metroidvania

## Introduction

Unity Metroidvania is a metroidvania game written in C# using the Unity Engine.
Straightforward.

> [!NOTE]
> You can check the older version (for Unity 2021) in the branch
> [v1](https://github.com/kennedyvnak/unity-metroidvania/tree/v1)

![ThePaleMoonlightSample](/Docs/ReadmeImages/ThePaleMoonlightSample.png)

## Credits to the artists

### Main

- [Player](https://aamatniekss.itch.io/fantasy-knight-free-pixelart-animated-character)

### Entities

- [Undead Executioner](https://darkpixel-kronovi.itch.io/undead-executioner)
- [Skeleton](https://astrobob.itch.io/animated-pixel-art-skeleton)

### Environments

- [The Pale Moonlight](https://corwin-zx.itch.io/the-pale-moonlight)
- [Moon Graveyard](https://anokolisa.itch.io/moon-graveyard)

### UI

- [Status Bar](https://sweenus.itch.io/sharp-hud)

## Packages used in project

- New Input System
- Unity Addressables
- Unity Localization
- Unity URP
- Unity DOTS

## Features

- Platform player controller built with component system and state machine
- Settings management (audio, screen, game settings...)
- Events using Scriptable Objects with tracker
- Input system for keyboard and gamepad
- Serialization System with data viewing in editor
- Scene Management
- Game Over
- UI transitions and animations

## Setup

### Github Installation

1. Install the unity version 6000.0.32f1
2. Clone this repository into a directory in your computer
3. Open project in Unity Hub
