local helpers = require("mods").requireFromPlugin("helpers")

local refill = {}

refill.name = "LylyraHelper/SS2024/CursedRefill"
refill.depth = -100
refill.placements = {
    {
        name = "Cursed Refill",
        data = 
			helpers.createPlacementData(1, {
            oneUse = false
        })
    }
}

function refill.texture(room, entity)
    return "objects/LylyraHelper/ss2024/cursedRefill/idle00"
end

return refill