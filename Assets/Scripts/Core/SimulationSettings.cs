﻿using UnityEngine;
using System;

/// <summary>
/// ScriptableObject chứa các thông số cấu hình cho simulation.
/// Tạo asset: Right-click > Create > Solar System > Simulation Settings
/// 
/// === VẤN ĐỀ VỚI KHOẢNG CÁCH THỰC ===
/// Hệ Mặt Trời thực tế CỰC KỲ TRỐNG RỖNG:
///   - Mercury: 0.387 AU, Neptune: 30.07 AU → chênh 78 lần
///   - Bán kính Trái Đất: 0.0000426 AU → VÔ HÌNH ở zoom toàn cảnh
///   - Nếu Sun = quả bóng rổ, Trái Đất = hạt đậu cách 26 mét, Neptune cách 780 mét!
///
/// === GIẢI PHÁP: TÁCH PHYSICS KHỎI VISUAL ===
/// Physics chạy chính xác với khoảng cách thật (AU) → quỹ đạo đúng.
/// Visual dùng khoảng cách NÉN → nhìn đẹp, các hành tinh gần nhau hơn.
///
/// === 2 CHẾ ĐỘ ===
/// 1. Realistic: 1 AU = 1 Unity unit (cho nghiên cứu, giáo dục)
/// 2. GameFriendly: Nén khoảng cách bằng power function → đẹp mắt
///
/// === CÔNG THỨC NÉN KHOẢNG CÁCH ===
/// visual_dist = baseDistance + distanceMultiplier × real_dist^compressionPower
///
/// Với compressionPower = 0.5 (căn bậc 2):
///   Mercury: √0.387 = 0.622 → visual 1.24
///   Earth:   √1.0   = 1.0   → visual 2.0
///   Jupiter: √5.203 = 2.281 → visual 4.56
///   Neptune: √30.07 = 5.484 → visual 10.97
///
/// Từ chênh lệch 78× (thực tế) → chỉ còn 8.8× (visual) → DỄ NHÌN HƠN RẤT NHIỀU!
/// </summary>
[CreateAssetMenu(fileName = "SimulationSettings", menuName = "Solar System/Simulation Settings")]
public class SimulationSettings : ScriptableObject
{
    // ==================== SIMULATION MODE ====================
    
    public enum SimMode
    {
        /// <summary>Khoảng cách thật, quỹ đạo chính xác. Cho giáo dục/nghiên cứu.</summary>
        Realistic,
        /// <summary>Khoảng cách nén, hành tinh to hơn, dễ nhìn. Cho game/demo.</summary>
        GameFriendly
    }

    [Header("=== SIMULATION MODE ===")]
    [Tooltip("Realistic = đúng vật lý. GameFriendly = nén khoảng cách cho đẹp.")]
    public SimMode mode = SimMode.GameFriendly;

    [Header("=== GRAVITATIONAL CONSTANT ===")]
    [Tooltip("G trong hệ đơn vị AU³/(M☉·day²). Giá trị chuẩn: 2.9592e-4")]
    public double gravitationalConstant = 2.9592e-4;

    [Header("=== TIME & GRAVITY CONTROL ===")]
    [Tooltip("Số ngày Earth mô phỏng per real-time second. VD: 10 = 10 ngày/giây")]
    public float timeScale = 10f;

    [Tooltip("Hệ số nhân lực hấp dẫn. 1.0 = Bình thường. Dùng để xem thiên tai khi tăng/giảm trọng lực hệ.")]
    [Range(0.1f, 10f)]
    public float gravityMultiplier = 1.0f;

    [Tooltip("Số sub-steps per FixedUpdate. Tăng = chính xác hơn, nặng hơn. 4-8 là tốt.")]
    [Range(1, 32)]
    public int subSteps = 4;

    [Header("=== PHYSICS ===")]
    [Tooltip("Softening factor để tránh chia cho 0 khi 2 body quá gần nhau")]
    public double softeningFactor = 1e-9;

    // ==================== DISTANCE COMPRESSION (GameFriendly mode) ====================

