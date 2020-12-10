// ========================================================
// des：
// author: shenyi
// time：2020-07-02 17:28:59
// version：1.0
// ========================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

namespace Game
{
	public partial class EntityBehavior : EventObject, IMonoPoolObject<EntityBehavior> 
    {
		
		private const string RecycleName = "objRecyle";
		private static Vector3 OnRecylePos = new Vector3(-888f, -888f, -888f);
		private Dictionary<Type, EntityComp> entitiesCompList = new Dictionary<Type, EntityComp> ();

		public Action<EntityComp[]> onBodyCreate;

		public void UpdateLogic (float fUpdateTime) {
			var Enumerator = entitiesCompList.GetEnumerator ();
			while (Enumerator.MoveNext ()) {
				Enumerator.Current.Value.OnUpdate (fUpdateTime * logicSpeed);
			}
            foreach (EntityComp entitycmp in entitiesCompList.Values)
            {
                entitycmp.OnUpdate(fUpdateTime * logicSpeed);
            }

            if (characterController != null) {
				if (characterController.isGrounded) {
					if (lastGroundState == false) onLand?.Invoke();
					lastGroundState = true;
				} else {
					if (lastGroundState == true) onJump?.Invoke();
					lastGroundState = false;
				}
			}

		}

		private void FixedUpdate () {
			var Enumerator = entitiesCompList.GetEnumerator ();
			while (Enumerator.MoveNext ()) {
				Enumerator.Current.Value.OnFixedUpdate (Time.fixedDeltaTime);
			}
		}

        private void Update()
        {
            var Enumerator = entitiesCompList.GetEnumerator();
            while (Enumerator.MoveNext())
            {
                Enumerator.Current.Value.OnUpdate(Time.deltaTime);
            }
        }

        /// <summary>
        /// EntityBehavior回收进对象池时被调用
        /// </summary>
        /// <returns></returns>
        public EntityBehavior Downcast()
        {
			if (body != null)
			{
				ResourceManager.RecyclePrefab(body);
				body = null;
			}
			if (m_CharacterController != null)
			{
				Destroy(m_CharacterController);
				m_CharacterController = null;
			}
			this.gameObject.name = RecycleName;
			onBodyCreate = null;
			isSyncable = false;
			isHero = false;
			sceneid = 0;
			entityType = 0;
			uid = string.Empty;
			sceneid = 0;
			destroyed = false;
			bodyLoading = false;
			logicSpeed = 1f;
			RemoveAllEntityComp();
			body = null;
			head = null;
			middle = null;
			root = null;
			transform.position = OnRecylePos;
			transform.localScale = Vector3.one;
			if (transform.childCount > 0)
			{
				int max = transform.childCount;
				for (int index = 0; index < max; index++)
				{
					Transform trans = transform.GetChild(index);
					if (trans != null)
						GameObject.Destroy(trans.gameObject);
				}
			}
			return this;
        }

	}
}