local ashWind = {}

ashWind.name = "LylyraHelper/ASHWind"
ashWind.canBackground = false
ashWind.canForeground = true

blendingModes = {"HSV", "RGB"}

ashWind.fieldInformation = {
    numWinds = {
        fieldType = "integer"
    },
    initAngle = {
        fieldType = "number"
    }
}

ashWind.defaultData = {
    x = 0.0,
    y = 0.0,
    numWinds = 5,
    initAngle = 3.4,
    initAngleVarience = 0.1,
    speed = 10.0,
    twist = 1.0,
    bend = 1.0,
    frequency = 2,
    speedVarience = 0.1
}
return ashWind