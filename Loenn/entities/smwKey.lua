local drawableSprite = require("structs.drawable_sprite")

local smwKey = {}

smwKey.name = "LylyraHelper/SMWKey"
smwKey.depth = 100
smwKey.placements = {
    name = "SMW Key"
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "objects/LylyraHelper/smwKey/smwKey"

function smwKey.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end

return smwKey;