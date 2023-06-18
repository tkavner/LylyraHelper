module LylyraHexagonalGodray

using ..Ahorn, Maple

@mapdef Effect "LylyraHelper/HexagonalGodray" HexagonalGodray(only::String="*", exclude::String="", color::String="FFFFFF", fadeColor::String="FFFFFF", numberOfRays::Integer=6, speedX::Number=0.0, speedY::Number=8.0, rotation::Number=0.0, rotationRandomness::Number=0.0, hsvBlending::Bool=false)

placements = HexagonalGodray

function Ahorn.canFgBg(effect::HexagonalGodray)
    return true, true
end

end