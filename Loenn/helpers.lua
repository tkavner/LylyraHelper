-- Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
-- See the LICENCE file in the repository root for full licence text.

--borrowed wholesale to guarentee 
local consts = require("mods").requireFromPlugin("consts")

local helpers = {}

--https://gist.github.com/jasonbradley/4357406 stolen from here cuz i cbf to write yet another color conversion function
function helpers.hex2rgb(hex)
    hex = hex:gsub("#","")
    return tonumber("0x"..hex:sub(1,2)), tonumber("0x"..hex:sub(3,4)), tonumber("0x"..hex:sub(5,6))
end

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

local function dumpInternal(o, limiter)
    if (limiter <= 0) then
        return ""
    end
    if type(o) == 'table' then
        local s = '{ '
        for k,v in pairs(o) do
            if type(k) ~= 'number' then k = '"'..k..'"' end
            s = s .. '['..k..'] = ' .. dumpInternal(v, limiter-1) .. ','
        end
        return s .. '} '
   else
      return tostring(o)
   end
end

function helpers.starts(String,Start)
   return string.len(String) >= string.len(Start) and string.sub(String,1,string.len(Start))==Start
end

local function tableContains(table, value)
  for k,v in pairs(table) do
    if (table[k] == value) then
      return true
    end
  end
  return false
end
--convenient for probing tables
--attempts find all paths
function helpers.findElementPathInTable(toSearch, element, limiter)
    local knownTableEntries = {}
    local nextRoundSearchableEntries = {}
    local currenRoundSearchableEntries = {}
    table.insert(currenRoundSearchableEntries, toSearch)
    local foundPath = ""
    local found = false
    limiter = limiter or 1000
    while not found do
        for currentPath,v in pairs(currenRoundSearchableEntries) do
            if tableContains(knownTableEntries, v) then

            else
                if (type(v) == 'table') then
                    for k,v2 in pairs(v) do --iterate all elements of current table being searched, place all subtables in nextRound
                        
                        if (type(k) == "string" and helpers.starts(k, element)) then 
                            foundPath = foundPath.."\nPath for "..tostring(element)..":"..tostring(currentPath)..k
                        elseif (type(v2) == "string" and helpers.starts(v2, element)) then
                            foundPath = foundPath.."\nPath for "..tostring(element)..":"..tostring(currentPath)..","..k
                        end
                        if (type(v2) == 'table') then 
                            if not tableContains(knownTableEntries, k) then
                                nextRoundSearchableEntries[currentPath .. ','..tostring(k)] = v2

                            end
                        end
                    end
                    table.insert(knownTableEntries, v)
                else 
                end
            end
        end
        currenRoundSearchableEntries = nextRoundSearchableEntries
        nextRoundSearchableEntries = {}
        limiter = limiter - 1
        if limiter < 0 then 
            break 
        end
    end
    return foundPath
end

function helpers.findElementFromString(toSearch, element, limiter)
    local knownTableEntries = {}
    local nextRoundSearchableEntries = {}
    local currenRoundSearchableEntries = {}
    table.insert(currenRoundSearchableEntries, toSearch)
    local foundPath = ""
    local found = false
    limiter = limiter or 1000
    while not found do
        for currentPath,v in pairs(currenRoundSearchableEntries) do
            if tableContains(knownTableEntries, v) then

            else
                if (type(v) == 'table') then
                    for k,v2 in pairs(v) do --iterate all elements of current table being searched, place all subtables in nextRound
                        
                        if (type(k) == "string" and helpers.starts(k, element)) then 
                            return v
                        elseif (type(v2) == "string" and helpers.starts(v2, element)) then
                            return v
                        end
                        if (type(v2) == 'table') then 
                            if not tableContains(knownTableEntries, k) then
                                nextRoundSearchableEntries[currentPath .. ','..tostring(k)] = v2

                            end
                        end
                    end
                    table.insert(knownTableEntries, v)
                else 
                end
            end
        end
        currenRoundSearchableEntries = nextRoundSearchableEntries
        nextRoundSearchableEntries = {}
        limiter = limiter - 1
        if limiter < 0 then 
            break 
        end
    end
    return nil
end

function helpers.dumpTable(o, limiter)
   return dumpInternal(o, limiter)
end
function helpers.print_table(node)
    local cache, stack, output = {},{},{}
    local depth = 1
    local output_str = "{\n"

    while true do
        local size = 0
        for k,v in pairs(node) do
            size = size + 1
        end

        local cur_index = 1
        for k,v in pairs(node) do
            if (cache[node] == nil) or (cur_index >= cache[node]) then

                if (string.find(output_str,"}",output_str:len())) then
                    output_str = output_str .. ",\n"
                elseif not (string.find(output_str,"\n",output_str:len())) then
                    output_str = output_str .. "\n"
                end

                -- This is necessary for working with HUGE tables otherwise we run out of memory using concat on huge strings
                table.insert(output,output_str)
                output_str = ""

                local key
                if (type(k) == "number" or type(k) == "boolean") then
                    key = "["..tostring(k).."]"
                else
                    key = "['"..tostring(k).."']"
                end

                if (type(v) == "number" or type(v) == "boolean") then
                    output_str = output_str .. string.rep('\t ',depth) .. key .. " = "..tostring(v)
                elseif (type(v) == "table") then
                    output_str = output_str .. string.rep('\t ',depth) .. key .. " = {\n"
                    table.insert(stack,node)
                    table.insert(stack,v)
                    cache[node] = cur_index+1
                    break
                else
                    output_str = output_str .. string.rep('\t',depth) .. key .. " = '"..tostring(v).."'"
                end

                if (cur_index == size) then
                    output_str = output_str .. "\n" .. string.rep('\t ',depth-1) .. "}"
                else
                    output_str = output_str .. ","
                end
            else
                -- close the table
                if (cur_index == size) then
                    output_str = output_str .. "\n" .. string.rep('\t ',depth-1) .. "}"
                end
            end

            cur_index = cur_index + 1
        end

        if (size == 0) then
            output_str = output_str .. "\n" .. string.rep('\t ',depth-1) .. "}"
        end

        if (#stack > 0) then
            node = stack[#stack]
            stack[#stack] = nil
            depth = cache[node] == nil and depth + 1 or depth - 1
        else
            break
        end
    end

    -- This is necessary for working with HUGE tables otherwise we run out of memory using concat on huge strings
    table.insert(output,output_str)
    output_str = table.concat(output)

    file = io.open("testlyradump.txt", "w")
    file:write(output_str)
    file:close()
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