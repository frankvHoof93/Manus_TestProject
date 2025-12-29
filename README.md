# Manus_Test – A Pathfinding Playground

This repository contains a shared **pathfinding core library** with multiple frontends.

The project is designed to explore, implement, and compare different shortest-path algorithms on the same underlying graph model, while keeping all domain logic independent from any UI or engine.

The project was commissioned by [MANUS](https://www.manus-meta.com/) to serve as an example of my coding skills and architectural insights. It was created over the course of **approximately 2–2.5 days**.

## Problem Statement

Imagine that you have a city of people in a remote land. Being that there's a lot of space lying around, they are constantly expanding. They do so in the following fashion:

First, a group of people break away from the main city ("city 0") and build a new city. After doing so, they build two roads:
- One to the most recently constructed city
- One back to one other randomly chosen city that has already been built

This process continues for an indefinite number of cities. The first city to be built only builds a road back to city 0.

You don't care about the 'real life' physical location of the cities,  just assume that they exist and that the roads between them are all of equal length.

Write a program to simulate this expansion. Your code should work for up to 20 cities. You should have a variable somewhere in your code that controls how many cities to simulate. After you're done simulating the expansion, print some output showing each city and which other cities it is connected to. For example:
```
    City 0    Connections: [1, 2, 3]
    City 1    Connections: [0, 2]
    City 2    Connections: [1, 0, 3]
    City 3    Connections: [2, 0]
```
Now, imagine that you've been asked by this nation's postal service to write a program to determine the quickest path from one city to another. 
Assuming that all roads between cities are of equal length, write some code to find the shortest path between any two given cities.
Your code should work for any two input cities, and print out the path as a list of cities.

For the example above, this might look like:
```
    Shortest Path from City 3 to City 1: [3, 2, 1]
```
---

## Folder Structure
```
ROOT/
├─ Core/          # Shared C# library (Unity package + .NET library)
├─ ConsoleApp/    # Console frontend using Core
├─ UnityApp/      # Unity frontend using Core as a local package
└─ BuildOutput/   # Centralized build artifacts (bin/obj)
```
---

## Core

The **Core** project is a pure C# library that contains all domain logic.

Characteristics:
- No Unity dependencies
- Can be referenced by a .NET application
- Can be imported into Unity as a package
- Single source of truth for all algorithms and data structures

Contents include:
- Data structures (City, Street, Path)
- City generation logic
- Pathfinding algorithms:
  - Dijkstra
  - A*
  - Bellman–Ford
  - Floyd–Warshall
  - Johnson
  - Bidirectional Dijkstra


## ConsoleApp

The Console application provides an interactive way to try out the Core library.

Features:
- Generate a random world of cities and streets
- Choose a pathfinding algorithm
- Find the shortest path between two cities
- Switch algorithms without restarting the application

Run it with:

`dotnet run --project ConsoleApp`

## UnityApp

The **Unity** project visualizes the same Core logic.

Key points:
- Core is imported as a local Unity package
- No Unity-specific code exists inside Core
- Visualization and interaction live exclusively in the Unity project

The Core package is referenced via its ***package.json***

---

## Build Output

All build artifacts are redirected to a centralized location.
This keeps the Project-folders clean, preventing Unity or the Console-app from importing Build-Output when directly referencing the Core-library.
```
BuildOutput/
├─ Core/
│  ├─ bin/
│  └─ obj/
├─ ConsoleApp/
│  ├─ bin/
│  └─ obj/
```

## Requirements
- .NET SDK compatible with the configured TargetFramework
- Unity 6000+
- No external runtime dependencies

## Implementation Details

This project was implemented over the course of roughly 2 to 2.5 days.

The initial implementation was done entirely inside Unity. This was a deliberate choice: working in a visual environment felt more engaging and less monotonous than starting in a console-only setup, especially during early architecture development and debugging. The full pathfinding system (including data structures, algorithms, and visualization) was implemented in Unity in approximately 8-10 hours (most of which was spent on the visualization).

From the start, reusable logic was already separated into a Core folder within the Unity project. This logic was data-oriented, creating a clear deliniation between the logic required by the problem statement and the visualization-classes added to provide som visual flair. After the Unity implementation was complete, this Core code was extracted into a standalone C# class library, where it was cleaned up and comments were added. A ConsoleApp was then added as a second front-end, allowing the same algorithms to be tried out outside of Unity.

The final steps involved:

- Adding a package.json so the Core library could be consumed as a Unity package
- Introducing a Directory.Build.props file to centralize and relocate build outputs (preventing Unity from picking up bin/obj folders)
- Final cleanup and pushing the project to GitHub

These final integration and structural steps took roughly 2 to 3 hours.

## Unity Integration: Package vs DLL

When integrating the Core library into Unity, there were two viable options:

### Using a DLL

#### Pros

- Clean, minimal footprint inside Unity
- Clear separation between engine code and external logic
- Well-suited for production environments
- Easy to lock versions

#### Cons

- Requires a build step outside of Unity
- Slower iteration when making frequent changes
- Typically benefits from CI/CD to automate builds and updates

### Using a Unity Package (package.json)

#### Pros

- Extremely easy to iterate on during development
- Changes are immediately reflected in Unity
- No manual build step required
- Easy to maintain during active development

#### Cons

- Slightly less “clean” than a DLL-based workflow
- Unity sees source files directly instead of a compiled artifact

For this project, ease of use and development speed were the primary drivers, which is why the Core library is integrated as a Unity package. This made rapid iteration and shared development between the ConsoleApp and Unity project straightforward.

In a production-ready scenario, the more likely approach would be to build the Core library as a DLL, combined with some form of CI/CD pipeline to automatically build and pull the latest version into Unity.

## AI Disclosure

Large Language Models (LLMs) were used as a supporting tool during the development of this project. Their use was intentional, limited in scope, and focused on reducing repetitive or low-value work rather than replacing design or decision-making.

### Algorithm Implementation

AI assistance was used to help generate C# implementations of well-known pathfinding algorithms based on existing and widely available pseudo-code (e.g. Dijkstra, A*, Floyd–Warshall, Johnson, etc.).

Translating standard pseudo-code into idiomatic, type-safe C# that fits a custom interface is a largely mechanical and repetitive process. Given how extensively documented these algorithms already are, an LLM proved well-suited for producing initial implementations that matched the interfaces and data structures I had designed.

This allowed me to focus on:

- Overall application architecture
- Algorithm selection and integration
- API design and maintainability
- Multi-front-end support (Unity + ConsoleApp)

### Documentation & Project Configuration

AI was also used to assist with:

- Creating the Directory.Build.props configuration
- Drafting sections of this README.md
- Minor wording and formatting improvements in documentation

### Verification & Responsibility

All AI-generated code and text was manually reviewed, validated, and adjusted by me before being committed. This includes:

- Verifying algorithm correctness
- Ensuring consistency with project architecture
- Fixing edge cases and integration issues
- Confirming documentation accuracy

The final responsibility for correctness, structure, and design of this project lies entirely with me.