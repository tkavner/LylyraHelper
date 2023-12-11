-- Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
-- See the LICENCE file in the repository root for full licence text.

--borrowed wholesale to guarentee 
local consts = require("mods").requireFromPlugin("consts")

local helpers = {}

function helpers.union(...)
    local tbl = {}
    local source = {...}
    for _,t in ipairs(source) do
        for k,v in pairs(t) do tbl[k] = v end
    end
    return tbl
end

function helpers.colorWithAlpha(color, alpha)
    return { color[1], color[2], color[3], alpha }
end

--thank you to samah for this amazing plugin versioning system
function helpers.createPlacementData(pluginVersion, data)
    return helpers.union({
        modVersion = consts.modVersion,
        pluginVersion = pluginVersion,
    }, data)
end


function helpers.splitString(inputstr, sep)
        if sep == nil then
                sep = "%s"
        end
        local t={}
        for str in string.gmatch(inputstr, "([^"..sep.."]+)") do
                table.insert(t, str)
        end
        return t
end

return helpers