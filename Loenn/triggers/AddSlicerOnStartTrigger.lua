local addSlicerOnStartTrigger = {}
addSlicerOnStartTrigger.name = "LylyraHelper/AddSlicerOnStartTrigger"
addSlicerOnStartTrigger.placements = {}

local directions = {"Up", "Down", "Right", "Left", "All"}

for _, dir in ipairs(directions) do
	local placement = {
		name = "AddSlicerOnStartTrigger ("..dir..")",
		data = {
			sliceOnImpact = false,
			singleUse = false,
			entityTypes = "",
			direction = dir,
			roomwide = false
		}
	}
	table.insert(addSlicerOnStartTrigger.placements, placement)
end


return addSlicerOnStartTrigger