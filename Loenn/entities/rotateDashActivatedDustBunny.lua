local drawableSpriteStruct = require("structs.drawable_sprite")
local helpers = require("mods").requireFromPlugin("helpers")
local consts = require("mods").requireFromPlugin("consts")

local nodeAlpha = 0.3
local dustEdgeColor = {1.0, 0.0, 0.0}

local dustBunny = {}
dustBunny.name = "LylyraHelper/DashActivatedDustBunnies/Rotate"

dustBunny.nodeLimits = {1, 1}
dustBunny.nodeLineRenderType = "circle"
dustBunny.ignoredFields = consts.ignoredFields


dustBunny.placements = {
    name = "Dash Activated Dust Bunny (Rotate)",
    data = helpers.createPlacementData('1',
    {
        TravelTime = 0.15,
        DashesPerFullCycle = 2
    })
}


local function getSprite(room, entity, alpha)
    local sprites = {}

    local dust = entity.dust
    local star = entity.star

    local dustBaseTexture = "danger/dustcreature/base00"
    local dustBaseOutlineTexture = "dust_creature_outlines/base00"
    local dustBaseSprite = drawableSpriteStruct.fromTexture(dustBaseTexture, entity)
    local dustBaseOutlineSprite = drawableSpriteStruct.fromInternalTexture(dustBaseOutlineTexture, entity)

    dustBaseOutlineSprite:setColor(dustEdgeColor)

    table.insert(sprites, dustBaseOutlineSprite)
    table.insert(sprites, dustBaseSprite)

    if alpha then
        for _, sprite in ipairs(sprites) do
            sprite:setAlpha(alpha)
        end
    end

    return sprites
end



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

function dustBunny.nodeSprite(room, entity, node)
    local entityCopy = table.shallowcopy(entity)

    entityCopy.x = node.x
    entityCopy.y = node.y

    return getSprite(room, entityCopy, nodeAlpha)
end

return dustBunny
