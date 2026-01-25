# Trivia Board Game - Implementation Plan

## Overview

Expand the board game with a trivia question system featuring 8 categories, CSV-based questions, points system, and circular board navigation. Keep BasicBoardGame intact.

---

## Proposed Changes

### Core Data Systems

#### **CSV Question Manager**

**[NEW]** `Assets/Scripts/Trivia/QuestionManager.cs`
- Singleton pattern
- CSV parsing (Question, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, Category)
- Question pool per category (8 categories)
- Track asked questions per game session
- Priority queue: unanswered → incorrectly answered → correctly answered
- Public methods: `GetQuestion(category)`, `MarkAnswered(questionId, correct)`

**[NEW]** `Assets/Resources/Questions/TriviaQuestions.csv`
- Store all trivia questions
- Format: `Question,OptionA,OptionB,OptionC,OptionD,CorrectAnswer,Category`
- Categories: Science, History, Geography, Sports, Entertainment, Literature, Art, General

---

### Points & Progress System

#### **[NEW]** `Assets/Scripts/Trivia/PlayerProgress.cs`
- Track points per player
- Track categories completed (8 total)
- Win condition: All 8 categories answered correctly
- Persist across turns

#### **[MODIFY]** `Assets/Scripts/GameControl.cs`
- Add reference to PlayerProgress
- Integrate points display
- Handle final round trigger

---

### Trivia UI

#### **[NEW]** `Assets/Scripts/Trivia/TriviaPopup.cs`
- Show/hide popup
- Display question & 4 options
- Handle button clicks (A/B/C/D)
- Confirm button logic
- Correct/wrong feedback animation
- Award points or penalty

**UI Hierarchy:**
```
Canvas
└─ TriviaPopup (Panel)
    ├─ QuestionText
    ├─ OptionA_Button
    ├─ OptionB_Button
    ├─ OptionC_Button
    ├─ OptionD_Button
    ├─ ConfirmButton
    └─ FeedbackText
```

---

### Special Tile Integration

#### **[MODIFY]** `Assets/Scripts/SpecialTile.cs`
- Add `QuestionTile` to TileEffect enum
- Add category field for question tiles

#### **[MODIFY]** `Assets/Scripts/GameControl.cs`
- Detect QuestionTile landing
- Open TriviaPopup
- Pass category to QuestionManager
- Handle answer result:
  - **Correct**: Add points, mark category complete, grant reroll
  - **Wrong**: Subtract points, continue turn

---

### Circular Board Navigation

#### **[MODIFY]** `Assets/Scripts/FollowThePath.cs`
- Add `isCircular` boolean flag
- Modify waypoint index wrapping: `waypointIndex = waypointIndex % waypoints.Length`
- Handle forward/backward movement with wrapping
- Remove "end of board" logic for circular mode

#### **Win Condition**
- Linear (BasicBoardGame): Reach final waypoint
- Circular (TriviaBoardGame): Complete all 8 categories

---

### Scene Separation

#### **[NEW]** TriviaBoardGame Scene
- Copy SampleScene as base
- Set `FollowThePath.isCircular = true`
- Add question tiles with category assignments
- Include TriviaPopup UI
- Link to QuestionManager

#### **[KEEP]** BasicBoardGame (SampleScene)
- No changes to existing logic
- Linear board remains functional
- No trivia system

---

## Data Structures

```csharp
public class TriviaQuestion
{
    public int id;
    public string question;
    public string optionA, optionB, optionC, optionD;
    public string correctAnswer; // "A", "B", "C", or "D"
    public string category;
}

public class PlayerProgress
{
    public int points;
    public HashSet<string> completedCategories;
    public bool IsReadyForFinal => completedCategories.Count == 8;
}
```

---

## Implementation Order

1. **CSV System** - QuestionManager + parser
2. **Points System** - PlayerProgress tracking
3. **Trivia UI** - Popup design and logic
4. **Integration** - Wire to GameControl
5. **Circular Navigation** - FollowThePath modifications
6. **Scene Setup** - TriviaBoardGame configuration

---

## Testing Plan

1. CSV loading and question retrieval
2. Question popup display
3. Answer validation
4. Points tracking
5. Category completion
6. Circular board wrapping
7. Final round trigger

---

> [!IMPORTANT]
> Preserve BasicBoardGame functionality - all new trivia code should be optional or scene-specific.
