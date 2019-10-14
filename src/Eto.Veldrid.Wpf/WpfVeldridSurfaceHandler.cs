﻿using Eto.Veldrid;
using Eto.Veldrid.Wpf;
using System;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.OpenGL;

[assembly: Eto.ExportHandler(typeof(VeldridSurface), typeof(WpfVeldridSurfaceHandler))]

namespace Eto.Veldrid.Wpf
{
	public class WpfVeldridSurfaceHandler : Eto.Wpf.Forms.ManualBubbleWindowsFormsHostHandler<WinForms.WinVeldridUserControl, VeldridSurface, VeldridSurface.ICallback>, VeldridSurface.IHandler
	{
		public int RenderWidth => WinFormsControl.Width;
		public int RenderHeight => WinFormsControl.Height;

		public WpfVeldridSurfaceHandler() : base(new WinForms.WinVeldridUserControl())
		{
			WinFormsControl.HandleCreated += WinFormsControl_HandleCreated;
		}

		public override void AttachEvent(string id)
		{
			switch (id)
			{
				case VeldridSurface.DrawEvent:
					WinFormsControl.Paint += (sender, e) => Callback.OnDraw(Widget, EventArgs.Empty);
					break;
				default:
					base.AttachEvent(id);
					break;
			}
		}

		private void WinFormsControl_HandleCreated(object sender, EventArgs e)
		{
			if (Widget.Backend == GraphicsBackend.OpenGL)
			{
				WinFormsControl.CreateOpenGLContext();

				Callback.OnOpenGLReady(Widget, EventArgs.Empty);
			}

			Callback.OnControlReady(Widget, EventArgs.Empty);
		}

		/// <summary>
		/// Prepare this VeldridSurface to use OpenGL.
		/// </summary>
		public void InitializeOpenGL()
		{
			var platformInfo = new OpenGLPlatformInfo(
				VeldridGL.GetGLContextHandle(),
				VeldridGL.GetProcAddress,
				WinFormsControl.MakeCurrent,
				VeldridGL.GetCurrentContext,
				VeldridGL.ClearCurrentContext,
				VeldridGL.DeleteContext,
				VeldridGL.SwapBuffers,
				VeldridGL.SetVSync,
				VeldridGL.SetSwapchainFramebuffer,
				VeldridGL.ResizeSwapchain);

			Widget.GraphicsDevice = GraphicsDevice.CreateOpenGL(
				Widget.GraphicsDeviceOptions,
				platformInfo,
				(uint)RenderWidth,
				(uint)RenderHeight);

			Widget.Swapchain = Widget.GraphicsDevice.MainSwapchain;

			Callback.OnVeldridInitialized(Widget, EventArgs.Empty);
		}

		/// <summary>
		/// Prepare this VeldridSurface to use a graphics API other than OpenGL.
		/// </summary>
		public void InitializeOtherApi()
		{
			Control.Loaded += OneTimeControlInit;
		}

		private void OneTimeControlInit(object sender, System.Windows.RoutedEventArgs e)
		{
			// To embed Veldrid in an Eto control, all these platform-specific
			// versions of InitializeOtherApi use the technique outlined here:
			//
			//   https://github.com/mellinoe/veldrid/issues/155
			//
			var source = SwapchainSource.CreateWin32(
				WinFormsControl.Handle,
				Marshal.GetHINSTANCE(typeof(VeldridSurface).Module));

			Widget.Swapchain = Widget.GraphicsDevice.ResourceFactory.CreateSwapchain(
				new SwapchainDescription(
					source,
					(uint)RenderWidth,
					(uint)RenderHeight,
					PixelFormat.R32_Float,
					false));

			Control.Loaded -= OneTimeControlInit;

			Callback.OnVeldridInitialized(Widget, EventArgs.Empty);
		}
	}
}