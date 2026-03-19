using System;
using System.Reflection;
using UnityEngine;

public class TypeMasterAIWordbankTiers : MonoBehaviour
{
    [Header("Wordbank 10 ระดับ (0 = ง่ายสุด, 9 = ยากสุด)")]
    [Tooltip("ระดับ 0 (ง่ายสุด): เปลี่ยนตรงนี้ = เปลี่ยน 'ชุดคำ' ที่ผู้เล่นจะได้รับเมื่อ AI อยู่ระดับ 0")]
    [SerializeField] private MonoBehaviour level0;

    [Tooltip("ระดับ 1: เปลี่ยนตรงนี้ = เปลี่ยนชุดคำของระดับ 1")]
    [SerializeField] private MonoBehaviour level1;

    [Tooltip("ระดับ 2: เปลี่ยนตรงนี้ = เปลี่ยนชุดคำของระดับ 2")]
    [SerializeField] private MonoBehaviour level2;

    [Tooltip("ระดับ 3: เปลี่ยนตรงนี้ = เปลี่ยนชุดคำของระดับ 3")]
    [SerializeField] private MonoBehaviour level3;

    [Tooltip("ระดับ 4: เปลี่ยนตรงนี้ = เปลี่ยนชุดคำของระดับ 4")]
    [SerializeField] private MonoBehaviour level4;

    [Tooltip("ระดับ 5: เปลี่ยนตรงนี้ = เปลี่ยนชุดคำของระดับ 5")]
    [SerializeField] private MonoBehaviour level5;

    [Tooltip("ระดับ 6: เปลี่ยนตรงนี้ = เปลี่ยนชุดคำของระดับ 6")]
    [SerializeField] private MonoBehaviour level6;

    [Tooltip("ระดับ 7: เปลี่ยนตรงนี้ = เปลี่ยนชุดคำของระดับ 7")]
    [SerializeField] private MonoBehaviour level7;

    [Tooltip("ระดับ 8: เปลี่ยนตรงนี้ = เปลี่ยนชุดคำของระดับ 8")]
    [SerializeField] private MonoBehaviour level8;

    [Tooltip("ระดับ 9 (ยากสุด): เปลี่ยนตรงนี้ = เปลี่ยนชุดคำของระดับ 9")]
    [SerializeField] private MonoBehaviour level9;

    private object[] resolvedProviders;
    private MethodInfo[] resolvedGetWordMethods;

    private void Awake()
    {
        ResolveProviders();
    }

    private void OnValidate()
    {
        // ช่วยให้ตั้งค่าแล้วเห็นผลทันทีใน Editor
        ResolveProviders();
    }

    public int TierCount => 10;

    public bool TryGetWord(int tierIndex, out string word)
    {
        word = string.Empty;
        ResolveIfNeeded();

        if (tierIndex < 0 || tierIndex >= TierCount) return false;
        var provider = resolvedProviders?[tierIndex];
        var mi = resolvedGetWordMethods?[tierIndex];
        if (provider == null || mi == null) return false;

        try
        {
            word = (string)mi.Invoke(provider, null);
            return !string.IsNullOrEmpty(word);
        }
        catch
        {
            return false;
        }
    }

    public MonoBehaviour GetBehaviour(int tierIndex)
    {
        return tierIndex switch
        {
            0 => level0,
            1 => level1,
            2 => level2,
            3 => level3,
            4 => level4,
            5 => level5,
            6 => level6,
            7 => level7,
            8 => level8,
            9 => level9,
            _ => null,
        };
    }

    private void ResolveIfNeeded()
    {
        if (resolvedProviders == null || resolvedProviders.Length != TierCount) ResolveProviders();
        if (resolvedGetWordMethods == null || resolvedGetWordMethods.Length != TierCount) ResolveProviders();
    }

    private void ResolveProviders()
    {
        resolvedProviders = new object[TierCount];
        resolvedGetWordMethods = new MethodInfo[TierCount];

        for (int i = 0; i < TierCount; i++)
        {
            var behaviour = GetBehaviour(i);
            if (behaviour == null) continue;

            var type = behaviour.GetType();
            var mi = type.GetMethod(
                "GetWord",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null
            );

            if (mi == null || mi.ReturnType != typeof(string))
            {
                Debug.LogError($"TypeMasterAIWordbankTiers: ระดับ {i} ({type.Name}) ไม่มีเมธอด GetWord():string");
                continue;
            }

            resolvedProviders[i] = behaviour;
            resolvedGetWordMethods[i] = mi;
        }
    }
}
