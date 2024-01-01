local stringField = require("ui.forms.fields.string")
local state = require("loaded_state")
local utils = require("utils")
local languageRegistry = require("language_registry")
local uiElements = require("ui.elements")
local decalStruct = require("structs.decal")
local helpers = require("mods").requireFromPlugin("helpers")
local contextWindow = require("ui.windows.selection_context_window")
local windows = require("ui.windows")
local listOfTypeField = {}
listOfTypeField.fieldType = "LylyraHelper.DecalWallpaperField"

local function findValue(table, value)
  for k,v in pairs(table) do
    if table[k] and table[k]._textStr and helpers.starts(table[k]._textStr, value) then
      return k
    end
  end
  return false
end

local function getEntitiesInBox(formField, room)
    --absolutely cursed workaround until I can convince loenn devs to include the entity a formfield belongs to be easily accessible
    local edittingText = formField.label.__ui.root.all[findValue(formField.label.__ui.root.all, "Editing Selection")]:getText()
    local index = edittingText:match'^.*() '
    local PaperID = tonumber(string.sub(edittingText,index,string.len(edittingText)))
    local ourPaper = nil
    for _, entity in ipairs(room.entities) do
        if tonumber(entity["_id"]) == PaperID then
            ourPaper = entity
            break
        end
    end
    if ourPaper == nil then
        return ""
    end
    local PaperX = ourPaper.x
    local PaperY = ourPaper.y
    local PaperWidth = ourPaper.width
    local PaperHeight = ourPaper.height
    local r1 = utils.rectangle(PaperX, PaperY, PaperWidth, PaperHeight)
    local builder = ""
    for _, decal in ipairs(room.decalsFg) do
            
        local r2 = nil
        local e2Width = decal.width or decal.Width or 8
        local e2Height = decal.height or decal.Height or 8
        r2 = utils.rectangle(decal.x, decal.y, e2Width, e2Height)
        local deltaX = decal.x - PaperX
        local deltaY = decal.y - PaperY
        if utils.aabbCheck(r1, r2) then
            builder = builder..(decal.texture)..","..(deltaX)..","..(deltaY)..";"
        end
    end    
    
    return string.sub(builder, 1, -2)        


end

local function buttonPressed(formField)
    return function (element)
        
        formField.field.text = tostring(getEntitiesInBox(formField, state.getSelectedRoom()))

        formField:notifyFieldChanged()
    end
end

local function fieldCallback(self, value, prev)
    local text = self.text or ""
    local font = self.label.style.font
    local button = self.button

    -- should just be button.width, but that isn't correct initially :(
    local offset = -font:getWidth(button.text) - (2 * button.style.padding)

    self.button.x = -font:getWidth(text) + self.minWidth + offset
    self.button.y = 20
end

function listOfTypeField.getElement(name, value, options)
    -- Add extra options and pass it onto string field
    local language = languageRegistry.getLanguage()

    options.valueTransformer = valueTransformer
    options.displayTransformer = displayTransformer
    options.validator = function(v)
        if not v then
            v = ""
        end

        local string = tostring(v)

        return type(string) == "string"
    end

    local formField = stringField.getElement(name, value, options)

    local button = uiElements.button("Set From FGDecals", buttonPressed(formField))

    button.style.padding *= 0.36
    button.style.spacing = 0
    button.tooltipText = "*FrostHelper required*\nThis field will be set to the names of all FG Decals whose *centers* OF ALL PAPER are overlapped by the paper. This will overwrite any information currently there.\n\nMAKE SURE TO SET "-- tostring(language.ui.lylyrahelper.typeField.tooltip)
    formField.field:addChild(button)
    formField.field.button = button

    local orig = formField.field.cb
    formField.field.cb = function (...)
        orig(...)
        fieldCallback(...)
    end

    --formField.formFieldChanged = fieldChangedCallback

    fieldCallback(formField.field, formField.field.text, "")
    --formField.field.cb(formField.field, formField.field.text, "")

    return formField
end

return listOfTypeField