local helpers = require("mods").requireFromPlugin("helpers")
local consts = require("mods").requireFromPlugin("consts")

local controller = {}

controller.name = "LylyraHelper/KuwaharaBlurController"
controller.depth = -100
controller.ignoredFields = consts.ignoredFields
controller.placements = {
    {
        name = "main",
        data =
        helpers.createPlacementData(1, {
            flag = "",
            on = true,
            oneTime = false,
            revertable = true

        })
    }
}

function controller.texture(room, entity)
    return "objects/LylyraHelper/slicerController/slicerController"
end

return controller
