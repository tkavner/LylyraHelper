local stringField = require("ui.forms.fields.string")
local state = require("loaded_state")
local utils = require("utils")
local languageRegistry = require("language_registry")
local uiElements = require("ui.elements")
local listOfTypeField = {}
listOfTypeField.fieldType = "LylyraHelper.TypeField"


--much of this was adapted from frosthelper's source code because ja is a saint of open sourcing and letting me see how to do this stuff
local function getEntitiesInBox(room)
    local theEntity = nil
    for _, entity in ipairs(room.entities) do
        if entity and entity["_name"] and (entity["_name"] == "LylyraHelper/SlicerController" or entity["_name"] == "Slicer Controller") then
            theEntity = entity
            break
        end
    end
    if theEntity and theEntity.width and theEntity.height  then
        local r1 = utils.rectangle(theEntity.x, theEntity.y, theEntity.width, theEntity.height)
        local listofEntityNames = {}
        for _, entity in ipairs(room.entities) do
            local r2 = nil
            local e2Width = entity.width or entity.Width or 8
            local e2Height = entity.height or entity.Height or 8
            r2 = utils.rectangle(entity.x, entity.y, e2Width, e2Height)
            if utils.aabbCheck(r1, r2) then
                if not (utils.contains(entity["_name"], listofEntityNames) or entity["_name"] == "player") then
                    if theEntity["_name"] ~= entity["_name"] then
                        table.insert(listofEntityNames, entity["_name"])
                    end
                end
            end
        end
        local nameAsString = ""
        --format table into string here
        for _, entityname in ipairs(listofEntityNames) do
            nameAsString = nameAsString..entityname..","
        end
        if #nameAsString == 0 then
            return " "
        end
        return string.sub(nameAsString, 1, -2)
    else
        return "SlicerControllerNotFound"
    end
end

local function buttonPressed(formField)
    return function (element)

        formField.field.text = tostring(getEntitiesInBox(state.getSelectedRoom()))
        if formField.field.text == "" then 
            formField.field.text = "" 
        end
        formField.field.index = #formField.field.text

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

    local button = uiElements.button("Set from Intersection", buttonPressed(formField))

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