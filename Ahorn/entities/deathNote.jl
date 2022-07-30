module LylyraHelperDeathNote

using ..Ahorn, Maple

@mapdef Entity "LylyraHelper/DeathNote" DeathNote(x::Integer, y::Integer, width::Integer=32, height::Integer=32)

const placements = Ahorn.PlacementDict(
    "Death Note (Lylyra Helper)" => Ahorn.EntityPlacement(
        DeathNote
    )
)

Ahorn.minimumSize(entity::DeathNote) = 16, 16
Ahorn.resizable(entity::DeathNote) = true, true

function Ahorn.selection(entity::DeathNote)
   x, y = Ahorn.position(entity)
   width = Int(get(entity.data, "width", 32))
   height = Int(get(entity.data, "height", 32))

   return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DeathNote, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height)
end

end