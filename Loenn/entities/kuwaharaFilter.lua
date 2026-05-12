local helpers = require("mods").requireFromPlugin("helpers")

local controller = {}

controller.name = "LylyraHelper/KuwaharaBlurController"
controller.depth = -100
controller.placements = {
    {
        name = "Kuwahara Blur Controller",
        data =
        helpers.createPlacementData(1, {
            flag = "",
            on = true,
            oneTime = false

        })
    }
}

function controller.texture(room, entity)
    return "objects/LylyraHelper/slicerController/slicerController"
end

return controller
