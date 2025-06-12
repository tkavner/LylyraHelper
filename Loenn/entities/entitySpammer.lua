local helpers = require("mods").requireFromPlugin("helpers")

local spammer = {}

spammer.name = "LylyraHelper/Dev/EntitySpammer"
spammer.depth = -100
spammer.placements = {
    {
        name = "Entity Spammer (IF YOU ARE A MAPPER TELL LYRA THIS GOT MERGED TO MAIN IT SHOULD NOT BE HERE)",
        data = helpers.createPlacementData('1', {
            oneUse = false
        })
    }
}

function spammer.texture(room, entity)
    return "objects/LylyraHelper/ss2024/uncursedRefill/idle00"
end

return spammer
