local addSlicerOnLoadTrigger = {}
addSlicerOnLoadTrigger.name = "LylyraHelper/AddSlicerTrigger"
addSlicerOnLoadTrigger.placements = {}
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
	},
	slicerSettings = {
		fieldType = "LylyraHelper.TypeField"
	}
}

for _, dir in ipairs(directions) do
	local placement = {
		name = "Add Slicer Trigger ("..dir..")",
		data = {
			sliceOnImpact = false,
			singleUse = false,
			entityTypes = "",
			direction = dir,
			roomwide = false,
			cutSize = 16,
			slicerLength = 8,
			onLoadOnly=false,
			flag="",
			invert=false,
			sliceableEntityTypes=""
		}
	}
	table.insert(addSlicerOnLoadTrigger.placements, placement)
end


return addSlicerOnLoadTrigger