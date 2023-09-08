local drawableSprite = require("structs.drawable_sprite")

local slicerController = {}
slicerController.placements = {}
slicerController.name = "LylyraHelper/SlicerController"
slicerController.minimumSize = {48, 48}

slicerController.fieldInformation = {
	sliceableEntityTypes = {
		fieldType = "LylyraHelper.TypeField"
	}
}

table.insert(slicerController.placements, {
		name = "Slicer Controller",
		data = {
			width = 32,
			height = 32,
			sliceableEntityTypes = ""
			}
		})



function slicerController.sprite(room, entity)
	local sprite = drawableSprite.fromTexture("objects/LylyraHelper/slicerController/slicerController", {x = entity.x + entity.width / 2, y = entity.y + entity.height / 2, atlas = atlas})
	
	return sprite

end

return slicerController