local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local atlases = require("atlases")
local utils = require("utils")
local drawing = require("utils.drawing")

local doors = {}

local smwDoorVert = {}

smwDoorVert.name = "LylyraHelper/SS2024/WhimsyDoor"
smwDoorVert.depth = 0
smwDoorVert.minimumSize = {8, 8}
smwDoorVert.placements = {}
smwDoorVert.canResize = {false, true}


local smwDoorHoriz = {}

smwDoorHoriz.name = "LylyraHelper/SS2024/WhimsyDoorHorizontal"
smwDoorHoriz.depth = 0
smwDoorHoriz.minimumSize = {8, 8}
smwDoorHoriz.placements = {}
smwDoorHoriz.canResize = {true, false}


table.insert(smwDoorHoriz.placements, {
	name = "Whimsy Door (Horizontal)",
    data = {
		width = 24,
        height = 8
    }
})

table.insert(smwDoorVert.placements, {
	name = "Whimsy Door (Vertical)",
    data = {
		width = 8,
        height = 24
    }
})




local texture = "objects/LylyraHelper/ss2024/smwDoor/smwDoor"

local lockH = "objects/LylyraHelper/ss2024/smwDoor/lockh00"

local chainHLeft = "objects/LylyraHelper/ss2024/smwDoor/chainHTop00"
local chainHRight = "objects/LylyraHelper/ss2024/smwDoor/chainHBot00"

local chainVTop = "objects/LylyraHelper/ss2024/smwDoor/chainTop00"
local chainVBottom = "objects/LylyraHelper/ss2024/smwDoor/chainBot00"

local lockV = "objects/LylyraHelper/ss2024/smwDoor/pomf00"

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

function smwDoorVert.sprite(room, entity)
	local sprites = {}
	local height = entity.height
	local x = entity.x
	local y = entity.y
	for i=0,height/8 - 1,1 do
		local sprite = drawableSprite.fromTexture((i%2==0) and chainVTop or chainVBottom, {x = x - 8, y = y + i * 8, atlas = atlas})
		sprite:setJustification(0.0, 0.0)
		table.insert(sprites, sprite)
	end

	
	local sprite1 = drawableSprite.fromTexture(lockV, {x = x-12, y = y + height/2-16, atlas = atlas})
	sprite1:setJustification(0.0, 0.0)
	table.insert(sprites, sprite1)


	return sprites
end

function smwDoorHoriz.sprite(room, entity)
	local sprites = {}
	local width = entity.width
	local x = entity.x
	local y = entity.y
	for i=0,width/8 - 1,1 do
		local sprite = drawableSprite.fromTexture((i%2==0) and chainHRight or chainHLeft, {x = x + i * 8, y = y - 8, atlas = atlas})
		sprite:setJustification(0.0, 0.0)
		table.insert(sprites, sprite)
	end
	local sprite1 = drawableSprite.fromTexture(lockH, {x = x + width/2 - 16, y = y - 12, atlas = atlas})
	sprite1:setJustification(0.0, 0.0)
	table.insert(sprites, sprite1)

	return sprites
end

function smwDoorVert.rotate(room, entity, direction)
    return true
end

local door = {smwDoorVert, smwDoorHoriz};

return door 