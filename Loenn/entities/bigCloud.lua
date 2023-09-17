local drawableSprite = require("structs.drawable_sprite")

local bigCloud = {}
bigCloud.placements = {}
bigCloud.canResize = {false, false}
bigCloud.name = "LylyraHelper/BigCloud"

table.insert(bigCloud.placements, {
		name = "Big Cloud",
		data = {
				width = 64,
				xOffset = 32,
				yOffset = 5,
				fragile = false
			}
		})
table.insert(bigCloud.placements, {
		name = "Big Cloud (Fragile)",
		data = {
				width = 64,
				xOffset = 32,
				yOffset = 5,
				fragile = true
			}
		})



function bigCloud.sprite(room, entity)

	local fragile = entity.fragile
	local filepath = "objects/LylyraHelper/bigCloud/bigcloud00"
	if fragile then filepath = "objects/LylyraHelper/fragileBigCloud/bigcloud00" end
	local sprite = drawableSprite.fromTexture(filepath, {x = entity.x, y = entity.y, atlas = atlas})
	
	sprite:setJustification(0.5, 0.5)
	
	return sprite

end

return bigCloud