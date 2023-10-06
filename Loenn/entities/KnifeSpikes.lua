local spikeHelper = require("helpers.spikes")
local utils = require("utils")
local logging = require("logging")
local consts = require("mods").requireFromPlugin("consts")
local helpers = require("mods").requireFromPlugin("helpers")

local knifeSpikes = {}

local directions = {"Up", "Down", "Left", "Right"}

-- for each spike direction, we'll let lönn's spike helper generate most of what we need and won't need to take care of
-- then we can replace or modify what was already generated to finish up the plugins
for _, dir in ipairs(directions) do
    local dirLower = string.lower(dir)
    local spikes = spikeHelper.createEntityHandler("LylyraHelper/KnifeSpikes" .. dir, dirLower, false, false)
	for _, placement in ipairs(spikes.placements) do
		placement.data.sliceOnImpact = false
		placement.data.sliceableEntityTypes = ""
	end
    table.insert(knifeSpikes, spikes)
end

return knifeSpikes