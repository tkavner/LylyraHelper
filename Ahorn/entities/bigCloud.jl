module LylyraHelperBigCloud

using ..Ahorn, Maple

@mapdef Entity "LylyraHelper/BigCloud" BigCloud(x::Integer, y::Integer, fragile::Bool=true)

const placements = Ahorn.PlacementDict(
    "Big Cloud (Lylyra Helper)" => Ahorn.EntityPlacement(
        BigCloud
    )
)

const sprite = "objects/LylyraHelper/bigCloud/bigcloud00"

function Ahorn.selection(entity::BigCloud)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BigCloud) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end