# Trivia Board Game - Setup Guide

## ✅ What's Been Implemented

**Core Systems:**
- ✅ CSV Question Manager with priority queue
- ✅ Player Progress tracking (points + categories)
- ✅ Trivia Popup UI
- ✅ Question Tile integration
- ✅ Circular board navigation

**Files Created:**
- `Assets/Scripts/Trivia/QuestionManager.cs`
- `Assets/Scripts/Trivia/PlayerProgress.cs`
- `Assets/Scripts/Trivia/TriviaPopup.cs`
- `Assets/Resources/Questions/TriviaQuestions.csv` (20 sample questions)

**Files Modified:**
- `Assets/Scripts/SpecialTile.cs` - Added QuestionTile effect
- `Assets/Scripts/GameControl.cs` - Added trivia integration
- `Assets/Scripts/FollowThePath.cs` - Added circular mode

---

## 🎮 Unity Setup Steps

### 1. Create Manager GameObjects

In your **TriviaBoardGame** scene:

**A. QuestionManager**
1. Create Empty GameObject: `QuestionManager`
2. Add `QuestionManager.cs` script
3. In Inspector:
   - **Question CSV**: Drag `Assets/Resources/Questions/TriviaQuestions.csv`

**B. PlayerProgress**
1. Create Empty GameObject: `PlayerProgress`
2. Add `PlayerProgress.cs` script
3. In Inspector (optional adjustments):
   - Points For Correct: 10
   - Points For Wrong: -5
   - Categories To Win: 8

---

### 2. Create Trivia Popup UI

**UI Hierarchy:**
```
Canvas
└─ TriviaPopup (Panel)
    ├─ QuestionText (Text)
    ├─ CategoryText (Text)
    ├─ OptionA_Button (Button with Text child)
    ├─ OptionB_Button (Button with Text child)
    ├─ OptionC_Button (Button with Text child)
    ├─ OptionD_Button (Button with Text child)
    ├─ ConfirmButton (Button)
    ├─ FeedbackText (Text)
    ├─ Player1PointsText (Text)
    └─ Player2PointsText (Text)
```

**Steps:**
1. Create UI Panel (darkened background)
2. Add all Text/Button elements as shown above
3. Add `TriviaPopup.cs` to the Panel
4. **Link all UI elements in Inspector**:
   - Popup Panel → the panel itself
   - Question Text → QuestionText
   - Category Text → CategoryText  
   - Option A-D Buttons → the 4 buttons
   - Confirm Button → ConfirmButton
   - Feedback Text → FeedbackText
   - Player Points → both point displays

5. **Initially hide the panel** (uncheck in Inspector)

---

### 3. Setup Question Tiles

**On waypoints that should be question tiles:**
1. Select waypoint GameObject
2. Add `SpecialTile.cs` component (if not already)
3. In Inspector:
   - **Effect**: QuestionTile
   - **Question Category**: Choose from:
     - Science
     - History
     - Geography
     - Sports
     - Entertainment
     - Literature
     - Art
     - General

**Recommendation:** Place 8 question tiles (1 per category) around the board

---

### 4. Enable Circular Mode

**For each player's FollowThePath:**
1. Select Player1 GameObject
2. Find `FollowThePath` component
3. Check **Is Circular** ✅
4. Repeat for Player2

---

### 5. Keep BasicBoardGame Intact

**Important:** Your existing `SampleScene` should work unchanged:
- No QuestionManager needed
- No TriviaPopup needed
- FollowThePath `isCircular` = unchecked (linear)
- All question tiles will be ignored without TriviaPopup

---

## 📋 CSV Question Format

Add questions to `TriviaQuestions.csv`:

```csv
Question,OptionA,OptionB,OptionC,OptionD,CorrectAnswer,Category
"What is 2+2?",3,4,5,6,B,Science
```

**Rules:**
- Use quotes around questions/options with commas
- CorrectAnswer: A, B, C, or D
- Categories: One of the 8 defined categories

---

## 🎯 Game Flow

1. **Player lands on Question Tile**
2. **Popup appears** with question
3. **Player selects answer** (A/B/C/D)
4. **Click Confirm**
5. **Feedback shown** (correct/wrong)
6. **Points awarded/deducted**
7. **If correct**: Category marked complete + reroll granted
8. **If wrong**: Turn switches
9. **Win condition**: Complete all 8 categories → Final round (TBD)

---

## 🧪 Testing Checklist

- [ ] QuestionManager loads CSV correctly
- [ ] Popup appears when landing on question tile
- [ ] All 4 options display properly
- [ ] Answer validation works
- [ ] Points update correctly
- [ ] Category completion tracking
- [ ] Reroll granted on correct answer
- [ ] Circular board wraps properly
- [ ] BasicBoardGame still works (no changes)

---

## 📝 Next Steps (Optional)

- Create final round trigger when all 8 categories complete
- Add more questions to CSV
- Create win condition for trivia mode
- Add animations/polish to popup
- Add category progress UI (visual badges)

---

**Ready to test!** Open TriviaBoardGame scene and play! 🎲✨
