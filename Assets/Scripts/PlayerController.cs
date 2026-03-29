using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Typer typerScript; // ลาก GameObject ที่มีสคริปต์ Typer มาใส่ช่องนี้
    public Animator anim;     // ลาก Animator ของตัวละครมาใส่

    [Header("Animation Settings")]
    // เวลาหน่วงก่อนกลับท่ายืนปกติ (อาจจะปรับให้นานขึ้นนิดนึง เช่น 0.3 - 0.5 วินาที ให้เห็นท่าฟันชัดๆ)
    public float attackDuration = 0.3f; 

    private int lastScore = 0;

    void Start()
    {
        if (typerScript != null)
        {
            // ดึงคะแนนเริ่มต้นมาเก็บไว้
            lastScore = typerScript.Score; 
        }
    }

    void Update()
    {
        if (typerScript == null) return;

        int currentScore = typerScript.Score;

        // ถ้าคะแนนปัจจุบัน มากกว่า คะแนนก่อนหน้า = พิมพ์จบคำได้สำเร็จ!
        if (currentScore > lastScore)
        {
            PlayAttackAnimation();
        }

        // อัปเดตคะแนนล่าสุดไว้เทียบในเฟรมถัดไป
        lastScore = currentScore; 
    }

    private void PlayAttackAnimation()
    {
        StopAllCoroutines(); 
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        anim.SetBool("isAtk", true);
        yield return new WaitForSeconds(attackDuration);
        anim.SetBool("isAtk", false);
    }
}