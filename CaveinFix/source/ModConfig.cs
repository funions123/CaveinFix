namespace CaveinFix;

public class ModConfig
{
    /// <summary>
    /// Multiplies the instability of all unstable rock blocks.
    /// 1.0 = default behaviour. Lower values make blocks more stable (fewer cave-ins);
    /// higher values make blocks less stable (more cave-ins). Clamped to [0, 1] after multiplication.
    /// </summary>
    public float InstabilityMultiplier { get; set; } = 1.0f;
}
