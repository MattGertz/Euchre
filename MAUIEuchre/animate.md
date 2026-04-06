# Card Animation Plan

## Overview

Add card movement animations in five scenarios. MAUI provides `TranslateTo()` for smooth
position-based animations on any `VisualElement`, which is the primary tool we'll use.

All animations use a temporary `Image` element added to the `AbsoluteLayout`, translated from
source to destination, then removed. The permanent card Image at the destination is shown
after the animation completes.

---

## Scenarios

### (a) Deal for Deal (face-up cards choosing dealer)

**Current behavior:** Card appears instantly at destination, card sound plays.

**Animated behavior:**
1. Create a temp `Image` with the card's face image.
2. Position it at the kitty/center location in the AbsoluteLayout.
3. Animate (`TranslateTo`) from center to the destination card slot.
4. On completion: show the real card image at the destination, remove temp image.
5. Play card sound.

**Duration:** ~150-200ms per card. This is rapid since there can be many cards dealt
before a Jack appears.

**Question:** Should cards overlap during animation, or wait for each to finish?
Current code already has `await Task.Delay(_timerSleepDuration)` between cards,
so sequential is natural.

---

### (b) Dealing Hands (5 cards to each player)

**Current behavior:** Cards appear instantly at destination.

**Animated behavior:**
1. Determine dealer position (the source of the deal).
2. Create a temp `Image` with `cardback.png` (unless peek mode, then face image).
3. Position at dealer's location.
4. Animate from dealer to destination card slot.
5. On completion: show real card at destination, remove temp image.
6. Play card sound at end of each card.

**Duration:** ~150ms per card. 20 cards dealt = ~3 seconds total with delays.

**Question:** In the WPF version, are cards dealt one-at-a-time or in batches of 2-3
as in real Euchre? Current code deals them one at a time in `DealACard`.

---

### (c) Playing a Card (trick play)

**Current behavior:** Card disappears from hand, appears at played position.

**Animated behavior:**
1. Hide the card at its hand position.
2. Create a temp `Image` with the card's face image.
3. Position at the card's hand slot location.
4. Animate from hand slot to the played-card position (center area).
5. On completion: show the played card image at center, remove temp image.

**Duration:** ~200-250ms. This is a deliberate action, can be slightly slower.

**Consideration:** The player's hand position may differ per seat:
- Bottom (Player): cards are horizontal, played card goes up to center
- Top (Partner): cards are horizontal, played card goes down to center
- Left (LeftOpponent): cards may be rotated, played card goes right to center
- Right (RightOpponent): cards may be rotated, played card goes left to center

The temp image should NOT rotate during animation — it starts at the hand orientation
and arrives at the played orientation (which is always vertical for center cards).

---

### (d) Kitty Card Swap (dealer picks up, discards)

**Current behavior:** Card images swap instantly.

**Animated behavior:**
Two sub-animations:
1. **Pick up from kitty:** Animate kitty card from center to dealer's hand position.
   Card may be face-up or face-down depending on peek mode and whether dealer is human.
2. **Discard to kitty:** Animate discarded card from dealer's hand to kitty position.
   Always face-down.

**Consideration:** The arriving card must end up vertical from the player's perspective
regardless of which seat the dealer is in. If the dealer is Left/Right opponent, the
card in-hand is rotated 90°, but during animation it transitions to/from vertical.

**Question:** Should these be two distinct visible animations, or can the swap happen
as one combined visual? The WPF version does it instantly. A brief animation
(150ms each) would add nice polish without being slow.

---

### (e) Clearing Tricks (cards whisk away)

**Current behavior:** Played cards disappear instantly.

**Animated behavior:**
1. Determine trick winner's seat (the direction cards fly toward).
2. For each of the 4 (or 3) played cards, animate them simultaneously from their
   played positions outward past the winner's seat edge, going offscreen.
3. On completion: hide all played card images.

**Direction mapping:**
- Winner is Player (bottom): cards fly down off bottom edge
- Winner is Partner (top): cards fly up off top edge
- Winner is LeftOpponent: cards fly left off left edge
- Winner is RightOpponent: cards fly right off right edge

**Duration:** ~300-400ms. Simultaneous animation of all cards. "Whimsical" per user's
request — could add slight rotation and/or stagger start times by 30-50ms each.

**Also applies to:** End of hand clearing. Same animation, same direction.

---

## Implementation Approach

### Helper Method
```csharp
private async Task AnimateCard(ImageSource imageSource, 
    double fromX, double fromY, double toX, double toY,
    uint duration = 200)
{
    var tempImage = new Image { Source = imageSource, WidthRequest = 80, HeightRequest = 104 };
    AbsoluteLayout.SetLayoutBounds(tempImage, new Rect(fromX, fromY, 80, 104));
    GameCanvas.Children.Add(tempImage);
    
    await tempImage.TranslateTo(toX - fromX, toY - fromY, duration, Easing.CubicOut);
    
    GameCanvas.Children.Remove(tempImage);
}
```

### Position Helpers
Need a method to get the absolute position of a card slot within the AbsoluteLayout:
```csharp
private (double x, double y) GetCardPosition(Image cardImage)
{
    return (AbsoluteLayout.GetLayoutBounds(cardImage).X, 
            AbsoluteLayout.GetLayoutBounds(cardImage).Y);
}
```

### Integration Points

Each scenario modifies an existing async method:
- (a): `DealACardForDeal()` — add animation before showing final card
- (b): `DealACard()` — add animation before showing final card  
- (c): `PlaySelectedCard()` — hide source, animate, then show at destination
- (d): Pickup/discard in `Bid1PickUp` state handling
- (e): `PrepTrick()` / hand-end cleanup — animate before hiding

### Questions for User

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
