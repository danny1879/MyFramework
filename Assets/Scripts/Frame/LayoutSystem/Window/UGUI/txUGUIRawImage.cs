﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class txUGUIRawImage : txUGUIObject, IShaderWindow
{
	protected RawImage mRawImage;
	protected WindowShader mWindowShader;
	protected bool mIsNewMaterial;
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		mRawImage = mObject.GetComponent<RawImage>();
		if (mRawImage == null)
		{
			mRawImage = mObject.AddComponent<RawImage>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if(mRawImage == null)
		{
			logError(GetType() + " can not find " + typeof(RawImage) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
		string materialName = getMaterialName();
		// 不再将默认材质替换为自定义的默认材质,只判断其他材质
		if (!isEmpty(materialName) && materialName != CommonDefine.BUILDIN_UI_MATERIAL)
		{
			bool newMaterial = !mShaderManager.isSingleShader(materialName);
			if (newMaterial)
			{
				setMaterialName(materialName, newMaterial);
			}
		}
	}
	public override void destroy()
	{
		// 卸载创建出的材质
		if (mIsNewMaterial)
		{
#if !UNITY_EDITOR
			destroyGameObject(mRawImage.material);
#endif
			mRawImage.material = null;
		}
		mRawImage.texture = null;
		mRawImage = null;
		base.destroy();
	}
	public virtual void setWindowShader<T>() where T : WindowShader, new()
	{
		mWindowShader = new T();
	}
	public virtual T getWindowShader<T>() where T : WindowShader
	{
		return mWindowShader as T;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mRawImage.material != null)
		{
			mWindowShader?.applyShader(mRawImage.material);
		}
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		base.setAlpha(alpha, fadeChild);
		Color color = mRawImage.color;
		color.a = alpha;
		mRawImage.color = color;
	}
	public virtual void setTexture(Texture tex, bool useTextureSize = false)
	{
		if (mRawImage == null)
		{
			return;
		}
		mRawImage.texture = tex;
		if (useTextureSize && tex != null)
		{
			setWindowSize(getTextureSize());
		}
	}
	public Texture getTexture()
	{
		if(mRawImage == null)
		{
			return null;
		}
		return mRawImage.texture;
	}
	public Vector2 getTextureSize()
	{
		if (mRawImage.texture == null)
		{
			return Vector2.zero;
		}
		return new Vector2(mRawImage.texture.width, mRawImage.texture.height);
	}
	public string getTextureName()
	{
		if (mRawImage == null || mRawImage.texture == null)
		{
			return null;
		}
		return mRawImage.texture.name;
	}
	public void setTextureName(string name, bool useTextureSize = false)
	{
		if (isEmpty(name))
		{
			setTexture(null, useTextureSize);
			return;
		}
		// 允许同步加载时,使用同步加载
		if (mResourceManager.syncLoadAvalaible())
		{
			Texture tex = mResourceManager.loadResource<Texture>(name, true);
			setTexture(tex, useTextureSize);
		}
		// 否则只能使用异步加载
		else
		{
			mResourceManager.loadResourceAsync<Texture>(name, onTextureLoaded, useTextureSize, true);
		}
	}
	public string getMaterialName()
	{
		if (mRawImage == null || mRawImage.material == null)
		{
			return null;
		}
		return mRawImage.material.name;
	}
	public void setMaterialName(string materialName, bool newMaterial)
	{
		if (mRawImage == null)
		{
			return;
		}
		mIsNewMaterial = newMaterial;
		// 查看是否允许同步加载
		if (mResourceManager.syncLoadAvalaible())
		{
			Material mat = null;
			Material loadedMaterial = mResourceManager.loadResource<Material>(CommonDefine.R_MATERIAL_PATH + materialName, true);
			if (mIsNewMaterial)
			{
				mat = new Material(loadedMaterial);
				mat.name = materialName + "_" + mID;
			}
			else
			{
				mat = loadedMaterial;
			}
			mRawImage.material = mat;
		}
		else
		{
			LoadMaterialParam param;
			mClassPool.newClass(out param);
			param.mMaterialName = materialName;
			param.mNewMaterial = mIsNewMaterial;
			mResourceManager.loadResourceAsync<Material>(CommonDefine.R_MATERIAL_PATH + materialName, onMaterialLoaded, param, true);
		}
	}
	//-------------------------------------------------------------------------------------------------------------------------------------------------
	protected void onTextureLoaded(Object res, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		// userData表示是否使用图片尺寸设置窗口大小
		setTexture(res as Texture, (bool)userData);
	}
	protected void onMaterialLoaded(Object res, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		if (mRawImage == null)
		{
			return;
		}
		Material material = res as Material;
		var param = userData as LoadMaterialParam;
		if (param.mNewMaterial)
		{
			Material newMat = new Material(material);
			newMat.name = param.mMaterialName + "_" + mID;
			mRawImage.material = newMat;
		}
		else
		{
			mRawImage.material = material;
		}
		mClassPool.destroyClass(param);
	}
}