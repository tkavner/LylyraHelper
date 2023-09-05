local drawableSprite = require("structs.drawable_sprite")

local slicerController = {}
slicerController.placements = {}
slicerController.canResize = {false, false}
slicerController.name = "LylyraHelper/SlicerController"

table.insert(slicerController.placements, {
		name = "Slicer Controller",
		data = {
			sliceableEntityTypes = ""
			}
		})



function slicerController.sprite(room, entity)
	local sprite = drawableSprite.fromTexture("objects/LylyraHelper/slicerController/slicerController", {x = entity.x, y = entity.y, atlas = atlas})
	
	sprite:setJustification(0.5, 0.5)
	
	return sprite

end

return slicerController