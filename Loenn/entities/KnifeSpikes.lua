local spikeHelper = require("helpers.spikes")

local spikeUp = spikeHelper.createEntityHandler("LylyraHelper/KnifeSpikesUp", "up")
spikeUp.Name = "LylyraHelper/KnifeSpikesUp"
local spikeDown = spikeHelper.createEntityHandler("LylyraHelper/KnifeSpikesDown", "down")
spikeDown.Name = "LylyraHelper/KnifeSpikesDown"
local spikeLeft = spikeHelper.createEntityHandler("LylyraHelper/KnifeSpikesLeft", "left")
spikeLeft.Name = "LylyraHelper/KnifeSpikesLeft"
local spikeRight = spikeHelper.createEntityHandler("LylyraHelper/KnifeSpikesRight", "right")
spikeRight.Name = "LylyraHelper/KnifeSpikesRight"
local knifeSpikes = {}

for _, placement in ipairs(spikeUp.placements) do
	placement.data.SliceOnImpact = false
end
for _, placement in ipairs(spikeDown.placements) do
	placement.data.SliceOnImpact = false
end
for _, placement in ipairs(spikeLeft.placements) do
	placement.data.SliceOnImpact = false
end
for _, placement in ipairs(spikeRight.placements) do
	placement.data.SliceOnImpact = false
end
return {spikeUp, spikeDown, spikeLeft, spikeRight}