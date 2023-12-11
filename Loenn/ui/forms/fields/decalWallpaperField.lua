local stringField = require("ui.forms.fields.string")
local state = require("loaded_state")
local utils = require("utils")
local languageRegistry = require("language_registry")
local uiElements = require("ui.elements")
local decalStruct = require("structs.decal")
local listOfTypeField = {}
listOfTypeField.fieldType = "LylyraHelper.DecalWallpaperField"


local function getEntitiesInBox(room)
    --a workaround until I can convince loenn devs to include the entity a formfield belongs to be easily accessible
    
    
    for _, entity in ipairs(room.entities) do
        if entity and entity["decalStampData"] and not entity.lockStampedDecals and entity.wallpaperMode == "From FG Decals" then
            local r1 = utils.rectangle(entity.x, entity.y, entity.width, entity.height)

            local builder = ""
            for __, decal in ipairs(room.decalsFg) do
            
                local r2 = nil
                local e2Width = decal.width or decal.Width or 8
                local e2Height = decal.height or decal.Height or 8
                r2 = utils.rectangle(decal.x, decal.y, e2Width, e2Height)
                local deltaX = decal.x - entity.x
                local deltaY = decal.y - entity.y
                if utils.aabbCheck(r1, r2) then
                    builder = builder..(decal.texture)..","..(deltaX)..","..(deltaY)..";"
                end
            end
            entity.decalStampData = string.sub(builder, 1, -2)
        end
    end

end

local function buttonPressed(formField)
    return function (element)
        getEntitiesInBox(state.getSelectedRoom())

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
    button.tooltipText = "*FrostHelper required*\nThis field will be set to the names of all entities overlapped by the Slicer Controller. This will overwrite any information currently there."-- tostring(language.ui.lylyrahelper.typeField.tooltip)
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