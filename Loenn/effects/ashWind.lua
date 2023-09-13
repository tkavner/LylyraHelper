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
    numWinds = 5,
    initAngle = 0.0,
    speed = 10.0,
    twist = 1.0,
    bend = 1.0
}
return ashWind