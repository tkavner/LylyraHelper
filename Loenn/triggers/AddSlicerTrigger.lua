local addSlicerOnLoadTrigger = {}
addSlicerOnLoadTrigger.name = "LylyraHelper/AddSlicerTrigger"
addSlicerOnLoadTrigger.placements = {}

local directions = {"Up", "Down", "Right", "Left", "All"}


addSlicerOnLoadTrigger.fieldInformation = {
    direction = {
        options = directions,
        editable = false
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
			knifeLength = 8
		}
	}
	table.insert(addSlicerOnLoadTrigger.placements, placement)
end


return addSlicerOnLoadTrigger