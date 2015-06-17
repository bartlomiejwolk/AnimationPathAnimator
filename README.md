README
======

AnimationPath Animator is a path animator extension for Unity 5.

Licensed under MIT license. See LICENSE file in the project root.

![AnimationPath Tools](/Resources/cover_screenshot.png?raw=true "AnimationPath Tools Scene view")

Features
--------

- Linear and Bezier paths
- Custom rotation, animation speed and tilting
- Wrap modes: Once, Loop and PingPong
- Rotation by following target object
- Rotation by looking ahead of the path
- Autoplay, play with delay
- Position and rotation lerp
- Realtime animation preview in edit and play mode
- Editing path in play mode
- Custom node events
- Synchronizing multiple animators
- Synchronizing animation with music
- Comfortable keyboard shortcuts
- Ability to fully customize shortcuts and other advanced options
- Playback control with API
- Node export as transforms
- Separate path asset file for every path
- Separate config asset files
- [Quick Start tutorial](https://youtu.be/M_7y2k4UgOc) on youtube
- Example scenes
- [API reference documentation](http://animationpathanimator.airtime-productions.com "Online API")

[Video Teaser](https://youtu.be/wS1hQ5641zQ "AnimationPath Animator Unity 5 Extension Teaser ")<br>

Resources
---
* [Quick Start Tutorial](https://youtu.be/M_7y2k4UgOc)
* [Blog Post](https://bartlomiejwolk.wordpress.com/2015/03/27/animationpath-animator/)    
* [Unity Forum Thread](http://forum.unity3d.com/threads/open-source-unity-5-animationpath-animator-beta.321802/)
* [Youtube Playlist](https://www.youtube.com/playlist?list=PLtjvHab0cn92H1T7TojFkuohx1ngpy069)

Quick Start
------------------

- Clone repository (or extract [zip package](https://github.com/bartlomiejwolk/animationpathanimator/archive/master.zip)) to any location in `Assets` folder.
- Go to `AnimationPathAnimator` folder and run `Examples.unitypackage` to extract example assets.
- Go to `Assets/AnimationPathAnimator_examples` folder and open example scene.
- Enter play mode to start animation or select `*Path` game object to edit path in the scene.

Shortcuts
---------

- `L` Jump to next node.
- `H` Jump to previous node.
- `K` Long jump forward.
- `J` Short jump forward.
- `Alt + H` Jump to the beginning.
- `Alt + L` Jump to the end.
- `Alt + K` Small jump forward.
- `Alt + J` Small jump backward.
- `Space` Play/pause animation (only in play mode).

Help
-----

Just create an issue and I'll do my best to help.

Contributions
------------

Pull requests, ideas, questions and any feedback at all are welcome.

Versioning
----------

Example: `v0.2.3f1`

- `0` Introduces breaking changes.
- `2` Major release. Adds new features.
- `3` Minor release. Bug fixes and refactoring.
- `f1` Quick fix.
