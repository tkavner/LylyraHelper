local helpers = require("mods").requireFromPlugin("helpers")

local refill = {}

refill.name = "LylyraHelper/SS2024/UncursedRefill"
refill.depth = -100
refill.placements = {
    {
        name = "Uncursed Refill",
        data = helpers.createPlacementData('1', {
            oneUse = false
        })
    }
}

function refill.texture(room, entity)
    return "objects/LylyraHelper/ss2024/uncursedRefill/idle00"
end

return refill