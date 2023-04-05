README

This file is an explanation of Entities and names in LylyraHelper. Feel free to redraw whatever you want, although the Paper varieties and BubbleScissors Crystal are the most dire in my opinion. Scissor animations I think look okay but definately willing to replace them because I know you make good stuff. Scissors should have baked in outlines, Paper should not (will be added in code for dynamic outlines)

This contains a full pre-release for Lylyra Helper, meaning you should be able to place anything in a map the somewhat jank and deprecated Ahorn implementation, although DashPaper and BubbledScissors do have functional Lonn Files and DashPaper should be placed from Lonn.

Try not to play with the ChessBlocks. Those should be in a seperate helper and are currently a secret (not so good secret to those in SJ) but I somehow started making them in here a long time ago. I don't remember why. I would simply remove them + other dead files but I am honestly feeling pretty bad right now and the last thing I want to do is test to make sure the code still work after removing them.

Try to keep this stuff secret to those outside SJ.

///////////////////
Basic Terminology:

Legacy Stuff:

CloudBlock: DashPaper / paper started as an attempt to create something called a "Cloudblock". If you see any reference to "CloudBlock" thats what this is about.
Hole: Old System used in CloudBlocks / Paper. Should not exist anymore
BigCloud and ChessBlock: ignore these for now pls. Subject to removal from repo.

Current Stuff

Scissors: Spawnable projectile. Cuts specific items in half, leaving ~4 tile gap. Originally meant to work with Paper. Will split item into two pieces if the item is big enough relative to the cut location. Crashes and despawns upon hitting a wall. Scissors have been implemented to cut Paper, Kevin Blocks, Fall Blocks, and Dream Blocks at the moment. This will probably be the standard list of implementations, as I feel it makes for some interesting gameplay while not being overloaded / having a bunch of dumb nuance 

(Basically I fear if I let any set of items be "Cut" that it could be confusing for players if Scissors become a popular mod item and they all have different ideas of what they should cut and just having it cut everything by default wouldn't be very interesting).

A full list of proposed implementations for various vanilla entities can be found at https://docs.google.com/document/d/1d_HfhnIocStPm57r8yFxiiM0FucEcGsF1Bvp_7cFS7U/edit?usp=sharing

Paper: Destructible 9tile grid. Cut by Scissors. Comes in two current varieties: Dash Paper and Death Note. Paper can be merged with other paper similar to Cassette blocks.

Dash Paper: Type of Paper. Refills dash inventory when you dash in the field. If the "Spawn Scissors" function is set (Called Trapped Dash Paper), also spawns 1 (for cardinal) and 2 (for diagonal / intercardianal) scissor relative to the dash direction. Diagonal Scissors have been considered but were ultimately nixxed as the implementation details for cutting on a diagonal are messy and don't lead to as interesting gameplay. Trapped Dash Paper additionally spawns white PaperScrap particles.

Death Note: Type of Paper. Kills player on touch.

Bubbled Scissors: Implementation details subject to change. Theo Crystal that despawns and spawns scissors when hitting a seeker barrier.

///////////////////
Art Stuff:

Eventually all of this stuff should be customizable.

ScissorShards: Particle type for scissors. Spawns in the mouth of the scissors.
Scissors (Spawn): An animation that should last about 0.5 Seconds. Currently just the first frame of the idle animation.
Scissors (Idle): Animation used while scissors are moving.
Scissors (Break): Animation used when scissors are breaking. Currently 8 frames.

Scissor files can be found in objects/LylyraHelper/scissors

Scissor animation notes: kinda cursed, uses a 64 x 64 texture so I can easily implement directional stuff. Check out files to see what I mean.

Paper Textures: Currently a 9 tile for Lonn and a custom tileset png thing for Celeste. Will be switching over entirely to a 9 tile (unless a cool tileset is made). Additionally needs a gap png which contains the rough cutting edges you see in the video. its a 5x5 tile array. Special cases occur when a rough edge / gap tile needs to go on the edge. These go in a 3 x3 ring in the middle of the file.
Current Tileset images are as follows:

DashPaper: objects/LylyraHelper/dashpaper/cloudblocknew.png, objects/LylyraHelper/dashpaper/cloudblocknew9Tile.png, objects/LylyraHelper/dashpaper/cloudblockgap.png
DeathNote: objects/LylyraHelper/dashpaper/deathnote.png, objects/LylyraHelper/dashpaper/deathnotegap.png

PaperScraps: Particle type for Paper upon cutting. Recolored versions (recolors done in code) are used for Kevins, DashBlocks, and Fall Blocks.

Other:

objects/LylyraHelper/dashpaper/cloudblocknewScissors9Tile.png was originally a file for trapped dash paper. Only currently used in Lonn to differentiate Dash Paper from Trapped Dash Paper.
objects/LylyraHelper/ was originally the file location for what became Paper


///////////////////
RoadMap after I finish this stuff:


Hexagonal Godrays: A port of the godray honeycomb I made for tiltthestars's Bee Berserk. V simple. Basically just needs a Lonn file.

TriggerPaper: Triggers a Trigger on dash / enter / jump / climb. Basically a TriggerTrigger but also Paper.

Scissor Gem: Spawns scissors (front/behind) relative to next dash.

Laser Cutter: Trip wire that instantly cuts things.

