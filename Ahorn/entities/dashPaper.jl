module LylyraHelperDashPaper

using ..Ahorn, Maple

@mapdef Entity "LylyraHelper/DashPaper" DashPaper(x::Integer, y::Integer, width::Integer=32, height::Integer=32, spawnScissors::Bool=True, fragileScissors::Bool=True, noEffects::Bool=False)

const placements = Ahorn.PlacementDict(
    "Dash Paper (Lylyra Helper)" => Ahorn.EntityPlacement(
        DashPaper
    )
)

Ahorn.minimumSize(entity::DashPaper) = 16, 16
Ahorn.resizable(entity::DashPaper) = true, true

function Ahorn.selection(entity::DashPaper)
   x, y = Ahorn.position(entity)
   width = Int(get(entity.data, "width", 32))
   height = Int(get(entity.data, "height", 32))

   return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashPaper, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height)
end

end