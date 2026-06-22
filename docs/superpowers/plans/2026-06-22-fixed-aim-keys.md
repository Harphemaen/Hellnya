# Fixed Aim Keys Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make aim up increase the M18 shot angle until max, and aim down decrease it until min.

**Architecture:** Change only `M18Player` input handling. Replace cycling angle state with clamped one-step adjustment.

**Tech Stack:** Unity 2020.3 C#.

---

### Task 1: Clamp Aim Adjustment

**Files:**
- Modify: `Assets/Scripts/Player/M18Player.cs`

- [x] Make `aimUpKey` add `aimAngleStep` and clamp at `baseShotAngle + primaryMaxAngle`.
- [x] Make `aimDownKey` subtract `aimAngleStep` and clamp at `baseShotAngle`.
- [x] Remove the no-longer-needed `aimCycleDirection` state.
- [x] Compile changed runtime scripts with Unity assemblies.
