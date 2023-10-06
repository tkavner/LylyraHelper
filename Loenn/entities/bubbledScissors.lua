local drawableSprite = require("structs.drawable_sprite")
local consts = require("mods").requireFromPlugin("consts")
local helpers = require("mods").requireFromPlugin("helpers")

local bubbledScissors = {}

bubbledScissors.name = "bubbledScissors"
bubbledScissors.depth = 100
bubbledScissors.placements = {
    name = "bubbled_scissors",
}
bubbledScissors.ignoredFields = consts.ignoredFields

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "characters/theoCrystal/idle00"

function bubbledScissors.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end
