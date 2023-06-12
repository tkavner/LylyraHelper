local drawableSprite = require("structs.drawable_sprite")

local bigCloud = {}
bigCloud.placements = {}
bigCloud.name = "LylyraHelper/BigCloud"

table.insert(bigCloud.placements, {
		name = "Big Cloud",
		data = {
				width = 64,
				xOffset = 32,
				yOffset = 5
			}
		})

function bigCloud.sprite(room, entity)
	local sprite = drawableSprite.fromTexture("objects/LylyraHelper/laserCutter/bigcloud00", {x = entity.x, y = entity.y, atlas = atlas})
	
	sprite:setJustification(0.5, 0.5)
	
	return sprite

end

return bigCloud