local spikeHelper = require("helpers.spikes")
local utils = require("utils")
local logging = require("logging")
local consts = require("mods").requireFromPlugin("consts")
local helpers = require("mods").requireFromPlugin("helpers")


local directions = {"Up", "Down", "Left", "Right"}


local spikeOptions = {
    triggerSpike = false,
    originalTriggerSpike = false,
    placementData = {
        sliceOnImpact = false,
        sliceableEntityTypes = ""
    },
    placementName = "LylyraHelper/KnifeSpikes",
    directionNames = {
        up = "LylyraHelper/KnifeSpikesUp", 
        down = "LylyraHelper/KnifeSpikesDown", 
        left = "LylyraHelper/KnifeSpikesLeft", 
        right = "LylyraHelper/KnifeSpikesRight"
    }
}
-- for each spike direction, we'll let lönn's spike helper generate most of what we need and won't need to take care of
-- then we can replace or modify what was already generated to finish up the plugins

local knifeSpikes = spikeHelper.createEntityHandlers(spikeOptions)

return knifeSpikes