﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Objects;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    [Cached(Type = typeof(IManiaHitObjectComposer))]
    public class ManiaHitObjectComposer : HitObjectComposer<ManiaHitObject>, IManiaHitObjectComposer
    {
        private DrawableManiaEditRuleset drawableRuleset;

        public ManiaHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        /// <summary>
        /// Retrieves the column that intersects a screen-space position.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position.</param>
        /// <returns>The column which intersects with <paramref name="screenSpacePosition"/>.</returns>
        public Column ColumnAt(Vector2 screenSpacePosition) => drawableRuleset.GetColumnByPosition(screenSpacePosition);

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public ManiaPlayfield Playfield => ((ManiaPlayfield)drawableRuleset.Playfield);

        public IScrollingInfo ScrollingInfo => drawableRuleset.ScrollingInfo;

        public int TotalColumns => Playfield.TotalColumns;

        public override SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition)
        {
            var column = ColumnAt(screenSpacePosition);

            if (column == null)
                return new SnapResult(screenSpacePosition, null);

            var hoc = column.HitObjectContainer;

            // convert to local space of column so we can snap and fetch correct location.
            Vector2 localPosition = hoc.ToLocalSpace(screenSpacePosition);

            var scrollInfo = drawableRuleset.ScrollingInfo;

            if (scrollInfo.Direction.Value == ScrollingDirection.Down)
            {
                // We're dealing with screen coordinates in which the position decreases towards the centre of the screen resulting in an increase in start time.
                // The scrolling algorithm instead assumes a top anchor meaning an increase in time corresponds to an increase in position,
                // so when scrolling downwards the coordinates need to be flipped.
                localPosition.Y = hoc.DrawHeight - localPosition.Y;
            }

            double targetTime = scrollInfo.Algorithm.TimeAt(localPosition.Y, EditorClock.CurrentTime, scrollInfo.TimeRange.Value, hoc.DrawHeight);

            // apply beat snapping
            targetTime = BeatSnapProvider.SnapTime(targetTime);

            // convert back to screen space
            screenSpacePosition = ScreenSpacePositionAtTime(targetTime, column);

            return new ManiaSnapResult(screenSpacePosition, targetTime, column);
        }

        public Vector2 ScreenSpacePositionAtTime(double time, Column column = null)
        {
            var hoc = (column ?? Playfield.GetColumn(0)).HitObjectContainer;
            var scrollInfo = drawableRuleset.ScrollingInfo;

            var pos = scrollInfo.Algorithm.PositionAt(time, EditorClock.CurrentTime, scrollInfo.TimeRange.Value, hoc.DrawHeight);

            if (scrollInfo.Direction.Value == ScrollingDirection.Down)
            {
                // as explained above
                pos = hoc.DrawHeight - pos;
            }

            return hoc.ToScreenSpace(new Vector2(hoc.DrawWidth / 2, pos));
        }

        protected override DrawableRuleset<ManiaHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
        {
            drawableRuleset = new DrawableManiaEditRuleset(ruleset, beatmap, mods);

            // This is the earliest we can cache the scrolling info to ourselves, before masks are added to the hierarchy and inject it
            dependencies.CacheAs(drawableRuleset.ScrollingInfo);

            return drawableRuleset;
        }

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new ManiaBlueprintContainer(drawableRuleset.Playfield.AllHitObjects);

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new NoteCompositionTool(),
            new HoldNoteCompositionTool()
        };
    }
}
