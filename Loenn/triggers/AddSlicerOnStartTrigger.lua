local addSlicerOnLoadTrigger = {}
addSlicerOnLoadTrigger.name = "LylyraHelper/AddSlicerOnLoadTrigger"
addSlicerOnLoadTrigger.placements = {}

local directions = {"Up", "Down", "Right", "Left", "All"}

for _, dir in ipairs(directions) do
	local placement = {
		name = "AddSlicerOnLoadTrigger ("..dir..")",
		data = {
			sliceOnImpact = false,
			singleUse = false,
			entityTypes = "",
			direction = dir,
			roomwide = false
		}
	}
	table.insert(addSlicerOnLoadTrigger.placements, placement)
end


return addSlicerOnLoadTrigger