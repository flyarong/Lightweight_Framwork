// ========================================================
// des：
// author: shenyi
// time：2020-12-10 19:16:30
// version：1.0
// ========================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
	public class EntityBehaviorManager
	{
		/// <summary>
		/// 模型信息
		/// </summary>
		private static Dictionary<int, ModelConfig> Id2ModelCondig = new Dictionary<int, ModelConfig>();
		/// <summary>
		/// 实体对象池
		/// </summary>
		/// <typeparam name="EntityBehavior"></typeparam>
		/// <returns></returns>
		private static MonoObjectPool<EntityBehavior> entitiesPool = new MonoObjectPool<EntityBehavior>();

		/// <summary>
		/// 按照uid存取
		/// </summary>
		/// <typeparam name="string"></typeparam>
		/// <typeparam name="EntityBehavior"></typeparam>
		/// <returns></returns>
		private static Dictionary<string, EntityBehavior> entityBehaviors = new Dictionary<string, EntityBehavior>();

		/// <summary>
		/// 用于按照顺序遍历
		/// </summary>
		/// <typeparam name="EntityBehavior"></typeparam>
		/// <returns></returns>
		private static LinkedList<EntityBehavior> entityBehaviorsQueue = new LinkedList<EntityBehavior>();

		private static GameObject m_EntityContainer;
		public static GameObject entityContainer
		{
			get
			{
				if (m_EntityContainer == null)
				{
					m_EntityContainer = new GameObject("Models");
					//m_EntityContainer.AddComponent()
				}

				m_EntityContainer.transform.localScale = Vector3.one;
				return m_EntityContainer;
			}
		}

		public async static Task Init()
		{
			ModelConfigAsset modelConfig = await ResourceManager.LoadAsset("Assets/Art/Assets/Config/ModelConfig.asset") as ModelConfigAsset;
			ModelConfig[] configs = modelConfig.Configs;
			for (int i = 0; i < configs.Length; i++)
			{
				int id = configs[i].ID;
				Id2ModelCondig.Add(id, configs[i]);
			}
		}


		private static async void CreateBody(EntityBehavior entity)
		{
			entity.bodyLoading = true;
			string uid = entity.uid;
			ModelConfig config;
			if (!Id2ModelCondig.TryGetValue(entity.res_id, out config))
				return;
			var obj = await ResourceManager.LoadPrefabFromePool(config.Resource);
			if (!obj) return;
			// || proxyid != entity.proxyId 感觉这里逻辑id不同时 没必要回收body
			// 先这样改一下
			if (entity == null || uid != entity.uid || entity.bodyLoading == false)
			{
				ResourceManager.RecyclePrefab(obj); // 只回收Body
				return;
			}

			EntityBehavior p = GetEntity(entity.uid);
			if (p != null && p != entity)
			{
				Debug.LogError("这个错误可以无视 保留查看而已 entity create error");
				ResourceManager.RecyclePrefab(obj); // 只回收Body
				return;
			}

			GameObject go = obj as GameObject;
			if (p != null)
			{
				go.SetActive(true);
				go.name = "@Body_Entity@";
				p.Body = go;
				go.transform.SetParent(p.transform, false);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				LayerMask defaultLayer = go.layer;

				if (p.entityType == 101)
				{
					ModelUtils.SetLayerWithChildren(p.Body, "Hero");
				}
				p.InitComp(true);
				entity.bodyLoading = false;
			}
			else
			{
				ResourceManager.RecyclePrefab(go); // 只回收Body
				entity.bodyLoading = false;
			}
		}
		/// <summary>
		/// 走帧逻辑
		/// </summary>
		/// <param name="fUpdateTime"></param>
		public static void Update(float fUpdateTime)
		{
			int DeleteDelay = 10; /// 每帧最多删除的数量
			int CeateResDelay = 10; /// 每帧最多加载的数量
			int CeateDelay = 3; /// 创建延迟
			var Enumerator = entityBehaviorsQueue.First; ///Enumertor实体
			// 遍历队列中的所有实体
			while (Enumerator != null)
			{
				EntityBehavior entity = Enumerator.Value;
				if (entity.Destroyed)
				{
					Enumerator = Enumerator.Next;
					// 每帧最多删除
					if (DeleteDelay > 0)
					{
						DestroyEntity(entity);
						DeleteDelay--;
					}
				}
				else
				{
					if ((CeateResDelay > 0 || entity.isHero == true) && entity.Body == null && entity.bodyLoading == false)
					{
						CeateResDelay--;
						CreateBody(entity);
					}
					if (entity.onBodyCreate != null && entity.Body != null && (CeateDelay > 0 || entity.isHero == true))
					{
						entity.onBodyCreate = null;
						CeateDelay--;
					}
					if (entity.Body != null && entity.onBodyCreate == null)
						entity.UpdateLogic(fUpdateTime);

					Enumerator = Enumerator.Next;
				}
			}
		}

		/// <summary>
		/// 创建实体
		/// </summary>
		/// <param name="sceneid"></param>
		/// <param name="uid"></param>
		/// <param name="proxyId"></param>
		/// <param name="res">预设体AddressableKey</param>
		/// <param name="bornPos">出生位置</param>
		/// <param name="orientation">方向</param>
		/// <param name="scale">缩放</param>
		/// <param name="entityType">实体类型</param>
		/// <param name="onBodyCreated">实体的组件</param>
		/// <returns></returns>
		public static EntityBehavior CreateEntity(int sceneid, string uid, int res_id, Vector3 bornPos, float orientation, int entityType, Action<EntityComp[]> onBodyCreated = null)
		{
			// uid 相同
			if (entityBehaviors.ContainsKey(uid))
			{
				Debug.LogError("CreateEntity error exist uid=" + uid);
				return null;
			}


			EntityBehavior entity = entitiesPool.Alloc();
			//GameObject go = new GameObject();
			//EntityBehavior entity = go.AddComponent<EntityBehavior>();

			if (entity == null)
				Debug.LogError("entity is nil");

			entity.res_id = res_id;
			// if (entityType == 1) {
			// 	entity.isHero = uid == heroUID;
			// } else {
			// 	entity.isHero = false;
			// }

			// 设置父物体
			GameObject parent = entity.gameObject;
			if (!parent.activeSelf)
				parent.SetActive(true);
			parent.transform.localScale = Vector3.one;
			parent.transform.SetParent(entityContainer.transform, false);
			parent.transform.position = bornPos;

			entity.uid = uid;
			entity.onBodyCreate = null;
			entity.sceneid = sceneid;
			entity.entityType = entityType;
			// 把实体放到容器中

			entityBehaviors.Add(uid, entity);
			//entityBehaviors_p.Add (proxyId, entity);
			entityBehaviorsQueue.AddFirst(entity);

			entity.transform.rotation = Quaternion.Euler(0, orientation, 0);
			entity.onBodyCreate = onBodyCreated;
			//entity.onBodyCreate = null;
			entity.InitComp(false);

			return entity;
		}

		/// <summary>
		/// 删除指定实体
		/// </summary>
		/// <param name="entity"></param>
		private static void DestroyEntity(EntityBehavior entity)
		{
			// 从队列移除
			entityBehaviorsQueue.Remove(entity);
			if (entity && entity.gameObject)
			{
				GameObject body = entity.Body;
				if (body != null)
				{
					body.name = entity.gameObject.name;
					//ResourceManager.RecyclePrefab(body.gameObject);   // 只回收Body
				}

				// 从对象池移除
				if (entity.bodyLoading) //强制删除免得回调重复赋值
					entitiesPool.Recycle(entity); // TODO 对象池提供一个的方法立即回收的方法
				else
					entitiesPool.Recycle(entity);
			}


		}

		/// <summary>
		/// 通过uid删除指定的实体
		/// </summary>
		/// <param name="uid"></param>
		public static void DestroyEntity(string uid)
		{
			if (entityBehaviors.ContainsKey(uid))
			{
				EntityBehavior entity = entityBehaviors[uid];
				entity.Destroyed = true;
				entityBehaviors.Remove(uid);
				//暂时先这样
				DestroyEntity(entity);
			}
			else
			{
				Debug.Log("DestroyEntity error not found uid=" + uid);
			}
		}

		/// <summary>
		/// 销毁所有Entity TODO
		/// </summary>
		/// <param name="uid"></param>
		public static void DestroyAllEntity()
		{
			List<string> ids = new List<string>();
			foreach (var item in entityBehaviors)
				ids.Add(item.Key);
			ids.ForEach(p => DestroyEntity(p));
		}

		public static EntityBehavior GetEntity(string uid)
		{
			EntityBehavior entity = null;
			entityBehaviors.TryGetValue(uid, out entity);
			return entity;
		}

		public static void Cleanup()
		{
			entitiesPool.Cleanup();
		}
	}
}