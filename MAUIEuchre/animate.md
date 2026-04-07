# Card Animation Plan

## Overview

Add card movement animations controlled by a "Show animations" gameplay option (persisted via
`GameSettings.ShowAnimations` / `_modeShowAnimations`). All animations flow through one method:

```csharp
private async Task AnimateCards(Image[]? sources, Image destination, uint duration = 500)
```

- If `_modeShowAnimations` is false, this is a no-op (returns immediately).
- `sources` is an array of XAML Image elements to animate. If null, cards originate from
  center of table (427, 367) — the midpoint of the 855×735 canvas.
- `destination` is the target XAML Image element. The animation ends at its LayoutBounds position.
- All cards in the array animate simultaneously (Task.WhenAll).
- Default duration is 500ms but is stored in a field (`_animationDuration`) so it can be
  tuned later without code changes.

## The AnimateCards Method — Detailed Behavior

For each source Image in the array (or a virtual center-of-table origin if sources is null):

1. **Create a temp Image** in `EuchreGrid` (the AbsoluteLayout).
   - Copy the source image's `Source`, `WidthRequest`, `HeightRequest`, `Rotation` from
     the source Image (or use cardback.png at rotation 0 for null sources).
2. **Position it** at the source's LayoutBounds (X, Y) — or center-of-table if null.
3. **Compute deltas**: destination LayoutBounds minus starting position for TranslateTo,
   and destination `Rotation` minus starting rotation for RotateTo.
4. **Determine face change**: If the source shows cardback but destination shows a face
   (or vice-versa), the image swap happens at the midpoint of animation (~250ms mark for
   500ms animation). We run first-half, swap Source, then second-half. If no change needed,
   run as one continuous animation.
5. **Run the animation**: TranslateTo + RotateTo in parallel, with Easing.CubicInOut.
6. **On completion**: Remove temp Image from EuchreGrid.

The caller is responsible for making the destination Image visible (showing the final card)
and hiding the source Image(s) — the animation method only handles the visual motion.

## The Option

- **GameSettings.cs**: Add `ShowAnimations` bool property (Preferences, default false).
- **EuchreOptions.xaml**: Add "Show animations" checkbox to Gameplay options section.
- **EuchreOptions.xaml.cs**: Wire up load/save like other options.
- **EuchreTable.xaml.cs**: Add `_modeShowAnimations` field, load from GameSettings in NewGame.

## Integration Points (Call Sites)

### (a) Deal for Deal — `DealACardForDeal()`

Cards fly from center-of-table to each player's card slot during dealer selection.

```
await AnimateCards(null, gameTableTopCards[(int)player, slot]);
```

- sources = null → center of table
- destination = the dealt card slot
- Card starts as cardback at rotation 0, destination shows face-up card at seat rotation.
  Face change + rotation change happen during animation.

### (b) Dealing Hands — `DealACard()`

Cards fly from center-of-table to each player's hand slot. All cards show cardback
(face-down) at destination except the player's own cards (face-up but only if not peek mode —
well, actually `DealACard` always sets face-down for non-Player).

```
await AnimateCards(null, gameTableTopCards[(int)player, slot]);
```

- sources = null → center of table
- destination = the hand slot
- Card starts as cardback at rotation 0, ends as cardback at seat rotation.
  Rotation changes during animation; image stays cardback throughout.

### (c) Playing a Card — `PlaySelectedCard()`

Card flies from player's hand slot to their played-card position (center area).

```
await AnimateCards(
    new[] { gameTableTopCards[(int)player.Seat, index] },
    gameTableTopCards[(int)player.Seat, 5]);
```

- sources = the hand card Image (slot 0-4)
- destination = the played card Image (slot 5 — LeftOpponentCard, RightOpponentCard, etc.)
- For Player/Partner: card is face-up, no rotation change (both 0° or both 180°... actually
  Player is 0° in hand and 0° when played; Partner is 180° in hand and 0° when played
  per the played card positions — need to check). Actually the played card position is set
  via `SetCardImage` which sets rotation from the card's perspective. The card perspective
  changes to `player.Seat` and `rotationAngle` is calculated.
- For Left/Right opponents: card transitions from 90°/270° (hand) to 0° (played center).
  Also transitions from face-down (cardback) to face-up.
- The animation handles both the rotation and face-change smoothly.

### (d) Kitty Card Swap — `SwapCardWithKitty()`

Two separate animations:
1. **Kitty → dealer's hand**: The top kitty card (KittyCard1) flies to the replaced hand slot.
   ```
   await AnimateCards(new[] { KittyCard1 }, gameTableTopCards[(int)player.Seat, index]);
   ```
   - Face change at midpoint: kitty is face-up, hand card may be face-down (for AI dealers).
   - Rotation change: kitty is 0° (Player perspective), hand slot is at seat rotation.

2. **Dealer's hand → kitty**: The discarded card flies to kitty position.
   ```
   await AnimateCards(
       new[] { gameTableTopCards[(int)player.Seat, index] },
       KittyCard1);
   ```
   - Face change: hand card (face-up or face-down) to face-down kitty.
   - Rotation change: seat rotation → 0°.

### (e) Clearing Tricks — `PrepTrick()` (called on Continue click)