    [Header("=== DISTANCE COMPRESSION (GameFriendly) ===")]
    [Tooltip("Khoảng cách tối thiểu giữa Sun và hành tinh gần nhất (Unity units)")]
    public float baseDistance = 1.5f;

    [Tooltip("Hệ số nhân khoảng cách sau khi nén")]
    public float distanceMultiplier = 2.0f;

    [Tooltip("Luỹ thừa nén: 1.0 = linear (không nén), 0.5 = căn bậc 2, 0.3 = nén mạnh.\n" +
             "Giá trị càng nhỏ → các hành tinh ngoài càng gần hơn tương đối.")]
    [Range(0.2f, 1.0f)]
    public float compressionPower = 0.45f;

    // ==================== PLANET SIZE (GameFriendly mode) ====================

    [Header("=== PLANET VISUAL SIZE ===")]
    [Tooltip("Bật TrailRenderer để vẽ quỹ đạo")]
    public bool showOrbits = true;

    [Tooltip("Hệ số phóng đại kích thước hành tinh. 0.55 = Friendly (dễ nhìn), 0.01 = Realistic (Siêu nhỏ, y hệt thực tế)")]
    public float visualScaleMultiplier = 0.55f;

    [Tooltip("Chiều dài trail (số giây real-time)")]
    public float trailDuration = 30f;

    // ==================== SUN DRIFT (mô phỏng Sun di chuyển trong thiên hà) ====================

    [Header("=== SUN DRIFT ===")]
    [Tooltip("Bật mô phỏng Sun bay lên trên (vuông góc mặt phẳng quỹ đạo)")]
    public bool enableSunDrift = true;

    [Tooltip("Tốc độ Sun drift (Unity units/giây real-time). Giá trị nhỏ = chậm rãi, đẹp mắt.")]
    [Range(0f, 0.2f)]
    public float sunDriftSpeed = 0.1f;

    // ==================== METHODS ====================

    /// <summary>
    /// Chuyển khoảng cách thực (AU) → khoảng cách visual (Unity units).
    /// 
    /// Realistic mode: trả về nguyên bản.
    /// GameFriendly mode: nén bằng power function.
    ///
    /// Công thức: visual = baseDistance + multiplier × realDist^power
    /// 
    /// VÍ DỤ với default settings (base=1.5, mult=2.0, power=0.45):
    ///   Mercury (0.387 AU) → 1.5 + 2.0 × 0.387^0.45 = 2.78 units
    ///   Earth   (1.0 AU)   → 1.5 + 2.0 × 1.0^0.45   = 3.50 units
    ///   Jupiter (5.203 AU) → 1.5 + 2.0 × 5.203^0.45  = 5.87 units
    ///   Neptune (30.07 AU) → 1.5 + 2.0 × 30.07^0.45  = 11.13 units
    ///
    /// Tỉ lệ Neptune/Mercury: 11.13/2.78 = 4.0× (thay vì 78× thực tế!)
    /// → Tất cả fit trong camera view dễ dàng.
    /// </summary>
    public float RealToVisualDistance(double realDistAU)
    {
        if (realDistAU <= 0) return 0f;

        if (mode == SimMode.Realistic)
            return (float)realDistAU;

        // Power compression: nén khoảng cách lớn, giữ khoảng cách nhỏ
        float compressed = Mathf.Pow((float)realDistAU, compressionPower);
        return baseDistance + distanceMultiplier * compressed;
    }

    /// <summary>
    /// Chuyển position physics (AU) → position visual (Unity units).
    /// Giữ nguyên hướng, chỉ scale khoảng cách từ gốc.
    /// </summary>
    public DoubleVector3 PhysicsToVisualPosition(DoubleVector3 physicsPos)
    {
        if (mode == SimMode.Realistic)
            return physicsPos;

        double dist = physicsPos.magnitude;
        if (dist < 1e-10) return DoubleVector3.zero;

        float visualDist = RealToVisualDistance(dist);
        DoubleVector3 direction = physicsPos / dist;
        return direction * visualDist;
    }
}

