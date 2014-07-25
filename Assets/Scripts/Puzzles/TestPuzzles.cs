using System.Collections;

using UnityEngine;

[PuzzleCategory("Test")]
public static class TestPuzzles
{
    [Puzzle(
        name: "AND Gate",
        desc: "A simple test puzzle.",
        diff: Difficulty.Medium,
        width: 27, height: 17,
        inputs: 2, outputs: 1
    )]
    public static bool ANDGate(bool[] input, bool[] output)
    {
        return (input[0] && input[1]) == output[0];
    }
}
