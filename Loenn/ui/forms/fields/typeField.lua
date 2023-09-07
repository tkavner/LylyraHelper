local stringField = require("ui.forms.fields.string")
local state = require("loaded_state")
local utils = require("util.utils")

local listOfTypeField = {}
listOfTypeField.fieldType = "LylyraHelper.TypeField"

local function getEntitiesInBox(room)
    local theEntity = nil
    for entity in room.entities do
        if entity and entity.name == "TBA-LockOn-Controller-Entity" then
            theEntity = Entity
            break
        end
    end
    local r1 = utils.rectangle(theEntity.x, theEntity.y, theEntity.Width, theEntity.Height)
    if theEntity then
        local listofEntityNames = {}
        for entity in room.entities do
            local r2 = utils.rectangle(entity.x, entity.y, entity.Width, entity.Height)
            if utils.aabbCheck(r1, r2) then
                if not utils.contains(entity.name, listofEntityNames)
                    tables.insert(listofEntityNames, entity.name)
                end
            end
        end
        local namesAsString = ""
        --format table into string here
        for _, entityname in ipairs(listofEntityNames) do
            nameAsString = nameAsString..entityname..","
        end
        return string.sub(nameAsString, 1, -2)
    else
        return ""
    end

end

local function buttonPressed(formField)
    return function (element)
        
        



        formField.field.text = getEntitiesInBox(state.getSelectedRoom())--should return the list of stuff here
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

    self.button.x = -font:getWidth(text) + self.minWidth + offset - 40
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

        return utils.isString(string)
    end
    options.options = attachGroupHelper.findAllGroupsAsList(loadedState.getSelectedRoom())

    local formField = stringField.getElement(name, value, options)

    local button = uiElements.button("Auto Add", buttonPressed(formField))

    button.style.padding *= 0.36
    button.style.spacing = 0
    button.tooltipText = "Placeholder Tooltip"-- tostring(language.ui.lylyrahelper.typeField.tooltip)
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