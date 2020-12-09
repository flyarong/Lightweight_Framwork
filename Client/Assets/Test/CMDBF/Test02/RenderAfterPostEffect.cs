﻿using UnityEngine;
using UnityEngine.Rendering;

public class RenderAfterPostEffect : MonoBehaviour
{
	private CommandBuffer commandBuffer = null;
	private Renderer targetRenderer = null;

	void OnEnable()
	{
		targetRenderer = this.GetComponentInChildren<Renderer>();
		if (targetRenderer)
		{
			targetRenderer.enabled = false;
			commandBuffer = new CommandBuffer();
			commandBuffer.DrawRenderer(targetRenderer, targetRenderer.sharedMaterial);
			//直接加入相机的CommandBuffer事件队列中
			Camera.main.AddCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
		}
	}

	void OnDisable()
	{
		if (targetRenderer)
		{
			//移除事件，清理资源
			if (Camera.main)
				Camera.main.RemoveCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
			commandBuffer.Clear();
			targetRenderer.enabled = true;
		}
	}
}