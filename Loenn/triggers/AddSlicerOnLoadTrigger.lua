local addSlicerOnLoadTrigger = {}
addSlicerOnLoadTrigger.name = "LylyraHelper/AddSlicerOnLoadTrigger"
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
		name = "AddSlicerOnLoadTrigger ("..dir..")",
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