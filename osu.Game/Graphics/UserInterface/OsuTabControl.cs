﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabControl<T> : TabControl<T>
    {
        protected override DropDown<T> CreateDropDown() => new OsuTabDropDown();

        protected override TabItem<T> CreateTabItem(T value) => new OsuTabItem<T> { Value = value };

        protected override bool InternalContains(Vector2 screenSpacePos) => base.InternalContains(screenSpacePos) || DropDown.Contains(screenSpacePos);

        public OsuTabControl()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("OsuTabControl only supports enums as the generic type argument");

            foreach (var val in (T[])Enum.GetValues(typeof(T)))
                AddItem(val);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.Blue;
        }

        private Color4? accentColour;
        public Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                var dropDown = DropDown as OsuTabDropDown;
                if (dropDown != null)
                    dropDown.AccentColour = value;
                foreach (var item in TabContainer.Children.OfType<OsuTabItem<T>>())
                    item.AccentColour = value;
            }
        }

        public class OsuTabDropDown : OsuDropDown<T>
        {
            protected override DropDownHeader CreateHeader() => new OsuTabDropDownHeader
            {
                AccentColour = AccentColour,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            };

            protected override DropDownMenuItem<T> CreateMenuItem(string text, T value)
            {
                var item = base.CreateMenuItem(text, value);
                item.ForegroundColourHover = Color4.Black;
                return item;
            }

            public OsuTabDropDown()
            {
                DropDownMenu.Anchor = Anchor.TopRight;
                DropDownMenu.Origin = Anchor.TopRight;

                RelativeSizeAxes = Axes.X;

                DropDownMenu.Background.Colour = Color4.Black.Opacity(0.7f);
                DropDownMenu.MaxHeight = 400;
            }

            public class OsuTabDropDownHeader : OsuDropDownHeader
            {
                public override Color4 AccentColour
                {
                    get { return base.AccentColour; }
                    set
                    {
                        base.AccentColour = value;
                        Foreground.Colour = value;
                    }
                }

                protected override bool OnHover(InputState state)
                {
                    Foreground.Colour = BackgroundColour;
                    return base.OnHover(state);
                }

                protected override void OnHoverLost(InputState state)
                {
                    Foreground.Colour = BackgroundColourHover;
                    base.OnHoverLost(state);
                }

                public OsuTabDropDownHeader()
                {
                    RelativeSizeAxes = Axes.None;
                    AutoSizeAxes = Axes.X;

                    BackgroundColour = Color4.Black.Opacity(0.5f);

                    Background.Height = 0.5f;
                    Background.CornerRadius = 5;
                    Background.Masking = true;

                    Foreground.RelativeSizeAxes = Axes.None;
                    Foreground.AutoSizeAxes = Axes.X;
                    Foreground.RelativeSizeAxes = Axes.Y;
                    Foreground.Margin = new MarginPadding(5);

                    Foreground.Children = new Drawable[]
                    {
                        new TextAwesome
                        {
                            Icon = FontAwesome.fa_ellipsis_h,
                            TextSize = 14,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        }
                    };

                    Padding = new MarginPadding { Left = 5, Right = 5 };
                }
            }
        }
    }
}
