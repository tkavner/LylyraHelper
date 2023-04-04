local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")

local paper = {}

paper.name = "LylyraHelper/DashPaper"
paper.depth = 0
paper.minimumSize = {24, 24}
paper.placements = {}

table.insert(paper.placements, {
	name = "Dash Paper",
    data = {
		width = 24,
        height = 24,
        fragileScissors = false,
        spawnScissors = false
    }
})


local frameTextures = {
    none = "objects/LylyraHelper/dashPaper/cloudblocknew9tile",
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

local papercolor = {98 / 255, 34 / 255, 43 / 255}

function paper.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24
	

    local frameTexture = frameTextures["none"]
    local ninePatch = drawableNinePatch.fromTexture(frameTexture, ninePatchOptions, x, y, width, height)

    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, kevinColor)

    local sprites = ninePatch:getDrawableSprite()

    table.insert(sprites, 1, rectangle:getDrawableSprite())

    return sprites
end

function paper.rotate(room, entity, direction)

    return true
end

return paper