module LylyraHelperAutoReturnFallingBlock

using ..Ahorn, Maple

@mapdef Entity "LylyraHelper/SS2024/AutoReturnFallingBlock" AutoReturnFallingBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8, climbFall::Bool=true, behind::Bool=false,resetDelay::Number=1.0, flagOnReset::String="", flagOnFall::String = "", flagOnLand::String = "", flagTrigger::String = "",resetFlagState::Bool = true,fallFlagState::Bool = true, landFlagState::Bool = true, maxSpeed::Number = 160.0,acceleration::Number = 500.0,direction::String = "Down",landingSound::String = "",returnSound::String = "",shakeSound::String = "",invertFlagTrigger::Bool = true,returnMaxSpeed::Number = 160.0,returnAcceleration::Number = 500.0)



const placements = Ahorn.PlacementDict(
    "Auto Return Falling Block" => Ahorn.EntityPlacement(
        AutoReturnFallingBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    ),
)

Ahorn.editingOptions(entity::AutoReturnFallingBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions(),
    "direction" => Maple.move_block_directions
)

Ahorn.minimumSize(entity::AutoReturnFallingBlock) = 8, 8
Ahorn.resizable(entity::AutoReturnFallingBlock) = true, true

Ahorn.selection(entity::AutoReturnFallingBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::AutoReturnFallingBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end