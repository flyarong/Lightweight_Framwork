--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by shenyi.
--- DateTime: 2020/8/13
-----------------------------------------------------------
require("Logic/Entity/Common/EntityType")
require("Logic/Entity/Common/PropertyNames")
require("Logic/Entity/Common/CompIndex")
---@class Entity:Updatable
Entity = BaseClass("Entity", Updatable)
-----------------------------------------------------------
local compIndex = CompIndex
local aoiTrans = AOITrans
local aoiStatus = AOIStatus

---@param aoiData AOIData
function Entity:ctor(aoiData)
    local sceneId = RoleModel.RoleAttrib.sceneId
    ---@type AOIData
    self.aoiData = aoiData

    ---@type Game.EntityBehavior
    self.behavior = EntityBehaviorManager.CreateEntity(
            sceneId,
            aoiData.aoiId,
            aoiData.attrib.modelId,
            { x = aoiData.trans.pos_x, y = aoiData.trans.pos_y, z = aoiData.trans.pos_z },
            aoiData.trans.forward,
            aoiData.attrib.type,
            function(components)
                coroutine.start(function()
                    ---@type Game.NameComp
                    self.nameComp = components[compIndex.Name]
                    ---@type Game.AnimComp
                    self.animComp = components[compIndex.Anim]
                    ---@type Game.RotateComp
                    self.rotateComp = components[compIndex.Rotate]

                    self:OnBodyCreate(components)
                end)
            end)
    ---@type UnityEngine.GameObject
    self.gameObject = self.behavior.gameObject
    ---@type UnityEngine.Transform
    self.transform = self.behavior.transform
    ---共享位置表
    local transTab = self.behavior.TransTable
    for name, key in pairs(aoiTrans) do
        transTab[key] = aoiData.trans[name]
    end
    self.aoiData.trans = transTab
    ---共享状态表
    local statusTab = self.behavior.StatusTable
    for name, key in pairs(aoiStatus) do
        statusTab[key] = aoiData.status[name]
    end
    self.aoiData.status = statusTab
end

---@ 实体元素创建的时候回调
function Entity:OnBodyCreate(components) end

function Entity:dtor()
    self.behavior:RemoveAllListeners()
    EntityBehaviorManager.DestroyEntity(self.aoiData.aoiId)
end