﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using MaterialSkin.Animations;

namespace MaterialSkin.Controls
{
    using ControlRenderExtension;
    /// <summary>
    /// Material design-like progress bar
    /// </summary>
    public class MaterialProgressBar : ProgressBar, IMaterialControl
    {
        private readonly AnimationManager animationManager;
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialHealthBar"/> class.
        /// </summary>
        public MaterialProgressBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            animationManager = new AnimationManager(false)
            {
                Increment = 0.05,
                AnimationType = AnimationType.EaseOut
            };
            animationManager.OnAnimationProgress += sender => Invalidate();
            animationManager.OnAnimationFinished += AnimationManager_OnAnimationFinished;
        }

        private void AnimationManager_OnAnimationFinished(object sender)
        {
            Invalidate();
        }

        private int _value;

        public new int Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value < _value)
                {
                    if (_value > 0) animationManager.StartNewAnimation(AnimationDirection.In, new object[] { true, _value });
                    _value = value;
                }
                else
                {
                    if (_value > 0) animationManager.StartNewAnimation(AnimationDirection.In, new object[] { false, _value });
                    _value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the depth.
        /// </summary>
        /// <value>
        /// The depth.
        /// </value>
        [Browsable(true)]
        public int Depth { get; set; }

        [Browsable(true),Category("Appearance")]
        public bool DecreaseHighlight { get; set; }

        public bool OnRight { get; set; }

        /// <summary>
        /// Gets the skin manager.
        /// </summary>
        /// <value>
        /// The skin manager.
        /// </value>
        [Browsable(false)]
        public MaterialSkinManager SkinManager
        {
            get { return MaterialSkinManager.Instance; }
        }

        /// <summary>
        /// Gets or sets the state of the mouse.
        /// </summary>
        /// <value>
        /// The state of the mouse.
        /// </value>
        [Browsable(false)]
        public MouseState MouseState { get; set; }

        /// <summary>
        /// Performs the work of setting the specified bounds of this control.
        /// </summary>
        /// <param name="x">The new <see cref="P:System.Windows.Forms.Control.Left" /> property value of the control.</param>
        /// <param name="y">The new <see cref="P:System.Windows.Forms.Control.Top" /> property value of the control.</param>
        /// <param name="width">The new <see cref="P:System.Windows.Forms.Control.Width" /> property value of the control.</param>
        /// <param name="height">The new <see cref="P:System.Windows.Forms.Control.Height" /> property value of the control.</param>
        /// <param name="specified">A bitwise combination of the <see cref="T:System.Windows.Forms.BoundsSpecified" /> values.</param>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, 5, specified);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(SkinManager.GetApplicationBackgroundColor());
            e.Graphics.FillRectangle(SkinManager.GetDisabledOrHintBrush(), 0, 0, Width, Height);
            int doneProgress = (int)(Width * ((double)Value / Maximum));
            if (OnRight)
            {
                e.Graphics.FillRectangle(SkinManager.ColorScheme.PrimaryBrush, Width - doneProgress, 0, Width, Height);
            }
            else
            {
                e.Graphics.FillRectangle(SkinManager.ColorScheme.PrimaryBrush, 0, 0, doneProgress, Height);
            }
            if (animationManager.IsAnimating())
            {
                for (int i = 0; i < animationManager.GetAnimationCount(); i++)
                {
                    double animationValue = animationManager.GetProgress(i);
                    bool animationDirection = (bool)animationManager.GetData(i)[0];
                    int animationData = (int)animationManager.GetData(i)[1];
                    if (animationDirection)
                    {
                        int oldProgress = (int)(((float)(animationData - _value) / Maximum) * Width);
                        if (OnRight)
                        {
                            if (DecreaseHighlight) e.Graphics.FillRectangle(SkinManager.Theme == MaterialSkinManager.Themes.DARK ? SkinManager.ColorScheme.DarkPrimaryBrush : SkinManager.ColorScheme.LightPrimaryBrush, Width - doneProgress - oldProgress, 0, oldProgress, Height);
                            e.Graphics.FillRectangle(SkinManager.ColorScheme.PrimaryBrush, (float)(Width - doneProgress - oldProgress + (oldProgress * animationValue)), 0, (float)(oldProgress - oldProgress * animationValue), Height);
                        }
                        else
                        {
                            if (DecreaseHighlight) e.Graphics.FillRectangle(SkinManager.Theme == MaterialSkinManager.Themes.DARK ? SkinManager.ColorScheme.DarkPrimaryBrush : SkinManager.ColorScheme.LightPrimaryBrush, doneProgress, 0, oldProgress, Height);
                            e.Graphics.FillRectangle(SkinManager.ColorScheme.PrimaryBrush, doneProgress, 0, (float)(oldProgress - oldProgress * animationValue), Height);
                        }
                    }
                    else
                    {
                        int oldProgress = (int)(((float)(_value - animationData) / Maximum) * Width);
                        if (OnRight)
                        {
                            e.Graphics.FillRectangle(new SolidBrush(SkinManager.GetApplicationBackgroundColor()), Width - doneProgress, 0, (float)(oldProgress - oldProgress * animationValue), Height);
                            e.Graphics.FillRectangle(SkinManager.GetDisabledOrHintBrush(), Width - doneProgress, 0, (float)(oldProgress - oldProgress * animationValue), Height);
                        }
                        else
                        {
                            e.Graphics.FillRectangle(new SolidBrush(SkinManager.GetApplicationBackgroundColor()), doneProgress - oldProgress + (float)(oldProgress * animationValue), 0, (float)(oldProgress - oldProgress * animationValue), Height);
                            e.Graphics.FillRectangle(SkinManager.GetDisabledOrHintBrush(), doneProgress - oldProgress + (float)(oldProgress * animationValue), 0, (float)(oldProgress - oldProgress * animationValue), Height);
                        }
                    }
                }
            }
            this.DrawChildShadow(e.Graphics);
        }
    }
}
