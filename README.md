# Unity 2D Board Game

A local multiplayer turn-based board game built with Unity. Players compete to reach the end of the path by rolling dice, navigating obstacles, and utilizing special tiles.

## 🎮 Game Overview

- **Genre**: 2D Board Game / Party Game
- **Players**: 2 Players (Local Multiplayer)
- **Engine**: Unity 2D
- **Status**: Prototype / In Development

## ✨ Features

- **Turn-Based Gameplay**: Classic roll-and-move mechanics.
- **Physics-Based Dice**: Visual 3D/2D dice rolling simulation.
- **Special Tiles**:
  - 🟢 **Shortcut**: Teleport to a further point on the path.
  - 🔴 **Skip Turn**: Lose your next turn.
  - ⭐ **Extra Roll**: Get an immediate second turn.
- **Player Animations**:
  - Smooth hopping movement between waypoints.
  - "Juice" effects: Squashing/stretching on landing and bouncing on interaction.
- **Dynamic Path System**: Waypoint-based movement system handled by `FollowThePath`.

## 🛠️ Configuration & Technical Details

- **Scripts**:
  - `GameControl.cs`: Manages the game state, turns, UI updates, and win conditions.
  - `FollowThePath.cs`: Handles player movement physics, waypoint navigation, and animations.
  - `Dice.cs`: Manages dice randomization and result processing.
  - `SpecialTile.cs`: Defines the behavior of interactive board tiles.
- **Input**: Mouse/Touch interaction for rolling dice and UI buttons.

## 🚀 Getting Started

1. **Clone the repository**:
   ```bash
   git clone https://github.com/YourUsername/Unity-2D-Board-Game.git
   ```
2. **Open in Unity**:
   - Launch Unity Hub.
   - Click "Add" and select the repository folder.
   - Open the project (Version match recommended).
3. **Play**:
   - Open the Main Scene/SampleScene.
   - Press the Play button in the Editor.

## 📝 Roadmap

- [ ] Add more special tile types.
- [ ] Implement AI for single-player mode.
- [ ] Add sound effects and background music.
- [ ] Improve UI/UX with animations.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
