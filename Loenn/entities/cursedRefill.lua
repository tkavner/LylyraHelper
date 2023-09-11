local refill = {}

refill.name = "LylyraHelper/CursedRefill"
refill.depth = -100
refill.placements = {
    {
        name = "Cursed Refill",
        data = {
            oneUse = false
        }
    }
}

function refill.texture(room, entity)
    return "objects/refill/idle00"
end

return refill