# Pixelvex-MonoGame-Resources
A collection of source code for a few 3D techniques, designed to work with MonoGame (although the concepts could apply to any engine).

My code is for an entity-component architecture, so you will see references to GEntity and GComponent. Entity component means your game is organized into objects called "Entities", and those contain references to "Components" that add functionality to your Entity. For example, a Dodgeball entity would have a PhysicsComponent, DodgeballControllerComponent, ModelComponent, and a TrailComponent. There will be one manager for each type of component that knows when to update and draw all active components of that type. It's a lot easier to use than it sounds, and I've included the source for my GEntity and GComponent in the root directory. Take a look at https://www.gamedev.net/resources/_/technical/game-programming/understanding-component-entity-systems-r3013 if you want to learn more about Entity Component.

ParticleEffect.fx: a modified version of Microsoft's 3D particle sample to make the shader work with MonoGame. Adds soft particles and some options for color and rotation over time. Also includes the modified ParticleVertex declaration that goes with it.

