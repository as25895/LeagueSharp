﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ezEvade
{
    internal class SpellDrawer
    {
        public static Menu menu;
        private static float gameTime { get { return Game.ClockTime * 1000; } }
        private static Obj_AI_Hero myHero { get { return ObjectManager.Player; } }


        public SpellDrawer(Menu mainMenu)
        {
            Drawing.OnDraw += Drawing_OnDraw;

            menu = mainMenu;
            Game_OnGameLoad();
        }

        private void Game_OnGameLoad()
        {
            //Game.PrintChat("SpellDrawer loaded");

            Menu drawMenu = new Menu("Draw", "Draw");
            drawMenu.AddItem(new MenuItem("DrawSkillShots", "Draw SkillShots").SetValue(true));

            Menu dangerMenu = new Menu("DangerLevel Drawings", "DangerLevelDrawings");
            Menu lowDangerMenu = new Menu("Low", "0");
            lowDangerMenu.AddItem(new MenuItem("Width", "Line Width").SetValue(new Slider(3, 1, 15)));
            lowDangerMenu.AddItem(new MenuItem("Color", "Color").SetValue(new Circle(true, Color.FromArgb(60, 255, 255, 255))));

            Menu normalDangerMenu = new Menu("Normal", "1");
            normalDangerMenu.AddItem(new MenuItem("Width", "Line Width").SetValue(new Slider(3, 1, 15)));
            normalDangerMenu.AddItem(new MenuItem("Color", "Color").SetValue(new Circle(true, Color.FromArgb(140, 255, 255, 255))));

            Menu highDangerMenu = new Menu("High", "2");
            highDangerMenu.AddItem(new MenuItem("Width", "Line Width").SetValue(new Slider(4, 1, 15)));
            highDangerMenu.AddItem(new MenuItem("Color", "Color").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            Menu extremeDangerMenu = new Menu("Extreme", "3");
            extremeDangerMenu.AddItem(new MenuItem("Width", "Line Width").SetValue(new Slider(4, 1, 15)));
            extremeDangerMenu.AddItem(new MenuItem("Color", "Color").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            /*
            Menu undodgeableDangerMenu = new Menu("Undodgeable", "Undodgeable");
            undodgeableDangerMenu.AddItem(new MenuItem("Width", "Line Width").SetValue(new Slider(6, 1, 15)));
            undodgeableDangerMenu.AddItem(new MenuItem("Color", "Color").SetValue(new Circle(true, Color.FromArgb(255, 255, 0, 0))));*/

            dangerMenu.AddSubMenu(lowDangerMenu);
            dangerMenu.AddSubMenu(normalDangerMenu);
            dangerMenu.AddSubMenu(highDangerMenu);
            dangerMenu.AddSubMenu(extremeDangerMenu);

            drawMenu.AddSubMenu(dangerMenu);
            
            menu.AddSubMenu(drawMenu);           
        }

        private void DrawLineRectangle(Vector2 start, Vector2 end, int radius, int width, Color color)
        {
            var dir = (end - start).Normalized();
            var pDir = dir.Perpendicular();

            var rightStartPos = start + pDir * radius;
            var leftStartPos = start - pDir * radius;
            var rightEndPos = end + pDir * radius;
            var leftEndPos = end - pDir * radius;

            var rStartPos = Drawing.WorldToScreen(new Vector3(rightStartPos.X, rightStartPos.Y, myHero.Position.Z));
            var lStartPos = Drawing.WorldToScreen(new Vector3(leftStartPos.X, leftStartPos.Y, myHero.Position.Z));
            var rEndPos = Drawing.WorldToScreen(new Vector3(rightEndPos.X, rightEndPos.Y, myHero.Position.Z));
            var lEndPos = Drawing.WorldToScreen(new Vector3(leftEndPos.X, leftEndPos.Y, myHero.Position.Z));

            Drawing.DrawLine(rStartPos, rEndPos, width, color);
            Drawing.DrawLine(lStartPos, lEndPos, width, color);
            Drawing.DrawLine(rStartPos, lStartPos, width, color);
            Drawing.DrawLine(lEndPos, rEndPos, width, color);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (menu.SubMenu("Draw").Item("DrawSkillShots").GetValue<bool>() == false)
            {
                return;
            }     

            foreach (KeyValuePair<int, Spell> entry in SpellDetector.drawSpells)
            {
                Spell spell = entry.Value;
                var spellDrawingConfig = Evade.menu.SubMenu("Draw").SubMenu("DangerLevelDrawings")
                    .SubMenu(""+EvadeHelper.GetSpellDangerLevel(spell)).Item("Color").GetValue<Circle>();
                var spellDrawingWidth = Evade.menu.SubMenu("Draw").SubMenu("DangerLevelDrawings")
                    .SubMenu("" + EvadeHelper.GetSpellDangerLevel(spell)).Item("Width").GetValue<Slider>().Value;

                if (Evade.menu.SubMenu("Spells").SubMenu(spell.info.charName + spell.info.spellName + "Settings").Item("DrawSpell").GetValue<bool>()
                    && spellDrawingConfig.Active)
                {
                    if (spell.info.spellType == SpellType.Line)
                    {
                        Vector2 spellPos = SpellDetector.GetCurrentSpellPosition(spell);
                        DrawLineRectangle(spellPos, spell.endPos, (int)EvadeHelper.GetSpellRadius(spell), spellDrawingWidth, spellDrawingConfig.Color);
                    }
                    else if (spell.info.spellType == SpellType.Circular)
                    {
                        Render.Circle.DrawCircle(new Vector3(spell.endPos.X, spell.endPos.Y, myHero.Position.Z), (int)EvadeHelper.GetSpellRadius(spell), spellDrawingConfig.Color, spellDrawingWidth);
                    }
                    else if (spell.info.spellType == SpellType.Cone)
                    {

                    }
                }
            }
        }
    }
}
