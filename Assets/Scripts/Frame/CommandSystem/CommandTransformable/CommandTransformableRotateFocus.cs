﻿using System;
using System.Collections.Generic;
using UnityEngine;

class CommandTransformableRotateFocus : Command
{
	public Transformable mTarget;
	public Vector3 mOffset;
	public override void init()
	{
		base.init();
		mTarget = null;
		mOffset = Vector3.zero;
	}
	public override void execute()
	{
		Transformable obj = mReceiver as Transformable;
		TransformableComponentRotateFocus component = obj.getComponent(out component);
		// 停止其他旋转组件
		obj.breakComponent<IComponentModifyRotation>(component.GetType());
		component.setActive(true);
		component.setFocusTarget(mTarget);
		component.setFocusOffset(mOffset);
	}
}