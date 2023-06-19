local drawableSprite = require("structs.drawable_sprite")
local atlases = require("atlases")
local utils = require("utils")
local drawing = require("utils.drawing")

local laserCutter = {}


laserCutter.name = "LylyraHelper/LaserCutter"
laserCutter.placements = {}
laserCutter.canResize = {false, false}
laserCutter.depth = 0

local directions = {"Up", "Down", "Left", "Right"}
local modes = {"Pulse", "In Front", "Breakbeam"}

laserCutter.fieldInformation = {
    direction = {
        options = directions,
        editable = false
    },
	cutSize = {
		fieldType = "integer",
		minimumValue = 8
	},
	breakbeamThickness = {
		fieldType = "integer",
		minimumValue = 1
	},
	mode = {
		options = modes,
		editable = false
	}
}
	




for _, dir in ipairs(directions) do
	for __, m in ipairs(modes) do
		table.insert(laserCutter.placements, {
			name = "Laser Cutter ("..m..", "..dir..")",
			data = {
				cooldown = 2.0,
				firingLength = 1.0,
				cutSize = 32,
				direction = dir,
				mode = m,
				flag="",
				invert=false
			}
		})
	end
end

function laserCutter.sprite(room, entity)
	local sprite = drawableSprite.fromTexture("objects/LylyraHelper/laserCutter/idle00", {x = entity.x, y = entity.y, atlas = atlas})
	direction = entity.direction
	if (direction == "Right") then
	sprite.rotation = math.pi / 2
		sprite:setJustification(0.1, 1)
	elseif (direction == "Down") then
		sprite.rotation = math.pi
		sprite:setJustification(0.9, 1)
	elseif (direction == "Left") then
		sprite.rotation = 3 * math.pi / 2
		
		sprite:setJustification(0.9, 0.2)
	else 
		sprite:setJustification(0.1, 0.2)
	end
	return sprite
end

return laserCutter