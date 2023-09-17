local starGodray = {}

starGodray.name = "LylyraHelper/StarGodray"
starGodray.canBackground = false
starGodray.canForeground = true

blendingModes = {"HSV", "RGB"}

starGodray.fieldInformation = {
    color = {
        fieldType = "color"
    },
    fadeColor = {
        fieldType = "color"
    },
    numberOfRays = {
        fieldType = "integer",
        minimumValue = 1
    },
    blendingMode = {
        options = blendingModes,
        editable = false
    }
}

starGodray.defaultData = {
    color = "FFFFFF",
    fadeColor = "FFFFFF",
    numberOfRays = 6,
    speedX = 0.0,
    speedY = 8.0,
    rotation = 0.0,
    rotationRandomness = 0.0,
    blendingMode="HSV"
}

return starGodray