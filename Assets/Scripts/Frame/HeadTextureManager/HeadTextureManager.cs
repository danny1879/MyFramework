﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 微信头像下载管理器
public class HeadTextureManager : FrameComponent
{
    protected Dictionary<string, HeadLoadInfo> mHeadTextureList;
	public HeadTextureManager(string name)
		:base(name)
	{
        mHeadTextureList = new Dictionary<string, HeadLoadInfo>();
	}
	public override void destroy()
	{
		foreach(var item in mHeadTextureList)
		{
			destroyGameObject(ref item.Value.mTexture);
		}
		mHeadTextureList.Clear();
		base.destroy();
	}
	public Texture getHead(string openID)
	{
		if(mHeadTextureList.ContainsKey(openID))
		{
			return mHeadTextureList[openID].mTexture;
		}
		return null;
	}
	public void requestLoadTexture(string url, string openID, HeadDownloadCallback doneCallback)
	{
		if(isEmpty(url) || isEmpty(openID))
		{
			doneCallback?.Invoke(null, openID);
			return;
		}
		if (mHeadTextureList.ContainsKey(openID))
		{
			HeadLoadInfo info = mHeadTextureList[openID];
			if(info.mURL == url)
			{
				if (doneCallback != null)
				{
					// 已经下载过了,则直接调用回调
					if (info.mState == LOAD_STATE.LS_LOADED)
					{
						doneCallback(info.mTexture, openID);
					}
					// 正在下载,则将回调添加到列表中
					else if (info.mState == LOAD_STATE.LS_LOADING)
					{
						if (!info.mCallbackList.Contains(doneCallback))
						{
							info.mCallbackList.Add(doneCallback);
						}
					}
				}
			}
			else
			{
				// 头像链接修改了,则需要重新下载
				if (info.mState == LOAD_STATE.LS_LOADED)
				{
					// 先销毁旧头像
					destroyGameObject(ref info.mTexture);
					// 下载新头像
					info.mState = LOAD_STATE.LS_LOADING;
					info.mURL = url;
					if (doneCallback != null)
					{
						info.mCallbackList.Add(doneCallback);
					}
					mResourceManager.loadAssetsFromUrl<Texture>(url, onLoadWechatHead, openID);
				}
				// 如果头像正在下载,则只能等待头像下载完毕
				else if (info.mState == LOAD_STATE.LS_LOADING)
				{
					if (!info.mCallbackList.Contains(doneCallback))
					{
						info.mCallbackList.Add(doneCallback);
					}
				}
			}
		}
		else
		{
			HeadLoadInfo info = new HeadLoadInfo();
			info.mOpenID = openID;
			info.mTexture = null;
			info.mState = LOAD_STATE.LS_LOADING;
			info.mURL = url;
			if (doneCallback != null)
			{
				info.mCallbackList.Add(doneCallback);
			}
			mHeadTextureList.Add(openID, info);
			mResourceManager.loadAssetsFromUrl<Texture>(url, onLoadWechatHead, openID);
		}
	}
	//----------------------------------------------------------------------------------------------------------------------------
	protected void onLoadWechatHead(Object tex, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		string openID = userData as string;
		Texture head = tex as Texture;
		HeadLoadInfo info = mHeadTextureList[openID];
		if (head != null)
		{
			info.mTexture = head;
			info.mState = LOAD_STATE.LS_LOADED;
		}
		int callbackCount = info.mCallbackList.Count;
		for (int i = 0; i < callbackCount; ++i)
		{
			info.mCallbackList[i](head, openID);
		}
		info.mCallbackList.Clear();
		// 头像下载失败,删除下载信息,当有再次请求头像时再重新下载
		if(tex == null)
		{
			mHeadTextureList.Remove(openID);
		}
	}
}
