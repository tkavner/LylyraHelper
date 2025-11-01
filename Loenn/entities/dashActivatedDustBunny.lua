local drawableSpriteStruct = require("structs.drawable_sprite")
local helpers = require("mods").requireFromPlugin("helpers")
local consts = require("mods").requireFromPlugin("consts")

local dustEdgeColor = {1.0, 0.0, 0.0}

local dustBunny = {}
dustBunny.name = "LylyraHelper/DashActivatedDustBunny"
dustBunny.ignoredFields = consts.ignoredFields

dustBunny.nodeLimits = {1, 999}
dustBunny.nodeLineRenderType = "line"

dustBunny.placements = {
    name = "Dash Activated Dust Bunny",
    data = helpers.createPlacementData('2',
    {
        TravelTime = 0.15
    })
}

function dustBunny.sprite(room, entity)
    local position = {
        x = entity.x,
        y = entity.y
    }

    local baseTexture = "danger/dustcreature/base00"
    local baseOutlineTexture = "dust_creature_outlines/base00"
    local baseSprite = drawableSpriteStruct.fromTexture(baseTexture, position)
    local baseOutlineSprite = drawableSpriteStruct.fromInternalTexture(baseOutlineTexture, entity)

    baseOutlineSprite:setColor(dustEdgeColor)

    return {
        baseOutlineSprite,
        baseSprite
    }
end

return dustBunny
