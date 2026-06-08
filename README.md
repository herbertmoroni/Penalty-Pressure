# Overview

**PenaltyPressure** is a front-view 2D soccer penalty kick game. You play as the shooter facing the goal, with an AI goalkeeper defending it.

How to play:
- Click and hold on the ball, then drag upward to shoot
- Drag **left or right** to aim your shot toward that side of the goal
- Drag **further up** for more power — a short drag shoots low, a long drag shoots high, but too much power sends it over the crossbar
- The goalkeeper patrols the goal at random speeds and directions — time your shot to beat them
- Score a goal and the crowd cheers for 9 seconds before the next kick
- Get saved or shoot wide and the crowd groans — click anywhere to try again

[Software Demo Video](https://youtu.be/rGxh2LF2YyA)

# Development Environment

- **Unity 6** (6000.4.6f1) — game engine and editor
- **Visual Studio / VS Code** — C# script editing
- **C#** — primary programming language, using Unity's MonoBehaviour framework
- **TextMesh Pro** — animated in-game UI text (GOAAALLLL! celebration)
- **Universal Render Pipeline (URP) 2D** — 2D sprite rendering
- **Unity Audio system** — AudioClip and AudioSource for crowd sounds and background music

# Useful Websites

* [Unity Documentation](https://docs.unity3d.com)
* [TextMesh Pro Documentation](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest)
* [Unity AudioSource API](https://docs.unity3d.com/ScriptReference/AudioSource.html)
* [Game Inspiration — Grimace Penalty Soccer Challenge](https://www.filereadynow.com/blog/grimace-penalty-soccer-challenge-thats-hard-to-resist)

# Future Work

* Add a score counter to track goals across multiple kicks in a session
* Show miss/save feedback text on screen (e.g. "SAVED!" or "WIDE!")
* Add a visual drag indicator while aiming to help players understand shot direction and power
* Add mobile touch input support
* Add sound volume controls
