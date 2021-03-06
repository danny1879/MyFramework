﻿using System;
using System.Collections;
using System.Collections.Generic;

public class LONG : OBJECT
{
	protected const int TYPE_SIZE = sizeof(long);
	public long mValue;
	public LONG()
	{
		mType = typeof(long);
		mSize = TYPE_SIZE;
	}
	public LONG(long value)
	{
		mValue = value;
		mType = typeof(long);
		mSize = TYPE_SIZE;
	}
	public override void zero() { mValue = 0; }
	public void set(long value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		bool success;
		mValue = readLong(buffer, ref index, out success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeLong(buffer, ref index, mValue);
	}
}