local ashWind = {}

ashWind.name = "LylyraHelper/SS2024/AtmosphericWind"
ashWind.canBackground = true
ashWind.canForeground = true

blendingModes = {"HSV", "RGB"}

ashWind.fieldInformation = {
    color = {
        fieldType = "color"
    },
    fadeColor = {
        fieldType = "color"
    },
    initAngle = {
        fieldType = "number"
    },
    pointsPointWind = {
        fieldType = "integer"
    },
}

ashWind.defaultData = {
    initAngle = 0,
    initAngleVariance = 0.1,
    angularJerk = 0.05,
    startingAngularAcceleration = 0.0,
    frequency = 2.0,
    pointsPointWind = 600,
    speed = 6.0,
    speedVariance = 0.1,
    windLifespan = 15,
    maxAngularAcceleration = 1.0,
    scrollX = 1.0,
    scrollY = 1.0,
    color = "FFFFFF",
    fadeColor = "FFFFFF",
    transparency = 0.3,
    hsvBlending = true
}

return ashWind