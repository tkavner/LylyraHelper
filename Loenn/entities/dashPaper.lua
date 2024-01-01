local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local atlases = require("atlases")
local utils = require("utils")
local drawing = require("utils.drawing")
local depths = require("consts.object_depths")
local consts = require("mods").requireFromPlugin("consts")
local helpers = require("mods").requireFromPlugin("helpers")

local paper = {}

paper.name = "LylyraHelper/DashPaper"
paper.depth = depths.SolidsBelow
paper.minimumSize = {24, 24}
paper.placements = {}
paper.ignoredFields = consts.ignoredFields

local wallpaperModes = {"Preset: Refill Gem", "From FG Decals"}


paper.fieldInformation = {
	decalStampData = {
		fieldType = "LylyraHelper.DecalWallpaperField"
	},
	wallpaperMode = {
        options = wallpaperModes,
		editable = false
    },
}

table.insert(paper.placements, {
	name = "Dash Paper",
    data = helpers.createPlacementData('1', {
		width = 24,
        height = 24,
        spawnScissors = false,
		noParticleEffects = false,
        noTrail = false,
		sliceableEntityTypes = "",
		regenerationDelay = 0.0,
		decalStampData = "",
		wallpaperMode = "Preset: Refill Gem"
		--flag = "",
		--invertFlag = false
    })
})

table.insert(paper.placements,
{
	name = "Dash Paper (With Scissors)",
    data = helpers.createPlacementData('1', {
		width = 24,
        height = 24,
        spawnScissors = true,
		noParticleEffects = false,
        noTrail = false,
		sliceableEntityTypes = "",
		regenerationDelay = 0.0,
		decalStampData = "",
		wallpaperMode = "From FG Decals"
		--flag = "",
		--invertFlag = false
    })
})


local leftTopCorners = {  }
leftTopCorners[0] = { 0, 0 }
leftTopCorners[1] =  { 7, 3 }
leftTopCorners[2] = { 7, 4 }


local leftBottomCorners =  { { 0, 5 },  { 6, 3 },  { 6, 4 } }
local rightTopCorners =  {  { 5, 0 },  { 6, 2 },  { 7, 2 } }
local rightBottomCorners =  {  { 5, 5 },  { 6, 5 },  { 7, 5 } }

local rightBottomCornersInvert =  {  { 6, 0 } };
local leftBottomCornersInvert =  {  { 7, 0 } };
local rightTopCornersInvert =  {  { 6, 1 } };
local leftTopCornersInvert =  {  { 7, 1 } };

local topSide =  {  { 1, 0 },  { 2, 0 },  { 3, 0 },  { 4, 0 } }

local bottomSide =  {  { 1, 5 },  { 2, 5 },  { 3, 5 },  { 4, 5 } }

local leftSide =  {  { 0, 1 },  { 0, 2 },  { 0, 3 },  { 0, 4 } }

local rightSide =  {  { 5, 1 },  { 5, 2 },  { 5, 3 },  { 5, 4 } }

local middle =  {
             { 1, 1 },  { 1, 2 },  { 1, 3 },  { 1, 4 },
             { 2, 1 },  { 2, 2 },  { 2, 3 },  { 2, 4 },
             { 3, 1 },  { 3, 2 },  { 3, 3 },  { 3, 4 },
             { 4, 1 },  { 4, 2 },  { 4, 3 },  { 4, 4 }}

local holeTopSide =  {  { 1, 0 },  { 2, 0 },  { 3, 0 } }

local holeTopSideLeftEdge =  {  { 1, 1 } }
local holeTopSideRightEdge =  {  { 1, 2 } }
local holeBottomSide =  {  { 1, 4 },  { 2, 4 },  { 3, 4 } }
local holeBottomSideLeftEdge =  {  { 2, 3 } }
local holeBottomSideRightEdge =  {  { 3, 3 } }
local holeLeftSide =  {  { 0, 1 },  { 0, 2 },  { 0, 3 } }
local holeLeftSideTopEdge =  {  { 3, 1 } }
local holeLeftSideBottomEdge =  {  { 3, 2 } }
local holeRightSide =  {  { 4, 1 },  { 4, 2 },  { 4, 3 } }
local holeRightSideTopEdge =  {  { 1, 2 } }
local holeRightSideBottomEdge =  {  { 1, 3 } }

local holeLeftTopCorner =  {  { 0, 0 } }
local holeRightTopCorner =  {  { 4, 0 } }
local holeRightBottomCorner =  {  { 4, 4 } }
local holeLeftBottomCorner =  {  { 0, 4 } }

local holeEmpty =  {  { 2, 2 } }

local frameTextures = {
    none = "objects/LylyraHelper/dashpaper/dashpaper",
	scissors = "objects/LylyraHelper/dashpaper/cloudblocknewScissors9tile" 
}

local decorations = {
    top =  {
		standard = "objects/LylyraHelper/dashpaper/dash_paper_decoration_up_24",
		wide = "objects/LylyraHelper/dashpaper/dash_paper_decoration_up_32"
	},
	center = {
		standard = "objects/LylyraHelper/dashpaper/dash_paper_decoration_center_32_32",
		wide = "objects/LylyraHelper/dashpaper/dash_paper_decoration_center_40_32",
		tall = "objects/LylyraHelper/dashpaper/dash_paper_decoration_center_32_40",
		big = "objects/LylyraHelper/dashpaper/dash_paper_decoration_center_40_40",
	},
	bottom = {
		standard = "objects/LylyraHelper/dashpaper/dash_paper_decoration_bottom_24",
		wide = "objects/LylyraHelper/dashpaper/dash_paper_decoration_bottom_32"
	}
}

