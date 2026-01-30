using UnityEngine;

// หน้าชนะ (Win) เป็นคลาสเล็ก ๆ ที่ reuse GameOverScreen
// - จุดประสงค์: ให้ใน Inspector/Scene ใช้คอมโพเนนต์คนละตัว (GameWinScreen) ได้ง่าย
// - การทำงาน: เรียก base.Show(points, true) เพื่อเปิดโหมดชนะ (YOU WIN + ปุ่ม Next)
public class GameWinScreen : GameOverScreen
{
    /// <summary>
    /// Show(points): เปิดหน้าจอ “ชนะ” พร้อมคะแนน
    /// - เรียก base.Show(points, true) เพื่อให้ GameOverScreen ทำงานในโหมดชนะ
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - ถ้าอยากเพิ่มเอฟเฟกต์เฉพาะตอนชนะก่อนโชว์ UI ก็เพิ่มที่นี่ได้
    /// </summary>
    public new void Show(int points)
    {
        base.Show(points, true);
    }
}
