local hexGodray = {}

hexGodray.name = "LylyraHelper/HexagonalGodray"
hexGodray.canBackground = false
hexGodray.canForeground = true

blendingModes = {"HSV", "RGB"}

hexGodray.fieldInformation = {
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
    },
    renderBorderExtendX = {
        minimumValue = 0
    },
    renderBorderExtendY = {
        minimumValue = 0
    }
}

hexGodray.defaultData = {
    color = "FFFFFF",
    fadeColor = "FFFFFF",
    numberOfRays = 6,
    speedX = 0.0,
    speedY = 8.0,
    rotation = 0.0,
    rotationRandomness = 0.0,
    blendingMode="HSV",
    renderBorderExtendX=0.0,
    renderBorderExtendY=0.0
}

return hexGodray