local scissorsTexture = "objects/LylyraHelper/scissors/cutright00"

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

local papercolor = {202 / 255, 199 / 255, 227 / 255}

local function dump(o)
   if type(o) == 'table' then
      local s = '{ '
      for k,v in pairs(o) do
         if type(k) ~= 'number' then k = '"'..k..'"' end
         s = s .. '['..k..'] = ' .. dump(v) .. ','
      end
      return s .. '} '
   else
      return tostring(o)
   end
end

function paper.sprite(room, entity)
	local sprites = {}
	
	
	local tileIncrementerH = 0
	local tileIncrementerV = 0
	
    local x, y = entity.x or 0, entity.y or 0
	local frameTexture = frameTextures["none"]
    local width, height = entity.width or 24, entity.height or 24
	local tileSize = 8
    local tileWidth = 8
    local tileHeight = 8
    local borderLeft = tileWidth
    local borderRight = tileWidth
    local borderTop = tileHeight
    local borderBottom = tileHeight
    local realSize = false
	local tileIncrementer = 0
	
	
	local rectangle = drawableRectangle.fromRectangle("fill", x, y, width, height, papercolor)
	table.insert(sprites, rectangle)
	

	if entity.wallpaperMode == "From FG Decals" then
		if not (entity.decalStampData == nil and entity.decalStampData == "") then
		
			for decalStr in string.gmatch(entity.decalStampData, "([^;]+)") do
				local decalData = helpers.splitString(decalStr, ",")
				print(dump(decalData))
				local decalSprite = drawableSprite.fromTexture(decalData[1], {x = x + tonumber(decalData[2]), y = y + tonumber(decalData[3]), atlas = atlas})
				table.insert(sprites, decalSprite)
			end
		end
	else
		
		for i=0,width - 8,8 do
			local spriteTop = drawableSprite.fromTexture(frameTexture, {x = x + i, y = y, atlas = atlas})
			spriteTop:useRelativeQuad((tileIncrementer % 4 + 1) * 8, 0, tileWidth, tileHeight, false, false)
			table.insert(sprites, spriteTop)
			local spriteBottom = drawableSprite.fromTexture(frameTexture, {x = x + i, y = y + height - 8, atlas = atlas})
			spriteBottom:useRelativeQuad((tileIncrementer % 4 + 1) * 8, 5 * 8, tileWidth, tileHeight, false, false)
			table.insert(sprites, spriteBottom)
			tileIncrementer = tileIncrementer + 1
		end
		if width >= 32 then
			local borderSize = "standard"
			if width / 8 % 2 == 0 then
				borderSize = "wide"
			end
			local offsetBorder = -2
			if width / 8 % 2 == 1 then
				offsetBorder = -1.5
			end
			local topDecoration = drawableSprite.fromTexture(decorations["top"][borderSize], {x = x + width / 2 + offsetBorder * 8, y = y, atlas = atlas})
		
			topDecoration:setJustification(0.0, 0.0)
			local bottomDecoration = drawableSprite.fromTexture(decorations["bottom"][borderSize], {x = x + width / 2 + offsetBorder * 8, y = y + height - 16, atlas = atlas})
			bottomDecoration:setJustification(0.0, 0.0)
			table.insert(sprites, topDecoration)
			table.insert(sprites, bottomDecoration)
			if height >= 48 then
				borderSize = "standard"
				if (width / 8 % 2 == 1) then
					borderSize = "wide"
				end
				if (height / 8 % 2 == 1) then
					if borderSize == "wide" then 
						borderSize = "big"
					else 
						borderSize = "tall"
					end
				end
				local xOffset = -2
				if width / 8 % 2 == 1 then
					xOffset = -2.5
				end
				local yOffset = -2
				if height / 8 % 2 == 1 then
					yOffset = -2.5
				end
				local centerDecoration = drawableSprite.fromTexture(decorations["center"][borderSize], {x = x + width / 2 + xOffset * 8, y = y + height / 2 + yOffset * 8, atlas = atlas})
				centerDecoration:setJustification(0.0, 0.0)
				table.insert(sprites, centerDecoration)
			end
		end
	end
	if entity.spawnScissors then
		
		if width == 24 then
			scissorsSprite = drawableSprite.fromTexture(scissorsTexture, {x = x + width / 2 - 12, y = y + height / 2 - 32, atlas = atlas})
			scissorsSprite:setJustification(0.0, 0.0)
			scissorsSprite:useRelativeQuad(11, 0, 24, 64, false, false)
			
			table.insert(sprites, scissorsSprite)
		else
			local scissorsSprite = drawableSprite.fromTexture(scissorsTexture, {x = x + width / 2 - 24, y = y + height / 2 - 32, atlas = atlas})
			scissorsSprite:setJustification(0.0, 0.0)
			table.insert(sprites, scissorsSprite)
		end
	end
	
	return sprites
end


	

function paper.rotate(room, entity, direction)
    return true
end


return paper