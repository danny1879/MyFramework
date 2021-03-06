﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FrameComponent : ComponentOwner
{
	protected GameObject mObject;
	protected bool mDestroy;       // 是否已经销毁
	protected bool mCreateObject;
	protected int mInitOrder;
	protected int mUpdateOrder;
	protected int mDestroyOrder;
	public FrameComponent(string name)
		: base(name) { }
	public virtual void init()
	{
		if(mCreateObject)
		{
			mObject = createGameObject(GetType().ToString(), mGameFramework.getGameFrameObject());
		}
		mDestroy = false;
		initComponents();
	}
	public override void destroy()
	{
		destroyGameObject(mObject);
		mObject = null;
		mDestroy = true;
		base.destroy();
	}
	public void setInitOrder(int order) { mInitOrder = order; }
	public void setUpdateOrder(int order) { mUpdateOrder = order; }
	public void setDestroyOrder(int order) { mDestroyOrder = order; }
	public GameObject getObject() { return mObject; }
	public bool isDestroy() { return mDestroy; }
	public virtual void onDrawGizmos() { }
	// a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
	static public int compareInit(FrameComponent a, FrameComponent b)
	{
		return sign(a.mInitOrder - b.mInitOrder);
	}
	// a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
	static public int compareUpdate(FrameComponent a, FrameComponent b)
	{
		return sign(a.mUpdateOrder - b.mUpdateOrder);
	}
	// a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
	static public int compareDestroy(FrameComponent a, FrameComponent b)
	{
		return sign(a.mDestroyOrder - b.mDestroyOrder);
	}
}