All played cards (slot 5 for each seat) fly en masse to the winning team's tricks-taken
label. The `trickPlayerWhoPlayedHighestCardSoFar` determines the destination:
- Player or Partner won → fly to `YourTricks` label
- LeftOpponent or RightOpponent won → fly to `TheirTricks` label

```csharp
// Collect visible played cards
var playedCards = new List<Image>();
for (var i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
{
    if (gameTableTopCards[(int)i, 5].IsVisible)
        playedCards.Add(gameTableTopCards[(int)i, 5]);
}

Image tricksLabel = (trickPlayerWhoPlayedHighestCardSoFar == EuchrePlayer.Seats.Player ||
                     trickPlayerWhoPlayedHighestCardSoFar == EuchrePlayer.Seats.Partner)
                    ? (Image)YourTricks   // Actually this is a Label — need a position proxy
                    : (Image)TheirTricks;
```

**Issue**: YourTricks/TheirTricks are Labels, not Images. The AnimateCards method takes
Image for destination. We need to handle this: either create a hidden proxy Image at the
label's position, or extract position from the Label directly in AnimateCards (make the
destination parameter `View` instead of `Image`).

**Resolution**: Change the destination parameter type to `View` so it can accept Labels.
The method only reads its LayoutBounds for position — it doesn't need Source/Rotation from
the destination. When destination is not an Image, use rotation 0 and skip Source logic.

**Updated signature**:
```csharp
private async Task AnimateCards(Image[]? sources, View destination, uint duration = 500)
```

Cards shrink (ScaleTo 0.3) as they approach the tricks label, giving a "collected" feel.

## Key Constants / Positions

| Element | LayoutBounds |
|---------|-------------|
| Center of table (virtual) | 427, 367 |
| YourTricks label | 12, 641, 144, 72 |
| TheirTricks label | 697, 641, 144, 72 |
| Player played card | 390, 476 |
| Partner played card | 390, 162 |
| Left Opp played card | 158, 312 |
| Right Opp played card | 623, 312 |
| KittyCard1 | 600, 476 |

## Rotation Angles by Seat

| Seat | Hand rotation | Played card rotation |
|------|--------------|---------------------|
| Player | 0° | 0° (via Perspective) |
| Partner | 180° | 0° (Perspective changes to Player) |
| LeftOpponent | 90° | 0° |
| RightOpponent | 270° | 0° |

## Animation Details

- **Easing**: CubicInOut for smooth acceleration/deceleration.
- **Face-change midpoint**: At duration/2, swap the temp Image's Source. This avoids a
  jarring instant flip — the card visually "turns over" at the halfway point.
- **Rotation**: RotateTo runs in parallel with TranslateTo, both same duration.
- **Scale for trick clearing**: ScaleTo(0.3) in parallel, applied only when destination
  is a Label (the tricks-taken indicator).
- **All array items animate simultaneously** via Task.WhenAll.

## Implementation Order

1. Add `ShowAnimations` to GameSettings, EuchreOptions, and EuchreTable field.
2. Implement `AnimateCards` method.
3. Wire into `DealACardForDeal` (scenario a).
4. Wire into `DealACard` (scenario b).
5. Wire into `PlaySelectedCard` (scenario c).
6. Wire into `SwapCardWithKitty` (scenario d).
7. Wire into `PrepTrick` (scenario e — trick clearing).
8. Test each scenario, tune timing.

1. Should the card sound play at the START or END of each animation? (Current: end)
2. For trick clearing (e), should all 4 cards animate simultaneously, or stagger?
3. For dealing (b), should we match real Euchre dealing pattern (2-3-2-3 or 3-2-3-2)?
4. What animation duration feels right? I'll start with 200ms and adjust.
5. For the kitty swap (d), is a visible animation important, or is instant OK?
   (It's quick and somewhat hidden by other UI activity.)

---

## Score Card Images (Point 3)

The score images are composites of `cardfacefiveof*.png` + `cardback.png` overlapped to show
requisite pips, as done when keeping score in real Euchre.

### Current State
- All are RGB mode (no transparency) — background is opaque white
- Various sizes (89×115 to 147×150)
- "Zero" images (`scorethemzero.png`, `scoreuszero.png`) still show the OLD cardback
  with "C#Euchre" text — these need regeneration with the new cardback

### What's Needed
1. All score images need transparent backgrounds (convert white background → transparent)
2. The "zero" images need regeneration using the new cardback.png
3. All score images need updated with chamfered card edges

### Can This Be Scripted?
**Partially.** A script can:
- Make white backgrounds transparent (flood fill from corners)
- No — since the cards themselves have white interiors, a simple white→transparent 
  won't work. The background white and card white are the same color.

**The overlap compositing is the hard part.** To properly regenerate these, you need:
- The exact overlap offsets for each score value (how many pips show)  
- The rotation of the cardback vs the face card
- The "us" images are vertical, "them" images are horizontal (rotated 90°)

**Recommendation:** This is best done manually in Photoshop:
1. Re-create each composite using the new chamfered cards + new cardback
2. Export with transparent backgrounds
3. The overlap patterns are well-established Euchre scoring conventions

Alternatively, if you provide me the exact overlap offsets and orientations, I could
write a Pillow script to composite them programmatically.
