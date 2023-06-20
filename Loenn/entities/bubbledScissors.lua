local drawableSprite = require("structs.drawable_sprite")

local bubbledScissors = {}

bubbledScissors.name = "bubbledScissors"
bubbledScissors.depth = 100
bubbledScissors.placements = {
    name = "bubbled_scissors",
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "characters/theoCrystal/idle00"

function bubbledScissors.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end
