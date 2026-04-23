using System.Collections.Generic;

public class Wordbank9 : AdaptiveWordbankAI
{
    private static readonly string[] words =
    {
        "พรหมาสตร์", "อัคนิวาต", "นาคบาศ", "สัพพัญญุตญาณ", "พุทธปฏิภาณญาณ", "อภิธรรมปิฎก", "ปฏิปทามัชฌิมา", "อนุปาทิเสสนิพพาน",
        "ทศพิน", "แผลงศร", "สหัสกุมาร", "สัพเพธรรมอนัตตา", "อนิจจังทุกขังอนัตตา", "สัทธาสูร", "วิสุทธิมรรค", "อภิญญาญาณ",
        "วิรุญจำบัง", "รณพักตร์", "ปรมัตถสัจจกถา", "กุมภกรรณ"
    };

    protected override IReadOnlyList<string> OriginalWords => words;

    protected override bool AutoLoadDefaultTieredListFromResources => false;
}
