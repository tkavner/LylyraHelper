module LylyraHelperCloudBlock

using ..Ahorn, Maple

@mapdef Entity "LylyraHelper/CloudBlock" CloudBlock(x::Integer, y::Integer, width::Integer=32, height::Integer=32)

const placements = Ahorn.PlacementDict(
    "Cloud Block (Lylyra Helper)" => Ahorn.EntityPlacement(
        CloudBlock
    )
)

Ahorn.minimumSize(entity::CloudBlock) = 16, 16
Ahorn.resizable(entity::CloudBlock) = true, true

function Ahorn.selection(entity::CloudBlock)
   x, y = Ahorn.position(entity)
   width = Int(get(entity.data, "width", 32))
   height = Int(get(entity.data, "height", 32))

   return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CloudBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height)
end

end