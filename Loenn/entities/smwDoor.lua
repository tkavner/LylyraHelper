local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local atlases = require("atlases")
local utils = require("utils")
local drawing = require("utils.drawing")

local smwDoor = {}

smwDoor.name = "LylyraHelper/SMWDoor"
smwDoor.depth = 0
smwDoor.minimumSize = {8, 8}
smwDoor.placements = {}
smwDoor.canResize = {false, true}

table.insert(smwDoor.placements, {
	name = "SMW Door",
    data = {
		width = 8,
        height = 24
    }
})


local scissorsTexture = "objects/LylyraHelper/smwDoor/smwDoor"

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

function smwDoor.sprite(room, entity)
	local sprites = {}
	local height = entity.height
	local x = entity.x
	local y = entity.y
	for i=0,height/8 - 1,1 do
		local sprite = drawableSprite.fromTexture(scissorsTexture, {x = x, y = y + i * 8, atlas = atlas})
		sprite:setJustification(0.0, 0.0)
		table.insert(sprites, sprite)
	end

	return sprites
end

function smwDoor.rotate(room, entity, direction)
    return true
end


return smwDoor