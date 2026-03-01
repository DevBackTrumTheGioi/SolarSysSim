# Solar System Simulation Extensibility Plan

This document outlines proposed features to expand the simulation from a basic gravity model into a fully interactive sandbox universe.

## 1. Cosmic Events (Vũ khí và Sự kiện Cấp độ Thiên Hà)
These features focus on large-scale disruptions and interactive additions to the solar system.

*   **Black Hole Mechanics:** 
    *   If a body's mass exceeds a specific threshold (simulated Chandrasekhar limit), it collapses into a black hole.
    *   Massive increase in gravitational pull.
    *   Visual shaders (gravitational lensing/distortion) and light absorption effects.
    *   Ability to "eat" other planets, adding their mass to the singularity.
*   **Delete Planet (Xoá hành tinh):**
    *   UI interaction to instantly delete a specific planet from the simulation.
    *   Deleted planets can be restored by resetting the simulation.

## 2. Orbital & Visual Mechanics (Nâng cấp Hệ Quỹ Đạo & Trực Quan)
Enhancing the visual feedback and scientific demonstration of orbits.

*   **Asteroid Belt:** Procedural generation of hundreds of low-mass particles between Mars and Jupiter. Tests the limits of the N-body solver and provides a destructible environment for Rogue Planets.
*   **Velocity-Based Trail Colors:** Modifying the `TrailRenderer`/`LineRenderer` to use a color gradient based on the body's current instantaneous acceleration or velocity (e.g., red at perihelion, blue at aphelion).
*   **Predicted Trajectory:** Computing physics steps ahead of time without moving the actual objects, rendering a ghostly path of where the object will go (useful when editing distances/velocities while paused).

## 3. Planet Evolution (Hệ thống Tiến hóa Hành tinh)
Adding thermodynamics and tidal forces.

*   **Tidal Disruption (Roche Limit):** If a small body gets too close to a massive body (like Jupiter or a Black Hole) without colliding, tidal forces rip it apart into remnants (rings/debris).
*   **Habitable Zone / Relative Temperature:** Calculating the surface temperature of a planet based on the inverse-square law of its distance to the Sun. Displaying statuses like "Habitable", "Scorched", or "Ice Age" in the UI.

## 4. God-Mode Controls (Tính năng God-Mode Controls)
Improving player interaction with the physics engine.

*   **Slingshot Click & Drag:** While paused, click a planet and drag an arrow to set its velocity vector graphically instead of typing numbers.
*   **Mouse Gravity Well:** Holding a modifier key and clicking spawns a temporary, incredibly massive invisible point at the cursor, pulling planets towards the mouse.
