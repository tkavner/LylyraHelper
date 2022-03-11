module LylyraHelperChessBlock

using ..Ahorn, Maple

@mapdef Entity "LylyraHelper/ChessBlock" ChessBlock(x::Integer, y::Integer, width::Integer=32, height::Integer=32, color::String="White", type::String="Rook", dashes::Integer=1)

const placements = Ahorn.PlacementDict()
const directions = ["Rook", "Bishop", "Queen"]
const colors = ["White", "Black"]

for i in directions
    for j in colors
        key = "Chess Block ($(j) $(i)) (Lylyra Helper)"
        placements[key] = Ahorn.EntityPlacement(
            ChessBlock,
            "rectangle",
            Dict{String, Any}(
                "type" => "$(i)",
                "color" => "$(j)",
            )
        )
    end
end

Ahorn.minimumSize(entity::ChessBlock) = 8, 8
Ahorn.resizable(entity::ChessBlock) = true, true

Ahorn.editingOptions(entity::ChessBlock) = Dict{String, Any}(
    "color" => String["White", "Black"],
    "type" => String["Rook", "Bishop", "Queen"]
)

function Ahorn.selection(entity::ChessBlock)
    x, y = Ahorn.position(entity)
    
    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height)]

end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ChessBlock)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))
    color = String(get(entity.data, "color", "Black"))
    type = String(get(entity.data, "type", "Rook"))
    dashes = Int(get(entity.data, "dashes", 1))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    if color == "Black"
        Ahorn.drawRectangle(ctx, 0, 0, width, height, Ahorn.defaultBlackColor)
        Ahorn.drawRectangle(ctx, 3, 3, width - 6, height - 6, Ahorn.defaultWhiteColor)
        Ahorn.drawRectangle(ctx, 4, 4, width - 8, height - 8, Ahorn.defaultBlackColor)
    else
        Ahorn.drawRectangle(ctx, 0, 0, width, height, Ahorn.defaultWhiteColor)
        Ahorn.drawRectangle(ctx, 3, 3, width - 6, height - 6, Ahorn.defaultBlackColor)
        Ahorn.drawRectangle(ctx, 4, 4, width - 8, height - 8, Ahorn.defaultWhiteColor)
    end

    pieceSprite = "objects/LylyraHelper/chessBlock/$(color)$(type)"
    numberSprite = "objects/LylyraHelper/chessBlock/$(color)$(dashes)"
    Ahorn.drawSprite(ctx, pieceSprite, width / 2, height / 2)
    if dashes > 0
        Ahorn.drawSprite(ctx, numberSprite, width / 2, height / 2)
    end
end

end