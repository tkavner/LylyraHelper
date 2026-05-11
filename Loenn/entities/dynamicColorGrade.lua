local helpers = require("mods").requireFromPlugin("helpers")

local dynamicColorGrade = {}

dynamicColorGrade.name = "LylyraHelper/DynamicColorGrade"
dynamicColorGrade.depth = -100
dynamicColorGrade.placements = {
    {
        name = "Dynamic Color Grade",
        data =
        helpers.createPlacementData(1, {
            oneUse = false
        })
    }
}

function dynamicColorGrade.texture(room, entity)
    return "objects/LylyraHelper/slicerController/slicerController"
end

return dynamicColorGrade
