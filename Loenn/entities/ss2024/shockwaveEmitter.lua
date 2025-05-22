local shockwaveEmitter = {}

shockwaveEmitter.name = "LylyraHelper/SS2024/ShockwaveEmitter"
shockwaveEmitter.depth = 100

modes = {"Kill", "Knockback"}
drawModes = {"energyWave", "displacement"}

shockwaveEmitter.fieldInformation = {
	mode = {
		options = modes,
        editable = false
	},
	drawModes = {
		options = modes,
        editable = false
	}
}
shockwaveEmitter.placements = {
    {
        name = "Shockwave Emitter",
        data = {
            focalRatio = "1.5",
            initialSize = "1",
            timers = "3,3,5",
            shockwaveThickness = "3",
            expand = "30",
            breakoutSpeeds = "30",
            flag = "shockwaveStarter",
            cycle = false,
            absoluteMaxGlobs = "4000",
            renderPointsOnMesh = "2000",
            launchPower = "1",
            ignorePlayerSpeedChecks=false,
            mode="Kill",
            noSprite = false,
            renderMode="energyWave"
        }

    }
}

function shockwaveEmitter.texture(room, entity)
    return "objects/LylyraHelper/ss2024/ellipticalShockwave/hydro_ancientgenerator00"
end

return shockwaveEmitter