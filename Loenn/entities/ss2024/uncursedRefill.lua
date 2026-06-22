local helpers = require("mods").requireFromPlugin("helpers")

local consts = require("mods").requireFromPlugin("consts")
local refill = {}

refill.name = "LylyraHelper/SS2024/UncursedRefill"
refill.depth = -100
refill.placements = {
    {
        name = "UncursedRefill",
        data = helpers.createPlacementData('1', {
            oneUse = false,
            textureLocation="objects/LylyraHelper/ss2024/uncursedRefill"
        })
    }
}
refill.ignoredFields = consts.ignoredFields

function refill.texture(room, entity)
    return "objects/LylyraHelper/ss2024/uncursedRefill/idle00"
end

return refill
