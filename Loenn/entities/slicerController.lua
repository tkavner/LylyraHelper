local drawableSprite = require("structs.drawable_sprite")
local consts = require("mods").requireFromPlugin("consts")
local helpers = require("mods").requireFromPlugin("helpers")

local slicerController = {}
slicerController.placements = {}
slicerController.name = "LylyraHelper/SlicerController"
slicerController.minimumSize = {48, 48}

slicerController.ignoredFields = consts.ignoredFields
local mods = require("mods")
local v = require("utils.version_parser")

slicerController.fieldInformation = {
	sliceableEntityTypes = {
		fieldType = mods.hasLoadedMod("FrostHelper") and "LylyraHelper.TypeField" or "string"
	}
}

table.insert(slicerController.placements, {
		name = "Slicer Controller",
		data = {
			width = 48,
			height = 48,
			sliceableEntityTypes = ""
			}
		})



function slicerController.sprite(room, entity)
	local width = entity.width or 48
	local height = entity.height or 48
	local sprite = drawableSprite.fromTexture("objects/LylyraHelper/slicerController/slicerController", {x = entity.x + width / 2, y = entity.y + height / 2, atlas = atlas})
	
	return sprite
end

return slicerController