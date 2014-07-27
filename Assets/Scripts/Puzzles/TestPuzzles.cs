using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[PuzzleCategory("Test")]
public class TestPuzzles
{
    [Puzzle(
        name: "AND Gate",
        desc: "Output RED if both inputs are RED, otherwise output BLUE.",
        diff: Difficulty.Trivial,
        width: 13, height: 13,
        inputPeriod: 2
    )]
    [Input("A", 0), Input("B", 2), Output("Q", 0)]
    public class AndGate : Puzzle
    {
        public override bool ShouldAccept(bool[] input, bool[] output)
        {
            return input.All(x => x) == output[0];
        }
    }
}
