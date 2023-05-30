local drawableSprite = require("structs.drawable_sprite")
local atlases = require("atlases")
local utils = require("utils")
local drawing = require("utils.drawing")

local laserCutter = {}
local laserCutterBreakbeam = {}
local laserCutterFlag = {}
local laserCutterPulse = {}
local laserCutterInFront = {}

laserCutterBreakbeam.name = "LylyraHelper/LaserCutterBreakbeam"
laserCutterBreakbeam.placements = {}
laserCutterBreakbeam.canResize = {false, false}
laserCutterBreakbeam.depth = 0

laserCutterFlag.name = "LylyraHelper/LaserCutterFlag"
laserCutterFlag.placements = {}
laserCutterFlag.canResize = {false, false}
laserCutterFlag.depth = 0

laserCutterPulse.name = "LylyraHelper/LaserCutterPulse"
laserCutterPulse.placements = {}
laserCutterPulse.canResize = {false, false}
laserCutterPulse.depth = 0

laserCutterInFront.name = "LylyraHelper/LaserCutterInFront"
laserCutterInFront.placements = {}
laserCutterInFront.canResize = {false, false}
laserCutterInFront.depth = 0

local directions = {"Up", "Down", "Left", "Right"}
local modes = {"Flag", "Pulse", "In Front", "Breakbeam"}


laserCutterInFront.fieldInformation = {
    direction = {
        options = directions,
        editable = false
    }
}

laserCutterPulse.fieldInformation = {
    direction = {
        options = directions,
        editable = false
    }
}

laserCutterFlag.fieldInformation = {
    direction = {
        options = directions,
        editable = false
    }
}

laserCutterBreakbeam.fieldInformation = {
    direction = {
        options = directions,
        editable = false
    }
}


for _, dir in ipairs(directions) do
	table.insert(laserCutterFlag.placements, {
		name = "Laser Cutter (Flag, "..dir..")",
		data = {
				frequency = 2.0,
				firingLength = 1.0,
				direction = dir,
				flag = "laser_cutter_activate"
			}
		})
	table.insert(laserCutterPulse.placements, {
		name = "Laser Cutter (Pulse, "..dir..")",
		data = {
				frequency = 2.0,
				firingLength = 1.0,
				direction = dir
			}
		})
	table.insert(laserCutterInFront.placements, {
		name = "Laser Cutter (In Front, "..dir..")",
		data = {
				frequency = 2.0,
				firingLength = 1.0,
				direction = dir
			}
		})
	table.insert(laserCutterBreakbeam.placements, {
		name = "Laser Cutter (Breakbeam, "..dir..")",
		data = {
				frequency = 2.0,
				firingLength = 1.0,
				direction = dir,
				breakBeamThickness=32
			}
		})
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

function laserCutterBreakbeam.sprite(room, entity)
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

function laserCutterFlag.sprite(room, entity)
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

function laserCutterPulse.sprite(room, entity)
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

function laserCutterInFront.sprite(room, entity)
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

table.insert(laserCutter, laserCutterFlag)
table.insert(laserCutter, laserCutterInFront)
table.insert(laserCutter, laserCutterBreakbeam)
table.insert(laserCutter, laserCutterPulse)

return laserCutter