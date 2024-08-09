local fakeTilesHelper = require("helpers.fake_tiles")

local fallingBlock = {}

fallingBlock.placements = {}
local directions = {"Up", "Down", "Left", "Right"}

fallingBlock.name = "LylyraHelper/SS2024/AutoReturnFallingBlock"


for _, dir in ipairs(directions) do
    table.insert(fallingBlock.placements, 
        {
        name = "Auto Return Falling Block ("..dir..")",
        data = {
            tiletype = "3",
            climbFall = true,
            behind = false,
            width = 8,
            height = 8,
            resetDelay = 1.0,
            flagOnReset = "",
            flagOnFall = "",
            flagOnLand = "",
            flagTrigger = "",
            resetFlagState = true,
            fallFlagState = true,
            landFlagState = true,
            maxSpeed = 160.0,
            acceleration = 500.0,
            direction = dir,
            landingSound = "",
            returnSound = "",
            shakeSound = "",
            invertFlagTrigger = false,
            returnMaxSpeed = 160.0,
            returnAcceleration = 500.0


            }
        }
    )

end

function fallingBlock.fieldInformation(entity)
	local x = fakeTilesHelper.getFieldInformation("tiletype")
    local y = {
    direction = {
        options = directions,
        editable = false
    }, 
    tiletype = {
        options = fakeTilesHelper.getTilesOptions(layer),
        editable = false
    }}

	return y
end

fallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)

function fallingBlock.depth(room, entity)
    return entity.behind and 5000 or 0
end

return fallingBlock