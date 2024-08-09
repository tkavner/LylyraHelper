local helpers = require("mods").requireFromPlugin("helpers")

local noFastfallTrigger = {}
noFastfallTrigger.name = "LylyraHelper/NoFastfallTrigger"
noFastfallTrigger.placements = {
	name = "No Fastfall Trigger",
	data = 
			helpers.createPlacementData(1, {
		invert=false
	})
}


return noFastfallTrigger