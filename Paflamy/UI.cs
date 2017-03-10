using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;

using Android.Views;
using Android.Content;
using Android.Util;
using System.Collections.Generic;
using Size = System.Drawing.SizeF;
using System.Linq;

namespace Paflamy
{
    public static class UI
    {
        public static int SCREEN_WIDTH { get; private set; }
        public static int SCREEN_HEIGHT { get; private set; }

        public const float HORI_BORDER = 0;
        public const float VERT_BORDER = 0;

        public const float MENU_LEVEL_SCALE = 0.6f;
        public static float MENU_X_PADDING;
        public static float MENU_Y_PADDING;
        public static float MENU_LEVEL_MARGIN;
        public static float MENU_LEVEL_DIST;
        public static float MENU_LEVEL_WIDTH;
        public const int MENU_NEIGHBOR_COUNT = 2;

        public static RectangleF StartButton { get; private set; }
        public static readonly Color StartColor = Color.DodgerBlue;

        public static float TileWidth { get; private set; }
        public static float TileHeight { get; private set; }
        public static float MenuTileSize { get; private set; }

        public static int MenuLevelIndex { get; private set; }
        public static float MenuOffset { get; set; }

        public static List<Size> TileSizes { get; private set; }

        public static void Init(int width, int height)
        {
            SCREEN_WIDTH = width;
            SCREEN_HEIGHT = height;

            SetTileSize();

            MenuTileSize = TileWidth;
            MENU_X_PADDING = (1 - MENU_LEVEL_SCALE) / 2 * SCREEN_WIDTH;
            MENU_Y_PADDING = 0.15f * SCREEN_HEIGHT;
            MENU_LEVEL_MARGIN = 0.09f * SCREEN_WIDTH;
            MENU_LEVEL_WIDTH = SCREEN_WIDTH * MENU_LEVEL_SCALE;
            MENU_LEVEL_DIST = MENU_LEVEL_WIDTH + MENU_LEVEL_MARGIN;

            float bWidth = SCREEN_WIDTH / 3f;
            float bHeight = (SCREEN_HEIGHT - SCREEN_WIDTH) / 3f;
            StartButton = new RectangleF(bWidth, SCREEN_WIDTH + bHeight, bWidth, bHeight);
            Game.LevelChanged += SetTileSize;

            TileSizes = new List<Size>();
            foreach (var level in Game.LevelSet)
            {
                GetTileSize(level, out float w, out float h);
                TileSizes.Add(new Size(w, h));
            }
        }

        private static void SetTileSize()
        {
            GetTileSize(Game.Level, out float tileWidth, out float tileHeight);
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        private static void GetTileSize(Level level, out float width, out float height)
        {
            width = (SCREEN_WIDTH - 2 * HORI_BORDER) / level.Width;
            height = (SCREEN_HEIGHT - 2 * VERT_BORDER) / level.Height;
        }

        private static void UpdateMenuStage(double dt)
        {
            if (Input.Dragging || MenuOffset == 0)
                return;
            
            int sign = Math.Sign(Input.LastOffsetDelta);

            int moOldSign = Math.Sign(MenuOffset);
            MenuOffset += 15 * sign;
            int moNewSign = Math.Sign(MenuOffset);

            Util.Log($"menu offset: {MenuOffset}, i: {MenuLevelIndex}");

            if (Math.Abs(MenuOffset) > MENU_LEVEL_DIST / 2)
            {
                MenuLevelIndex -= sign;
                MenuOffset = (MENU_LEVEL_DIST - Math.Abs(MenuOffset)) * Math.Sign(MenuOffset) * -1;
            }
            else if (moOldSign != moNewSign)
            {
                MenuOffset = 0;
            }
        }

        public static void OnUpdate(double dt)
        {
            switch (Game.Stage)
            {
                case Stage.Playing: break;
                case Stage.Menu: UpdateMenuStage(dt); break;
                case Stage.Start: break;
                default: throw new Exception();
            }
        }
    }
}