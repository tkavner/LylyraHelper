local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")

local deathnote = {}

deathnote.name = "LylyraHelper/DeathNote"
deathnote.depth = 0
deathnote.minimumSize = {24, 24}
deathnote.placements = {}

table.insert(deathnote.placements, {
	name = "Death Note",
    data = {
		width = 24,
        height = 24,
    }
})


local frameTextures = {
    none = "objects/LylyraHelper/dashpaper/deathnote9tile",
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

function deathnote.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24
	

    local frameTexture = frameTextures["none"]
    local ninePatch = drawableNinePatch.fromTexture(frameTexture, ninePatchOptions, x, y, width, height)

    local sprites = ninePatch:getDrawableSprite()


    return sprites
end

function deathnote.rotate(room, entity, direction)

    return true
end
