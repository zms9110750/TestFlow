namespace zms9110750.TestFlow;

/// <summary>
/// 用来验证 CI 的 API 对比：这个类相对基线包（0.1.0）是"新增"，应当让 preview 走 additive 分支。
/// 动这一行只为触发一次 preview（workflow 文件自身的改动不在触发路径里）。第三次触发：验证前导零修复。
/// </summary>
public static class Calculator
{
    /// <summary>把两个整数相加。</summary>
    /// <param name="a">第一个加数。</param>
    /// <param name="b">第二个加数。</param>
    /// <returns>两个数之和。</returns>
    public static int Add(int a, int b)
    {
        return a + b;
    }
}
