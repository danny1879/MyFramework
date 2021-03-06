﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MultiTouchInfo : IClassObject
{
	public IMouseEventCollect mWindow;
	public int mFinger0;
	public int mFinger1;
	public Vector2 mStartPosition0;
	public Vector2 mStartPosition1;
	public Vector2 mCurPosition0;
	public Vector2 mCurPosition1;
	public TouchPhase mPhase;
	public void resetProperty()
	{
		mWindow = null;
		mFinger0 = 0;
		mFinger1 = 0;
		mStartPosition0 = Vector2.zero;
		mStartPosition1 = Vector2.zero;
		mCurPosition0 = Vector2.zero;
		mCurPosition1 = Vector2.zero;
		mPhase = TouchPhase.Began;
	}
}