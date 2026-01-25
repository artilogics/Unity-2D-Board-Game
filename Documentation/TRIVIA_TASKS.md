# Trivia Board Game Expansion

## Core Systems
- [/] CSV Question System
  - [x] Create CSV parser
  - [x] Question data structure
  - [x] Category tracking (8 categories)
  - [x] Question pool management (no repeats)
  - [x] Priority system (incorrect answers first when exhausted)

## Points System
- [x] Player points tracking
- [ ] UI display for points
- [ ] Point rewards/penalties
- [x] Win condition (correct from all 8 categories → final round)

## Trivia UI
- [x] Question popup panel
- [x] Question text display
- [x] ABCD option buttons
- [x] Confirm button
- [x] Correct/wrong feedback
- [x] Category progress indicator

## Game Logic
- [x] Question tile trigger
- [x] Answer validation
- [x] Reroll on correct answer
- [x] Category completion tracking
- [ ] Final round trigger

## Circular Board
- [x] Waypoint looping logic
- [x] Movement wrapping
- [x] Update FollowThePath for circular mode
- [ ] Win condition for circular board

## Scene Setup
- [ ] TriviaBoardGame scene setup
- [ ] Keep BasicBoardGame intact
- [ ] Game mode selection/detection
