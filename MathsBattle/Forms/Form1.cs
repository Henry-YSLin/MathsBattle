﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using MaterialSkin.Animations;
using MathsBattle.GameObjects;
using MathsBattle.GameObjects.Cards;
using MathsBattle.GameObjects.CustomControls;
using MathsBattle.GameObjects.Question;
using MaterialSkin.ControlRenderExtension;
using System.Reflection;

namespace MathsBattle
{

    public partial class Form1 : MaterialForm
    {
        #region Variables
        //variables for form1
        const int actionBarHeight = 24;
        const int statusBarHeight = 40;
        const bool NO_BG = false;

        //variables for start screen
        string[] tips = MathsBattle.Properties.Resources.Tips.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        int tipsNo;
        GameStartMode GameMode;
        enum GameStartMode { battle, exercise }

        //variables for battle
        int battleTick;
        Player me;
        Player opp;
        int questionNo = 1;
        int oppLevel = 0;

        //variables for exercise mode
        int exerciseTick;
        int exQuestionNo = 1;
        int exScore = 0;
        int exQuestionCount = 0;
        #endregion

        #region MyForm
        public Form1()
        {
            ((Control)this).SuspendDrawing();
            InitializeComponent();


            SkinManager.AddFormToManage(this);
            SkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            SkinManager.ColorScheme = new ColorScheme(Primary.Blue500, Primary.Blue700, Primary.Blue100, Accent.Blue200, TextShade.WHITE);
            if (NO_BG)
            {
#pragma warning disable CS0162 // 检测到无法访问的代码
                foreach (TabPage tb in Screens.TabPages)
#pragma warning restore CS0162 // 检测到无法访问的代码
                {
                    tb.BackgroundImage = null;
                }
            }
            else
            {
#pragma warning disable CS0162 // 检测到无法访问的代码
                if (SkinManager.Theme == MaterialSkinManager.Themes.DARK)
#pragma warning restore CS0162 // 检测到无法访问的代码
                {
                    foreach (TabPage tb in Screens.TabPages)
                    {
                        tb.BackgroundImage = Properties.Resources.StartScreenBGDark;
                    }
                }
                else
                {
                    foreach (TabPage tb in Screens.TabPages)
                    {
                        tb.BackgroundImage = Properties.Resources.StartScreenBG;
                    }
                }
            }
            if (SkinManager.Theme == MaterialSkinManager.Themes.DARK)
            {
                btnNextTip.Icon = MathsBattle.Properties.Resources.navRight;
                btnPrevTip.Icon = MathsBattle.Properties.Resources.navLeft;
            }
            else
            {
                btnNextTip.Icon = MathsBattle.Properties.Resources.navRightDark;
                btnPrevTip.Icon = MathsBattle.Properties.Resources.navLeftDark;
            }
            Random rnd = new Random((int)DateTime.Now.Ticks);
            tipsNo = rnd.Next(0, tips.Count());
            lblTips.Text = tips[tipsNo];
            //screenBattle.BackgroundImage = null;
            panelBattle.BackColor = Color.FromArgb(175, SkinManager.GetApplicationBackgroundColor());
            panelTips.BackColor = Color.FromArgb(175, SkinManager.GetApplicationBackgroundColor());
            panelRightDock.BackColor = Color.FromArgb(175, SkinManager.GetApplicationBackgroundColor());
            panelOppFooter.BackColor = Color.FromArgb(175, SkinManager.GetApplicationBackgroundColor());
            panelGameOverFooter.BackColor = Color.FromArgb(175, SkinManager.GetApplicationBackgroundColor());
            panelQuestionAlign.BackColor = Color.Transparent;
            foreach (Control c in panelBattle.Controls)
            {
                if (SupportsTransparentBackColor(c)) c.BackColor = Color.Transparent;
            }
            foreach (Control c in panelOppFooter.Controls)
            {
                if (SupportsTransparentBackColor(c)) c.BackColor = Color.Transparent;
            }
            foreach (Control c in panelRightDock.Controls)
            {
                if (SupportsTransparentBackColor(c)) c.BackColor = Color.Transparent;
            }
            foreach (Control c in panelTips.Controls)
            {
                if (SupportsTransparentBackColor(c)) c.BackColor = Color.Transparent;
            }
            foreach (Control c in panelGameOverFooter.Controls)
            {
                if (SupportsTransparentBackColor(c)) c.BackColor = Color.Transparent;
            }
            foreach (Control c in screenExercise.Controls)
            {
                if (SupportsTransparentBackColor(c)) c.BackColor = Color.Transparent;
            }
            panelExQuestionAlign.BackColor = SkinManager.GetApplicationBackgroundColor();
            ((Control)this).ResumeDrawing();
        }
        private bool SupportsTransparentBackColor(Control control)
        {
            MethodInfo getstyle = typeof(Control).GetMethod("GetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            return (bool)getstyle.Invoke(control, new object[] { ControlStyles.SupportsTransparentBackColor });
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            Size = new Size(871, 562);
            if (Screens.SelectedTab == screenStart)
            {
                Screens.Height = Height - actionBarHeight;
            }
            else
            {
                Screens.Height = Height - actionBarHeight - statusBarHeight;
            }
        }
        private void SwitchScreen(TabPage t)
        {
            ((Control)this).SuspendDrawing();
            Screens.SelectedTab = t;
            if (Screens.SelectedTab == screenStart)
            {
                Screens.Height = Height - actionBarHeight;
            }
            else
            {
                Screens.Height = Height - actionBarHeight - statusBarHeight;
            }
            ((Control)this).ResumeDrawing();
        }
        #endregion

        #region screenStart
        private void btnStartBattle_ClickAnimationFinished(object sender)
        {
            GameMode = GameStartMode.battle;
            SwitchScreen(screenGameSettings);
        }
        private void btnExercise_ClickAnimationFinished(object sender)
        {
            GameMode = GameStartMode.exercise;
            SwitchScreen(screenGameSettings);
        }
        private void btnNextTip_Click(object sender, EventArgs e)
        {
            tipsNo += 1;
            if (tipsNo >= tips.Count()) tipsNo = 0;
            lblTips.Text = tips[tipsNo];
        }
        private void btnPrevTip_Click(object sender, EventArgs e)
        {
            tipsNo -= 1;
            if (tipsNo < 0) tipsNo = tips.Count() - 1;
            lblTips.Text = tips[tipsNo];
        }
        #endregion

        #region screenGameSettings
        private void btnBack_ClickAnimationFinished(object sender)
        {
            SwitchScreen(screenStart);
            ClearSettings();
        }
        private void ClearSettings()
        {
            rb15Minute.Checked = false;
            rb10Minute.Checked = false;
            rb5Minute.Checked = false;
            rb2Minute.Checked = false;
        }
        private void btnStartGame_ClickAnimationFinished(object sender)
        {
            if (GameMode == GameStartMode.battle)
            {
                ResetBattle();
                if (rb2Minute.Checked)
                {
                    battleTick = 120;
                }
                else if (rb5Minute.Checked)
                {
                    battleTick = 300;
                }
                else if (rb10Minute.Checked)
                {
                    battleTick = 600;
                }
                else if (rb15Minute.Checked)
                {
                    battleTick = 900;
                }
                if (battleTick <= 0)
                {
                    return;
                }

                oppLevel++;
                List<Card> CS = new List<Card>();
                CS.Add(new NormalAttack());
                CS.Add(new MegaAttack());
                CS.Add(new ContinuedAttack());
                CS.Add(new ContinuedHealing());
                List<Card> CS2 = new List<Card>();
                CS2.Add(new NormalAttack());
                me = new GameObjects.Player(4, 100, 0, 100, 100, "You", CS);
                opp = new GameObjects.Player(1, 40, 3, 40, 3, "Opponent", CS2);
                SetNewOpp(oppLevel);
                me.OnValueChange += Me_OnValueChange;
                me.OnEffectsChange += Me_OnEffectsChange;
                opp.OnEffectsChange += Opp_OnEffectsChange;
                opp.OnValueChange += Opp_OnValueChange;
                opp.OnDead += Opp_OnDead;
                me.OnDead += Me_OnDead;

                newQuestion();
                newQuestion();
                me.FillActiveCard();
                updateCard();
                Opp_OnValueChange(opp);
                Me_OnValueChange(me);
                timerBattle.Start();

                SwitchScreen(screenBattle);

                lblKillBanner.Text = "Game Start!";
                lblKillBanner.Show();
                timerKillBanner.Start();
            }
            else if (GameMode == GameStartMode.exercise)
            {
                ResetExercise();
                if (rb2Minute.Checked)
                {
                    exerciseTick = 120;
                }
                else if (rb5Minute.Checked)
                {
                    exerciseTick = 300;
                }
                else if (rb10Minute.Checked)
                {
                    exerciseTick = 600;
                }
                else if (rb15Minute.Checked)
                {
                    exerciseTick = 900;
                }
                if (exerciseTick <= 0)
                {
                    return;
                }
                ExCountdown.Maximum = exerciseTick;
                newExQuestion();
                newExQuestion();
                newExQuestion();
                timerExercise.Start();
                SwitchScreen(screenExercise);
            }
            ClearSettings();
        }
        #endregion

        #region screenBattle
        private void Me_OnDead(Player sender)
        {
            ResetBattle();
            lblGameResult.Text = "You Died!";
            SwitchScreen(screenGameOver);
        }
        private void ResetBattle()
        {
            timerBattle.Stop();
            battleTick = 0;
            oppLevel = 0;
            for (int i = panelQuestionAlign.Controls.Count - 1; i >= 0; i--)
            {
                panelQuestionAlign.Controls.RemoveAt(i);
            }
            questionNo = 1;
            for (int i = panelMeEffect.Controls.Count - 1; i >= 0; i--)
            {
                panelMeEffect.Controls.RemoveAt(i);
            }
            for (int i = panelOppEffect.Controls.Count - 1; i >= 0; i--)
            {
                panelOppEffect.Controls.RemoveAt(i);
            }
            lblTimeLeft.Text = "00:00";
            card1.Text = "Card";
            card1.Info = "Card Info";
            card1.Image = MathsBattle.Properties.Resources.lightCard;
            card2.Text = "Card";
            card2.Info = "Card Info";
            card2.Image = MathsBattle.Properties.Resources.lightCard;
            card3.Text = "Card";
            card3.Info = "Card Info";
            card3.Image = MathsBattle.Properties.Resources.lightCard;
            card4.Text = "Card";
            card4.Info = "Card Info";
            card4.Image = MathsBattle.Properties.Resources.lightCard;
            meCard.Text = "Card";
            meCard.Info = "Card Info";
            oppCard.Text = "Card";
            oppCard.Info = "Card Info";
            meHP.Value = 0;
            meMP.Value = 0;
            oppHP.Value = 0;
            oppMP.Value = 0;
            lblMeHP.Text = "0/100";
            lblMeMP.Text = "0/100";
            lblOppHP.Text = "0/100";
            lblOppMP.Text = "0/100";
            panelCardInfo.Visible = false;
            lblCardInfo.Text = "Long Card Info";
            me = null;
            opp = null;
            lblKillBanner.Text = "Game Start!";
            lblKillBanner.Hide();
            timerKillBanner.Stop();
        }
        private void Opp_OnDead(Player sender)
        {
            lblKillBanner.Show();
            timerKillBanner.Start();
            me.HP += opp.MaxHP / 3;
            oppLevel++;
            SetNewOpp(oppLevel);
        }
        private void SetNewOpp(int lvl)
        {
            List<Card> CS = new List<Card>();
            if (lvl % 5 == 0)
            {
                CS.Add(new ContinuedAttack());
                CS.Add(new MegaAttack());
                opp.Reset(1, opp.MaxHP + 50, 3 + (int)Math.Floor(lvl * 0.2), opp.MaxHP + 50, 3 + (int)Math.Floor(lvl * 0.2), "Lvl " + lvl.ToString() + " BOSS", CS);
            }
            else
            {
                CS.Add(new NormalAttack());
                opp.Reset(1, opp.MaxHP + 50, 3 + (int)Math.Floor(lvl * 0.2), opp.MaxHP + 50, 3 + (int)Math.Floor(lvl * 0.2), "Lvl " + lvl.ToString() + " Opponent", CS);
            }
            lblOppName.Text = opp.Name;
        }
        private void newQuestion()
        {
            Question q = QuestionGenerator.Generate(null, new Type[] { typeof(SimplePercentage) }.ToList());
            QuestionCard card = new QuestionCard();
            card.Question = q;
            card.QuestionNo = questionNo;
            questionNo++;
            card.Dock = DockStyle.Bottom;
            card.OnAnswered += Card_OnAnswered;
            card.OnClosed += Card_OnClosed;
            panelQuestionAlign.Controls.Add(card);
        }
        private void Card_OnClosed(QuestionCard sender)
        {
            newQuestion();
            panelQuestionAlign.Controls.Remove(sender);
        }
        private void Card_OnAnswered(QuestionCard sender, bool isCorrect)
        {
            if (isCorrect) me.MP += 20;
            Me_OnValueChange(me);
            opp.MP -= 1;
            Opp_OnValueChange(opp);
        }
        private void Opp_OnEffectsChange(Player sender)
        {
            RefreshOppEffects();
        }
        private void Me_OnEffectsChange(Player sender)
        {
            RefreshMeEffects();
        }
        private void RefreshOppEffects()
        {
            for (int i = opp.Effects.Count - 1; i >= 0; i--)
            {
                if (opp.Effects[i].LoopCount == opp.Effects[i].LoopNo)
                {
                    opp.Effects.RemoveAt(i);
                }
            }
            for (int i = panelOppEffect.Controls.Count - 1; i >= 0; i--)
            {
                if (panelOppEffect.Controls[i].Height == 1)
                {
                    panelOppEffect.Controls.RemoveAt(i);
                }
            }
            foreach (TimedEvent te in opp.Effects)
            {
                bool handled = false;
                foreach (TimedEffectCard c in panelOppEffect.Controls)
                {
                    if (c.BindedTimedEvent == te) handled = true;
                }
                if (!handled)
                {
                    TimedEffectCard nc = new GameObjects.CustomControls.TimedEffectCard();
                    nc.BindedTimedEvent = te;
                    nc.Dock = DockStyle.Top;
                    nc.OnClosed += OppEffect_OnClosed;
                    panelOppEffect.Controls.Add(nc);
                }
            }
        }
        private void RefreshMeEffects()
        {
            for (int i = me.Effects.Count - 1; i >= 0; i--)
            {
                if (me.Effects[i].LoopCount == me.Effects[i].LoopNo)
                {
                    me.Effects.RemoveAt(i);
                }
            }
            for (int i = panelMeEffect.Controls.Count - 1; i >= 0; i--)
            {
                if (panelMeEffect.Controls[i].Height == 1)
                {
                    panelMeEffect.Controls.RemoveAt(i);
                }
            }
            foreach (TimedEvent te in me.Effects)
            {
                bool handled = false;
                foreach (TimedEffectCard c in panelMeEffect.Controls)
                {
                    if (c.BindedTimedEvent == te) handled = true;
                }
                if (!handled)
                {
                    TimedEffectCard nc = new GameObjects.CustomControls.TimedEffectCard();
                    nc.BindedTimedEvent = te;
                    nc.Dock = DockStyle.Top;
                    nc.OnClosed += MeEffect_OnClosed;
                    panelMeEffect.Controls.Add(nc);
                }
            }
        }
        private void OppEffect_OnClosed(TimedEffectCard sender)
        {
            panelOppEffect.Controls.Remove(sender);
        }
        private void MeEffect_OnClosed(TimedEffectCard sender)
        {
            panelMeEffect.Controls.Remove(sender);
        }
        private void Opp_OnValueChange(Player sender)
        {
            if (opp == null) return;
            oppHP.Maximum = opp.MaxHP;
            oppHP.Minimum = 0;
            oppHP.Value = opp.HP;
            lblOppHP.Text = opp.HP.ToString() + "/" + opp.MaxHP.ToString();
            oppMP.Maximum = opp.MaxMP;
            oppMP.Minimum = 0;
            oppMP.Value = opp.MP;
            lblOppMP.Text = opp.MP.ToString() + "/" + opp.MaxMP.ToString();
            if (opp.MP <= 0)
            {
                oppAttack();
            }
        }
        private void Me_OnValueChange(Player sender)
        {
            if (me == null) return;
            meHP.Maximum = me.MaxHP;
            meHP.Minimum = 0;
            meHP.Value = me.HP;
            lblMeHP.Text = me.HP.ToString() + "/" + me.MaxHP.ToString();
            meMP.Maximum = me.MaxMP;
            meMP.Minimum = 0;
            meMP.Value = me.MP;
            lblMeMP.Text = me.MP.ToString() + "/" + me.MaxMP.ToString();
            for (int i = 1; i < 5; i++)
            {
                MaterialSmallCard sm = panelBattle.Controls["card" + i.ToString()] as MaterialSmallCard;
                if (sm != null)
                {
                    Card c = sm.Tag as Card;
                    if (c != null)
                    {
                        if (me.MP >= c.Cost)
                        {
                            sm.Image = MathsBattle.Properties.Resources.lightCard;
                        }
                        else
                        {
                            sm.Image = MathsBattle.Properties.Resources.darkCard;
                        }
                    }
                }
            }
        }
        private void oppAttack()
        {
            opp.MP = opp.MaxMP;
            for (int i = opp.ActiveCard.Count - 1; i >= 0; i--)
            {
                Card c = opp.ActiveCard[i];
                oppCard.Tag = c;
                oppCard.Text = c.Name;
                oppCard.Info = "Cost: " + c.Cost.ToString();
                oppCard.Show();
                c.Action(opp, me);
                opp.ActiveCard.Remove(c);
                if (opp == null) return;
                opp.FillActiveCard();
                updateCard();
                RefreshMeEffects();
                RefreshOppEffects();
                Me_OnValueChange(me);
                Opp_OnValueChange(opp);
                oppCard.AutoExpand = false;
                timerOppRecentCard.Start();
            }
        }
        private void updateCard()
        {
            for (int i = 0; i < me.ActiveCard.Count; i++)
            {
                MaterialSmallCard sm = panelBattle.Controls["card" + (i + 1).ToString()] as MaterialSmallCard;
                if (sm != null)
                {
                    sm.Text = me.ActiveCard[i].Name;
                    sm.Info = "Cost: " + me.ActiveCard[i].Cost.ToString();
                    sm.Tag = me.ActiveCard[i];
                }
            }
        }
        private void GameCard_MouseEnter(object sender, EventArgs e)
        {
            MaterialSmallCard sm = (MaterialSmallCard)sender;
            Card c = (Card)sm.Tag;
            if (c != null)
            {
                lblCardInfo.Text = c.Info;
                panelCardInfo.Show();
            }
        }
        private void GameCard_MouseLeave(object sender, EventArgs e)
        {
            panelCardInfo.Hide();
        }
        private void GameCard_Click(object sender, EventArgs e)
        {
            MaterialSmallCard sm = (MaterialSmallCard)sender;
            Card c = (Card)sm.Tag;
            if (c != null)
            {
                if (me.MP < c.Cost) return;
                meCard.Tag = c;
                meCard.Text = c.Name;
                meCard.Info = "Cost: " + c.Cost.ToString();
                meCard.Show();
                me.MP -= c.Cost;
                c.Action(me, opp);
                me.ActiveCard.Remove(c);
                if (me == null) return;
                opp.MP -= 1;
                me.FillActiveCard();
                updateCard();
                RefreshMeEffects();
                RefreshOppEffects();
                Me_OnValueChange(me);
                Opp_OnValueChange(opp);
                meCard.AutoExpand = false;
                timerMeRecentCard.Start();
            }
        }
        private void lblKillBanner_VisibleChanged(object sender, EventArgs e)
        {
            if (lblKillBanner.Visible == false)
            {
                lblKillBanner.Text = "Opponent Defeated!";
            }
        }
        private void timerBattle_Tick(object sender, EventArgs e)
        {
            if (battleTick <= 0)
            {
                ResetBattle();
                lblGameResult.Text = "Time's Up!";
                SwitchScreen(screenGameOver);
                return;
            }
            battleTick -= 1;
            lblTimeLeft.Text = new DateTime(battleTick * 1000L * 10000).ToString("mm:ss");
            RefreshMeEffects();
            RefreshOppEffects();
        }
        private void timerOppRecentCard_Tick(object sender, EventArgs e)
        {
            oppCard.AutoExpand = true;
            timerOppRecentCard.Stop();
        }
        private void timerMeRecentCard_Tick(object sender, EventArgs e)
        {
            meCard.AutoExpand = true;
            timerMeRecentCard.Stop();
        }
        private void timerKillBanner_Tick(object sender, EventArgs e)
        {
            if (lblKillBanner.Text == "Opponent Defeated!")
            {
                lblKillBanner.Text = "Lvl " + oppLevel.ToString() + " Opponent";
            }
            else
            {
                lblKillBanner.Hide();
                timerKillBanner.Stop();
            }
        }
        #endregion

        #region screenExercise
        private void timerExercise_Tick(object sender, EventArgs e)
        {
            if (exerciseTick <= 0)
            {
                ResetExercise();
                lblGameResult.Text = "Time's Up!";
                SwitchScreen(screenGameOver);
                return;
            }
            exerciseTick -= 1;
            lblExTime.Text = new DateTime(exerciseTick * 1000L * 10000).ToString("mm:ss");
            ExCountdown.Value = exerciseTick;
        }
        private void newExQuestion()
        {
            Question q = QuestionGenerator.Generate(null, new Type[] { typeof(SimplePercentage) }.ToList());
            QuestionCard card = new QuestionCard();
            card.Question = q;
            card.QuestionNo = exQuestionNo;
            exQuestionNo++;
            card.Dock = DockStyle.Right;
            card.OnAnswered += ExCard_OnAnswered;
            card.OnClosed += ExCard_OnClosed;
            panelExQuestionAlign.Controls.Add(card);
        }
        private void ExCard_OnClosed(QuestionCard sender)
        {
            newExQuestion();
            panelExQuestionAlign.Controls.Remove(sender);
        }
        private void ExCard_OnAnswered(QuestionCard sender, bool isCorrect)
        {
            if (isCorrect)
            {
                exScore += 1;
                exQuestionCount += 1;
                lblExScore.Text = exScore.ToString() + " / " + exQuestionCount.ToString();
                lblExMark.Text = Math.Round(exScore / (float)exQuestionCount * 100, 1).ToString() + "%";
            }
            else
            {
                exQuestionCount += 1;
                lblExScore.Text = exScore.ToString() + " / " + exQuestionCount.ToString();
                lblExMark.Text = Math.Round(exScore / (float)exQuestionCount * 100, 1).ToString() + "%";
            }
        }
        private void ResetExercise()
        {
            timerExercise.Stop();
            exerciseTick = 0;
            for (int i = panelExQuestionAlign.Controls.Count - 1; i >= 0; i--)
            {
                panelExQuestionAlign.Controls.RemoveAt(i);
            }
            exScore = 0;
            exQuestionNo = 1;
            exQuestionCount = 0;
            lblExTime.Text = "00:00";
            lblExMark.Text = "0%";
            lblExScore.Text = "0 / 0";
            ExCountdown.Value = 0;
        }
        #endregion

        #region screenGameOver
        private void btnGameOverBack_ClickAnimationFinished(object sender)
        {
            SwitchScreen(screenStart);
        }
        #endregion
    }
}
