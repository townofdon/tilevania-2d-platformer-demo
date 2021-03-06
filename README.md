# tilevania-2d-platformer-demo

A demo 2D Platformer built in Unity.

This project started from a [simple tutorial from GameDev.TV](https://www.gamedev.tv/p/unity-2d-game-dev-course-2021/?coupon_code=NEWYEAR).
However, as the project scaled, I wanted to try more and more things such as:

- Start / GameOver / Win Screens
- Player Movement
  - Coyote time (allowing a late-jump from a platform - like Celeste)
  - Code-driven sprite animation state machine
  - Non-realistic gravity (player falls faster when returning to ground - this makes the game feel much more responsive and less "floaty")
  - Applied controller joystick dead zones
- Player UI - healthbar, lives, coins, etc.
- Attacks
  - Ability to squash enemies by stomping from above (like Mario)
  - Bow & arrow weapon
- Tracking player & enemy HP
- Dynamic music scoring based on map "zones" - adding more and more tracks play in sync despite being queued up at different times (e.g. strings, bass, drums etc.)
- Code-driven dynamic camera positioning
  - Facing camera left or right based on player position (like Hollow Knight)
- Particle effects!
- Time-freezing during climactic events (like a boss intro in Hollow Knight)

Things I wanted to do but weren't viable for an MVP:

- Checkpoints & dynamic player spawning (instead of hard-reloading the scene for each level)
- Secondary weapon: fireball attack
- Additional enemy types
  - Skeleton archer (distance attack)
  - Knight sentinel (melee attack)
  - Bats (swarm attack)
- Swimming mechanics & underwater levels (oxygen mgmt, eluding piranhas, etc.)
- Add sprint special ability
- Add dash or roll special ability
- Additional tilemap graphics (using the same tilemap over and over got a little monotonous TBH)

I pulled heavily from these resources as well:

- [Lost Relic Games - Escaping Unity Animator HELL](https://www.youtube.com/watch?v=nBkiSJ5z-hE)
- [Lost Relic Games - Nintendo Saved my Dream Indie Game from Disaster | Devlog](https://www.youtube.com/watch?v=a4M-21AMiQE)
- [Board to Bits Games - Better Jumping in Unity With Four Lines of Code](https://www.youtube.com/watch?v=7KiK0Aqtmzc)
- [Brackeys - START MENU in Unity](https://www.youtube.com/watch?v=zc8ac_qUXQY)
- [Brackeys - How to make a HEALTH BAR in Unity!](https://www.youtube.com/watch?v=BLfNP4Sc_iA)

If you have any feedback, positive or (constructively) negative, please hit me up via a GH issue or DM me on Twitter or Discord. :)
