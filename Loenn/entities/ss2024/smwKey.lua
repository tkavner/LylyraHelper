local drawableSprite = require("structs.drawable_sprite")

local smwKey = {}

smwKey.name = "LylyraHelper/SS2024/WhimsyKey"
smwKey.depth = 100
smwKey.placements = {
    name = "Whimsy Key",
    data = {
        grabbable = true,
        spritePath = "objects/LylyraHelper/ss2024/smwKey/leafkey",
        optimized = false
    }
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "objects/LylyraHelper/ss2024/smwKey/leafkey00"

function smwKey.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end

return smwKey
