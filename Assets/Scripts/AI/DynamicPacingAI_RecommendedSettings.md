# Dynamic Pacing AI - Recommended Settings for Testing

## Default Settings (Conservative - Good for testing)

### State Transition Thresholds
- **pressureWpmThreshold**: 30 WPM
- **pressureAccuracyThreshold**: 0.85 (85%)
- **recoveryMistakeThreshold**: 0.3 (30% mistake rate)
- **burstRequiredStreak**: 3 (3 perfect words in a row)

### Timer Adjustments
- **timerReducePercentPressure**: 0.1 (10% reduction)
  - If normal = 5s, Pressure = 4.5s
- **timerIncreasePercentRecovery**: 0.15 (15% increase)
  - If normal = 5s, Recovery = 5.75s

### Monster Integration
- **enemyAttackRateMultiplier_Normal**: 1.0
- **enemyAttackRateMultiplier_Pressure**: 1.2 (20% faster attacks)
- **enemyAttackRateMultiplier_Recovery**: 0.7 (30% slower attacks)
- **enemyAttackRateMultiplier_Burst**: 1.5 (50% faster - challenge!)

---

## Easy Mode (ผ่อนเกม)
Use this if players are struggling:

```csharp
recoveryMistakeThreshold = 0.4f;           // More forgiving (40% mistakes OK)
timerIncreasePercentRecovery = 0.25f;      // More time boost (25%)
burstRequiredStreak = 5;                    // Harder to trigger Burst
timerReducePercentPressure = 0.05f;        // Gentler reduction (5%)
enemyAttackRateMultiplier_Recovery = 0.5f; // Much slower attacks
```

---

## Hard Mode (ท้าทายมากขึ้น)
Use this if players find it too easy:

```csharp
recoveryMistakeThreshold = 0.2f;           // Less forgiving (20% mistakes OK)
timerIncreasePercentRecovery = 0.1f;       // Less time boost (10%)
burstRequiredStreak = 2;                    // Easier to trigger Burst
timerReducePercentPressure = 0.15f;        // Harder reduction (15%)
enemyAttackRateMultiplier_Pressure = 1.3f; // Even faster pressure
enemyAttackRateMultiplier_Burst = 1.8f;    // Extreme challenge!
```

---

## Testing Checklist

### ✓ Test Pressure State
- [ ] Play fast and perfect for 2+ words
- [ ] Timer should decrease by 10% (5s → 4.5s)
- [ ] Monster attacks faster (1.2x)
- [ ] Difficulty should stay normal or slightly increase
- [ ] Debug log: "State changed to Pressure"

### ✓ Test Recovery State
- [ ] Make mistakes in 2+ consecutive words
- [ ] Timer should increase by 15% (5s → 5.75s)
- [ ] Monster attacks slower (0.7x)
- [ ] Next word should be easier
- [ ] Debug log: "State changed to Recovery"

### ✓ Test Burst State
- [ ] Play perfectly for 3+ consecutive words
- [ ] Timer should suddenly decrease (5s → 4.25s)
- [ ] This is a short challenge
- [ ] Then return to previous state
- [ ] Debug log: "State changed to Burst"

### ✓ Test Normal State
- [ ] Play with medium speed and occasional mistakes
- [ ] Should stay in Normal state
- [ ] Timer unchanged at default
- [ ] Monster at default attack rate

---

## How to Monitor

Open Console and watch for logs:

```
DynamicPacingAI: Word 'perfect' completed. Good: 1, Mistakes: 0
DynamicPacingAI: Word 'word' completed. Good: 2, Mistakes: 0
DynamicPacingAI: State changed to Burst (#1). Good streak: 3, Mistake streak: 0
```

---

## Troubleshooting

**Problem**: DynamicPacingAI not being called
- [ ] Check if Typer.cs finds OnWordResult method
- [ ] Verify DynamicPacingAI is on same GameObject as Typer OR
- [ ] DynamicPacingAI is in wordbankBehaviour field

**Problem**: State not changing
- [ ] Check recoveryMistakeThreshold value
- [ ] Verify consecutiveGoodWords is incrementing
- [ ] Check console for "State changed" logs

**Problem**: Timer not adjusting
- [ ] Check ApplyTimerAdjustment() method is being called
- [ ] Verify countdownTime is being modified
- [ ] Check if another system is overriding it

---

## Recommended First Test
1. Load Level 1
2. Use **Default Settings**
3. Play 10-15 words
4. Watch Console for state transitions
5. Verify timer changes
6. Adjust as needed
