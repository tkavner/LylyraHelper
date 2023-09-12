local refill = {}

refill.name = "LylyraHelper/UncursedRefill"
refill.depth = -100
refill.placements = {
    {
        name = "Uncursed Refill",
        data = {
            oneUse = false
        }
    }
}

function refill.texture(room, entity)
    return "objects/LylyraHelper/uncurseddash/idle00"
end

return refill