﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class BYTES : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(byte);
	public byte[] mValue;
	public BYTES(int count)
	{
		mValue = new byte[count];
		mType = typeof(byte[]);
		mSize = TYPE_SIZE * mValue.Length;
	}
	public BYTES(byte[] value)
	{
		mValue = value;
		mType = typeof(byte[]);
		mSize = TYPE_SIZE * mValue.Length;
	}
	public override void setRealSize(ushort realSize)
	{
		mRealSize = realSize;
		mElementCount = mRealSize / TYPE_SIZE;
	}
	public override void setElementCount(int elementCount)
	{
		mElementCount = elementCount;
		mRealSize = (ushort)(mElementCount * TYPE_SIZE);
	}
	public override void zero() { memset(mValue, (byte)0); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (mVariableLength)
		{
			// 先读取数据的实际字节长度
			bool success;
			setRealSize(readUShort(buffer, ref index, out success));
			if(!success)
			{
				return success;
			}
			return readBytes(buffer, ref index, mValue, -1, -1, mElementCount);
		}
		else
		{
			return readBytes(buffer, ref index, mValue);
		}
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (mVariableLength)
		{
			// 先写入数据的实际字节长度
			if(!writeUShort(buffer, ref index, mRealSize))
			{
				return false;
			}
			return writeBytes(buffer, ref index, mValue, -1, -1, mElementCount);
		}
		else
		{
			return writeBytes(buffer, ref index, mValue);
		}
	}
	public void set(string value, Encoding encode, int destOffset = 0)
	{
		if(value != null)
		{
			byte[] bytes = stringToBytes(value, encode);
			int count = getMin(bytes.Length, mValue.Length);
			memcpy(mValue, bytes, destOffset, 0, count);
			if(mVariableLength)
			{
				setElementCount(count);
			}
		}
	}
	public void set(string value, int destOffset = 0)
	{
		if(value != null)
		{
			byte[] bytes = stringToBytes(value);
			int count = getMin(bytes.Length, mValue.Length);
			memcpy(mValue, bytes, destOffset, 0, count);
			if (mVariableLength)
			{
				setElementCount(count);
			}
		}
	}
	public void set(byte[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
	public void set(byte[] value, int destOffset, int srcOffset, int count)
	{
		int minCount = getMin(count, mValue.Length);
		memcpy(mValue, value, destOffset, srcOffset, minCount);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
	public string getString(int startIndex = 0, int length = -1)
	{
		if(length == -1)
		{
			length = mValue.Length - startIndex;
		}
		return bytesToString(mValue, startIndex, length);
	}
	public string getString(Encoding encoding, int startIndex = 0, int length = -1)
	{
		if (length == -1)
		{
			length = mValue.Length - startIndex;
		}
		return bytesToString(mValue, startIndex, length, encoding);
	}
}