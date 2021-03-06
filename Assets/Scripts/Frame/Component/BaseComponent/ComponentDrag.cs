﻿using UnityEngine;
using System;
using System.Collections;

public class ComponentDrag : GameComponent
{	
	protected OnDragStartCallback mOnDragStartCallback;
	protected OnDragCallback mOnDragEndCallback;		// 在结束拖拽,还未进行接收拖拽处理之前调用
	protected OnDragCallback mOnDragEndTotallyCallback;	// 在结束拖拽和接收拖拽处理全部完成以后调用
	protected OnDragCallback mOnDragingCallback;
	protected OnDragCallback mOnInterruptCallback;
	protected Vector3 mPrepareDragMousePosition;
	protected Vector3 mDragMouseOffset;
	protected Vector2 mAllowDragDirection;      // 允许开始拖拽的方向,为0则表示不限制开始拖拽的方向
	protected float mStartDragThreshold;
	protected float mDragStartAngleThreshold;   // 如果有允许开始拖拽的方向,则表示当实际拖拽方向与允许的方向夹角,弧度制
	protected bool mPreparingDrag;              // 鼠标按下时只是准备开始拖动,当鼠标在准备状态下移动一定距离以后才会开始真正的拖动
	protected bool mTouching;
	protected bool mDrag;
	protected int mTouchFinger;					// 如果是手指通过触屏操作的拖动,则表示拖动的触点
	public ComponentDrag()
	{
		mStartDragThreshold = 20.0f;
		mDragStartAngleThreshold = toRadian(45.0f);
	}
	public override void update(float elapsedTime)
	{
		int touchCount = Input.touchCount;
		// 鼠标操作的情况
		if (touchCount == 0)
		{
			// 左键按下时,鼠标悬停在物体上,则开始拖动
			if (!mDrag && mInputManager.getMouseCurrentDown(MOUSE_BUTTON.MB_LEFT))
			{
				checkStartDrag(getMousePosition());
			}
			Vector3 mousePosition = getMousePosition();
			if (mInputManager.getMouseCurrentUp(MOUSE_BUTTON.MB_LEFT))
			{
				onMouseUp(mousePosition);
			}
			if (mDrag || mPreparingDrag)
			{
				onMouseMove(ref mousePosition);
			}
		}
		// 触屏操作的情况
		else
		{
			// 单点操作时才允许拖动
			if(touchCount == 1)
			{
				Touch touch = Input.GetTouch(0);
				if (!mDrag)
				{
					if (touch.phase == TouchPhase.Began && checkStartTouchDrag(ref touch))
					{
						mTouchFinger = touch.fingerId;
					}
				}
				if(mTouchFinger == touch.fingerId)
				{
					Vector3 mousePosition = touch.position;
					if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
					{
						onMouseUp(mousePosition);
					}
					if ((mDrag || mPreparingDrag) && touch.phase == TouchPhase.Moved)
					{
						onMouseMove(ref mousePosition);
					}
				}
			}
			// 多点操作时会立即结束拖动,此时认为是中断了拖动操作
			else
			{
				mDrag = false;
				mPreparingDrag = false;
				onInterrupt();
			}
		}
	}
	public bool isDraging() { return mDrag; }
	public void setDragStartCallback(OnDragStartCallback callback){mOnDragStartCallback = callback;}
	public void setDragEndCallback(OnDragCallback callback){mOnDragEndCallback = callback;}
	public void setDragEndTotallyCallback(OnDragCallback callback) { mOnDragEndTotallyCallback = callback; }
	public void setDragingCallback(OnDragCallback callback){mOnDragingCallback = callback;}
	public void setInterruptCallback(OnDragCallback callback) { mOnInterruptCallback = callback; }
	public void setStartDragThreshold(float threshold) { mStartDragThreshold = threshold; }
	public void setAllowDragDirection(Vector2 allowDirection) { mAllowDragDirection = allowDirection; }
	public void setDragStartAngleThreshold(float radian) { mDragStartAngleThreshold = radian; }
	public override void setActive(bool active)
	{
		if(active == isActive())
		{
			return;
		}
		base.setActive(active);
		// 默认为未拖动状态
		mDrag = false;
		mPreparingDrag = false;
		// 激活组件时,如果鼠标在当前物体上已经是按下状态,则开始拖动
		if (active)
		{
			if (!mGlobalTouchSystem.isColliderRegisted(mComponentOwner as IMouseEventCollect))
			{
				logError("组件所属物体未注册碰撞体,无法进行拖拽. " + mComponentOwner.getName());
			}
			int touchCount = Input.touchCount;
			// 鼠标操作的情况
			if (touchCount == 0)
			{
				checkStartDrag(getMousePosition());
			}
			else
			{
				if (touchCount == 1)
				{
					Touch touch = Input.GetTouch(0);
					if (touch.phase == TouchPhase.Began && checkStartTouchDrag(ref touch))
					{
						mTouchFinger = touch.fingerId;
					}
				}
			}
		}
	}
	public override void notifyOwnerActive(bool active)
	{
		mDrag = false;
		mPreparingDrag = false;
	}
	public override void notifyOwnerDestroy()
	{
		mDrag = false;
		mPreparingDrag = false;
	}
	// 手动调用开始拖拽
	public bool startDrag(Vector3 mousePosition, Vector3 dragOffset)
	{
		mDrag = onDragStart(mousePosition);
		if(!mDrag)
		{
			return false;
		}
		mDragMouseOffset = dragOffset;
		mPreparingDrag = false;
		applyScreenPosition(ref mousePosition);
		onDraging(ref mousePosition);
		return true;
	}
	//--------------------------------------------------------------------------------------------------------------
	protected bool checkStartTouchDrag(ref Touch touch)
	{
		Vector3 mousePosition = touch.position;
		if (mouseInObject(ref mousePosition))
		{
			// 拖拽消息不向下传递, 从上往下查找,如果前面没有窗口需要有拖拽消息被处理,则当前窗口响应拖拽消息
			var hoverWindowList = mGlobalTouchSystem.getAllHoverWindow(ref mousePosition);
			foreach (var item in hoverWindowList)
			{
				if (item == mComponentOwner as IMouseEventCollect)
				{
					onMouseDown(mousePosition);
					return true;
				}
				else if (item.isDragable())
				{
					break;
				}
			}
		}
		return false;
	}
	protected void checkStartDrag(Vector3 mousePosition)
	{
		if (mInputManager.getMouseDown(MOUSE_BUTTON.MB_LEFT) && mouseInObject(ref mousePosition))
		{
			// 从上往下查找,如果前面没有窗口需要有拖拽消息被处理,则当前窗口响应拖拽消息
			var hoverWindowList = mGlobalTouchSystem.getAllHoverWindow(ref mousePosition);
			foreach (var item in hoverWindowList)
			{
				if (item == mComponentOwner as IMouseEventCollect)
				{
					onMouseDown(mousePosition);
					break;
				}
				else if (item.isDragable())
				{
					break;
				}
			}
		}
	}
	protected void onMouseDown(Vector3 mousePosition)
	{
		mPreparingDrag = true;
		mPrepareDragMousePosition = mousePosition;
	}
	protected void onMouseUp(Vector3 mousePosition)
	{
		// 确保鼠标抬起时所有状态标记为false
		mPreparingDrag = false;
		if (mDrag)
		{
			mDrag = false;
			onDragEnd(mousePosition);
		}
	}
	protected void onMouseMove(ref Vector3 mousePosition)
	{
		if (mPreparingDrag)
		{
			Vector2 mouseDelta = mousePosition - mPrepareDragMousePosition;
			if (lengthGreater(ref mouseDelta, mStartDragThreshold))
			{
				// 有拖拽方向要求时,只有拖拽方向与设置方向夹角不超过指定角度时才开始拖动
				if (!isVectorZero(mAllowDragDirection))
				{
					if (getAngleBetweenVector(mouseDelta, mAllowDragDirection) < mDragStartAngleThreshold)
					{
						mDrag = onDragStart(mousePosition);
					}
				}
				else
				{
					mDrag = onDragStart(mousePosition);
				}
				mPreparingDrag = false;
			}
		}
		if (mDrag)
		{
			applyScreenPosition(ref mousePosition);
			onDraging(ref mousePosition);
		}
	}
	protected virtual void applyScreenPosition(ref Vector3 screenPos) { }
	protected virtual Vector3 getScreenPosition() { return Vector3.zero; }
	protected virtual bool mouseInObject(ref Vector3 mousePosition) { return false; }
	// 物体开始拖动,返回值表示是否允许开始拖动
	protected virtual bool onDragStart(Vector3 mousePos)
	{
		mDragMouseOffset = mousePos - getScreenPosition();
		if (mOnDragStartCallback != null)
		{
			bool allowDrag = true;
			mOnDragStartCallback(mComponentOwner, ref allowDrag);
			return allowDrag;
		}
		return true;
	}
	// 当前物体拖动结束
	protected virtual void onDragEnd(Vector3 mousePos)
	{
		mOnDragEndCallback?.Invoke(mComponentOwner);
	}
	// 当前物体正在拖动
	protected virtual void onDraging(ref Vector3 mousePos)
	{
		mOnDragingCallback?.Invoke(mComponentOwner);
	}
	protected virtual void onInterrupt()
	{
		mOnInterruptCallback?.Invoke(mComponentOwner);
	}
	protected void notifyDragEndTotally()
	{
		mOnDragEndTotallyCallback?.Invoke(mComponentOwner);
	}
}