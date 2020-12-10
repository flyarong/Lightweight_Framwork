// ========================================================
// des：
// author: shenyi
// time：2020-07-09 10:11:59
// version：1.0
// ========================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

	public class EntityComp : RecycleObject<EntityComp>
	{
		public string compName = "";

		protected EntityBehavior behavior;

		public void SetBehavior(EntityBehavior behavior)
		{
			this.behavior = behavior;
		}

		public EntityComp() { } // 为了使用对象池 增加的无参构造方法 尽量不要用

		public EntityComp(EntityBehavior behavior)
		{
			this.behavior = behavior;
			compName = this.GetType().ToString();
		}

		public virtual void OnUpdate(float deltaTime) { }
		public virtual void OnFixedUpdate(float fixedDeltaTime) { }
		public virtual void OnAdd() { }
		public virtual void OnRemove() { }


		public static bool operator !=(EntityComp r, EntityComp l)
		{
			bool rnull = (((object)r) == null || r.behavior == null);
			bool lnull = (((object)l) == null || l.behavior == null);
			if (rnull && lnull)
				return false;

			if (rnull == true || rnull == true)
				return true;

			return !r.Equals(l);
		}


		public static bool operator ==(EntityComp r, EntityComp l)
		{
			bool rnull = (((object)r) == null || r.behavior == null);
			bool lnull = (((object)l) == null || l.behavior == null);
			//rnull = (rnull == false ? r.behavior == null : rnull);
			//lnull = (lnull == false ? l.behavior == null : lnull);
			if (rnull && lnull)
				return true;
			if (rnull == true || lnull == true)
				return false;

			return r.Equals(l);
		}

	}
}