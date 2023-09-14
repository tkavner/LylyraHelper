local ashWind = {}

ashWind.name = "LylyraHelper/ASHWind"
ashWind.canBackground = true
ashWind.canForeground = true

blendingModes = {"HSV", "RGB"}

ashWind.fieldInformation = {
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
    speed = 10.0,
    twist = 0.001,
    bend = 0.0,
    frequency = 2.0,
    speedVariance = 0.1,
    pointsPointWind = 600,
    windLifespan = 15,
    maxBend = 0.02
}
return ashWind