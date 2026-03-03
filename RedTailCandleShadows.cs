#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
using SharpDX;
using SharpDX.Direct2D1;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	[CategoryOrder("Shadow Settings", 1)]
	[CategoryOrder("Advanced", 2)]
	public class RedTailCandleShadows : Indicator
	{
		#region Private Variables
		private SharpDX.Direct2D1.SolidColorBrush	shadowBrushDX;
		private SharpDX.Direct2D1.SolidColorBrush	wickShadowBrushDX;
		private bool								brushesValid;
		#endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Draws subtle drop shadows beneath candles for enhanced visual depth and readability. Optimized for scalping on low timeframes.";
				Name						= "RedTail Candle Shadows";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= false;
				DrawOnPricePanel			= true;
				IsSuspendedWhileInactive	= true;
				
				// Shadow Settings
				ShadowColor					= System.Windows.Media.Brushes.Black;
				ShadowOpacity				= 20;
				OffsetX						= 2;
				OffsetY						= 2;
				ShadowBody					= true;
				ShadowWicks					= true;
				WickOpacityMultiplier		= 0.6;
				
				// Advanced
				EnableBlurEffect			= false;
				BlurPasses					= 1;
				BodyWidthAdjust				= 0;
			}
			else if (State == State.Terminated)
			{
				DisposeBrushes();
			}
		}

		#region Brush Management
		private void CreateBrushes()
		{
			DisposeBrushes();
			
			if (RenderTarget == null)
				return;

			try
			{
				System.Windows.Media.Color mediaColor = ((System.Windows.Media.SolidColorBrush)ShadowColor).Color;
				float opacity = Math.Max(0, Math.Min(100, ShadowOpacity)) / 100f;
				
				shadowBrushDX = new SharpDX.Direct2D1.SolidColorBrush(
					RenderTarget,
					new SharpDX.Color4(
						mediaColor.R / 255f,
						mediaColor.G / 255f,
						mediaColor.B / 255f,
						opacity));
				
				float wickOpacity = opacity * (float)Math.Max(0, Math.Min(1.0, WickOpacityMultiplier));
				wickShadowBrushDX = new SharpDX.Direct2D1.SolidColorBrush(
					RenderTarget,
					new SharpDX.Color4(
						mediaColor.R / 255f,
						mediaColor.G / 255f,
						mediaColor.B / 255f,
						wickOpacity));
				
				brushesValid = true;
			}
			catch (Exception ex)
			{
				Print("RedTail Candle Shadows - Brush creation error: " + ex.Message);
				brushesValid = false;
			}
		}
		
		private void DisposeBrushes()
		{
			brushesValid = false;
			
			if (shadowBrushDX != null)
			{
				shadowBrushDX.Dispose();
				shadowBrushDX = null;
			}
			if (wickShadowBrushDX != null)
			{
				wickShadowBrushDX.Dispose();
				wickShadowBrushDX = null;
			}
		}
		
		public override void OnRenderTargetChanged()
		{
			CreateBrushes();
		}
		#endregion

		protected override void OnBarUpdate() { }

		#region Rendering
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (ChartBars == null || !brushesValid || shadowBrushDX == null)
				return;

			int fromIndex	= ChartBars.FromIndex;
			int toIndex		= ChartBars.ToIndex;
			
			if (fromIndex > toIndex)
				return;

			float barWidth		= (float)chartControl.BarWidth;
			float halfWidth		= barWidth + BodyWidthAdjust;
			float offsetX		= (float)OffsetX;
			float offsetY		= (float)OffsetY;
			float wickWidth		= Math.Max(1f, barWidth * 0.15f);

			// Optional simple blur: draw multiple offset passes at lower opacity
			int passes = EnableBlurEffect ? Math.Max(1, Math.Min(3, BlurPasses)) : 1;
			
			for (int pass = 0; pass < passes; pass++)
			{
				float passOffsetX = offsetX + (EnableBlurEffect ? pass * 0.5f : 0);
				float passOffsetY = offsetY + (EnableBlurEffect ? pass * 0.5f : 0);
				
				if (EnableBlurEffect && pass > 0)
				{
					shadowBrushDX.Opacity	*= 0.5f;
					wickShadowBrushDX.Opacity *= 0.5f;
				}
				
				for (int i = fromIndex; i <= toIndex; i++)
				{
					float x = chartControl.GetXByBarIndex(ChartBars, i);
					
					double openVal	= ChartBars.Bars.GetOpen(i);
					double closeVal	= ChartBars.Bars.GetClose(i);
					double highVal	= ChartBars.Bars.GetHigh(i);
					double lowVal	= ChartBars.Bars.GetLow(i);
					
					float open	= chartScale.GetYByValue(openVal);
					float close	= chartScale.GetYByValue(closeVal);
					float high	= chartScale.GetYByValue(highVal);
					float low	= chartScale.GetYByValue(lowVal);
					
					float bodyTop		= Math.Min(open, close);
					float bodyBottom	= Math.Max(open, close);
					float bodyHeight	= bodyBottom - bodyTop;
					
					// Ensure minimum body height for dojis
					if (bodyHeight < 1f)
						bodyHeight = 1f;

					// Draw wick shadows first (behind body shadow)
					if (ShadowWicks)
					{
						// Upper wick shadow
						if (high < bodyTop)
						{
							RenderTarget.DrawLine(
								new SharpDX.Vector2(x + passOffsetX, high + passOffsetY),
								new SharpDX.Vector2(x + passOffsetX, bodyTop + passOffsetY),
								wickShadowBrushDX,
								wickWidth);
						}
						
						// Lower wick shadow
						if (low > bodyBottom)
						{
							RenderTarget.DrawLine(
								new SharpDX.Vector2(x + passOffsetX, bodyBottom + passOffsetY),
								new SharpDX.Vector2(x + passOffsetX, low + passOffsetY),
								wickShadowBrushDX,
								wickWidth);
						}
					}

					// Draw body shadow
					if (ShadowBody)
					{
						RenderTarget.FillRectangle(
							new SharpDX.RectangleF(
								x - halfWidth + passOffsetX,
								bodyTop + passOffsetY,
								halfWidth * 2f,
								bodyHeight),
							shadowBrushDX);
					}
				}
				
				// Restore opacity after blur pass
				if (EnableBlurEffect && pass > 0)
				{
					float opacity = Math.Max(0, Math.Min(100, ShadowOpacity)) / 100f;
					shadowBrushDX.Opacity = opacity;
					wickShadowBrushDX.Opacity = opacity * (float)WickOpacityMultiplier;
				}
			}
		}
		#endregion

		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Shadow Color", Description = "Color of the candle shadow", Order = 1, GroupName = "Shadow Settings")]
		public System.Windows.Media.Brush ShadowColor { get; set; }
		
		[Browsable(false)]
		public string ShadowColorSerializable
		{
			get { return Serialize.BrushToString(ShadowColor); }
			set { ShadowColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Range(1, 100)]
		[Display(Name = "Shadow Opacity %", Description = "Opacity of the shadow (1-100). Lower = more subtle.", Order = 2, GroupName = "Shadow Settings")]
		public int ShadowOpacity { get; set; }

		[NinjaScriptProperty]
		[Range(-10, 10)]
		[Display(Name = "Offset X (pixels)", Description = "Horizontal shadow offset. Positive = right.", Order = 3, GroupName = "Shadow Settings")]
		public int OffsetX { get; set; }

		[NinjaScriptProperty]
		[Range(-10, 10)]
		[Display(Name = "Offset Y (pixels)", Description = "Vertical shadow offset. Positive = down.", Order = 4, GroupName = "Shadow Settings")]
		public int OffsetY { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Shadow Body", Description = "Draw shadow under candle body", Order = 5, GroupName = "Shadow Settings")]
		public bool ShadowBody { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Shadow Wicks", Description = "Draw shadow under candle wicks", Order = 6, GroupName = "Shadow Settings")]
		public bool ShadowWicks { get; set; }

		[NinjaScriptProperty]
		[Range(0.1, 1.0)]
		[Display(Name = "Wick Opacity Multiplier", Description = "Multiplier applied to wick shadow opacity relative to body (0.1-1.0)", Order = 7, GroupName = "Shadow Settings")]
		public double WickOpacityMultiplier { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Enable Blur Effect", Description = "Simulate soft shadow with multiple render passes (slight performance cost)", Order = 1, GroupName = "Advanced")]
		public bool EnableBlurEffect { get; set; }

		[NinjaScriptProperty]
		[Range(1, 3)]
		[Display(Name = "Blur Passes", Description = "Number of blur passes (1-3). More = softer but heavier.", Order = 2, GroupName = "Advanced")]
		public int BlurPasses { get; set; }

		[NinjaScriptProperty]
		[Range(-5, 5)]
		[Display(Name = "Body Width Adjust", Description = "Pixels to add/subtract from shadow body width", Order = 3, GroupName = "Advanced")]
		public int BodyWidthAdjust { get; set; }
		#endregion
	}
}
