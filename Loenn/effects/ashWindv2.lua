local ashWind = {}

ashWind.name = "LylyraHelper/ASHWindv2"
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
    speed = 100.0,
    twist = 0.001,
    bend = 0.0,
    frequency = 2.0,
    speedVariance = 0.1,
    pointsPointWind = 10,
    windLifespan = 15,
    maxBend = 0.02,
    color = "FFFFFF",
    fadeColor = "FFFFFF",
    transparency = 0.3,
    hsvBlending = true
}
return ashWind