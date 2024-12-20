﻿module LylyraHelperBubbledScissors

using ..Ahorn, Maple

@mapdef Entity "LylyraHelper/BubbledScissors (WIP)" BubbledScissors(x::Integer, y::Integer, fragile::Bool=true)

const placements = Ahorn.PlacementDict(
    "BubbledScissors (LylyraHelper)" => Ahorn.EntityPlacement(
        BubbledScissors
    )
)

sprite = "object/LylyraHelper/BubbledScissors/bubble00.png"

function Ahorn.selection(entity::BubbledScissors)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y - 10)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BubbledScissors, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, -10)

end