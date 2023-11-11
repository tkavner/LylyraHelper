local helpers = require("mods").requireFromPlugin("helpers")
local consts = require("mods").requireFromPlugin("consts")

local addSlicerOnLoadTrigger = {}
addSlicerOnLoadTrigger.name = "LylyraHelper/AddSlicerTrigger"
addSlicerOnLoadTrigger.placements = {}
addSlicerOnLoadTrigger.ignoredFields = consts.ignoredFields

local directions = {"Up", "Down", "Right", "Left", "All"}


addSlicerOnLoadTrigger.fieldInformation = {
    direction = {
        options = directions,
        editable = false
    },
	cutSize = {
		fieldType = "integer",
		minimumValue=8
	},
	slicerLength = {
		fieldType = "integer",
		minimumValue=1
	}
}

for _, dir in ipairs(directions) do
	local placement = {
		name = "Add Slicer Trigger ("..dir..")",
		data = 
			helpers.createPlacementData(1, {
			sliceOnImpact = false,
			singleUse = false,
			entityTypes = "",
			direction = dir,
			roomwide = false,
			minimumCutSize = 16,
			slicerLength = 8,
			onLoadOnly=false,
			flag="",
			invert=false,
			sliceableEntityTypes=""
		})
	}
	table.insert(addSlicerOnLoadTrigger.placements, placement)
end



return addSlicerOnLoadTrigger