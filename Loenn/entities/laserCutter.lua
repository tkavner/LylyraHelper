local drawableSprite = require("structs.drawable_sprite")
local atlases = require("atlases")
local utils = require("utils")
local drawing = require("utils.drawing")

local laserCutter = {}


laserCutter.name = "LylyraHelper/LaserCutter"
laserCutter.depth = 0
laserCutter.minimumSize = {32, 32}
laserCutter.placements = {}


local directions = {"Up", "Down", "Left", "Right"}
local modes = {"Flag", "Pulse", "Breakbeam"}

for _, dir in ipairs(directions) do
	
	for __, mode in ipairs(modes) do
		table.insert(laserCutter.placements, {
		name = "Laser Cutter ("..mode..", "..dir..")",
		data = {
				width = 32,
				height = 32,
				mode = string.lower(mode),
				frequency = 2.0,
				flag = "laser_cutter_activate",
				firingLength = 1.0,
				direction = dir
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


function laserCutter.rotate(room, entity, direction)
    return true
end

return laserCutter